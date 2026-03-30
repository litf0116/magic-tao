import 'dart:async';
import 'dart:convert';
import 'package:dio/dio.dart';
import 'package:web_socket_channel/web_socket_channel.dart';
import 'package:web_socket_channel/status.dart' as status;

/// SignalR WebSocket 服务
/// 参考 UniApp signalr.ts 实现
class WebSocketService {
  // WebSocket 基础 URL（生产环境）
  static const String _wsBaseUrl = 'wss://www.molitao.top/ws';
  static const String _httpBaseUrl = 'https://www.molitao.top';

  // SignalR 协议分隔符
  static const String _messageSeparator = '\x1e'; // ASCII 30

  // SignalR 协议握手消息
  static const String _protocolHandshake = '{"protocol":"json","version":1}';

  // 心跳间隔（秒）
  static const int _pingIntervalSeconds = 15;
  static const int _reconnectDelaySeconds = 5;

  WebSocketChannel? _channel;
  Timer? _pingTimer;
  Timer? _reconnectTimer;
  StreamSubscription? _subscription;

  final StreamController<Map<String, dynamic>> _messageStreamController =
      StreamController<Map<String, dynamic>>.broadcast();

  bool _isConnected = false;
  bool _shouldReconnect = false;
  String? _connectionId;
  String? _token;
  Dio? _dio;

  Stream<Map<String, dynamic>> get messageStream =>
      _messageStreamController.stream;

  bool get isConnected => _isConnected;

  /// 连接 WebSocket
  /// [token] 用户访问令牌
  Future<void> connect({String? token}) async {
    print('========== WebSocket connect() 被调用 ==========');
    print(
      '[WebSocket] token: ${token?.substring(0, token.length > 20 ? 20 : token.length)}...',
    );

    if (_isConnected) {
      print('[WebSocket] 已经连接，跳过');
      return;
    }

    _token = token;
    _shouldReconnect = true;

    try {
      // 1. 调用 negotiate 获取 connectionId
      print('[WebSocket] 步骤1: 调用 negotiate...');
      _connectionId = await _negotiate();
      if (_connectionId == null) {
        print('[WebSocket] negotiate 失败，无法获取 connectionId');
        _scheduleReconnect();
        return;
      }

      print('[WebSocket] connectionId: $_connectionId');

      // 2. 构建 WebSocket URL
      final wsUrl = '$_wsBaseUrl?id=$_connectionId&access_token=$_token';
      print('[WebSocket] 步骤2: 连接 URL: $wsUrl');

      // 3. 建立 WebSocket 连接
      print('[WebSocket] 步骤3: 建立 WebSocket 连接...');
      _channel = WebSocketChannel.connect(Uri.parse(wsUrl));

      _subscription = _channel?.stream.listen(
        _onMessageReceived,
        onError: _onError,
        onDone: _onDone,
      );

      // 4. 发送协议握手
      print('[WebSocket] 步骤4: 发送协议握手...');
      _sendProtocolHandshake();

      _isConnected = true;
      print('[WebSocket] ========== 连接成功 ==========');

      // 5. 启动心跳
      _startPingTimer();
    } catch (e) {
      print('[WebSocket] 连接失败: $e');
      if (_shouldReconnect) {
        _scheduleReconnect();
      }
    }
  }

  /// 调用 negotiate 获取 connectionId
  Future<String?> _negotiate() async {
    try {
      _dio ??= Dio();

      final negotiateUrl = '$_httpBaseUrl/ws/negotiate';
      print('[WebSocket] negotiate URL: $negotiateUrl');
      print(
        '[WebSocket] negotiate Header: Authorization: Bearer ${_token?.substring(0, _token!.length > 20 ? 20 : _token!.length)}...',
      );

      final response = await _dio!.post(
        negotiateUrl,
        options: Options(headers: {'Authorization': 'Bearer $_token'}),
      );

      print('[WebSocket] negotiate 响应状态: ${response.statusCode}');
      print('[WebSocket] negotiate 响应数据: ${response.data}');

      if (response.statusCode == 200 && response.data != null) {
        final data = response.data;
        if (data is Map<String, dynamic>) {
          final connectionId = data['connectionId'] as String?;
          print('[WebSocket] 获取到 connectionId: $connectionId');
          return connectionId;
        }
      }

      print('[WebSocket] negotiate 响应格式异常');
      return null;
    } catch (e) {
      print('[WebSocket] negotiate 请求失败: $e');
      return null;
    }
  }

