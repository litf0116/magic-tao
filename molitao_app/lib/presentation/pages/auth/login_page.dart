import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:go_router/go_router.dart';
import 'package:fluwx/fluwx.dart' as fluwx;
import '../../../core/theme/app_colors.dart';
import '../../../data/repositories/auth_repository.dart';
import '../../../data/services/storage_service.dart';
import '../../../data/services/wechat_service.dart';
import '../../providers/user_provider.dart';

enum LoginTab { password, sms }

class LoginPage extends ConsumerStatefulWidget {
  final String? redirectPath;

  const LoginPage({super.key, this.redirectPath});

  @override
  ConsumerState<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends ConsumerState<LoginPage>
    with SingleTickerProviderStateMixin {
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  final _phoneController = TextEditingController();
  final _smsCodeController = TextEditingController();
  final _passwordFormKey = GlobalKey<FormState>();
  final _smsFormKey = GlobalKey<FormState>();
  final _authRepository = AuthRepository();
  final _storageService = StorageService();
  final _wechatService = WeChatService();

  bool _isLoading = false;
  bool _obscurePassword = true;
  String? _focusedField;
  int _countdown = 0;
  Timer? _timer;

  @override
  void initState() {
    super.initState();
    _initWeChat();
    _loadRememberedUsername();
    _restoreSmsCountdown();
  }

  Future<void> _loadRememberedUsername() async {
    final username = await _storageService.getRememberedUsername();
    if (username != null && username.isNotEmpty) {
      _usernameController.text = username;
    }
  }

  Future<void> _restoreSmsCountdown() async {
    final endTime = await _storageService.getSmsCountdownEndTime();
    if (endTime != null) {
      final now = DateTime.now().millisecondsSinceEpoch ~/ 1000;
      final remaining = endTime - now;
      if (remaining > 0) {
        setState(() => _countdown = remaining);
        _startTimerFromRemaining();
      } else {
        await _storageService.clearSmsCountdownEndTime();
      }
    }
  }

  void _startTimerFromRemaining() {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_countdown > 0) {
        setState(() => _countdown--);
      } else {
        timer.cancel();
        _storageService.clearSmsCountdownEndTime();
      }
    });
  }

  Future<void> _initWeChat() async {
    await _wechatService.initialize();
  }

  @override
  void dispose() {
    _timer?.cancel();
    _usernameController.dispose();
    _passwordController.dispose();
    _phoneController.dispose();
    _smsCodeController.dispose();
    super.dispose();
  }

  Future<void> _handlePasswordLogin() async {
    if (!_passwordFormKey.currentState!.validate()) return;

    setState(() => _isLoading = true);

    try {
      final result = await _authRepository.login(
        _usernameController.text.trim(),
        _passwordController.text,
      );

      if (result.accessToken != null && result.user != null && mounted) {
        await _storageService.setRememberedUsername(
          _usernameController.text.trim(),
        );

        await ref.read(userProvider.notifier).login(
              result.accessToken!,
              User(
                id: result.user!.id,
                userName: result.user!.userName,
                fullName: result.user!.fullName,
                phoneNumber: result.user!.phoneNumber,
                headImgUrl: result.user!.headImgUrl,
                depositBalance: result.user!.depositBalance?.toDouble(),
                permissions: result.user!.permissions ?? [],
                roleNames: result.user!.roleNames ?? [],
              ),
              roles: result.roles,
            );

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('登录成功')),
          );
        }
        context.go(widget.redirectPath ?? '/home');
      } else {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('登录失败，请重试')),
          );
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('登录失败，请重试')),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _handleSendSmsCode() async {
    final phone = _phoneController.text.trim();
    if (phone.isEmpty || !_isValidPhone(phone)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('请输入正确的手机号')),
      );
      return;
    }

    setState(() => _isLoading = true);

    try {
      await _authRepository.sendSmsCode(phone);
      _startCountdown();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('验证码已发送')),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('发送验证码失败')),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _startCountdown() {
    setState(() => _countdown = 60);
    final endTime = DateTime.now().millisecondsSinceEpoch ~/ 1000 + 60;
    _storageService.setSmsCountdownEndTime(endTime);
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_countdown > 0) {
        setState(() => _countdown--);
      } else {
        timer.cancel();
        _storageService.clearSmsCountdownEndTime();
      }
    });
  }

  bool _isValidPhone(String phone) {
    return RegExp(r'^1[3-9]\d{9}$').hasMatch(phone);
  }

  Future<void> _handleSmsLogin() async {
    if (!_smsFormKey.currentState!.validate()) return;

    final code = _smsCodeController.text.trim();
    if (code.length != 6) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('请输入6位验证码')),
      );
      return;
    }

    setState(() => _isLoading = true);

    try {
      final result = await _authRepository.phoneAuthenticate(
        _phoneController.text.trim(),
        code,
      );

      if (result.accessToken != null && result.user != null && mounted) {
        await ref.read(userProvider.notifier).login(
              result.accessToken!,
              User(
                id: result.user!.id,
                userName: result.user!.userName,
                fullName: result.user!.fullName,
                phoneNumber: result.user!.phoneNumber,
                headImgUrl: result.user!.headImgUrl,
                depositBalance: result.user!.depositBalance?.toDouble(),
                permissions: result.user!.permissions ?? [],
                roleNames: result.user!.roleNames ?? [],
              ),
              roles: result.roles,
            );

        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('登录成功')),
          );
        }
        context.go(widget.redirectPath ?? '/home');
      } else {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('登录失败，请重试')),
          );
        }
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('登录失败，请重试')),
        );
      }
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  Future<void> _handleWeChatLogin() async {
    setState(() => _isLoading = true);

    try {
      final installed = await _wechatService.checkWeChatInstalled();
      if (!installed) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('请先安装微信')),
          );
        }
        return;
      }

      late fluwx.WeChatResponseSubscriber subscriber;
      subscriber = (fluwx.WeChatResponse response) async {
        _wechatService.removeSubscriber(subscriber);

        if (response is fluwx.WeChatAuthResponse) {
          if (response.isSuccessful && response.code != null) {
            try {
              final result = await _authRepository.weixinAppLogin(
                response.code!,
              );

              if (result.accessToken != null &&
                  result.user != null &&
                  mounted) {
                await ref.read(userProvider.notifier).login(
                      result.accessToken!,
                      User(
                        id: result.user!.id,
                        userName: result.user!.userName,
                        fullName: result.user!.fullName,
                        phoneNumber: result.user!.phoneNumber,
                        headImgUrl: result.user!.headImgUrl,
                        depositBalance:
                            result.user!.depositBalance?.toDouble(),
                        permissions: result.user!.permissions ?? [],
                        roleNames: result.user!.roleNames ?? [],
                      ),
                      roles: result.roles,
                    );
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('登录成功')),
                  );
                }
                context.go(widget.redirectPath ?? '/home');
              } else {
                if (mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('微信登录失败')),
                  );
                }
              }
            } catch (e) {
              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('微信登录失败')),
                );
              }
            }
          } else if (response.errCode != 0) {
            if (mounted) {
              ScaffoldMessenger.of(context).showSnackBar(
                const SnackBar(content: Text('微信登录失败')),
              );
            }
          }
        }

        if (mounted) setState(() => _isLoading = false);
      };

      _wechatService.addSubscriber(subscriber);
      await _wechatService.login();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('微信登录失败')),
        );
      }
      if (mounted) setState(() => _isLoading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xfff6f6f6),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Column(
            children: [
              const SizedBox(height: 40),
              ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Image.network(
                  'https://image.molitao.top/20250330/gg4hck6wkx2ndrn46dbw0lcxwh5ik0hi.png',
                  width: 120,
                  height: 80,
                  fit: BoxFit.contain,
                  errorBuilder: (_, __, ___) => Container(
                    width: 120,
                    height: 80,
                    color: AppColors.divider,
                    child: const Icon(
                      Icons.account_circle,
                      size: 60,
                      color: AppColors.textHint,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 40),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(24),
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.05),
                      blurRadius: 10,
                      offset: const Offset(0, 2),
                    ),
                  ],
                ),
                child: Column(
                  children: [
                    _buildPasswordLoginForm(),
                    const SizedBox(height: 8),
                    const Row(
                      children: [
                        Expanded(child: Divider()),
                        Padding(
                          padding: EdgeInsets.symmetric(horizontal: 12),
                          child: Text(
                            '其他登录方式',
                            style: TextStyle(color: AppColors.textSecondary),
                          ),
                        ),
                        Expanded(child: Divider()),
                      ],
                    ),
                    const SizedBox(height: 24),
                    GestureDetector(
                      onTap: _isLoading ? null : _handleWeChatLogin,
                      child: Container(
                        width: 44,
                        height: 44,
                        decoration: BoxDecoration(
                          color: const Color(0xff07c160),
                          shape: BoxShape.circle,
                        ),
                        child: Center(
                          child: SvgPicture.asset(
                            'assets/images/wechat-icon.svg',
                            width: 28,
                            height: 28,
                            colorFilter: const ColorFilter.mode(
                              Colors.white,
                              BlendMode.srcIn,
                            ),
                          ),
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 24),
              const Text(
                '登录即代表您已同意《用户协议》',
                style: TextStyle(fontSize: 12, color: AppColors.textSecondary),
              ),
            ],
          ),
        ),
      ),
    );
  }


  Widget _buildPasswordLoginForm() {
    return Form(
      key: _passwordFormKey,
      child: Column(
        children: [
          _buildInputField(
            controller: _usernameController,
            hintText: '请输入账号',
            fieldName: 'account',
          ),
          const SizedBox(height: 20),
          _buildInputField(
            controller: _passwordController,
            hintText: '请输入密码',
            fieldName: 'password',
            obscureText: _obscurePassword,
            suffixIcon: IconButton(
              icon: Icon(
                _obscurePassword ? Icons.visibility_off : Icons.visibility,
                color: AppColors.textHint,
                size: 20,
              ),
              onPressed: () => setState(
                () => _obscurePassword = !_obscurePassword,
              ),
            ),
          ),
          const SizedBox(height: 12),
          Align(
            alignment: Alignment.centerRight,
            child: TextButton(
              onPressed: () {},
              child: const Text(
                '忘记密码？',
                style: TextStyle(color: AppColors.textSecondary),
              ),
            ),
          ),
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            height: 48,
            child: ElevatedButton(
              onPressed: _isLoading ? null : _handlePasswordLogin,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
              ),
              child: _isLoading
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Text(
                      '登录',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildSmsLoginForm() {
    return Form(
      key: _smsFormKey,
      child: Column(
        children: [
          _buildInputField(
            controller: _phoneController,
            hintText: '请输入手机号',
            fieldName: 'phone',
            keyboardType: TextInputType.phone,
          ),
          const SizedBox(height: 20),
          Row(
            children: [
              Expanded(
                child: _buildInputField(
                  controller: _smsCodeController,
                  hintText: '请输入验证码',
                  fieldName: 'smsCode',
                  keyboardType: TextInputType.number,
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 100,
                height: 44,
                child: TextButton(
                  onPressed: _countdown > 0 || _isLoading
                      ? null
                      : _handleSendSmsCode,
                  style: TextButton.styleFrom(
                    backgroundColor: _countdown > 0
                        ? AppColors.divider
                        : AppColors.primary.withValues(alpha: 0.1),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                  child: Text(
                    _countdown > 0 ? '${_countdown}s' : '获取验证码',
                    style: TextStyle(
                      fontSize: 13,
                      color: _countdown > 0
                          ? AppColors.textHint
                          : AppColors.primary,
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 44),
          SizedBox(
            width: double.infinity,
            height: 48,
            child: ElevatedButton(
              onPressed: _isLoading ? null : _handleSmsLogin,
              style: ElevatedButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(24),
                ),
              ),
              child: _isLoading
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Text(
                      '登录',
                      style: TextStyle(
                        fontSize: 16,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildInputField({
    required TextEditingController controller,
    required String hintText,
    required String fieldName,
    bool obscureText = false,
    Widget? suffixIcon,
    TextInputType? keyboardType,
  }) {
    final isFocused = _focusedField == fieldName;
    return Container(
      height: 44,
      decoration: BoxDecoration(
        border: Border(
          bottom: BorderSide(
            color: isFocused ? AppColors.primary : AppColors.divider,
            width: 1,
          ),
        ),
      ),
      child: TextFormField(
        controller: controller,
        obscureText: obscureText,
        keyboardType: keyboardType,
        decoration: InputDecoration(
          hintText: hintText,
          hintStyle: const TextStyle(fontSize: 15, color: AppColors.textHint),
          border: InputBorder.none,
          suffixIcon: suffixIcon,
        ),
        style: const TextStyle(fontSize: 15, color: AppColors.textPrimary),
        onTap: () => setState(() => _focusedField = fieldName),
        validator: (value) {
          if (value == null || value.isEmpty) {
            switch (fieldName) {
              case 'account':
                return '请输入账号';
              case 'password':
                return '请输入密码';
              case 'phone':
                return '请输入手机号';
              case 'smsCode':
                return '请输入验证码';
            }
          }
          if (fieldName == 'phone' && !_isValidPhone(value!)) {
            return '请输入正确的手机号';
          }
          return null;
        },
      ),
    );
  }
}
