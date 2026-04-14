import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:cached_network_image/cached_network_image.dart';
import '../../../data/models/user_model.dart';

/// 用户资料弹窗
/// 显示用户头像、编号、QQ、微信等信息
class UserProfileDialog extends StatelessWidget {
  final UserDto user;

  const UserProfileDialog({super.key, required this.user});

  /// 显示用户资料弹窗
  static Future<void> show(BuildContext context, UserDto user) {
    return showDialog(
      context: context,
      builder: (context) => UserProfileDialog(user: user),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      child: Container(
        padding: const EdgeInsets.all(20),
        constraints: const BoxConstraints(maxWidth: 320),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // 头部：头像 + 名字
            Row(
              children: [
                // 头像
                _buildAvatar(),
                const SizedBox(width: 16),
                // 名字
                Expanded(
                  child: Text(
                    user.name ?? user.fullName ?? '用户',
                    style: const TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 20),
            const Divider(height: 1),
            const SizedBox(height: 16),

            // 用户编号
            _buildInfoRow(
              context,
              label: '用户编号',
              value: user.id?.toString() ?? '',
              copyable: true,
            ),

            // QQ
            if (user.qq != null && user.qq!.isNotEmpty)
              _buildInfoRow(
                context,
                label: 'QQ',
                value: user.qq!,
                copyable: true,
              ),

            // 微信
            if (user.wx != null && user.wx!.isNotEmpty)
              _buildInfoRow(
                context,
                label: '微信',
                value: user.wx!,
                copyable: true,
              ),

            const SizedBox(height: 20),

            // 关闭按钮
            SizedBox(
              width: double.infinity,
              child: TextButton(
                onPressed: () => Navigator.pop(context),
                style: TextButton.styleFrom(
                  backgroundColor: const Color(0xFFf4835a),
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(vertical: 12),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
                child: const Text('关闭'),
              ),
            ),
          ],
        ),
      ),
    );
  }

  /// 构建头像
  Widget _buildAvatar() {
    String? avatarUrl;
    if (user.headImgUrl != null) {
      final headImg = user.headImgUrl!;
      if (headImg.startsWith('http')) {
        avatarUrl = headImg;
      } else {
        avatarUrl = 'https://image.molitao.top/$headImg';
      }
    }

    if (avatarUrl == null || avatarUrl.isEmpty) {
      // 默认头像
      return Container(
        width: 64,
        height: 64,
        decoration: BoxDecoration(
          color: const Color(0xFFf4835a),
          shape: BoxShape.circle,
        ),
        child: Center(
          child: Text(
            _getAvatarText(user.name ?? user.fullName ?? '用户'),
            style: const TextStyle(
              color: Colors.white,
              fontSize: 20,
              fontWeight: FontWeight.bold,
            ),
          ),
        ),
      );
    }

    return ClipRRect(
      borderRadius: BorderRadius.circular(8),
      child: CachedNetworkImage(
        imageUrl: avatarUrl,
        width: 64,
        height: 64,
        fit: BoxFit.cover,
        placeholder: (context, url) => Container(
          width: 64,
          height: 64,
          color: Colors.grey.shade200,
          child: const Center(child: CircularProgressIndicator(strokeWidth: 2)),
        ),
        errorWidget: (context, url, error) => Container(
          width: 64,
          height: 64,
          decoration: BoxDecoration(
            color: const Color(0xFFf4835a),
            shape: BoxShape.circle,
          ),
          child: Center(
            child: Text(
              _getAvatarText(user.name ?? user.fullName ?? '用户'),
              style: const TextStyle(
                color: Colors.white,
                fontSize: 20,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ),
      ),
    );
  }

  /// 构建信息行
  Widget _buildInfoRow(
    BuildContext context, {
    required String label,
    required String value,
    bool copyable = false,
  }) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Row(
        children: [
          SizedBox(
            width: 70,
            child: Text(
              label,
              style: TextStyle(color: Colors.grey.shade600, fontSize: 14),
            ),
          ),
          Expanded(
            child: Row(
              children: [
                Expanded(
                  child: Text(value, style: const TextStyle(fontSize: 14)),
                ),
                if (copyable)
                  IconButton(
                    icon: Icon(
                      Icons.copy,
                      size: 18,
                      color: Colors.grey.shade500,
                    ),
                    onPressed: () => _copyToClipboard(context, value),
                    padding: EdgeInsets.zero,
                    constraints: const BoxConstraints(),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  /// 复制到剪贴板
  void _copyToClipboard(BuildContext context, String text) {
    Clipboard.setData(ClipboardData(text: text));
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text('已复制到剪贴板'),
        duration: Duration(seconds: 1),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  /// 获取头像文字
  String _getAvatarText(String name) {
    if (name.isEmpty) return '用户';
    // 取第一个字符（如果是中文）或前两个字符
    final runes = name.runes.toList();
    if (runes.length == 1) {
      return String.fromCharCode(runes.first);
    }
    return name.substring(0, 2);
  }
}
