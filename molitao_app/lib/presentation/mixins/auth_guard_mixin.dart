import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/user_provider.dart';
import '../../data/services/navigation_service.dart';

/// 用于需要登录的页面的 Mixin
/// 在 initState 时自动检查登录状态
mixin AuthGuardMixin<T extends ConsumerStatefulWidget> on ConsumerState<T> {
  bool get requireLogin => true;

  @override
  void initState() {
    super.initState();
    if (requireLogin) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        _checkAuth();
      });
    }
  }

  void _checkAuth() {
    final userState = ref.read(userProvider);
    if (!userState.isLoggedIn || userState.token == null) {
      _showLoginDialog();
    }
  }

  void _showLoginDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: const Text('提示'),
        content: const Text('需要登录后才能继续'),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              NavigationService.instance.navigateToHome();
            },
            child: const Text('取消'),
          ),
          TextButton(
            onPressed: () {
              Navigator.of(context).pop();
              NavigationService.instance.navigateToLogin();
            },
            child: const Text('去登录'),
          ),
        ],
      ),
    );
  }
}

/// 用于需要登录的 ConsumerState 的简化基类
abstract class AuthenticatedConsumerState<T extends ConsumerStatefulWidget>
    extends ConsumerState<T>
    with AuthGuardMixin<T> {
  @override
  bool get requireLogin => true;
}