  /// 发送协议握手消息
  void _sendProtocolHandshake() {
    _sendRawMessage(_protocolHandshake);
    print('[WebSocket] 已发送协议握手');
  }

  /// 接收消息处理
  void _onMessageReceived(dynamic message) {
    try {
      final messageStr = message.toString();

      // SignalR 消息以 \x1e 分隔
      final messages = messageStr
          .split(_messageSeparator)
          .where((m) => m.isNotEmpty);

      for (final msg in messages) {
        final jsonMessage = json.decode(msg) as Map<String, dynamic>;
        final messageType = jsonMessage['type'] as int?;

        switch (messageType) {
          case 1: // Invocation
            _handleInvocation(jsonMessage);
            break;
          case 2: // StreamItem
            // 暂不处理
            break;
          case 3: // Completion
            _handleCompletion(jsonMessage);
            break;
          case 6: // Ping
            // 收到 ping，更新心跳时间
            break;
          case 7: // Close
            _handleClose(jsonMessage);
            break;
          default:
            // 其他消息类型，直接转发
            _messageStreamController.add(jsonMessage);
        }
      }
    } catch (e) {
      print('[WebSocket] 解析消息失败: $e');
    }
  }

  /// 处理 Invocation 消息
  void _handleInvocation(Map<String, dynamic> message) {
    final target = message['target'] as String?;
    final arguments = message['arguments'] as List<dynamic>?;

    if (target != null) {
      final routedMessage = {
        'target': target,
        'arguments': arguments,
        'original': message,
      };
      _messageStreamController.add(routedMessage);
    }
  }

  /// 处理 Completion 消息
  void _handleCompletion(Map<String, dynamic> message) {
    // 可以用于处理 invoke 的响应
    _messageStreamController.add({
      'type': 'completion',
      'invocationId': message['invocationId'],
      'result': message['result'],
      'error': message['error'],
    });
  }

  /// 处理 Close 消息
  void _handleClose(Map<String, dynamic> message) {
    print('[WebSocket] 收到 Close 消息: $message');
    _isConnected = false;
    _stopPingTimer();

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  /// 错误处理
  void _onError(dynamic error) {
    print('[WebSocket] 错误: $error');
    _isConnected = false;
    _stopPingTimer();

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  /// 连接关闭处理
  void _onDone() {
    print('[WebSocket] 连接已关闭');
    _isConnected = false;
    _stopPingTimer();

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  /// 启动心跳定时器
  void _startPingTimer() {
    _pingTimer?.cancel();
    _pingTimer = Timer.periodic(Duration(seconds: _pingIntervalSeconds), (
      timer,
    ) {
      if (_isConnected) {
        _sendPing();
      }
    });
  }

  /// 停止心跳定时器
  void _stopPingTimer() {
    _pingTimer?.cancel();
    _pingTimer = null;
  }

  /// 发送心跳 ping
  void _sendPing() {
    // SignalR ping 消息格式
    final pingMessage = '{"type":6}';
    _sendRawMessage(pingMessage);
  }

  /// 发送原始消息（带分隔符）
  void _sendRawMessage(String message) {
    if (_channel != null && _isConnected) {
      _channel?.sink.add(message + _messageSeparator);
    }
  }

  /// 发送消息（公共接口）
  void sendMessage(Map<String, dynamic> message) {
    if (_channel != null && _isConnected) {
      final jsonStr = json.encode(message);
      _sendRawMessage(jsonStr);
    }
  }

  /// 调用服务器方法（类似 UniApp 的 invoke）
  Future<dynamic> invoke(String methodName, {List<dynamic>? args}) async {
    if (!_isConnected) {
      throw Exception('WebSocket 未连接');
    }

    final invocationId = DateTime.now().millisecondsSinceEpoch.toString();

    final message = {
      'type': 1, // Invocation
      'target': methodName,
      'arguments': args ?? [],
      'invocationId': invocationId,
    };

    // 发送消息
    sendMessage(message);

    // 等待响应（简化实现，实际应该用 Completer）
    // 这里暂时返回 null，后续可以完善
    return null;
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
    _stopPingTimer();
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
