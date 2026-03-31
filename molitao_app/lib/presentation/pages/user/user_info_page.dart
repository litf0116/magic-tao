import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:molitao_app/presentation/providers/user_provider.dart';
import 'package:molitao_app/data/repositories/user_repository.dart';
import 'package:molitao_app/data/models/user_model.dart';

class UserInfoPage extends ConsumerStatefulWidget {
  const UserInfoPage({super.key});

  @override
  ConsumerState<UserInfoPage> createState() => _UserInfoPageState();
}

class _UserInfoPageState extends ConsumerState<UserInfoPage> {
  final _formKey = GlobalKey<FormState>();
  final _nicknameController = TextEditingController();
  final _qqController = TextEditingController();
  final _wxController = TextEditingController();

  bool _isLoading = false;
  bool _isSaving = false;
  String? _headImgUrl;

  @override
  void initState() {
    super.initState();
    _loadUserData();
  }

  @override
  void dispose() {
    _nicknameController.dispose();
    _qqController.dispose();
    _wxController.dispose();
    super.dispose();
  }

  Future<void> _loadUserData() async {
    setState(() => _isLoading = true);

    try {
      final userState = ref.read(userProvider);
      if (userState.user != null) {
        final user = userState.user!;
        _nicknameController.text = user.fullName ?? user.userName ?? '';
        _qqController.text = user.qq ?? '';
        _wxController.text = user.wx ?? '';
        _headImgUrl = user.headImgUrl;
      }
    } catch (e) {
      _showSnackBar('获取用户信息失败');
    } finally {
      setState(() => _isLoading = false);
    }
  }

  Future<void> _saveUserInfo() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSaving = true);

    try {
      final userState = ref.read(userProvider);
      final currentUser = userState.user;

      if (currentUser == null) {
        _showSnackBar('用户未登录');
        setState(() => _isSaving = false);
        return;
      }

      // 构建更新后的用户数据
      final updatedUserDto = UserDto(
        id: currentUser.id,
        userName: currentUser.userName,
        name: _nicknameController.text.trim(),
        fullName: _nicknameController.text.trim(),
        headImgUrl: _headImgUrl,
        qq: _qqController.text.trim(),
        wx: _wxController.text.trim(),
        phoneNumber: currentUser.phoneNumber,
      );

      // 调用更新接口
      final response = await UserRepository().updateUser(updatedUserDto);

      if (response != null) {
        // 更新本地状态
        ref
            .read(userProvider.notifier)
            .updateUser(
              currentUser.copyWith(
                fullName: _nicknameController.text.trim(),
                qq: _qqController.text.trim(),
                wx: _wxController.text.trim(),
              ),
            );
        _showSnackBar('修改成功');
        context.pop();
      } else {
        _showSnackBar('保存失败');
      }
    } catch (e) {
      _showSnackBar('保存失败');
    } finally {
      setState(() => _isSaving = false);
    }
  }

  void _showSnackBar(String message) {
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final userState = ref.watch(userProvider);

    return Scaffold(
      appBar: AppBar(
        title: const Text('个人信息'),
        backgroundColor: const Color(0xfff4835a),
        foregroundColor: Colors.white,
        actions: [
          if (_isSaving)
            const Center(
              child: Padding(
                padding: EdgeInsets.only(right: 16),
                child: SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: Colors.white,
                  ),
                ),
              ),
            ),
        ],
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Form(
                key: _formKey,
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // 用户编号（只读）
                    _buildFormItem(
                      label: '用户编号',
                      child: Text(
                        userState.user?.id?.toString() ?? '-',
                        style: const TextStyle(color: Colors.grey),
                      ),
                    ),

                    const SizedBox(height: 16),

                    // 头像（只读展示）
                    _buildFormItem(
                      label: '头像',
                      child: Container(
                        width: 100,
                        height: 100,
                        decoration: BoxDecoration(
                          color: Colors.grey[200],
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.grey[300]!),
                        ),
                        child: _headImgUrl != null && _headImgUrl!.isNotEmpty
                            ? ClipRRect(
                                borderRadius: BorderRadius.circular(8),
                                child: Image.network(
                                  _headImgUrl!,
                                  fit: BoxFit.cover,
                                  errorBuilder: (context, error, stack) =>
                                      _buildAvatarPlaceholder(),
                                ),
                              )
                            : _buildAvatarPlaceholder(),
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.only(top: 8),
                      child: Text(
                        '头像暂时无法修改',
                        style: TextStyle(fontSize: 12, color: Colors.grey[600]),
                      ),
                    ),

                    const SizedBox(height: 16),

                    // 昵称
                    _buildFormItem(
                      label: '昵称',
                      child: TextFormField(
                        controller: _nicknameController,
                        decoration: const InputDecoration(
                          hintText: '请输入昵称',
                          border: OutlineInputBorder(),
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 8,
                          ),
                        ),
                        validator: (value) {
                          if (value == null || value.trim().isEmpty) {
                            return '昵称不能为空';
                          }
                          return null;
                        },
                      ),
                    ),

                    const SizedBox(height: 16),

                    // QQ
                    _buildFormItem(
                      label: 'QQ',
                      child: TextFormField(
                        controller: _qqController,
                        decoration: const InputDecoration(
                          hintText: '请输入QQ号',
                          border: OutlineInputBorder(),
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 8,
                          ),
                        ),
                        keyboardType: TextInputType.number,
                      ),
                    ),

                    const SizedBox(height: 16),

                    // 微信
                    _buildFormItem(
                      label: '微信',
                      child: TextFormField(
                        controller: _wxController,
                        decoration: const InputDecoration(
                          hintText: '请输入微信号',
                          border: OutlineInputBorder(),
                          contentPadding: EdgeInsets.symmetric(
                            horizontal: 12,
                            vertical: 8,
                          ),
                        ),
                      ),
                    ),

                    const SizedBox(height: 24),

                    // 提交按钮
                    SizedBox(
                      width: double.infinity,
                      child: ElevatedButton(
                        onPressed: _isSaving ? null : _saveUserInfo,
                        style: ElevatedButton.styleFrom(
                          backgroundColor: const Color(0xfff4835a),
                          foregroundColor: Colors.white,
                          padding: const EdgeInsets.symmetric(vertical: 14),
                          shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        child: _isSaving
                            ? const SizedBox(
                                width: 20,
                                height: 20,
                                child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white,
                                ),
                              )
                            : const Text(
                                '提交',
                                style: TextStyle(
                                  fontSize: 16,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
    );
  }

  Widget _buildAvatarPlaceholder() {
    return Column(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        Icon(Icons.person, size: 40, color: Colors.grey[400]),
        const SizedBox(height: 4),
        Text('暂无头像', style: TextStyle(fontSize: 12, color: Colors.grey[500])),
      ],
    );
  }

  Widget _buildFormItem({required String label, required Widget child}) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
        ),
        const SizedBox(height: 8),
        child,
      ],
    );
  }
}
