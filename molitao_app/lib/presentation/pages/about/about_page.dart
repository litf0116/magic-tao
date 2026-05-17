import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:package_info_plus/package_info_plus.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../../data/api/api_client.dart';
import '../../../data/api/api_endpoints.dart';

class AboutPage extends ConsumerStatefulWidget {
  const AboutPage({super.key});

  @override
  ConsumerState<AboutPage> createState() => _AboutPageState();
}

class _AboutPageState extends ConsumerState<AboutPage> {
  String _appVersion = '';
  String _buildNumber = '';
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
        queryParameters: {
          'platform': Platform.isIOS ? 'ios' : 'android',
          'currentVersionCode': 0,
          'versionName': _appVersion,
        },
      );

      if (response.data != null && response.data['success'] == true) {
        final result = response.data['result'];
        if (result != null && result['hasUpdate'] == true) {
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
              Text('v${_updateInfo?['latestVersionName'] ?? _appVersion}'),
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
                  _updateInfo?['description'] ?? '修复已知问题，优化用户体验',
                  style: const TextStyle(
                    fontSize: 14,
                    color: Color(0xff666666),
                  ),
                ),
              ),
              if (_updateInfo?['isForceUpdate'] == true)
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
            if (_updateInfo?['isForceUpdate'] != true)
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

  Future<void> _downloadUpdate() async {
    final downloadUrl = _updateInfo?['downloadUrl'];
    if (downloadUrl != null && downloadUrl.toString().isNotEmpty) {
      try {
        final uri = Uri.parse(downloadUrl.toString());
        if (await canLaunchUrl(uri)) {
          await launchUrl(uri, mode: LaunchMode.externalApplication);
        } else {
          if (mounted) {
            ScaffoldMessenger.of(context).showSnackBar(
              const SnackBar(
                content: Text('无法打开下载链接'),
                backgroundColor: Color(0xffF44336),
              ),
            );
          }
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('下载失败: $e'),
              backgroundColor: const Color(0xffF44336),
            ),
          );
        }
      }
    } else {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('暂无下载地址，请稍后重试'),
            backgroundColor: Color(0xffFF9800),
          ),
        );
      }
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
}
