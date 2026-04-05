import 'dart:async';
import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:web_socket_channel/web_socket_channel.dart';
import 'package:web_socket_channel/status.dart' as status;

/// WebSocket 服务
/// 与 UniApp 实现保持一致，使用 /ws/pre-connect 端点
class WebSocketService {
  // HTTP 基础 URL
  static const String _httpBaseUrl = 'https://www.molitao.top';

  // 重连延迟（秒）
  static const int _reconnectDelaySeconds = 5;

  WebSocketChannel? _channel;
  Timer? _reconnectTimer;
  StreamSubscription? _subscription;

  final StreamController<Map<String, dynamic>> _messageStreamController =
      StreamController<Map<String, dynamic>>.broadcast();

  bool _isConnected = false;
  bool _shouldReconnect = false;
  int? _websocketId;
  String? _token;
  Dio? _dio;

  Stream<Map<String, dynamic>> get messageStream =>
      _messageStreamController.stream;

  bool get isConnected => _isConnected;

  /// 连接 WebSocket
  /// 与 UniApp chatStore.connectServer() 保持一致
  Future<void> connect({String? token}) async {
    print('========== WebSocket connect() 被调用 ==========');

    if (_isConnected) {
      print('[WebSocket] 已经连接，跳过');
      return;
    }

    _token = token;
    _shouldReconnect = true;

    try {
      // 1. 调用 pre-connect 获取 WebSocket URL
      print('[WebSocket] 步骤1: 调用 /ws/pre-connect...');
      final preConnectResult = await _preConnect();
      if (preConnectResult == null) {
        print('[WebSocket] pre-connect 失败');
        _scheduleReconnect();
        return;
      }

      // 后端返回的数据结构是 {result: {server: "...", websocketId: ...}}
      // 或者是 {result: {code: 0, server: "...", websocketId: ...}}
      final result = preConnectResult['result'] as Map<String, dynamic>?;
      final serverUrl =
          result?['server'] as String? ?? preConnectResult['server'] as String?;
      _websocketId =
          (result?['websocketId'] ?? preConnectResult['websocketId']) as int?;

      if (serverUrl == null || serverUrl.isEmpty) {
        print('[WebSocket] 未获取到 server URL');
        _scheduleReconnect();
        return;
      }

      // 直接使用后端返回的 WebSocket 服务器地址
      final correctedServerUrl = serverUrl;

      print('[WebSocket] 获取到 server: $serverUrl');
      print('[WebSocket] 修正后 server: $correctedServerUrl');
      print('[WebSocket] websocketId: $_websocketId');

      // 2. 建立 WebSocket 连接
      print('[WebSocket] 步骤2: 建立 WebSocket 连接...');
      _channel = WebSocketChannel.connect(Uri.parse(correctedServerUrl));

      _subscription = _channel?.stream.listen(
        _onMessageReceived,
        onError: _onError,
        onDone: _onDone,
      );

      _isConnected = true;
      print('[WebSocket] ========== 连接成功 ==========');
    } catch (e) {
      print('[WebSocket] 连接失败: $e');
      if (_shouldReconnect) {
        _scheduleReconnect();
      }
    }
  }

  /// 调用 pre-connect 获取 WebSocket 连接信息
  Future<Map<String, dynamic>?> _preConnect() async {
    try {
      _dio ??= Dio();

      final preConnectUrl = '$_httpBaseUrl/ws/pre-connect';
      print('[WebSocket] pre-connect URL: $preConnectUrl');

      final response = await _dio!.post(
        preConnectUrl,
        options: Options(headers: {'Authorization': 'Bearer $_token'}),
      );

      print('[WebSocket] pre-connect 响应状态: ${response.statusCode}');
      print('[WebSocket] pre-connect 响应数据: ${response.data}');

      if (response.statusCode == 200 && response.data != null) {
        return response.data as Map<String, dynamic>;
      }

      return null;
    } catch (e) {
      print('[WebSocket] pre-connect 请求失败: $e');
      return null;
    }
  }

  /// 接收消息处理
  /// 与 UniApp chatStore.onmessage() 保持一致
  void _onMessageReceived(dynamic message) {
    try {
      final messageStr = message.toString();
      print('[WebSocket] 收到消息: $messageStr');

      Map<String, dynamic> jsonMessage;

      // 解析 JSON
      if (messageStr.startsWith('{') || messageStr.startsWith('[')) {
        jsonMessage = json.decode(messageStr) as Map<String, dynamic>;
      } else {
        print('[WebSocket] 非JSON消息，跳过');
        return;
      }

      // 检查错误消息
      if (jsonMessage['type'] == 'Error') {
        print('[WebSocket] 收到错误消息: ${jsonMessage['receipt']}');
        return;
      }

      // 转换消息类型：将数值类型转换为字符串类型（与 UniApp 保持一致）
      _convertMessageType(jsonMessage);

      // 发送到消息流
      _messageStreamController.add(jsonMessage);
    } catch (e) {
      print('[WebSocket] 解析消息失败: $e');
    }
  }

  /// 转换消息类型
  /// 与 UniApp chatStore.onmessage 中的 typeMap 保持一致
  void _convertMessageType(Map<String, dynamic> msg) {
    final type = msg['type'];
    if (type is! int) return;

    const typeMap = {
      1: 'Text',
      2: 'Image',
      3: 'File',
      10: 'Receipt',
      100: 'Welcome',
      101: 'Goodbye',
      102: 'BanUser',
      110: 'Backout',
      1000: 'AuctionStart',
      1002: 'AuctionBid',
      1010: 'AuctionEnd',
      1011: 'AuctionDeal',
      2000: 'KasecStatusChanged',
      '-1': 'Error',
    };

    if (typeMap.containsKey(type)) {
      msg['type'] = typeMap[type];
      print('[WebSocket] 消息类型转换: $type -> ${typeMap[type]}');
    }
  }

  /// 错误处理
  void _onError(dynamic error) {
    print('[WebSocket] 错误: $error');
    _isConnected = false;

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  /// 连接关闭处理
  void _onDone() {
    print('[WebSocket] 连接已关闭');
    _isConnected = false;

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  /// 发送消息（公共接口）
  void sendMessage(Map<String, dynamic> message) {
    if (_channel != null && _isConnected) {
      final jsonStr = json.encode(message);
      print('[WebSocket] 发送消息: $jsonStr');
      _channel?.sink.add(jsonStr);
    } else {
      print('[WebSocket] 未连接，无法发送消息');
    }
  }

  /// 计划重连
  void _scheduleReconnect() {
    _reconnectTimer?.cancel();
    _reconnectTimer = Timer(Duration(seconds: _reconnectDelaySeconds), () {
      if (_shouldReconnect) {
        print('[WebSocket] 尝试重连...');
        connect(token: _token);
      }
    });
  }

  /// 断开连接
  Future<void> disconnect() async {
    print('[WebSocket] 断开连接');
    _shouldReconnect = false;
    _reconnectTimer?.cancel();
    _subscription?.cancel();
    _channel?.sink.close(status.goingAway);
    _isConnected = false;
  }

  /// 销毁服务
  void dispose() {
    disconnect();
    _messageStreamController.close();
    _dio?.close();
  }
}
