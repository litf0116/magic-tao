import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:package_info_plus/package_info_plus.dart';
import '../../../domain/entities/my_count_entity.dart';
import '../../../data/api/api_client.dart';
import '../../../data/api/api_endpoints.dart';
import '../../providers/user_provider.dart';

class ProfilePage extends ConsumerStatefulWidget {
  const ProfilePage({super.key});

  @override
  ConsumerState<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends ConsumerState<ProfilePage> {
  MyCountEntity _myCount = MyCountEntity(friend: 0, depositBalance: 0);
  bool _isLoadingCount = false;
  String _appVersion = '';

  @override
  void initState() {
    super.initState();
    _loadMyCount();
    _loadAppVersion();
  }

  Future<void> _loadAppVersion() async {
    try {
      final packageInfo = await PackageInfo.fromPlatform();
      if (mounted) {
        setState(() {
          _appVersion = packageInfo.version;
        });
      }
    } catch (e) {
      debugPrint('Failed to get package info: $e');
    }
  }

  /// 每次进入页面时重新加载数据
  Future<void> _loadMyCount() async {
    final userState = ref.read(userProvider);
    if (!userState.isLoggedIn) {
      print('[ProfilePage] 用户未登录，跳过加载统计数据');
      return;
    }

    if (_isLoadingCount) return; // 防止重复请求

    setState(() => _isLoadingCount = true);

    try {
      print('[ProfilePage] 开始获取统计数据...');
      final response = await ApiClient().dio.get(ApiEndpoints.getMyCount);
      print('[ProfilePage] API 响应: ${response.data}');

      if (response.data != null && mounted) {
        final entity = MyCountEntity.fromJson(
          response.data as Map<String, dynamic>,
        );
        print(
          '[ProfilePage] 解析成功: friend=${entity.friend}, depositBalance=${entity.depositBalance}',
        );
        setState(() {
          _myCount = entity;
          _isLoadingCount = false;
        });
      }
    } catch (e) {
      print('[ProfilePage] 获取统计数据失败: $e');
      if (mounted) {
        setState(() => _isLoadingCount = false);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final userState = ref.watch(userProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          '个人中心',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.settings, color: Colors.white),
            onPressed: () => context.push('/settings'),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _loadMyCount,
        child: Container(
          color: const Color(0xfff6f6f6),
          padding: const EdgeInsets.all(16.0),
          child: SingleChildScrollView(
            physics: const AlwaysScrollableScrollPhysics(),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // User info card
                Container(
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(8.0),
                    boxShadow: [
                      BoxShadow(
                        color: Colors.grey.withValues(alpha: 0.2),
                        spreadRadius: 1,
                        blurRadius: 5,
                        offset: const Offset(0, 2),
                      ),
                    ],
                  ),
                  padding: const EdgeInsets.all(16.0),
                  child: Stack(
                    children: [
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          // User avatar and name
                          GestureDetector(
                            onTap: () {
                              if (userState.user != null &&
                                  userState.isLoggedIn) {
context.push('/edit-profile');
                              }
                            },
                            child: Row(
                              children: [
                                if (!userState.isLoggedIn ||
                                    userState.user == null)
                                  Container(
                                    width: 48,
                                    height: 48,
                                    decoration: BoxDecoration(
                                      color: Colors.grey[300],
                                      shape: BoxShape.circle,
                                    ),
                                    child: const Icon(
                                      Icons.person,
                                      color: Colors.grey,
                                    ),
                                  )
                                else
                                  ClipRRect(
                                    borderRadius: BorderRadius.circular(24),
                                    child: Image.network(
                                      userState.user!.headImgUrl ?? '',
                                      width: 48,
                                      height: 48,
                                      fit: BoxFit.cover,
                                      errorBuilder:
                                          (context, error, stackTrace) {
                                        return Container(
                                          width: 48,
                                          height: 48,
                                          decoration: BoxDecoration(
                                            color: Colors.grey[300],
                                            shape: BoxShape.circle,
                                          ),
                                          child: const Icon(
                                            Icons.person,
                                            color: Colors.grey,
                                          ),
                                        );
                                      },
                                    ),
                                  ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: Text(
                                    userState.isLoggedIn &&
                                            userState.user != null
                                        ? userState.user!.fullName ??
                                            userState.user!.userName ??
                                            '未登录'
                                        : '未登录',
                                    style: const TextStyle(
                                      fontSize: 18,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ),
                          const SizedBox(height: 16),
                          // Stats - 始终显示
                          GridView.count(
                            shrinkWrap: true,
                            physics: const NeverScrollableScrollPhysics(),
                            crossAxisCount: 4,
                            childAspectRatio: 1.0,
                            children: [
                              _buildStatItem(_myCount.friend.toString(), '好友'),
                              GestureDetector(
                                onTap: () => context.push('/user/depositLog'),
                                child: _buildStatItem(
                                  _myCount.depositBalance.toInt().toString(),
                                  '魔力值',
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                      // Positioned(
                      //   right: 0,
                      //   top: 0,
                      //   child: IconButton(
                      //     icon: const Icon(
                      //       Icons.settings,
                      //       color: Colors.black87,
                      //       size: 28,
                      //     ),
                      //     tooltip: '个人信息设置',
                      //     onPressed: () {
                      //       if (userState.isLoggedIn &&
                      //           userState.user != null) {
                      //         context.push('/profile/user-info');
                      //       } else {
                      //         ScaffoldMessenger.of(context).showSnackBar(
                      //           const SnackBar(content: Text('请先登录')),
                      //         );
                      //       }
                      //     },
                      //   ),
                      // ),
                    ],
                  ),
                ),

                const SizedBox(height: 16),

                // Logout button (only when logged in)
                if (userState.isLoggedIn)
                  Center(
                    child: SizedBox(
                      width: double.infinity,
                      child: Padding(
                        padding: const EdgeInsets.symmetric(vertical: 16.0),
                        child: ElevatedButton(
                          onPressed: () => _showLogoutDialog(context, ref),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Colors.red,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(vertical: 14.0),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(8.0),
                            ),
                          ),
                          child: const Text(
                            '退出登录',
                            style: TextStyle(fontSize: 16),
                          ),
                        ),
                      ),
                    ),
                  ),

                // App version
                Align(
                  alignment: Alignment.center,
                  child: Text(
                    'v$_appVersion',
                    style: const TextStyle(color: Colors.grey),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildStatItem(String value, String label) {
    return Container(
      alignment: Alignment.center,
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(
            value,
            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
          ),
          const SizedBox(height: 4),
          Text(label, style: const TextStyle(fontSize: 12, color: Colors.grey)),
        ],
      ),
    );
  }

  void _showLogoutDialog(BuildContext context, WidgetRef ref) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('提示'),
          content: const Text('确定要退出登录吗？'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                _logout(ref);
              },
              child: const Text('确定', style: TextStyle(color: Colors.red)),
            ),
          ],
        );
      },
    );
  }

  void _logout(WidgetRef ref) {
    ref.read(userProvider.notifier).logout();
  }
}
