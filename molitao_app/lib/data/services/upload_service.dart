import 'dart:io';
import 'package:dio/dio.dart';
import '../api/api_client.dart';
import '../api/api_endpoints.dart';

/// 图片上传服务
/// 上传图片到又拍云存储
class UploadService {
  static const String _upyunDomain = 'http://image.molitao.top';
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
      final response = await _apiClient.dio.get(
        ApiEndpoints.getUploadSignature,
      );

      if (response.statusCode == 200 && response.data != null) {
        return response.data as Map<String, dynamic>;
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

      final formData = FormData.fromMap({
        'file': await MultipartFile.fromFile(filePath),
      });

      // 构建授权头
      final authorization = signature['authorization'] as String?;
      final date = signature['date'] as String?;
      final policy = signature['policy'] as String?;

      final headers = <String, String>{};
      if (authorization != null) {
        headers['Authorization'] = authorization;
      }
      if (date != null) {
        headers['Date'] = date;
      }
      if (policy != null) {
        headers['policy'] = policy;
      }

      // 上传到又拍云
      final response = await _upyunDio.post(
        '$_upyunDomain/$_upyunBucket/$remotePath',
        data: formData,
        options: Options(headers: headers),
      );

      if (response.statusCode == 200) {
        // 又拍云返回的数据中包含 url
        final data = response.data;
        if (data is Map && data['url'] != null) {
          return '$_upyunDomain${data['url']}';
        }
        // 如果返回格式不同，尝试其他方式
        return '$_upyunDomain/$remotePath';
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
