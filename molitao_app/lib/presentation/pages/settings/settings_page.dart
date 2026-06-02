import "package:flutter/material.dart";
import "package:flutter_riverpod/flutter_riverpod.dart";
import "package:go_router/go_router.dart";
import "package:shared_preferences/shared_preferences.dart";
import "package:package_info_plus/package_info_plus.dart";
import "package:path_provider/path_provider.dart";
import "dart:io";
import 'package:molitao_app/presentation/providers/user_provider.dart';
import 'package:molitao_app/data/services/notification_permission_service.dart';
import "../../../core/theme/app_colors.dart";

class SettingsPage extends ConsumerStatefulWidget {
  const SettingsPage({super.key});

  @override
  ConsumerState<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends ConsumerState<SettingsPage>
    with WidgetsBindingObserver {
  bool _pushNotificationEnabled = true;
  NotificationPermissionState _systemPermissionState =
      NotificationPermissionState.unknown;
  String _cacheSize = "计算中...";
  String _appVersion = "";

  final _permissionService = NotificationPermissionService();

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addObserver(this);
    _loadSettings();
    _checkSystemPermission();
    _calculateCacheSize();
    _loadAppVersion();
  }

  @override
  void dispose() {
    WidgetsBinding.instance.removeObserver(this);
    super.dispose();
  }

  @override
  void didChangeAppLifecycleState(final AppLifecycleState state) {
    // 用户从系统设置返回时，重新检查权限状态
    if (state == AppLifecycleState.resumed) {
      _checkSystemPermission();
    }
  }

  /// 检查系统通知权限状态
  Future<void> _checkSystemPermission() async {
    try {
      var state = await _permissionService.getPermissionState();
      if (mounted) {
        setState(() {
          _systemPermissionState = state;
          // 如果系统权限关闭，同步关闭 App 内开关
          if (state != NotificationPermissionState.granted) {
            _pushNotificationEnabled = false;
          }
        });
      }
    } catch (e) {
      debugPrint("[Settings] 检查权限失败: $e");
    }
  }

  Future<void> _loadAppVersion() async {
    try {
      var packageInfo = await PackageInfo.fromPlatform();
      if (mounted) {
        setState(() {
          _appVersion = "v${packageInfo.version}";
        });
      }
    } catch (e) {
      debugPrint("Failed to get package info: $e");
    }
  }

  Future<void> _loadSettings() async {
    var prefs = await SharedPreferences.getInstance();
    setState(() {
      _pushNotificationEnabled =
          prefs.getBool("push_notification_enabled") ?? true;
    });
  }

  Future<void> _savePushNotificationSetting(final bool value) async {
    var prefs = await SharedPreferences.getInstance();
    await prefs.setBool("push_notification_enabled", value);
  }

  /// 处理推送通知开关变化
  Future<void> _handlePushNotificationChange(final bool value) async {
    if (value) {
      // 用户想开启推送
      if (_systemPermissionState != NotificationPermissionState.granted) {
        // 系统权限未开启，引导用户
        var granted =
            await _permissionService.checkAndRequestPermission(context);
        if (granted) {
          // 用户开启权限，返回后刷新状态
          await _checkSystemPermission();
          if (mounted) {
            setState(() {
              _pushNotificationEnabled = true;
            });
            await _savePushNotificationSetting(true);
          }
        }
        return;
      }
    }

    // 更新开关状态
    setState(() {
      _pushNotificationEnabled = value;
    });
    await _savePushNotificationSetting(value);
  }

  Future<void> _calculateCacheSize() async {
    try {
      var cacheDir = await getTemporaryDirectory();
      var size = await _getDirSize(cacheDir);
      setState(() {
        _cacheSize = _formatBytes(size);
      });
    } catch (e) {
      setState(() {
        _cacheSize = "未知";
      });
    }
  }

