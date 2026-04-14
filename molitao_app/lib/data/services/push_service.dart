import 'dart:async';
import 'package:audioplayers/audioplayers.dart';
import 'package:flutter/foundation.dart';
import 'package:jpush_flutter/jpush_flutter.dart';
import 'package:jpush_flutter/jpush_interface.dart';

class PushService {
  static final PushService _instance = PushService._internal();
  factory PushService() => _instance;
  PushService._internal();

  late JPushFlutterInterface _jpush;
  final StreamController<PushMessage> _messageController =
      StreamController<PushMessage>.broadcast();
  final StreamController<PushMessage> _clickController =
      StreamController<PushMessage>.broadcast();
  final AudioPlayer _audioPlayer = AudioPlayer();

  String _registrationId = '';
  bool _isInitialized = false;

  Stream<PushMessage> get onMessage => _messageController.stream;
  Stream<PushMessage> get onClick => _clickController.stream;
  String get registrationId => _registrationId;

  Future<void> init() async {
    if (_isInitialized) return;

    print('[Push] 开始初始化...');

    try {
      _jpush = JPush.newJPush();
      print('[Push] JPush 实例已创建');

      _jpush.setup(
        appKey: '4e91398522bb1286f6452efb',
        channel: 'developer-default',
        production: true,
        debug: true,
      );
      print('[Push] setup 完成');

      _jpush.applyPushAuthority();

      _jpush.addEventHandler(
        onOpenNotification: (message) async {
          debugPrint('[Push] 点击通知: $message');
          _handleNotification(message, isClick: true);
        },
        onReceiveNotification: (message) async {
          debugPrint('[Push] 收到通知: $message');
          _handleNotification(message, isClick: false);
        },
        onReceiveMessage: (message) async {
          debugPrint('[Push] 收到自定义消息: $message');
          _handleCustomMessage(message);
        },
        onConnected: (message) async {
          debugPrint('[Push] 连接成功: $message');
        },
      );

      final rid = await _jpush.getRegistrationID();
      _registrationId = rid ?? '';
      debugPrint('[Push] Registration ID: $_registrationId');

      _isInitialized = true;
      debugPrint('[Push] 初始化成功');
    } catch (e) {
      debugPrint('[Push] 初始化失败: $e');
    }
  }

  void _handleNotification(
    Map<String, dynamic> message, {
    required bool isClick,
  }) {
    // JPush 通知格式: {alert: "内容", title: "标题", extras: {...}}
    final pushMessage = PushMessage(
      title: message['title']?.toString() ?? '通知',
      content:
          message['alert']?.toString() ?? message['content']?.toString() ?? '',
      extras: Map<String, dynamic>.from(message['extras'] ?? {}),
    );

    if (isClick) {
      _clickController.add(pushMessage);
    } else {
      // 收到前台推送时播放声音
      _playNotificationSound();
      _messageController.add(pushMessage);
    }
  }

  Future<void> _playNotificationSound() async {
    try {
      await _audioPlayer.play(AssetSource('sounds/cgsys11.mp3'));
      debugPrint('[Push] 播放通知声音');
    } catch (e) {
      debugPrint('[Push] 播放声音失败: $e');
    }
  }

  void _handleCustomMessage(Map<String, dynamic> message) {
    final pushMessage = PushMessage(
      title: message['title']?.toString() ?? '',
      content: message['content']?.toString() ?? '',
      extras: Map<String, dynamic>.from(message['extras'] ?? {}),
    );
    _messageController.add(pushMessage);
  }

  Future<void> setAlias(String alias) async {
    try {
      await _jpush.setAlias(alias);
      debugPrint('[Push] 设置别名成功: $alias');
    } catch (e) {
      debugPrint('[Push] 设置别名失败: $e');
    }
  }

  Future<void> deleteAlias() async {
    try {
      await _jpush.deleteAlias();
      debugPrint('[Push] 删除别名成功');
    } catch (e) {
      debugPrint('[Push] 删除别名失败: $e');
    }
  }

  void setBadge(int badge) {
    _jpush.setBadge(badge);
  }

  void clearAllNotifications() {
    _jpush.clearAllNotifications();
  }

  void dispose() {
    _messageController.close();
    _clickController.close();
    _audioPlayer.dispose();
  }
}

class PushMessage {
  final String title;
  final String content;
  final Map<String, dynamic> extras;

  PushMessage({
    required this.title,
    required this.content,
    required this.extras,
  });

  String? get path => extras['path']?.toString();
  String? get auctionItemId => extras['auctionItemId']?.toString();
  String? get type => extras['type']?.toString();
}
