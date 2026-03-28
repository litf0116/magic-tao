import 'package:fluwx/fluwx.dart';

class WeChatService {
  static final WeChatService _instance = WeChatService._internal();
  factory WeChatService() => _instance;
  WeChatService._internal();

  static const String _appId = 'wxbfbe7d50ed28ed41';
  static const String _universalLink = 'https://www.molitao.top/wechat/';

  bool _isInitialized = false;

  Future<void> initialize() async {
    if (_isInitialized) return;

    await registerWxApi(
      appId: _appId,
      universalLink: _universalLink,
      doOnAndroid: true,
      doOnIOS: true,
    );

    _isInitialized = true;
  }

  Future<bool> isWeChatInstalled() async {
    return await isWeChatInstalled;
  }

  Future<WeChatAuthResponse?> login() async {
    if (!_isInitialized) {
      await initialize();
    }

    final installed = await isWeChatInstalled();
    if (!installed) {
      throw Exception('请先安装微信');
    }

    final result = await sendWeChatAuth(
      scope: 'snsapi_userinfo',
      state: 'molitao_flutter_app',
    );

    return result;
  }

  Future<WeChatPaymentResponse?> pay({
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

    return await payWithWeChat(
      appId: appId,
      partnerId: partnerId,
      prepayId: prepayId,
      packageValue: packageValue,
      nonceStr: nonceStr,
      timeStamp: timeStamp,
      sign: sign,
    );
  }
}
