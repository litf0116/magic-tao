import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../presentation/pages/tabbar/main_tab_page.dart';
import '../../presentation/pages/home/home_page.dart';
import '../../presentation/pages/chat/chat_list_page.dart';
import '../../presentation/pages/trading_post/trading_post_page.dart';
import '../../presentation/pages/trading_post/post_detail_page.dart';
import '../../presentation/pages/trading_post/add_post_page.dart';
import '../../presentation/pages/contacts/contacts_page.dart';
import '../../presentation/pages/profile/profile_page.dart';
import '../../presentation/pages/user/deposit_log_page.dart';
import '../../presentation/pages/user/user_info_page.dart';
import '../../presentation/pages/user/balance_log_page.dart';
import '../../presentation/pages/user/user_list_page.dart';
import '../../presentation/pages/announce/announce_list_page.dart';
import '../../presentation/pages/auth/login_page.dart';
import '../../presentation/pages/auction/auction_page.dart';

final GoRouter router = GoRouter(
  initialLocation: '/home',
  routes: [
    GoRoute(
      path: '/login',
      name: 'login',
      builder: (context, state) => const LoginPage(),
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
                  builder: (context, state) => const UserInfoPage(),
                ),
                GoRoute(
                  path: 'balance-log',
                  name: 'balance-log',
                  builder: (context, state) => const BalanceLogPage(),
                ),
                GoRoute(
                  path: 'user-list',
                  name: 'user-list',
                  builder: (context, state) => const UserListPage(),
                ),
                GoRoute(
                  path: 'announce',
                  name: 'announce',
                  builder: (context, state) {
                    final categoryId = state.uri.queryParameters['categoryId'];
                    return AnnounceListPage(
                      categoryId: categoryId != null
                          ? int.tryParse(categoryId)
                          : null,
                    );
                  },
                ),
              ],
            ),
          ],
        ),
      ],
    ),
    GoRoute(
      path: '/auction',
      name: 'auction',
      builder: (context, state) => const AuctionPage(),
    ),
  ],
);

final routerProvider = Provider<GoRouter>((ref) {
  return router;
});
