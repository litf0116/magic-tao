import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../providers/user_provider.dart';

class SettingsPage extends ConsumerStatefulWidget {
  const SettingsPage({super.key});

  @override
  ConsumerState<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends ConsumerState<SettingsPage> {
  bool _pushNotificationEnabled = true;
  String _cacheSize = '12.5MB';
  final String _appVersion = 'v1.0.0';

  @override
  Widget build(BuildContext context) {
    final userState = ref.watch(userProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          '设置',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: Container(
        color: const Color(0xfff6f6f6),
        padding: const EdgeInsets.all(16.0),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildSectionCard(
                title: '账户管理',
                children: [
                  _buildListTile(
                    icon: Icons.person_outline,
                    title: '个人信息',
                    onTap: () => context.push('/profile/user-info'),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.lock_outline,
                    title: '修改密码',
                    onTap: () => _showChangePasswordDialog(context),
                    showArrow: true,
                  ),
                ],
              ),
              const SizedBox(height: 16),
              _buildSectionCard(
                title: '消息通知',
                children: [
                  _buildSwitchTile(
                    icon: Icons.notifications_outlined,
                    title: '推送通知',
                    value: _pushNotificationEnabled,
                    onChanged: (value) {
                      setState(() {
                        _pushNotificationEnabled = value;
                      });
                    },
                  ),
                ],
              ),
              const SizedBox(height: 16),
              _buildSectionCard(
                title: '通用设置',
                children: [
                  _buildListTile(
                    icon: Icons.cleaning_services_outlined,
                    title: '清除缓存',
                    trailing: Text(
                      _cacheSize,
                      style: const TextStyle(
                        fontSize: 14,
                        color: Color(0xff999999),
                      ),
                    ),
                    onTap: () => _showClearCacheDialog(context),
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.info_outline,
                    title: '关于我们',
                    onTap: () => context.push('/profile/about'),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.description_outlined,
                    title: '用户协议',
                    onTap: () => _showUserAgreement(context),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.privacy_tip_outlined,
                    title: '隐私政策',
                    onTap: () => _showPrivacyPolicy(context),
                    showArrow: true,
                  ),
                ],
              ),
              const SizedBox(height: 24),
              if (userState.isLoggedIn)
                Center(
                  child: SizedBox(
                    width: double.infinity,
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
                      child: const Text('退出登录', style: TextStyle(fontSize: 16)),
                    ),
                  ),
                ),
              const SizedBox(height: 16),
              Align(
                alignment: Alignment.center,
                child: Text(
                  _appVersion,
                  style: const TextStyle(color: Color(0xff999999)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSectionCard({
    required String title,
    required List<Widget> children,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          title,
          style: const TextStyle(
            fontSize: 16,
            fontWeight: FontWeight.bold,
            color: Color(0xff1a1a1a),
          ),
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
          child: Column(children: children),
        ),
      ],
    );
  }

  Widget _buildListTile({
    required IconData icon,
    required String title,
    Widget? trailing,
    VoidCallback? onTap,
    bool showArrow = false,
  }) {
    return InkWell(
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
        child: Row(
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: const Color(0xfff6f6f6),
                borderRadius: BorderRadius.circular(8.0),
              ),
              child: Icon(icon, size: 24, color: const Color(0xff1a1a1a)),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                title,
                style: const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
              ),
            ),
            if (trailing != null) trailing,
            if (showArrow)
              const Icon(
                Icons.chevron_right,
                color: Color(0xff999999),
                size: 20,
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildSwitchTile({
    required IconData icon,
    required String title,
    required bool value,
    required ValueChanged<bool> onChanged,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: const Color(0xfff6f6f6),
              borderRadius: BorderRadius.circular(8.0),
            ),
            child: Icon(icon, size: 24, color: const Color(0xff1a1a1a)),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              title,
              style: const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
            ),
          ),
          Switch(
            value: value,
            onChanged: onChanged,
            activeThumbColor: const Color(0xfff4835a),
          ),
        ],
      ),
    );
  }

