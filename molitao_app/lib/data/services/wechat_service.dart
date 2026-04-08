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

  Future<WeChatSubscribeMsgResponse?> requestSubscribeMessage({
    int scene = 1,
    String? reserved,
  }) async {
    if (!_isInitialized) {
      await initialize();
    }

    final installed = await _fluwx.isWeChatInstalled;
    if (!installed) {
      return null;
    }

    await _fluwx.open(
      target: SubscribeMessage(
        appId: _appId,
        scene: scene,
        templateId: auctionStartTemplateId,
        reserved: reserved,
      ),
    );

    return null;
  }

  void addSubscriber(WeChatResponseSubscriber listener) {
    _fluwx.addSubscriber(listener);
  }

  void removeSubscriber(WeChatResponseSubscriber listener) {
    _fluwx.removeSubscriber(listener);
  }
}
