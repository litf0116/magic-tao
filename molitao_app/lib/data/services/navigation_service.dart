import 'package:go_router/go_router.dart';

class NavigationService {
  static final NavigationService instance = NavigationService._internal();

  GoRouter? _router;

  NavigationService._internal();

  GoRouter? get router => _router;

  void setRouter(GoRouter router) {
    _router = router;
  }

  void navigateToLogin() {
    _router?.go('/login');
  }

  void navigateToHome() {
    _router?.go('/home');
  }
}
