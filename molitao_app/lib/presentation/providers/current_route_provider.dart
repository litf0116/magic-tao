import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../core/router/app_router.dart' as app_router;

/// 当前路由状态
class CurrentRouteState {
  final String location;

  CurrentRouteState({required this.location});
}

/// 需要屏蔽推送横幅的页面
const _suppressBannerPages = [
  '/chat/auction', // 拍卖聊天页 - 竞拍中不需要弹窗打扰
];

/// 当前路由 Provider
class CurrentRouteNotifier extends StateNotifier<CurrentRouteState> {
  final GoRouter _router;

  CurrentRouteNotifier(this._router)
    : super(
        CurrentRouteState(
          location: _router.routerDelegate.currentConfiguration.uri.path,
        ),
      ) {
    _router.routerDelegate.addListener(_onRouteChanged);
  }

  void _onRouteChanged() {
    final config = _router.routerDelegate.currentConfiguration;
    state = CurrentRouteState(location: config.uri.path);
  }

  /// 检查当前页面是否应该屏蔽推送横幅
  bool shouldSuppressBanner() {
    return _suppressBannerPages.contains(state.location);
  }

  /// 获取当前页面路径
  String get currentLocation => state.location;

  @override
  void dispose() {
    _router.routerDelegate.removeListener(_onRouteChanged);
    super.dispose();
  }
}

/// CurrentRouteProvider
final currentRouteProvider =
    StateNotifierProvider<CurrentRouteNotifier, CurrentRouteState>((ref) {
      final router = ref.watch(app_router.routerProvider);
      return CurrentRouteNotifier(router);
    });