  Widget _buildDivider() {
    return const Divider(
      height: 1,
      indent: 68,
      endIndent: 16,
      color: Color(0xffeeeeee),
    );
  }

  void _showChangePasswordDialog(BuildContext context) {
    final oldPasswordController = TextEditingController();
    final newPasswordController = TextEditingController();
    final confirmPasswordController = TextEditingController();

    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('修改密码'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: oldPasswordController,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: '旧密码',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8.0),
                  ),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: newPasswordController,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: '新密码',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8.0),
                  ),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: confirmPasswordController,
                obscureText: true,
                decoration: InputDecoration(
                  labelText: '确认新密码',
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(8.0),
                  ),
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () {
                oldPasswordController.dispose();
                newPasswordController.dispose();
                confirmPasswordController.dispose();
                Navigator.of(context).pop();
              },
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                if (newPasswordController.text !=
                    confirmPasswordController.text) {
                  ScaffoldMessenger.of(
                    context,
                  ).showSnackBar(const SnackBar(content: Text('两次输入的密码不一致')));
                  return;
                }
                oldPasswordController.dispose();
                newPasswordController.dispose();
                confirmPasswordController.dispose();
                Navigator.of(context).pop();
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(const SnackBar(content: Text('密码修改成功')));
              },
              child: const Text(
                '确定',
                style: TextStyle(color: Color(0xfff4835a)),
              ),
            ),
          ],
        );
      },
    );
  }

  void _showClearCacheDialog(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('清除缓存'),
          content: const Text('确定要清除缓存吗？'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                setState(() {
                  _cacheSize = '0.0MB';
                });
                ScaffoldMessenger.of(
                  context,
                ).showSnackBar(const SnackBar(content: Text('缓存已清除')));
              },
              child: const Text(
                '确定',
                style: TextStyle(color: Color(0xfff4835a)),
              ),
            ),
          ],
        );
      },
    );
  }

  void _showUserAgreement(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('用户协议'),
          content: const SingleChildScrollView(
            child: Text(
              '欢迎使用魔力淘！\n\n'
              '在使用本应用前，请仔细阅读以下条款：\n\n'
              '1. 服务说明\n'
              '魔力淘是一个在线拍卖交易平台，为用户提供游戏虚拟物品的拍卖服务。\n\n'
              '2. 用户责任\n'
              '用户应遵守相关法律法规，不得利用平台进行违法违规活动。\n\n'
              '3. 交易规则\n'
              '所有交易均需遵守平台规则，确保交易公平、公正、公开。\n\n'
              '4. 隐私保护\n'
              '我们重视用户隐私，具体见隐私政策。',
              style: TextStyle(fontSize: 14),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text(
                '确定',
                style: TextStyle(color: Color(0xfff4835a)),
              ),
            ),
          ],
        );
      },
    );
  }

  void _showPrivacyPolicy(BuildContext context) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: const Text('隐私政策'),
          content: const SingleChildScrollView(
            child: Text(
              '魔力淘隐私政策\n\n'
              '生效日期：2024年1月1日\n\n'
              '我们重视并保护您的隐私，本政策说明我们如何收集、使用和保护您的个人信息：\n\n'
              '1. 信息收集\n'
              '我们收集您注册时提供的基本信息（用户名、手机号等）以及交易过程中产生的数据。\n\n'
              '2. 信息使用\n'
              '您的信息仅用于提供服务、改进用户体验和保障交易安全。\n\n'
              '3. 信息保护\n'
              '我们采用多种安全措施保护您的个人信息，防止未经授权的访问、使用或泄露。\n\n'
              '4. 信息共享\n'
              '除法律法规要求或经您同意外，我们不会向第三方共享您的个人信息。',
              style: TextStyle(fontSize: 14),
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text(
                '确定',
                style: TextStyle(color: Color(0xfff4835a)),
              ),
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
                ref.read(userProvider.notifier).logout();
              },
              child: const Text('确定', style: TextStyle(color: Colors.red)),
            ),
          ],
        );
      },
    );
  }
}