  Future<int> _getDirSize(final Directory dir) async {
    var size = 0;
    try {
      if (await dir.exists()) {
        await for (var entity in dir.list(
          recursive: true,
          followLinks: false,
        )) {
          if (entity is File) {
            size += await entity.length();
          }
        }
      }
    } catch (e) {
      debugPrint("Error calculating cache size: $e");
    }
    return size;
  }

  String _formatBytes(final int bytes) {
    if (bytes < 1024) return "$bytes B";
    if (bytes < 1024 * 1024) return "${(bytes / 1024).toStringAsFixed(1)} KB";
    if (bytes < 1024 * 1024 * 1024) {
      return "${(bytes / (1024 * 1024)).toStringAsFixed(1)} MB";
    }
    return "${(bytes / (1024 * 1024 * 1024)).toStringAsFixed(1)} GB";
  }

  @override
  Widget build(final BuildContext context) {
    var userState = ref.watch(userProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text(
          "设置",
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
      ),
      body: Container(
        color: const Color(0xfff6f6f6),
        padding: const EdgeInsets.all(16),
        child: SingleChildScrollView(
          physics: const AlwaysScrollableScrollPhysics(),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildSectionCard(
                title: "账户管理",
                children: [
                  _buildListTile(
                    icon: Icons.person_outline,
                    title: "个人信息",
                    onTap: () => context.push("/edit-profile"),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.security_outlined,
                    title: "账号安全",
                    onTap: () => context.push("/account-security"),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.lock_outline,
                    title: "修改密码",
                    onTap: () => _showChangePasswordDialog(context),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.delete_forever_outlined,
                    title: "注销账号",
                    onTap: () => context.push("/delete-account"),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.block,
                    title: "黑名单管理",
                    onTap: () => context.push("/block-list"),
                    showArrow: true,
                  ),
                ],
              ),
              const SizedBox(height: 16),
              _buildSectionCard(
                title: "消息通知",
                children: [
                  _buildNotificationSwitchTile(),
                ],
              ),
              const SizedBox(height: 16),
              _buildSectionCard(
                title: "通用设置",
                children: [
                  _buildListTile(
                    icon: Icons.support_agent,
                    title: "联系客服",
                    subtitle: "电话: 400-XXX-XXXX  微信号: molitao_service",
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.cleaning_services_outlined,
                    title: "清除缓存",
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
                    title: "关于我们",
                    onTap: () => context.push("/about"),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.description_outlined,
                    title: "用户协议",
                    onTap: () => context.push("/agreement?type=user-agreement"),
                    showArrow: true,
                  ),
                  _buildDivider(),
                  _buildListTile(
                    icon: Icons.privacy_tip_outlined,
                    title: "隐私政策",
                    onTap: () => context.push("/agreement?type=privacy-policy"),
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
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: const Text("退出登录", style: TextStyle(fontSize: 16)),
                    ),
                  ),
                ),
              const SizedBox(height: 16),
              Align(
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
    required final String title,
    required final List<Widget> children,
  }) => Column(
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

  Widget _buildListTile({
    required final IconData icon,
    required final String title,
    final Widget? trailing,
    final VoidCallback? onTap,
    final bool showArrow = false,
    final String? subtitle,
  }) => InkWell(
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
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
                  ),
                  if (subtitle != null) ...[
                    const SizedBox(height: 2),
                    Text(
                      subtitle,
                      style: const TextStyle(fontSize: 12, color: Color(0xff999999)),
                    ),
                  ],
                ],
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

  Widget _buildSwitchTile({
    required final IconData icon,
    required final String title,
    required final bool value,
    required final ValueChanged<bool> onChanged,
    final String? subtitle,
    final bool showWarning = false,
  }) => Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: showWarning
                  ? Colors.orange.withValues(alpha: 0.1)
                  : const Color(0xfff6f6f6),
              borderRadius: BorderRadius.circular(8.0),
            ),
            child: Icon(
              icon,
              size: 24,
              color: showWarning ? Colors.orange : const Color(0xff1a1a1a),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style:
                      const TextStyle(fontSize: 16, color: Color(0xff1a1a1a)),
                ),
                if (subtitle != null) ...[
                  const SizedBox(height: 2),
                  Text(
                    subtitle,
                    style: TextStyle(
                      fontSize: 12,
                      color:
                          showWarning ? Colors.orange : const Color(0xff999999),
                    ),
                  ),
                ],
              ],
            ),
          ),
          Switch(
            value: value,
            onChanged: onChanged,
            activeThumbColor: AppColors.primary,
            activeTrackColor: AppColors.primary.withValues(alpha: 0.5),
          ),
        ],
      ),
    );

  /// 构建通知开关组件（带权限状态检查）
  Widget _buildNotificationSwitchTile() {
    var isGranted =
        _systemPermissionState == NotificationPermissionState.granted;
    var showWarning = !isGranted;

    String? subtitle;
    if (_systemPermissionState ==
        NotificationPermissionState.permanentlyDenied) {
      subtitle = "通知权限已被禁用，点击去设置开启";
    } else if (_systemPermissionState == NotificationPermissionState.denied) {
      subtitle = "系统通知权限未开启，点击去开启";
    } else if (_systemPermissionState == NotificationPermissionState.unknown) {
      subtitle = "正在检查权限状态...";
    }

    return _buildSwitchTile(
      icon: Icons.notifications_outlined,
      title: "推送通知",
      subtitle: subtitle,
      showWarning: showWarning,
      value: _pushNotificationEnabled && isGranted,
      onChanged: _handlePushNotificationChange,
    );
  }

  Widget _buildDivider() => const Divider(
      height: 1,
      indent: 68,
      endIndent: 16,
      color: Color(0xffeeeeee),
    );

  void _showChangePasswordDialog(final BuildContext context) {
    var oldPasswordController = TextEditingController();
    var newPasswordController = TextEditingController();
    var confirmPasswordController = TextEditingController();

    showDialog(
      context: context,
      builder: (final BuildContext context) => AlertDialog(
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
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                if (newPasswordController.text.isEmpty ||
                    oldPasswordController.text.isEmpty) {
                  ScaffoldMessenger.of(
                    context,
                  ).showSnackBar(const SnackBar(content: Text('请填写完整信息')));
                  return;
                }
                if (newPasswordController.text.length < 6) {
                  ScaffoldMessenger.of(
                    context,
                  ).showSnackBar(const SnackBar(content: Text('密码长度不能少于6位')));
                  return;
                }
                if (newPasswordController.text !=
                    confirmPasswordController.text) {
                  ScaffoldMessenger.of(
                    context,
                  ).showSnackBar(const SnackBar(content: Text('两次输入的密码不一致')));
                  return;
                }
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
        ),
    ).then((final _) {
      oldPasswordController.dispose();
      newPasswordController.dispose();
      confirmPasswordController.dispose();
    });
  }

  Future<void> _clearCache() async {
    try {
      var cacheDir = await getTemporaryDirectory();
      if (await cacheDir.exists()) {
        await for (var entity in cacheDir.list(
          recursive: true,
          followLinks: false,
        )) {
          if (entity is File) {
            try {
              await entity.delete();
            } catch (e) {
              debugPrint("Failed to delete file: $e");
            }
          }
        }
      }
      await _calculateCacheSize();
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text("缓存已清除")));
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(SnackBar(content: Text("清除缓存失败: $e")));
      }
    }
  }

  void _showClearCacheDialog(final BuildContext context) {
    showDialog(
      context: context,
      builder: (final BuildContext context) => AlertDialog(
          title: const Text('清除缓存'),
          content: Text('确定要清除缓存吗？\n当前缓存: $_cacheSize'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () {
                Navigator.of(context).pop();
                _clearCache();
              },
              child: const Text(
                '确定',
                style: TextStyle(color: Color(0xfff4835a)),
              ),
            ),
          ],
        ),
    );
  }

  void _showLogoutDialog(final BuildContext context, final WidgetRef ref) {
    showDialog(
      context: context,
      builder: (final BuildContext context) => AlertDialog(
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
        ),
    );
  }
}
