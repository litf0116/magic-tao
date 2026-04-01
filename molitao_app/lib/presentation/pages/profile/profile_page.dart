import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
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

  @override
  void initState() {
    super.initState();
    _loadMyCount();
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
        title: const Text('个人中心'),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
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
                                context.push('/profile/user-info');
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
                      // Settings icon (only when logged in)
                      if (userState.isLoggedIn && userState.user != null)
                        Positioned(
                          right: 0,
                          top: 0,
                          child: IconButton(
                            icon: const Icon(Icons.settings),
                            onPressed: () => context.push('/profile/user-info'),
                          ),
                        ),
                    ],
                  ),
                ),

                const SizedBox(height: 16),

                // Work tools section
                const Text(
                  '工作台',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                GridView.count(
                  shrinkWrap: true,
                  physics: const NeverScrollableScrollPhysics(),
                  crossAxisCount: 2,
                  childAspectRatio: 3.0,
                  crossAxisSpacing: 8,
                  mainAxisSpacing: 8,
                  children: [
                    _buildWorkToolItem(
                      '魔力值增加',
                      Icons.security,
                      () => _payDeposit(context),
                    ),
                    _buildWorkToolItem(
                      '魔力值减少',
                      Icons.security,
                      () => _cashOut(context),
                    ),
                  ],
                ),

                const SizedBox(height: 16),

                // Buyer section
                const Text(
                  '买家',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
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
                  child: GridView.count(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    crossAxisCount: 4,
                    childAspectRatio: 1.0,
                    children: [
                      _buildSectionItem('出价中秒杀', Icons.payment),
                      _buildSectionItem('待收货', Icons.local_shipping_outlined),
                      _buildSectionItem(
                        '已成交',
                        Icons.receipt_long,
                        onTap: () =>
                            context.push('/profile/auction-success-list'),
                      ),
                    ],
                  ),
                ),

                const SizedBox(height: 16),

                // Seller section
                const Text(
                  '卖家',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
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
                  child: GridView.count(
                    shrinkWrap: true,
                    physics: const NeverScrollableScrollPhysics(),
                    crossAxisCount: 4,
                    childAspectRatio: 1.0,
                    children: [
                      _buildSectionItem('我要卖', Icons.sell_outlined),
                      _buildSectionItem('待发货', Icons.local_shipping_outlined),
                      _buildSectionItem('订单', Icons.receipt_long),
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
                const Align(
                  alignment: Alignment.center,
                  child: Text('v1.0.0', style: TextStyle(color: Colors.grey)),
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

  Widget _buildWorkToolItem(String label, IconData icon, VoidCallback onTap) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 8),
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
        child: Row(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: const Color(0xfff6f6f6),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, size: 24, color: Colors.black87),
            ),
            const SizedBox(width: 8),
            Text(
              label,
              style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSectionItem(String label, IconData icon, {VoidCallback? onTap}) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        alignment: Alignment.center,
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: const Color(0xfff6f6f6),
                shape: BoxShape.circle,
              ),
              child: Icon(icon, size: 24, color: Colors.black87),
            ),
            const SizedBox(height: 4),
            Text(
              label,
              style: const TextStyle(fontSize: 12, fontWeight: FontWeight.w500),
            ),
          ],
        ),
      ),
    );
  }

  void _payDeposit(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('提示'),
          content: const Text('微信支付功能正在开发中，敬请期待！'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('确定'),
            ),
          ],
        );
      },
    );
  }

  void _cashOut(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('提示'),
          content: const Text('提现功能尚未开放，敬请期待！'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('确定'),
            ),
          ],
        );
      },
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
