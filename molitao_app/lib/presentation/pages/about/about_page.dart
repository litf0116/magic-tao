import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:package_info_plus/package_info_plus.dart';
import '../../../data/api/api_client.dart';
import '../../../data/api/api_endpoints.dart';

class AboutPage extends ConsumerStatefulWidget {
  const AboutPage({super.key});

  @override
  ConsumerState<AboutPage> createState() => _AboutPageState();
}

class _AboutPageState extends ConsumerState<AboutPage> {
  String _appVersion = '1.3.0';
  String _buildNumber = '1';
  bool _isCheckingUpdate = false;
  Map<String, dynamic>? _updateInfo;

  @override
  void initState() {
    super.initState();
    _loadAppInfo();
  }

  Future<void> _loadAppInfo() async {
    try {
      final packageInfo = await PackageInfo.fromPlatform();
      if (mounted) {
        setState(() {
          _appVersion = packageInfo.version;
          _buildNumber = packageInfo.buildNumber;
        });
      }
    } catch (e) {
      debugPrint('Failed to get package info: $e');
    }
  }

  Future<void> _checkUpdate() async {
    if (_isCheckingUpdate) return;

    setState(() {
      _isCheckingUpdate = true;
    });

    try {
      final response = await ApiClient().dio.get(
        ApiEndpoints.checkUpdate,
        queryParameters: {'platform': 'android', 'version': _appVersion},
      );

      if (response.data != null && response.data['success'] == true) {
        final result = response.data['result'];
        if (result != null && result['needUpdate'] == true) {
          if (mounted) {
            setState(() {
              _updateInfo = result;
            });
            _showUpdateDialog();
          }
        } else {
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('已是最新版本'),
                backgroundColor: Color(0xff4CAF50),
              ),
            );
          }
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('检查更新失败: $e'),
            backgroundColor: const Color(0xffF44336),
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isCheckingUpdate = false;
        });
      }
    }
  }

  void _showUpdateDialog() {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                decoration: BoxDecoration(
                  color: const Color(0xfff4835a),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: const Text(
                  '新版本',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
              const SizedBox(width: 8),
              Text('v${_updateInfo?['latestVersion'] ?? _appVersion}'),
            ],
          ),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const Text('发现新版本，是否立即更新？', style: TextStyle(fontSize: 16)),
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: const Color(0xffFAF1F0),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  _updateInfo?['updateLog'] ?? '修复已知问题，优化用户体验',
                  style: const TextStyle(
                    fontSize: 14,
                    color: Color(0xff666666),
                  ),
                ),
              ),
              if (_updateInfo?['forceUpdate'] == true)
                Padding(
                  padding: const EdgeInsets.only(top: 12),
                  child: Row(
                    children: [
                      Icon(
                        Icons.info_outline,
                        size: 16,
                        color: Colors.orange[700],
                      ),
                      const SizedBox(width: 4),
                      Text(
                        '此版本为强制更新',
                        style: TextStyle(
                          fontSize: 12,
                          color: Colors.orange[700],
                        ),
                      ),
                    ],
                  ),
                ),
            ],
          ),
          actions: [
            if (_updateInfo?['forceUpdate'] != true)
              TextButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text(
                  '稍后更新',
                  style: TextStyle(color: Color(0xff999999)),
                ),
              ),
            ElevatedButton(
              onPressed: () {
                Navigator.of(context).pop();
                _downloadUpdate();
              },
              style: ElevatedButton.styleFrom(
                backgroundColor: const Color(0xfff4835a),
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
              child: const Text('立即更新'),
            ),
          ],
        );
      },
    );
  }

  void _downloadUpdate() {
    final downloadUrl = _updateInfo?['downloadUrl'];
    if (downloadUrl != null && downloadUrl.toString().isNotEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('正在跳转到下载页面...'),
          backgroundColor: Color(0xfff4835a),
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('暂无下载地址，请稍后重试'),
          backgroundColor: Color(0xffFF9800),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          '关于我们',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: Container(
        color: const Color(0xfff6f6f6),
        child: SingleChildScrollView(
          child: Column(
            children: [
              const SizedBox(height: 40),
              _buildAppInfo(),
              const SizedBox(height: 16),
              _buildMenuItems(),
              const SizedBox(height: 32),
              _buildFooter(),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildAppInfo() {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 32),
      child: Column(
        children: [
          Container(
            width: 88,
            height: 88,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(16),
              boxShadow: [
                BoxShadow(
                  color: const Color(0xfff4835a).withValues(alpha: 0.3),
                  spreadRadius: 2,
                  blurRadius: 12,
                  offset: const Offset(0, 4),
                ),
              ],
            ),
            child: ClipRRect(
              borderRadius: BorderRadius.circular(16),
              child: Image.asset(
                'assets/images/app-icon.png',
                fit: BoxFit.cover,
                errorBuilder: (context, error, stackTrace) {
                  return Container(
                    color: const Color(0xfff4835a),
                    child: const Icon(
                      Icons.store,
                      size: 48,
                      color: Colors.white,
                    ),
                  );
                },
              ),
            ),
          ),
          const SizedBox(height: 16),
          const Text(
            '魔力淘',
            style: TextStyle(
              fontSize: 24,
              fontWeight: FontWeight.bold,
              color: Color(0xff1a1a1a),
            ),
          ),
          const SizedBox(height: 8),
          Text(
            'v$_appVersion ($_buildNumber)',
            style: const TextStyle(fontSize: 14, color: Color(0xff999999)),
          ),
        ],
      ),
    );
  }

  Widget _buildMenuItems() {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.grey.withValues(alpha: 0.1),
            spreadRadius: 1,
            blurRadius: 5,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: [
          _buildMenuItem(
            icon: Icons.system_update_outlined,
            title: '检查更新',
            trailing: _isCheckingUpdate
                ? const SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Color(0xfff4835a),
                    ),
                  )
                : const Icon(
                    Icons.chevron_right,
                    color: Color(0xff999999),
                    size: 20,
                  ),
            onTap: _isCheckingUpdate ? null : _checkUpdate,
          ),
          _buildDivider(),
          _buildMenuItem(
            icon: Icons.description_outlined,
            title: '用户协议',
            onTap: () => _showAgreementDialog('用户协议', _userAgreement),
          ),
          _buildDivider(),
          _buildMenuItem(
            icon: Icons.privacy_tip_outlined,
            title: '隐私政策',
            onTap: () => _showAgreementDialog('隐私政策', _privacyPolicy),
          ),
          _buildDivider(),
          _buildMenuItem(
            icon: Icons.star_outline,
            title: '给我们评分',
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('感谢您的支持！'),
                  backgroundColor: Color(0xfff4835a),
                ),
              );
            },
          ),
          _buildDivider(),
          _buildMenuItem(
            icon: Icons.feedback_outlined,
            title: '意见反馈',
            onTap: () {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(
                  content: Text('功能开发中，敬请期待'),
                  backgroundColor: Color(0xffFF9800),
                ),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _buildMenuItem({
    required IconData icon,
    required String title,
    Widget? trailing,
    VoidCallback? onTap,
  }) {
    return InkWell(
      onTap: onTap,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        child: Row(
          children: [
            Container(
              width: 40,
              height: 40,
              decoration: BoxDecoration(
                color: const Color(0xffFAF1F0),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Icon(icon, size: 22, color: const Color(0xfff4835a)),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                title,
                style: const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
              ),
            ),
            if (trailing != null) trailing,
          ],
        ),
      ),
    );
  }

  Widget _buildDivider() {
    return const Divider(
      height: 1,
      indent: 68,
      endIndent: 16,
      color: Color(0xffEEEEEE),
    );
  }

  Widget _buildFooter() {
    return Column(
      children: [
        const Text(
          '专注游戏虚拟物品的实时秒杀拍卖',
          style: TextStyle(fontSize: 14, color: Color(0xff999999)),
        ),
        const SizedBox(height: 8),
        const Text(
          '© 2024-2026 魔力淘 All Rights Reserved',
          style: TextStyle(fontSize: 12, color: Color(0xffCCCCCC)),
        ),
        const SizedBox(height: 16),
      ],
    );
  }

  void _showAgreementDialog(String title, String content) {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text(title),
          content: SingleChildScrollView(
            child: Text(
              content,
              style: const TextStyle(fontSize: 14, height: 1.6),
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

  static const String _userAgreement = '''
欢迎使用魔力淘！

在使用本应用前，请仔细阅读以下条款：

1. 服务说明
魔力淘是一个在线拍卖交易平台，为用户提供游戏虚拟物品的拍卖服务。我们致力于为用户提供安全、便捷、高效的交易体验。

2. 用户责任
用户应遵守相关法律法规，不得利用平台进行违法违规活动。用户需对自己的账号安全负责，不得将账号借给他人使用。

3. 交易规则
所有交易均需遵守平台规则，确保交易公平、公正、公开。拍卖成交后，买卖双方应按时完成交易。

4. 费用说明
平台可能收取一定比例的服务费用，具体费用标准以平台公示为准。

5. 隐私保护
我们重视用户隐私，具体见隐私政策。未经用户同意，我们不会向第三方披露用户个人信息。

6. 免责声明
对于因不可抗力或第三方原因导致的服务中断或损失，平台不承担责任。

7. 协议修改
我们有权根据需要修改本协议，修改后的协议将在平台公布。
''';

  static const String _privacyPolicy = '''
魔力淘隐私政策

生效日期：2024年1月1日

我们重视并保护您的隐私，本政策说明我们如何收集、使用和保护您的个人信息：

1. 信息收集
我们收集您注册时提供的基本信息（用户名、手机号等）以及交易过程中产生的数据。这些信息包括但不限于：
- 账号信息：用户名、手机号、微信OpenID
- 交易信息：拍卖记录、出价记录、成交记录
- 设备信息：设备型号、操作系统、唯一设备标识

2. 信息使用
您的信息仅用于提供服务、改进用户体验和保障交易安全：
- 提供拍卖交易服务
- 发送交易通知和系统消息
- 改进产品功能和服务质量
- 保障账户和交易安全

3. 信息保护
我们采用多种安全措施保护您的个人信息：
- 数据加密传输和存储
- 严格的访问权限控制
- 定期安全审计和漏洞修复

4. 信息共享
除以下情况外，我们不会向第三方共享您的个人信息：
- 获得您的明确同意
- 法律法规要求
- 保护平台和用户的合法权益

5. 您的权利
您有权查询、更正、删除您的个人信息，有权注销账号。

6. 联系我们
如有任何问题，请通过以下方式联系我们：
- 邮箱：support@molitao.top
''';
}
