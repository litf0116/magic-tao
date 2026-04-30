import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:fluwx/fluwx.dart';

class WeChatService {
  static final WeChatService _instance = WeChatService._internal();
  factory WeChatService() => _instance;
  WeChatService._internal();

  static const String _appId = 'wxbfbe7d50ed28ed41';
  static const String _universalLink = 'https://www.molitao.top/wechat/';

  static const String auctionStartTemplateId =
      'aCmoAwuGevXMgA6mlq6x5pXrj7yNx5HJ6akzkHDCDPg';

  final Fluwx _fluwx = Fluwx();
  bool _isInitialized = false;

  Future<void> initialize() async {
    if (_isInitialized) return;

    await _fluwx.registerApi(
      appId: _appId,
      doOnAndroid: true,
      doOnIOS: true,
      universalLink: _universalLink,
    );

    _isInitialized = true;
  }

  Future<bool> checkWeChatInstalled() async {
    return await _fluwx.isWeChatInstalled;
  }

  Future<bool> login() async {
    if (!_isInitialized) {
      await initialize();
    }

    final installed = await _fluwx.isWeChatInstalled;
    if (!installed) {
      throw Exception('请先安装微信');
    }

    return await _fluwx.authBy(
      which: NormalAuth(scope: 'snsapi_userinfo', state: 'molitao_flutter_app'),
    );
  }

  Future<bool> pay({
    required String appId,
    required String partnerId,
    required String prepayId,
    required String packageValue,
    required String nonceStr,
    required int timeStamp,
    required String sign,
  }) async {
    if (!_isInitialized) {
      await initialize();
    }

    return await _fluwx.pay(
      which: Payment(
        appId: appId,
        partnerId: partnerId,
        prepayId: prepayId,
        packageValue: packageValue,
        nonceStr: nonceStr,
        timestamp: timeStamp,
        sign: sign,
      ),
    );
  }

  Future<bool> requestSubscribeMessage({
    int scene = 1,
    String? reserved,
  }) async {
    debugPrint('[WeChatService] requestSubscribeMessage 开始');
    debugPrint('[WeChatService] _isInitialized: $_isInitialized');

    if (!_isInitialized) {
      debugPrint('[WeChatService] 初始化微信 SDK...');
      await initialize();
    }

    final installed = await _fluwx.isWeChatInstalled;
    debugPrint('[WeChatService] 微信是否安装: $installed');

    if (!installed) {
      debugPrint('[WeChatService] 微信未安装，返回 false');
      return false;
    }

    debugPrint('[WeChatService] 调用微信一次性订阅消息');
    debugPrint('[WeChatService] appId: $_appId');
    debugPrint('[WeChatService] templateId: $auctionStartTemplateId');
    debugPrint('[WeChatService] scene: $scene');

    // 一次性订阅消息：用户同意后，微信会直接下发模板消息
    // 不需要回调来获取 openid（一次性订阅消息是微信直接发送）
    await _fluwx.open(
      target: SubscribeMessage(
        appId: _appId,
        scene: scene,
        templateId: auctionStartTemplateId,
        reserved: reserved,
      ),
    );

    debugPrint('[WeChatService] open 调用完成');
    // 一次性订阅消息调用 open 后直接返回，不需要等待结果
    // 微信会在用户授权后直接发送消息
    return true;
  }

  /// 带回调的订阅消息请求
  /// 返回 true 表示用户同意授权，返回 false 表示用户拒绝或取消
  Future<bool> requestSubscribeMessageWithCallback({
    int scene = 1,
    String? reserved,
  }) async {
    debugPrint('[WeChatService] requestSubscribeMessageWithCallback 开始');

    if (!_isInitialized) {
      debugPrint('[WeChatService] 初始化微信 SDK...');
      await initialize();
    }

    final installed = await _fluwx.isWeChatInstalled;
    debugPrint('[WeChatService] 微信是否安装: $installed');
    if (!installed) {
      debugPrint('[WeChatService] 微信未安装，返回 false');
      return false;
    }

    debugPrint('[WeChatService] 调用微信一次性订阅消息');
    debugPrint('[WeChatService] appId: $_appId');
    debugPrint('[WeChatService] templateId: $auctionStartTemplateId');
    debugPrint('[WeChatService] scene: $scene');

    // 创建 Completer 来等待用户操作结果
    final completer = Completer<bool>();

    // 添加一次性订阅监听器
    WeChatResponseSubscriber? listener;

    // 设置超时处理（如果用户直接返回App不操作）
    final timeoutTimer = Timer(const Duration(seconds: 30), () {
      debugPrint('[WeChatService] 订阅消息超时');
      if (!completer.isCompleted) {
        completer.complete(false);
      }
      if (listener != null) {
        _fluwx.removeSubscriber(listener);
      }
    });

    listener = (WeChatResponse response) {
      debugPrint('[WeChatService] 收到微信响应: ${response.runtimeType}');

      if (response is WeChatSubscribeMsgResponse) {
        debugPrint('[WeChatService] 订阅消息响应: errCode=${response.errCode}, action=${response.action}');

        timeoutTimer.cancel();

        // 移除监听器
        if (listener != null) {
          _fluwx.removeSubscriber(listener);
        }

        // 判断用户是否同意（与 UniApp 保持一致）
        // action = 'confirm' 表示用户同意
        // action = 'reject' 表示用户拒绝
        // action = 'ban' 表示用户拉黑（仍保存，因为极光推送保底）
        // action = 'filter' 表示在过滤列表（仍保存）
        // 只有用户明确拒绝时，才不保存
        if (response.action == 'reject') {
          debugPrint('[WeChatService] 用户拒绝订阅');
          if (!completer.isCompleted) {
            completer.complete(false);
          }
        } else {
          debugPrint('[WeChatService] 用户同意订阅 (action=${response.action})');
          if (!completer.isCompleted) {
            completer.complete(true);
          }
        }
      }
    };

    // 添加监听器
    _fluwx.addSubscriber(listener);

    // 调用微信订阅
    await _fluwx.open(
      target: SubscribeMessage(
        appId: _appId,
        scene: scene,
        templateId: auctionStartTemplateId,
        reserved: reserved,
      ),
    );

    debugPrint('[WeChatService] open 调用完成，等待用户操作...');

    return completer.future;
  }

  void addSubscriber(WeChatResponseSubscriber listener) {
    _fluwx.addSubscriber(listener);
  }

  void removeSubscriber(WeChatResponseSubscriber listener) {
    _fluwx.removeSubscriber(listener);
  }
}
