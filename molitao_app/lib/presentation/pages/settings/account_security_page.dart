import 'dart:async';
import 'package:flutter/material.dart';
import '../../../core/theme/app_colors.dart';
import '../../../data/models/login_binding_model.dart';
import '../../../data/services/login_binding_service.dart';
import '../../../data/repositories/auth_repository.dart';

class AccountSecurityPage extends StatefulWidget {
  const AccountSecurityPage({super.key});

  @override
  State<AccountSecurityPage> createState() => _AccountSecurityPageState();
}

class _AccountSecurityPageState extends State<AccountSecurityPage> {
  final _loginBindingService = LoginBindingService();
  final _authRepository = AuthRepository();
  List<LoginBindingDto> _bindings = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadBindings();
  }

  Future<void> _loadBindings() async {
    try {
      final bindings = await _loginBindingService.getLoginBindings();
      if (mounted) {
        setState(() {
          _bindings = bindings;
          _isLoading = false;
        });
      }
    } catch (e) {
      if (mounted) {
        setState(() => _isLoading = false);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('获取绑定信息失败: $e')),
        );
      }
    }
  }

  bool _hasBinding(LoginBindingType type) {
    return _bindings.any((b) => b.bindingType == type);
  }

  LoginBindingDto? _getBinding(LoginBindingType type) {
    try {
      return _bindings.firstWhere((b) => b.bindingType == type);
    } catch (e) {
      return null;
    }
  }

  Future<void> _showBindPhoneDialog() async {
    final phoneController = TextEditingController();
    final codeController = TextEditingController();
    int countdown = 0;
    Timer? timer;

    await showDialog(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('绑定手机号'),
              content: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  TextField(
                    controller: phoneController,
                    keyboardType: TextInputType.phone,
                    decoration: InputDecoration(
                      labelText: '手机号',
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                    ),
                  ),
                  const SizedBox(height: 12),
                  Row(
                    children: [
                      Expanded(
                        child: TextField(
                          controller: codeController,
                          keyboardType: TextInputType.number,
                          decoration: InputDecoration(
                            labelText: '验证码',
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                        ),
                      ),
                      const SizedBox(width: 12),
                      SizedBox(
                        width: 100,
                        child: TextButton(
                          onPressed: countdown > 0
                              ? null
                              : () async {
                                  final phone = phoneController.text.trim();
                                  if (phone.isEmpty ||
                                      !RegExp(r'^1[3-9]\d{9}$')
                                          .hasMatch(phone)) {
                                    ScaffoldMessenger.of(context).showSnackBar(
                                      const SnackBar(
                                          content: Text('请输入正确的手机号')),
                                    );
                                    return;
                                  }
                                  try {
                                    await _authRepository.sendSmsCode(
                                      phone,
                                      purpose: 'bindphone',
                                    );
                                    setDialogState(() => countdown = 60);
                                    timer = Timer.periodic(
                                        const Duration(seconds: 1), (t) {
                                      if (countdown > 0) {
                                        setDialogState(() => countdown--);
                                      } else {
                                        t.cancel();
                                      }
                                    });
                                    if (context.mounted) {
                                      ScaffoldMessenger.of(context).showSnackBar(
                                        const SnackBar(content: Text('验证码已发送')),
                                      );
                                    }
                                  } catch (e) {
                                    if (context.mounted) {
                                      ScaffoldMessenger.of(context).showSnackBar(
                                        SnackBar(
                                            content: Text('发送失败: $e')),
                                      );
                                    }
                                  }
                                },
                          style: TextButton.styleFrom(
                            backgroundColor: AppColors.primary.withValues(alpha: 0.1),
                            shape: RoundedRectangleBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          child: Text(
                            countdown > 0 ? '${countdown}s' : '获取验证码',
                            style: TextStyle(
                              fontSize: 13,
                              color: countdown > 0
                                  ? AppColors.textHint
                                  : AppColors.primary,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ],
              ),
              actions: [
                TextButton(
                  onPressed: () {
                    timer?.cancel();
                    Navigator.of(context).pop();
                  },
                  child: const Text('取消'),
                ),
                TextButton(
                  onPressed: () async {
                    final phone = phoneController.text.trim();
                    final code = codeController.text.trim();
                    if (phone.isEmpty || code.isEmpty) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(content: Text('请填写完整信息')),
                      );
                      return;
                    }
                    try {
                      await _loginBindingService.bindPhone(phone, code);
                      timer?.cancel();
                      Navigator.of(context).pop();
                      _loadBindings();
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          const SnackBar(content: Text('绑定成功')),
                        );
                      }
                    } catch (e) {
                      if (context.mounted) {
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text('绑定失败: $e')),
                        );
                      }
                    }
                  },
                  child: Text(
                    '确定',
                    style: TextStyle(color: AppColors.primary),
                  ),
                ),
              ],
            );
          },
        );
      },
    );

    timer?.cancel();
    phoneController.dispose();
    codeController.dispose();
  }

  Future<void> _showUnbindDialog(LoginBindingDto binding) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('解绑确认'),
          content: Text('确定要解绑${binding.displayName ?? "此登录方式"}吗？'),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text('取消'),
            ),
            TextButton(
              onPressed: () => Navigator.of(context).pop(true),
              child: const Text('确定', style: TextStyle(color: Colors.red)),
            ),
          ],
        );
      },
    );

    if (confirmed == true && binding.loginProvider != null) {
      try {
        await _loginBindingService.unbindLogin(binding.loginProvider!);
        _loadBindings();
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('解绑成功')),
          );
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(content: Text('解绑失败: $e')),
          );
        }
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text(
          '账号安全',
          style: TextStyle(fontSize: 20, color: Colors.white),
        ),
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      backgroundColor: const Color(0xfff6f6f6),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Text(
                    '登录方式',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Color(0xff1a1a1a),
                    ),
                  ),
                  const SizedBox(height: 8),
                  Container(
                    decoration: BoxDecoration(
                      color: Colors.white,
                      borderRadius: BorderRadius.circular(8),
                      boxShadow: [
                        BoxShadow(
                          color: Colors.grey.withValues(alpha: 0.2),
                          spreadRadius: 1,
                          blurRadius: 5,
                          offset: const Offset(0, 2),
                        ),
                      ],
                    ),
                    child: Column(
                      children: [
                        _buildBindingTile(
                          icon: Icons.chat,
                          iconColor: const Color(0xff07c160),
                          title: '微信登录',
                          binding: _getBinding(LoginBindingType.wechat),
                          onBind: null,
                          onUnbind: _hasBinding(LoginBindingType.wechat)
                              ? () => _showUnbindDialog(
                                    _getBinding(LoginBindingType.wechat)!)
                              : null,
                        ),
                        _buildDivider(),
                        _buildBindingTile(
                          icon: Icons.phone_android,
                          iconColor: AppColors.primary,
                          title: '手机号登录',
                          binding: _getBinding(LoginBindingType.phone),
                          onBind: _hasBinding(LoginBindingType.phone)
                              ? null
                              : _showBindPhoneDialog,
                          onUnbind: _hasBinding(LoginBindingType.phone)
                              ? () => _showUnbindDialog(
                                    _getBinding(LoginBindingType.phone)!)
                              : null,
                        ),
                        _buildDivider(),
                        _buildBindingTile(
                          icon: Icons.lock,
                          iconColor: Colors.blue,
                          title: '密码登录',
                          binding: _getBinding(LoginBindingType.password),
                          onBind: null,
                          onUnbind: _hasBinding(LoginBindingType.password)
                              ? () => _showUnbindDialog(
                                    _getBinding(LoginBindingType.password)!)
                              : null,
                        ),
                        _buildDivider(),
                        _buildBindingTile(
                          icon: Icons.apple,
                          iconColor: Colors.black,
                          title: 'Apple 登录',
                          binding: _getBinding(LoginBindingType.apple),
                          onBind: null, // Apple 绑定不在 App 内完成
                          onUnbind: _hasBinding(LoginBindingType.apple)
                              ? () => _showUnbindDialog(
                                    _getBinding(LoginBindingType.apple)!)
                              : null,
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 16),
                  const Text(
                    '提示：解绑所有登录方式后，下次登录将创建新账号',
                    style: TextStyle(fontSize: 12, color: AppColors.textSecondary),
                  ),
                ],
              ),
            ),
    );
  }

  Widget _buildBindingTile({
    required IconData icon,
    required Color iconColor,
    required String title,
    required LoginBindingDto? binding,
    required VoidCallback? onBind,
    required VoidCallback? onUnbind,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: Row(
        children: [
          Container(
            width: 40,
            height: 40,
            decoration: BoxDecoration(
              color: iconColor.withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Icon(icon, size: 24, color: iconColor),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 16,
                    color: Color(0xff1a1a1a),
                  ),
                ),
                if (binding != null && binding.providerKey != null)
                  Text(
                    binding.providerKey!,
                    style: const TextStyle(
                      fontSize: 12,
                      color: AppColors.textSecondary,
                    ),
                  ),
              ],
            ),
          ),
          if (binding != null)
            TextButton(
              onPressed: onUnbind,
              child: const Text(
                '解绑',
                style: TextStyle(color: Colors.red),
              ),
            )
          else if (onBind != null)
            TextButton(
              onPressed: onBind,
              child: Text(
                '绑定',
                style: TextStyle(color: AppColors.primary),
              ),
            )
          else
            const Text(
              '暂不支持',
              style: TextStyle(color: AppColors.textHint),
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
}
