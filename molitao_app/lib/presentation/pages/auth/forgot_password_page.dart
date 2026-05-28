import 'dart:async';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../../core/theme/app_colors.dart';
import '../../../data/repositories/auth_repository.dart';
import '../../../data/services/storage_service.dart';

/// 忘记密码页面 - 通过手机验证码重置密码
class ForgotPasswordPage extends StatefulWidget {
  const ForgotPasswordPage({super.key});

  @override
  State<ForgotPasswordPage> createState() => _ForgotPasswordPageState();
}

class _ForgotPasswordPageState extends State<ForgotPasswordPage> {
  final _phoneController = TextEditingController();
  final _smsCodeController = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _formKey = GlobalKey<FormState>();
  final _authRepository = AuthRepository();
  final _storageService = StorageService();

  bool _isLoading = false;
  bool _obscurePassword = true;
  bool _obscureConfirm = true;
  int _countdown = 0;
  Timer? _timer;
  bool _isResetSuccess = false;

  @override
  void dispose() {
    _phoneController.dispose();
    _smsCodeController.dispose();
    _passwordController.dispose();
    _confirmPasswordController.dispose();
    _timer?.cancel();
    super.dispose();
  }

  Future<void> _handleSendSmsCode() async {
    final phone = _phoneController.text.trim();
    if (phone.length != 11 || !RegExp(r'^1[3-9]\d{9}$').hasMatch(phone)) {
      _showSnackBar('请输入正确的手机号');
      return;
    }

    setState(() => _isLoading = true);
    try {
      await _authRepository.sendSmsCode(phone, purpose: 'resetpassword');
      _startCountdown();
      _showSnackBar('验证码已发送');
    } catch (e) {
      _showSnackBar('发送验证码失败：${e.toString().replaceFirst('Exception: ', '')}');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  void _startCountdown() {
    setState(() => _countdown = 60);
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (_countdown > 0) {
        setState(() => _countdown--);
      } else {
        timer.cancel();
      }
    });
  }

  Future<void> _handleResetPassword() async {
    if (!_formKey.currentState!.validate()) return;

    final phone = _phoneController.text.trim();
    final code = _smsCodeController.text.trim();
    final newPassword = _passwordController.text;

    setState(() => _isLoading = true);
    try {
      await _authRepository.phoneResetPassword(phone, code, newPassword);
      setState(() => _isResetSuccess = true);
    } catch (e) {
      _showSnackBar('重置失败：${e.toString().replaceFirst('Exception: ', '')}');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  void _showSnackBar(String message) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        backgroundColor: AppColors.primary,
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  Widget _buildInputField({
    required TextEditingController controller,
    required String hintText,
    required String fieldName,
    TextInputType? keyboardType,
    bool obscureText = false,
    Widget? suffixIcon,
    String? Function(String?)? validator,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: obscureText,
      keyboardType: keyboardType,
      validator: validator,
      decoration: InputDecoration(
        hintText: hintText,
        filled: true,
        fillColor: Colors.white,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: AppColors.divider),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: AppColors.divider),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: AppColors.primary, width: 1.5),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        suffixIcon: suffixIcon,
      ),
    );
  }

  Widget _buildSuccessView() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 32),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                color: AppColors.primary.withValues(alpha: 0.1),
                shape: BoxShape.circle,
              ),
              child: const Icon(
                Icons.check_circle_outline,
                size: 48,
                color: AppColors.primary,
              ),
            ),
            const SizedBox(height: 24),
            const Text(
              '密码重置成功',
              style: TextStyle(
                fontSize: 20,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 12),
            const Text(
              '请使用新密码登录',
              style: TextStyle(
                fontSize: 14,
                color: AppColors.textSecondary,
              ),
            ),
            const SizedBox(height: 40),
            SizedBox(
              width: double.infinity,
              height: 48,
              child: ElevatedButton(
                onPressed: () => context.go('/login'),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppColors.primary,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24),
                  ),
                ),
                child: const Text(
                  '返回登录',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.w500),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildResetForm() {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 手机号输入
          _buildInputField(
            controller: _phoneController,
            hintText: '请输入手机号',
            fieldName: 'phone',
            keyboardType: TextInputType.phone,
            validator: (value) {
              if (value == null || value.isEmpty) return '请输入手机号';
              if (!RegExp(r'^1[3-9]\d{9}$').hasMatch(value)) return '手机号格式不正确';
              return null;
            },
          ),
          const SizedBox(height: 16),
          // 验证码输入 + 获取验证码按钮
          Row(
            children: [
              Expanded(
                child: _buildInputField(
                  controller: _smsCodeController,
                  hintText: '请输入验证码',
                  fieldName: 'smsCode',
                  keyboardType: TextInputType.number,
                  validator: (value) {
                    if (value == null || value.isEmpty) return '请输入验证码';
                    return null;
                  },
                ),
              ),
              const SizedBox(width: 12),
              SizedBox(
                width: 100,
                height: 48,
                child: TextButton(
                  onPressed: _countdown > 0 || _isLoading
                      ? null
                      : _handleSendSmsCode,
                  style: TextButton.styleFrom(
                    backgroundColor: _countdown > 0
                        ? AppColors.divider
                        : AppColors.primary.withValues(alpha: 0.1),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
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
          const SizedBox(height: 16),
          // 新密码输入
          _buildInputField(
            controller: _passwordController,
            hintText: '新密码（至少6位）',
            fieldName: 'newPassword',
            obscureText: _obscurePassword,
            validator: (value) {
              if (value == null || value.isEmpty) return '请输入新密码';
              if (value.length < 6) return '密码至少6位';
              return null;
            },
            suffixIcon: IconButton(
              icon: Icon(
                _obscurePassword ? Icons.visibility_off : Icons.visibility,
                color: AppColors.textHint,
              ),
              onPressed: () => setState(() => _obscurePassword = !_obscurePassword),
            ),
          ),
          const SizedBox(height: 16),
          // 确认新密码输入
          _buildInputField(
            controller: _confirmPasswordController,
            hintText: '确认新密码',
            fieldName: 'confirmPassword',
            obscureText: _obscureConfirm,
            validator: (value) {
              if (value == null || value.isEmpty) return '请确认新密码';
              if (value != _passwordController.text) return '两次密码不一致';
              return null;
            },
            suffixIcon: IconButton(
              icon: Icon(
                _obscureConfirm ? Icons.visibility_off : Icons.visibility,
                color: AppColors.textHint,
              ),
              onPressed: () => setState(() => _obscureConfirm = !_obscureConfirm),
            ),
          ),
          const SizedBox(height: 40),
          // 确认重置按钮
          SizedBox(
            width: double.infinity,
            height: 48,
            child: ElevatedButton(
              onPressed: _isLoading ? null : _handleResetPassword,
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
                      '确认重置',
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('重置密码'),
        backgroundColor: Colors.transparent,
        elevation: 0,
        foregroundColor: AppColors.textPrimary,
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            if (!_isResetSuccess) ...[
              const Text(
                '请输入手机号和验证码',
                style: TextStyle(
                  fontSize: 24,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const SizedBox(height: 8),
              const Text(
                '通过手机验证码重置您的登录密码',
                style: TextStyle(
                  fontSize: 14,
                  color: AppColors.textSecondary,
                ),
              ),
              const SizedBox(height: 32),
              _buildResetForm(),
            ] else
              _buildSuccessView(),
          ],
        ),
      ),
    );
  }
}
