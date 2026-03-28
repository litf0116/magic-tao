import 'package:flutter/material.dart';
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
import '../../presentation/pages/auth/login_page.dart';
import '../../presentation/pages/auction/auction_page.dart';

final GoRouter router = GoRouter(
  initialLocation: '/',
  routes: [
    GoRoute(
      path: '/login',
      name: 'login',
      builder: (context, state) => const LoginPage(),
    ),
    ShellRoute(
      builder: (context, state, child) => MainTabPage(child: child),
      routes: [
        GoRoute(
          path: '/',
          name: 'home',
          builder: (context, state) => const HomePage(),
        ),
        GoRoute(
          path: '/chat',
          name: 'chat',
          builder: (context, state) => const ChatListPage(),
        ),
        GoRoute(
          path: '/trading-post',
          name: 'trading-post',
          builder: (context, state) => const TradingPostPage(),
        ),
        GoRoute(
          path: '/contacts',
          name: 'contacts',
          builder: (context, state) => const ContactsPage(),
        ),
        GoRoute(
          path: '/profile',
          name: 'profile',
          builder: (context, state) => const ProfilePage(),
        ),
        GoRoute(
          path: '/user/depositLog',
          name: 'deposit-log',
          builder: (context, state) => const DepositLogPage(),
        ),
        GoRoute(
          path: '/user/info',
          name: 'user-info',
          builder: (context, state) => const UserInfoPage(),
        ),
        GoRoute(
          path: '/auction',
          name: 'auction',
          builder: (context, state) => const AuctionPage(),
        ),
      ],
    ),
  ],
);

final routerProvider = Provider<GoRouter>((ref) {
  return router;
});
