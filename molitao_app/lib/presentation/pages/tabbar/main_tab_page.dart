import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:persistent_bottom_nav_bar_v2/persistent_bottom_nav_bar_v2.dart';

class MainTabPage extends StatelessWidget {
  final StatefulNavigationShell navigationShell;

  const MainTabPage({super.key, required this.navigationShell});

  @override
  Widget build(BuildContext context) {
    return PersistentTabView.router(
      navigationShell: navigationShell,
      tabs: [
        PersistentRouterTabConfig(
          item: ItemConfig(
            icon: Image.asset(
              'assets/images/tab/tab1_b.png',
              width: 24,
              height: 24,
              color: Colors.grey,
            ),
            inactiveIcon: Image.asset(
              'assets/images/tab/tab1.png',
              width: 24,
              height: 24,
              color: const Color(0xFFf4835a),
            ),
            title: '首页',
            activeForegroundColor: const Color(0xFFf4835a),
            inactiveForegroundColor: Colors.grey,
          ),
        ),
        PersistentRouterTabConfig(
          item: ItemConfig(
            icon: Image.asset(
              'assets/images/tab/tab2_b.png',
              width: 24,
              height: 24,
              color: Colors.grey,
            ),
            inactiveIcon: Image.asset(
              'assets/images/tab/tab2.png',
              width: 24,
              height: 24,
              color: const Color(0xFFf4835a),
            ),
            title: '会话列表',
            activeForegroundColor: const Color(0xFFf4835a),
            inactiveForegroundColor: Colors.grey,
          ),
        ),
        PersistentRouterTabConfig(
          item: ItemConfig(
            icon: Image.asset(
              'assets/images/add.png',
              width: 24,
              height: 24,
              color: Colors.grey,
            ),
            inactiveIcon: Image.asset(
              'assets/images/add.png',
              width: 24,
              height: 24,
              color: const Color(0xFFf4835a),
            ),
            title: '交易站',
            activeForegroundColor: const Color(0xFFf4835a),
            inactiveForegroundColor: Colors.grey,
          ),
        ),
        PersistentRouterTabConfig(
          item: ItemConfig(
            icon: Image.asset(
              'assets/images/tab/tab3_b.png',
              width: 24,
              height: 24,
              color: Colors.grey,
            ),
            inactiveIcon: Image.asset(
              'assets/images/tab/tab3.png',
              width: 24,
              height: 24,
              color: const Color(0xFFf4835a),
            ),
            title: '通讯录',
            activeForegroundColor: const Color(0xFFf4835a),
            inactiveForegroundColor: Colors.grey,
          ),
        ),
        PersistentRouterTabConfig(
          item: ItemConfig(
            icon: Image.asset(
              'assets/images/tab/tab4_b.png',
              width: 24,
              height: 24,
              color: Colors.grey,
            ),
            inactiveIcon: Image.asset(
              'assets/images/tab/tab4.png',
              width: 24,
              height: 24,
              color: const Color(0xFFf4835a),
            ),
            title: '个人中心',
            activeForegroundColor: const Color(0xFFf4835a),
            inactiveForegroundColor: Colors.grey,
          ),
        ),
      ],
      navBarBuilder: (navBarConfig) => Style1BottomNavBar(
        navBarConfig: navBarConfig,
        navBarDecoration: const NavBarDecoration(color: Colors.white),
      ),
    );
  }
}
