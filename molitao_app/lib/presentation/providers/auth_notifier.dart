import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'user_provider.dart';

/// 用于 GoRouter 监听登录状态变化
/// 当登录状态改变时，会通知 GoRouter 重新计算重定向
class AuthNotifier extends ChangeNotifier {
  final Ref _ref;
  bool _previousIsLoggedIn = false;

  AuthNotifier(this._ref) {
    // 监听 userProvider 的变化
    _ref.listen<UserState>(userProvider, (previous, next) {
      if (_previousIsLoggedIn != next.isLoggedIn) {
        _previousIsLoggedIn = next.isLoggedIn;
        notifyListeners();
      }
    });

    // 初始化当前状态
    _previousIsLoggedIn = _ref.read(userProvider).isLoggedIn;
  }

  bool get isLoggedIn => _ref.read(userProvider).isLoggedIn;
  String? get token => _ref.read(userProvider).token;
}

final authNotifierProvider = ChangeNotifierProvider<AuthNotifier>((ref) {
  return AuthNotifier(ref);
});
