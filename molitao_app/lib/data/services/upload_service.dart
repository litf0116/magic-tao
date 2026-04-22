import 'dart:convert';
import 'dart:io';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';

/// 图片上传服务
/// 上传图片到又拍云存储
class UploadService {
  static const String _upyunDomain = 'https://image.molitao.top';
  static const String _upyunBucket = 'molitao';
  static const String _upyunOperator = 'molitao';

  final ApiClient _apiClient = ApiClient();
  final Dio _upyunDio = Dio();

  /// 上传图片
  /// [filePath] 本地图片路径
  /// [userId] 用户ID，用于构建存储路径
  /// 返回图片完整URL
  Future<String?> uploadImage(String filePath, {String? userId}) async {
    try {
      // 1. 获取上传签名
      final signature = await _getSignature();
      if (signature == null) {
        print('[UploadService] 获取签名失败');
        return null;
      }

      // 2. 构建远程路径
      final timestamp = DateTime.now().millisecondsSinceEpoch;
      final path = 'wxapp/${userId ?? 'guest$timestamp'}/';
      final fileName = '$timestamp${_getFileExtension(filePath)}';
      final remotePath = '$path$fileName';

      // 3. 上传到又拍云
      final imageUrl = await _uploadToUpyun(
        filePath: filePath,
        remotePath: remotePath,
        signature: signature,
      );

      return imageUrl;
    } catch (e) {
      print('[UploadService] 上传图片失败: $e');
      return null;
    }
  }

  /// 获取上传签名
  Future<Map<String, dynamic>?> _getSignature() async {
    try {
      // 1. 构造 policy 配置
      final date = HttpDate.format(DateTime.now().toUtc());
      final opts = {
        'save-key':
            '/${_upyunOperator}/{year}-{mon}-{day}/upload_{random32}{.suffix}',
        'bucket': _upyunBucket,
        'expiration':
            (DateTime.now().millisecondsSinceEpoch ~/ 1000) + 3600 * 60,
        'date': date,
      };

      // 2. Base64 编码 policy
      final policyJson = jsonEncode(opts);
      final policy = base64Encode(utf8.encode(policyJson));

      // 3. 构造 data 字符串
      final data = 'POST&/${_upyunBucket}&$date&$policy';

      // 4. 调用后端 API 获取签名
      final response = await _apiClient.dio.get(
        ApiEndpoints.getUploadSignature,
        queryParameters: {'data': data, 'policy': policy},
      );

      // 5. 使用返回数据构造 authorization
      if (response.statusCode == 200 && response.data != null) {
        final sigData = response.data as Map<String, dynamic>;
        final signature = sigData['signature'] as String?;
        final operator = sigData['operator'] as String?;
        final policyReturned = sigData['policy'] as String?;
        final bucket = sigData['bucket'] as String?;
        final domainHost = sigData['domainHost'] as String?;

        if (signature == null || operator == null || policyReturned == null) {
          print('[UploadService] 签名数据不完整');
          return null;
        }

        return {
          'authorization': 'UPYUN $operator:$signature',
          'policy': policyReturned,
          'bucket': bucket ?? _upyunBucket,
          'domainHost': domainHost ?? _upyunDomain,
        };
      }
      return null;
    } catch (e) {
      print('[UploadService] 获取签名失败: $e');
      return null;
    }
  }

  /// 上传到又拍云
  Future<String?> _uploadToUpyun({
    required String filePath,
    required String remotePath,
    required Map<String, dynamic> signature,
  }) async {
    try {
      final file = File(filePath);
      if (!await file.exists()) {
        print('[UploadService] 文件不存在: $filePath');
        return null;
      }

      // 提取签名数据
      final authorization = signature['authorization'] as String?;
      final policy = signature['policy'] as String?;
      final bucket = signature['bucket'] as String?;
      final domainHost = signature['domainHost'] as String?;

      if (authorization == null || policy == null) {
        print('[UploadService] 签名数据不完整');
        return null;
      }

      // 又拍云要求 authorization 和 policy 都在 formData 中
      final formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(filePath),
        'authorization': authorization,
        'policy': policy,
      });

      // 上传到又拍云
      final uploadUrl = 'https://v0.api.upyun.com/${bucket ?? _upyunBucket}';
      final response = await _upyunDio.post(
        uploadUrl,
        data: formData,
        options: Options(
          validateStatus: (status) => status != null && status < 500,
        ),
      );

      print('[UploadService] ====== 又拍云响应详情 ======');
      print('[UploadService] 状态码: ${response.statusCode}');
      print('[UploadService] 响应数据: ${response.data}');
      print('[UploadService] 响应数据类型: ${response.data.runtimeType}');
      print('[UploadService] 响应头: ${response.headers.map}');
      print('[UploadService] ==========================');

      if (response.statusCode == 200 || response.statusCode == 201) {
        // 检查响应头中的 X-Upyun-Uri
        final upyunUri =
            response.headers.value('x-upyun-uri') ??
            response.headers.value('X-Upyun-Uri');

        if (upyunUri != null) {
          final imageUrl = '${domainHost ?? _upyunDomain}$upyunUri';
          print('[UploadService] 从响应头获取图片URL: $imageUrl');
          return imageUrl;
        }

        // 检查响应体
        final data = response.data;
        print('[UploadService] 响应数据类型检查: ${data.runtimeType}');

        // 又拍云返回的是 JSON 字符串，需要解析
        if (data is String && data.isNotEmpty) {
          try {
            final jsonData = jsonDecode(data) as Map<String, dynamic>;
            final url = jsonData['url'] as String?;
            if (url != null) {
              final imageUrl = '${domainHost ?? _upyunDomain}$url';
              print('[UploadService] 从JSON字符串解析图片URL: $imageUrl');
              return imageUrl;
            }
          } catch (e) {
            print('[UploadService] JSON解析失败: $e');
          }
        }

        // 检查是否已经是 Map
        if (data is Map && data['url'] != null) {
          final imageUrl = '${domainHost ?? _upyunDomain}${data['url']}';
          print('[UploadService] 从Map获取图片URL: $imageUrl');
          return imageUrl;
        }

        // 如果返回格式不同，尝试其他方式
        print('[UploadService] 无法从响应中获取URL，使用备用路径');
        return '${domainHost ?? _upyunDomain}/${bucket ?? _upyunBucket}/$remotePath';
      }

      return null;
    } catch (e) {
      print('[UploadService] 上传到又拍云失败: $e');
      return null;
    }
  }

  /// 获取文件扩展名
  String _getFileExtension(String filePath) {
    final lastDot = filePath.lastIndexOf('.');
    if (lastDot > 0 && lastDot < filePath.length - 1) {
      return filePath.substring(lastDot);
    }
    return '.jpg';
  }

  /// 释放资源
  void dispose() {
    _upyunDio.close();
  }
}
