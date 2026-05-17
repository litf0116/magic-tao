import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../presentation/pages/tabbar/main_tab_page.dart';
import '../../presentation/pages/home/home_page.dart';
import '../../presentation/pages/chat/chat_list_page.dart';
import '../../presentation/pages/chat/group_chat_page.dart';
import '../../presentation/pages/chat/private_chat_page.dart';
import '../../presentation/pages/chat/auction_chat_page.dart';
import '../../presentation/pages/trading_post/trading_post_page.dart';
import '../../presentation/pages/trading_post/post_detail_page.dart';
import '../../presentation/pages/trading_post/add_post_page.dart';
import '../../presentation/pages/contacts/contacts_page.dart';
import '../../presentation/pages/profile/profile_page.dart';
import '../../presentation/pages/user/deposit_log_page.dart';
import '../../presentation/pages/user/user_info_page.dart';
import '../../presentation/pages/user/balance_log_page.dart';
import '../../presentation/pages/user/auction_success_list_page.dart';
import '../../presentation/pages/announce/announce_list_page.dart';
import '../../presentation/pages/auth/login_page.dart';
import '../../presentation/pages/auth/qr_code_confirm_page.dart';
import '../../presentation/pages/settings/settings_page.dart';
import '../../presentation/pages/settings/account_security_page.dart';
import '../../presentation/pages/about/about_page.dart';
import '../../presentation/pages/agreement/agreement_page.dart';

import '../../presentation/providers/auth_notifier.dart';

/// 需要登录才能访问的路由
const List<String> _protectedRoutes = [
  '/chat',
  '/trading-post',
  '/contacts',
  '/profile',
];

/// 检查路由是否需要登录
bool _requiresAuth(String location) {
  return _protectedRoutes.any((route) => location.startsWith(route));
}

/// 创建 GoRouter
GoRouter _createRouter(AuthNotifier authNotifier) {
  return GoRouter(
    initialLocation: '/home',
    refreshListenable: authNotifier,
    redirect: (context, state) {
      final isLoggedIn = authNotifier.isLoggedIn;
      final location = state.uri.path;

      // 如果需要登录但未登录，重定向到登录页
      if (_requiresAuth(location) && !isLoggedIn) {
        return '/login?redirect=${Uri.encodeComponent(location)}';
      }

      // 如果已登录且访问登录页，重定向到首页
      if (isLoggedIn && location == '/login') {
        return '/home';
      }

      return null; // 不需要重定向
    },
    routes: [
      GoRoute(
        path: '/login',
        name: 'login',
        builder: (context, state) {
          final redirect = state.uri.queryParameters['redirect'];
          return LoginPage(redirectPath: redirect);
        },
      ),
      GoRoute(
        path: '/qr-code-confirm',
        name: 'qr-code-confirm',
        builder: (context, state) => const QrCodeConfirmPage(),
      ),
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) =>
            MainTabPage(navigationShell: navigationShell),
        branches: [
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/home',
                name: 'home',
                builder: (context, state) => const HomePage(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/chat',
                name: 'chat',
                builder: (context, state) => const ChatListPage(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/trading-post',
                name: 'trading-post',
                builder: (context, state) => const TradingPostPage(),
                routes: [
                  GoRoute(
                    path: 'add',
                    name: 'add-post',
                    builder: (context, state) => const AddPostPage(),
                  ),
                  GoRoute(
                    path: 'detail/:id',
                    name: 'post-detail',
                    builder: (context, state) {
                      final idStr = state.pathParameters['id'] ?? '0';
                      final id = int.tryParse(idStr) ?? 0;
                      return PostDetailPage(postId: id);
                    },
                  ),
                ],
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/contacts',
                name: 'contacts',
                builder: (context, state) => const ContactsPage(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/profile',
                name: 'profile',
                builder: (context, state) => const ProfilePage(),
                routes: [
                  GoRoute(
                    path: 'deposit-log',
                    name: 'deposit-log',
                    builder: (context, state) => const DepositLogPage(),
                  ),
                  GoRoute(
                    path: 'user-info',
                    name: 'user-info',
                    builder: (context, state) {
                      final extra = state.extra as Map<String, dynamic>?;
                      final userId = extra?['userId'] as int?;
                      return UserInfoPage(userId: userId);
                    },
                  ),
                  GoRoute(
                    path: 'balance-log',
                    name: 'balance-log',
                    builder: (context, state) => const BalanceLogPage(),
                  ),
                  GoRoute(
                    path: 'announce',
                    name: 'announce',
                    builder: (context, state) {
                      final categoryId =
                          state.uri.queryParameters['categoryId'];
                      return AnnounceListPage(
                        categoryId: categoryId != null
                            ? int.tryParse(categoryId)
                            : null,
                      );
                    },
                  ),
                  GoRoute(
                    path: 'auction-success-list',
                    name: 'auction-success-list',
                    builder: (context, state) => const AuctionSuccessListPage(),
                  ),
                ],
              ),
            ],
          ),
        ],
      ),
      // 设置页面（独立路由，隐藏 tabbar）
      GoRoute(
        path: '/settings',
        name: 'settings',
        builder: (context, state) => const SettingsPage(),
      ),
      GoRoute(
        path: '/account-security',
        name: 'account-security',
        builder: (context, state) => const AccountSecurityPage(),
      ),
      GoRoute(
        path: '/about',
        name: 'about',
        builder: (context, state) => const AboutPage(),
      ),
      // 协议页面
      GoRoute(
        path: '/agreement',
        name: 'agreement',
        builder: (context, state) => const AgreementPage(),
      ),
      // 聊天详情路由
      GoRoute(
        path: '/chat/auction',
        name: 'chat-auction',
        builder: (context, state) => const AuctionChatPage(),
      ),
      GoRoute(
        path: '/chat/group/:id',
        name: 'chat-group',
        builder: (context, state) {
          final idStr = state.pathParameters['id'] ?? '0';
          final id = int.tryParse(idStr) ?? 0;
          final name = state.uri.queryParameters['name'] ?? '群聊';
          return GroupChatPage(
            channel: '${id}_group',
            channelId: id,
            channelName: name,
          );
        },
      ),
      GoRoute(
        path: '/chat/private/:id',
        name: 'chat-private',
        builder: (context, state) {
          final idStr = state.pathParameters['id'] ?? '0';
          final id = int.tryParse(idStr) ?? 0;
          final name = state.uri.queryParameters['name'] ?? '用户';
          final avatar = state.uri.queryParameters['avatar'];
          return PrivateChatPage(
            friendId: id,
            friendName: name,
            friendAvatar: avatar,
          );
        },
      ),
    ],
  );
}

/// Router Provider
final routerProvider = Provider<GoRouter>((ref) {
  final authNotifier = ref.watch(authNotifierProvider);
  return _createRouter(authNotifier);
});
