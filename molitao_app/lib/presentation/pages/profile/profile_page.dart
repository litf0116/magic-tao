import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../domain/entities/my_count_entity.dart';
import '../../../providers/user_provider.dart';

class ProfilePage extends ConsumerWidget {
  const ProfilePage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final userState = ref.watch(userProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('个人中心'),
        backgroundColor: const Color(0xfff4835a), // Primary color
        foregroundColor: Colors.white,
      ),
      body: FutureBuilder<MyCountEntity>(
        future: _getMyCount(), // Using a local method instead of API service
        builder: (context, snapshot) {
          MyCountEntity? myCount;
          if (snapshot.hasData) {
            myCount = snapshot.data!;
          }

          return Container(
            color: const Color(0xfff6f6f6), // Background color
            padding: const EdgeInsets.all(16.0),
            child: SingleChildScrollView(
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
                          color: Colors.grey.withOpacity(0.2),
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
                                    userState.isLogin) {
                                  context.push('/user/info');
                                }
                              },
                              child: Row(
                                children: [
                                  if (!userState.isLogin ||
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
                                      userState.isLogin &&
                                              userState.user != null
                                          ? userState.user!.name ?? '未登录'
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
                            // Stats
                            if (myCount != null)
                              GridView.count(
                                shrinkWrap: true,
                                physics: const NeverScrollableScrollPhysics(),
                                crossAxisCount: 4,
                                childAspectRatio: 1.0,
                                children: [
                                  _buildStatItem(
                                    myCount.friend.toString(),
                                    '好友',
                                  ),
                                  GestureDetector(
                                    onTap: () {
                                      context.push('/user/depositLog');
                                    },
                                    child: _buildStatItem(
                                      myCount.depositBalance.toString(),
                                      '魔力值',
                                    ),
                                  ),
                                ],
                              ),
                          ],
                        ),
                        // Settings icon (only when logged in)
                        if (userState.isLogin && userState.user != null)
                          Positioned(
                            right: 0,
                            top: 0,
                            child: IconButton(
                              icon: const Icon(Icons.settings),
                              onPressed: () {
                                context.push('/user/info');
                              },
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
                      _buildWorkToolItem('魔力值增加', Icons.security, () {
                        _payDeposit(context, ref);
                      }),
                      _buildWorkToolItem('魔力值减少', Icons.security, () {
                        _cashOut(context);
                      }),
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
                          color: Colors.grey.withOpacity(0.2),
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
                        _buildSectionItem('已成交', Icons.receipt_long),
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
                          color: Colors.grey.withOpacity(0.2),
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
                  if (userState.isLogin && userState.user?.phoneNumber != null)
                    Padding(
                      padding: const EdgeInsets.symmetric(vertical: 16.0),
                      child: ElevatedButton(
                        onPressed: () {
                          _logout(ref);
                        },
                        style: ElevatedButton.styleFrom(
                          backgroundColor: Colors.red,
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(
                            vertical: 12.0,
                            horizontal: 16.0,
                          ),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8.0),
                          ),
                        ),
                        child: const Text('退出登录'),
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
          );
        },
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
              color: Colors.grey.withOpacity(0.2),
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

  void _payDeposit(BuildContext context, WidgetRef ref) {
    // Show a modal dialog explaining that WeChat payment is under development
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('提示'),
          content: const Text('微信支付功能正在开发中，敬请期待！'),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
              },
              child: const Text('确定'),
            ),
          ],
        );
      },
    );
  }

  void _cashOut(BuildContext context) {
    // Show a modal dialog explaining that cash out is not ready
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('提示'),
          content: const Text('提现功能尚未开放，敬请期待！'),
          actions: [
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
              },
              child: const Text('确定'),
            ),
          ],
        );
      },
    );
  }

  void _logout(WidgetRef ref) {
    ref.read(userProvider.notifier).logout();
  }

  // Mock implementation for getting user count data
  Future<MyCountEntity> _getMyCount() async {
    // Simulate API call delay
    await Future.delayed(const Duration(milliseconds: 500));

    // Return mock data - in real implementation, this would call the actual API
    return MyCountEntity(friend: 12, depositBalance: 100);
  }
}
