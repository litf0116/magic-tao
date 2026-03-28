import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../core/constants/app_constants.dart';

class MainTabPage extends StatefulWidget {
  final Widget child;
  const MainTabPage({super.key, required this.child});

  @override
  State<MainTabPage> createState() => _MainTabPageState();
}

class _MainTabPageState extends State<MainTabPage> {
  int _currentIndex = 0;

  final List<String> _tabRoutes = [
    AppConstants.homeRoute,
    AppConstants.chatRoute,
    AppConstants.tradingPostRoute,
    AppConstants.contactsRoute,
    AppConstants.profileRoute,
  ];

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final location = GoRouterState.of(context).uri.toString();
    _updateIndex(location);
  }

  void _updateIndex(String location) {
    for (int i = 0; i < _tabRoutes.length; i++) {
      if (location.startsWith(_tabRoutes[i])) {
        if (_currentIndex != i) {
          setState(() => _currentIndex = i);
        }
        return;
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: widget.child,
      bottomNavigationBar: _buildBottomNavigationBar(),
    );
  }

  Widget _buildBottomNavigationBar() {
    return Stack(
      alignment: Alignment.center,
      clipBehavior: Clip.none,
      children: [
        BottomNavigationBar(
          currentIndex: _currentIndex > 2 ? _currentIndex - 1 : _currentIndex,
          onTap: _onTabTapped,
          type: BottomNavigationBarType.fixed,
          selectedItemColor: const Color(0xFFf4835a),
          unselectedItemColor: Colors.grey,
          items: [
            _buildBottomNavigationBarItem(
              AppConstants.homeTab,
              _currentIndex == 0
                  ? AppConstants.homeTabIconActive
                  : AppConstants.homeTabIcon,
            ),
            _buildBottomNavigationBarItem(
              AppConstants.chatTab,
              _currentIndex == 1
                  ? AppConstants.chatTabIconActive
                  : AppConstants.chatTabIcon,
            ),
            _buildBottomNavigationBarItem(
              AppConstants.contactsTab,
              _currentIndex == 3
                  ? AppConstants.contactsTabIconActive
                  : AppConstants.contactsTabIcon,
            ),
            _buildBottomNavigationBarItem(
              AppConstants.profileTab,
              _currentIndex == 4
                  ? AppConstants.profileTabIconActive
                  : AppConstants.profileTabIcon,
            ),
          ],
        ),
        Positioned(
          top: 0,
          child: GestureDetector(
            onTap: () => _onTabTapped(2),
            child: Container(
              width: 56,
              height: 56,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: const Color(0xFFf4835a),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.2),
                    blurRadius: 8,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: Image.asset(
                AppConstants.tradingPostTabIcon,
                width: 24,
                height: 24,
                color: Colors.white,
              ),
            ),
          ),
        ),
      ],
    );
  }

  BottomNavigationBarItem _buildBottomNavigationBarItem(
    String label,
    String iconPath,
  ) {
    return BottomNavigationBarItem(
      icon: Image.asset(iconPath, width: 24, height: 24),
      label: label,
    );
  }

  void _onTabTapped(int index) {
    int actualIndex = index;
    if (index >= 2) {
      actualIndex = index + 1;
    }
    setState(() => _currentIndex = actualIndex);
    context.go(_tabRoutes[actualIndex]);
  }
}
