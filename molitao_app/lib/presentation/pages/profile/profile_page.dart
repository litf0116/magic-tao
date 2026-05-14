import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:package_info_plus/package_info_plus.dart';
import '../../../domain/entities/my_count_entity.dart';
import '../../../data/api/api_client.dart';
import '../../../data/api/api_endpoints.dart';
import '../../../data/repositories/payment_repository.dart';
import '../../providers/user_provider.dart';
import '../../../core/widgets/app_bottom_sheet.dart';

/// 魔力值保证金固定金额
const double _depositAmount = 51;

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
context.push('/settings');
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
                       Positioned(
                         right: 0,
                         top: 0,
                         child: IconButton(
                           icon: const Icon(
                             Icons.settings,
                             color: Colors.black87,
                             size: 28,
                           ),
                           tooltip: '个人信息设置',
onPressed: () {
                              if (userState.isLoggedIn && userState.user != null) {
                                context.push('/profile/user-info');
                              } else {
                                ScaffoldMessenger.of(context).showSnackBar(
                                  const SnackBar(content: Text('请先登录')),
                                );
                              }
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
    showAppBottomSheet(
      context: context,
      builder: (context) => _DepositBottomSheet(
        onDepositSuccess: () {
          // 刷新用户数据
          _loadMyCount();
        },
      ),
    );
  }

  void _cashOut(BuildContext context) {
    showAppBottomSheet(
      context: context,
      builder: (context) => _WithdrawalBottomSheet(
        onWithdrawSuccess: () {
          // 刷新用户数据
          _loadMyCount();
        },
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

/// 充值魔力值底部弹窗
class _DepositBottomSheet extends StatefulWidget {
  final VoidCallback? onDepositSuccess;

  const _DepositBottomSheet({this.onDepositSuccess});

  @override
  State<_DepositBottomSheet> createState() => _DepositBottomSheetState();
}

class _DepositBottomSheetState extends State<_DepositBottomSheet> {
  void _showMessage(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), duration: const Duration(seconds: 2)),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(
        bottom: MediaQuery.of(context).viewInsets.bottom,
      ),
      child: Container(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // 标题栏
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  '充值魔力值',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ],
            ),
            const SizedBox(height: 8),
            const Text(
              '魔力值保证金',
              style: TextStyle(color: Colors.grey, fontSize: 14),
            ),
            const SizedBox(height: 16),

            // 固定金额显示
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(vertical: 24),
              decoration: BoxDecoration(
                color: Colors.grey[50],
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.grey[200]!),
              ),
              child: Column(
                children: [
                  const Text(
                    '¥$_depositAmount',
                    style: TextStyle(
                      fontSize: 36,
                      fontWeight: FontWeight.bold,
                      color: Color(0xfff4835a),
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '保证金金额',
                    style: TextStyle(fontSize: 14, color: Colors.grey[600]),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 24),

            // PC 端充值引导
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(
                color: Colors.orange[50],
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.orange[200]!),
              ),
              child: Column(
                children: [
                  const Icon(
                    Icons.computer,
                    size: 40,
                    color: Color(0xfff4835a),
                  ),
                  const SizedBox(height: 12),
                  const Text(
                    '请前往 PC 端充值',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    '网站地址：www.molitao.top',
                    style: TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '保证金金额：¥$_depositAmount',
                    style: const TextStyle(fontSize: 14, color: Colors.grey),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),

            // 关闭按钮
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: () => Navigator.of(context).pop(),
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xfff4835a),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                child: const Text('我知道了', style: TextStyle(fontSize: 16)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

/// 提现底部弹窗
class _WithdrawalBottomSheet extends StatefulWidget {
  final VoidCallback? onWithdrawSuccess;

  const _WithdrawalBottomSheet({this.onWithdrawSuccess});

  @override
  State<_WithdrawalBottomSheet> createState() => _WithdrawalBottomSheetState();
}

class _WithdrawalBottomSheetState extends State<_WithdrawalBottomSheet> {
  final PaymentRepository _paymentRepository = PaymentRepository();
  final TextEditingController _amountController = TextEditingController();
  bool _isLoading = false;

  @override
  void dispose() {
    _amountController.dispose();
    super.dispose();
  }

  Future<void> _handleWithdraw() async {
    final amountText = _amountController.text.trim();
    if (amountText.isEmpty) {
      _showMessage('请输入提现金额');
      return;
    }

    final amount = double.tryParse(amountText);
    if (amount == null || amount <= 0) {
      _showMessage('请输入有效金额');
      return;
    }

    setState(() => _isLoading = true);

    try {
      // TODO: 提现功能需要商户配置完成后才能使用
      _showMessage('提现功能正在配置中，即将上线！');
      Navigator.of(context).pop();
    } catch (e) {
      _showMessage('提现失败: ${e.toString()}');
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), duration: const Duration(seconds: 2)),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: EdgeInsets.only(
        bottom: MediaQuery.of(context).viewInsets.bottom,
      ),
      child: Container(
        padding: const EdgeInsets.all(16),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // 标题栏
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  '提现',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                IconButton(
                  icon: const Icon(Icons.close),
                  onPressed: () => Navigator.of(context).pop(),
                ),
              ],
            ),
            const SizedBox(height: 8),
            const Text(
              '平台提现功能尚未完善，魔力值退还请联系管理员',
              style: TextStyle(color: Colors.grey, fontSize: 12),
            ),
            const SizedBox(height: 16),

            // 联系方式
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.orange[50],
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: Colors.orange[100]!),
              ),
              child: const Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '联系管理员老淡',
                    style: TextStyle(fontWeight: FontWeight.bold),
                  ),
                  SizedBox(height: 4),
                  Text('QQ：383875411'),
                  Text('微信：18845639111'),
                ],
              ),
            ),
            const SizedBox(height: 16),

            // 金额输入（预留）
            TextField(
              controller: _amountController,
              keyboardType: const TextInputType.numberWithOptions(
                decimal: true,
              ),
              enabled: false, //暂时禁用，等待功能上线
              decoration: InputDecoration(
                labelText: '提现金额',
                hintText: '功能上线后可输入',
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                suffixText: '元',
                filled: true,
                fillColor: Colors.grey[100],
              ),
            ),
            const SizedBox(height: 24),

            // 提示信息
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: Colors.blue[50],
                borderRadius: BorderRadius.circular(8),
              ),
              child: const Row(
                children: [
                  Icon(Icons.info_outline, color: Colors.blue, size: 20),
                  SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      '提现申请提交后，需管理员审核处理',
                      style: TextStyle(color: Colors.blue, fontSize: 12),
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 16),

            // 提交按钮
            SizedBox(
              width: double.infinity,
              child: ElevatedButton(
                onPressed: _isLoading ? null : _handleWithdraw,
                style: ElevatedButton.styleFrom(
                  backgroundColor: Colors.grey[400], // 灰色表示暂时不可用
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                child: const Text('功能即将上线', style: TextStyle(fontSize: 16)),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
