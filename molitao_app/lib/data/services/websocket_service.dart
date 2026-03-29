import 'dart:async';
import 'dart:convert';
import 'package:web_socket_channel/web_socket_channel.dart';
import 'package:web_socket_channel/status.dart' as status;

class WebSocketService {
  static const String _baseUrl = 'wss://www.molitao.top/ws';
  static const Duration _pingInterval = Duration(seconds: 30);
  static const Duration _reconnectDelay = Duration(seconds: 5);

  WebSocketChannel? _channel;
  Timer? _pingTimer;
  Timer? _reconnectTimer;
  StreamSubscription? _subscription;

  final StreamController<Map<String, dynamic>> _messageStreamController =
      StreamController<Map<String, dynamic>>.broadcast();

  bool _isConnected = false;
  bool _shouldReconnect = false;
  String? _connectionId;

  Stream<Map<String, dynamic>> get messageStream =>
      _messageStreamController.stream;

  bool get isConnected => _isConnected;

  Future<void> connect({String? connectionId}) async {
    if (_isConnected) return;

    _shouldReconnect = true;

    try {
      // If no connectionId provided, negotiate first
      if (connectionId == null) {
        _connectionId = await _negotiateConnection();
      } else {
        _connectionId = connectionId;
      }

      // Connect to WebSocket with connectionId
      final urlWithParams = '$_baseUrl?connectionId=$_connectionId';
      _channel = WebSocketChannel.connect(Uri.parse(urlWithParams));

      _subscription = _channel?.stream.listen(
        _onMessageReceived,
        onError: _onError,
        onDone: _onDone,
      );

      _isConnected = true;

      // Start ping timer
      _startPingTimer();
    } catch (e) {
      print('WebSocket connection failed: $e');
      if (_shouldReconnect) {
        _scheduleReconnect();
      }
    }
  }

  Future<String?> _negotiateConnection() async {
    // In a real implementation, you would make an HTTP call to negotiate
    // For now, we'll simulate this by generating a random connection ID
    // In practice, you'd call: POST /ws/pre-connect
    return 'conn_${DateTime.now().millisecondsSinceEpoch}';
  }

  void _onMessageReceived(dynamic message) {
    try {
      final jsonMessage = json.decode(message.toString());
      final messageType = jsonMessage['type'] as int?;

      switch (messageType) {
        case 1: // Invocation
          _handleInvocation(jsonMessage);
          break;
        case 6: // Ping
          // Respond with pong if needed
          _sendPong();
          break;
        case 7: // Close
          _handleClose(jsonMessage);
          break;
        default:
          // Handle unknown message types
          _messageStreamController.add(jsonMessage);
          break;
      }
    } catch (e) {
      print('Error parsing WebSocket message: $e');
    }
  }

  void _handleInvocation(Map<String, dynamic> message) {
    final target = message['target'] as String?;
    final arguments = message['arguments'] as List<dynamic>?;

    if (target != null) {
      // Route the message based on target
      final routedMessage = {
        'target': target,
        'arguments': arguments,
        'original': message,
      };
      _messageStreamController.add(routedMessage);
    }
  }

  void _handleClose(Map<String, dynamic> message) {
    _isConnected = false;
    _stopPingTimer();

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  void _onError(dynamic error) {
    print('WebSocket error: $error');
    _isConnected = false;
    _stopPingTimer();

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  void _onDone() {
    _isConnected = false;
    _stopPingTimer();

    if (_shouldReconnect) {
      _scheduleReconnect();
    }
  }

  void _startPingTimer() {
    _pingTimer = Timer.periodic(_pingInterval, (timer) {
      if (_isConnected) {
        _sendPing();
      }
    });
  }

  void _stopPingTimer() {
    _pingTimer?.cancel();
    _pingTimer = null;
  }

  void _sendPing() {
    final pingMessage = {'type': 6}; // Ping type
    sendMessage(pingMessage);
  }

  void _sendPong() {
    // In SignalR protocol, pongs are usually implicit
    // But we could send a specific pong message if needed
  }

  void sendMessage(Map<String, dynamic> message) {
    if (_channel != null && _isConnected) {
      _channel?.sink.add(json.encode(message));
    }
  }

  void _scheduleReconnect() {
    _reconnectTimer = Timer(_reconnectDelay, () {
      if (_shouldReconnect) {
        connect(connectionId: _connectionId);
      }
    });
  }

  Future<void> disconnect() async {
    _shouldReconnect = false;
    _stopPingTimer();
    _reconnectTimer?.cancel();
    _subscription?.cancel();
    _channel?.sink.close(status.goingAway);
    _isConnected = false;
  }

  void dispose() {
    disconnect();
    _messageStreamController.close();
  }
}
