import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:image_picker/image_picker.dart';
import 'package:molitao_app/presentation/providers/user_provider.dart';
import 'package:molitao_app/data/repositories/user_repository.dart';
import 'package:molitao_app/data/models/user_model.dart';
import 'package:molitao_app/data/services/upload_service.dart';

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
  final ImagePicker _picker = ImagePicker();
  final UploadService _uploadService = UploadService();

  bool _isLoading = false;
  bool _isSaving = false;
  bool _isUploading = false;
  String? _headImgUrl;
  String? _tempAvatarPath;

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

  Future<void> _pickAvatar() async {
    try {
      final XFile? image = await _picker.pickImage(
        source: ImageSource.gallery,
        maxWidth: 800,
        maxHeight: 800,
        imageQuality: 80,
      );

      if (image != null) {
        setState(() {
          _tempAvatarPath = image.path;
          _isUploading = true;
        });
        await _uploadAvatar(image.path);
      }
    } catch (e) {
      _showSnackBar('选择图片失败');
      setState(() {
        _tempAvatarPath = null;
        _isUploading = false;
      });
    }
  }

  Future<void> _uploadAvatar(String filePath) async {
    try {
      final userState = ref.read(userProvider);
      final userId = userState.user?.id?.toString();

      print('[UserInfoPage] 开始上传头像: filePath=$filePath, userId=$userId');

      final imageUrl = await _uploadService.uploadImage(
        filePath,
        userId: userId,
      );

      print('[UserInfoPage] 上传结果: imageUrl=$imageUrl');

      if (imageUrl != null) {
        // 添加时间戳参数强制刷新图片缓存
        final cacheBustingUrl =
            '$imageUrl?t=${DateTime.now().millisecondsSinceEpoch}';

        setState(() {
          _headImgUrl = cacheBustingUrl;
          _tempAvatarPath = null;
          _isUploading = false;
        });

        // 立即更新 userProvider，让其他页面也能看到新头像
        final currentUser = userState.user;
        if (currentUser != null) {
          print('[UserInfoPage] 更新 userProvider: headImgUrl=$cacheBustingUrl');
          ref
              .read(userProvider.notifier)
              .updateUser(currentUser.copyWith(headImgUrl: cacheBustingUrl));
        }

        _showSnackBar('头像上传成功');
      } else {
        print('[UserInfoPage] 上传失败: imageUrl 为 null');
        setState(() {
          _tempAvatarPath = null;
          _isUploading = false;
        });
        _showSnackBar('头像上传失败');
      }
    } catch (e) {
      print('[UserInfoPage] 上传异常: $e');
      setState(() {
        _tempAvatarPath = null;
        _isUploading = false;
      });
      _showSnackBar('头像上传失败');
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
      // 注意：isActive 和 depositBalance 是敏感字段，不应由客户端设置
      // 后端会忽略这些字段，保持原有值
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
                headImgUrl: _headImgUrl,
                qq: _qqController.text.trim(),
                wx: _wxController.text.trim(),
              ),
            );
        _showSnackBar('修改成功');
        if (mounted) context.pop();
      } else {
        _showSnackBar('保存失败');
      }
    } catch (e) {
      _showSnackBar('保存失败');
    } finally {
      if (mounted) setState(() => _isSaving = false);
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

                    // 头像
                    _buildFormItem(
                      label: '头像',
                      child: GestureDetector(
                        onTap: _isUploading ? null : _pickAvatar,
                        child: Container(
                          width: 100,
                          height: 100,
                          decoration: BoxDecoration(
                            color: Colors.grey[200],
                            borderRadius: BorderRadius.circular(8),
                            border: Border.all(color: Colors.grey[300]!),
                          ),
                          child: _isUploading
                              ? const Center(
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                  ),
                                )
                              : _tempAvatarPath != null
                              ? ClipRRect(
                                  borderRadius: BorderRadius.circular(8),
                                  child: Image.file(
                                    File(_tempAvatarPath!),
                                    fit: BoxFit.cover,
                                  ),
                                )
                              : _headImgUrl != null && _headImgUrl!.isNotEmpty
                              ? Stack(
                                  children: [
                                    ClipRRect(
                                      borderRadius: BorderRadius.circular(8),
                                      child: Image.network(
                                        _headImgUrl!,
                                        fit: BoxFit.cover,
                                        width: 100,
                                        height: 100,
                                        errorBuilder: (context, error, stack) =>
                                            _buildAvatarPlaceholder(),
                                      ),
                                    ),
                                    Positioned(
                                      right: 4,
                                      bottom: 4,
                                      child: Container(
                                        padding: const EdgeInsets.all(4),
                                        decoration: BoxDecoration(
                                          color: Colors.black54,
                                          borderRadius: BorderRadius.circular(
                                            12,
                                          ),
                                        ),
                                        child: const Icon(
                                          Icons.camera_alt,
                                          size: 16,
                                          color: Colors.white,
                                        ),
                                      ),
                                    ),
                                  ],
                                )
                              : Stack(
                                  children: [
                                    _buildAvatarPlaceholder(),
                                    Positioned(
                                      right: 4,
                                      bottom: 4,
                                      child: Container(
                                        padding: const EdgeInsets.all(4),
                                        decoration: BoxDecoration(
                                          color: Colors.black54,
                                          borderRadius: BorderRadius.circular(
                                            12,
                                          ),
                                        ),
                                        child: const Icon(
                                          Icons.add,
                                          size: 16,
                                          color: Colors.white,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                        ),
                      ),
                    ),
                    Padding(
                      padding: const EdgeInsets.only(top: 8),
                      child: Text(
                        '点击头像${_headImgUrl != null && _headImgUrl!.isNotEmpty ? '重新上传' : '上传'}',
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
                        onPressed: _isSaving || _isUploading
                            ? null
                            : _saveUserInfo,
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
