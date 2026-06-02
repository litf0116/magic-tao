import "package:flutter/material.dart";
import "package:flutter/services.dart";
import "package:cached_network_image/cached_network_image.dart";
import 'package:molitao_app/data/models/user_model.dart';

/// 用户资料弹窗
/// 显示用户头像、编号、QQ、微信等信息
class UserProfileDialog extends StatelessWidget {

  const UserProfileDialog({super.key, required this.user});
  final UserDto user;

  /// 显示用户资料弹窗
  static Future<void> show(final BuildContext context, final UserDto user) => showDialog(
      context: context,
      builder: (context) => UserProfileDialog(user: user),
    );

  @override
  Widget build(final BuildContext context) => Dialog(
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

  /// 构建头像
  Widget _buildAvatar() {
    String? avatarUrl;
    if (user.headImgUrl != null) {
      var headImg = user.headImgUrl!;
      if (headImg.startsWith("http")) {
        avatarUrl = headImg;
      } else {
        avatarUrl = "https://image.molitao.top/$headImg";
      }
    }

    if (avatarUrl == null || avatarUrl.isEmpty) {
      // 默认头像
      return Container(
        width: 64,
        height: 64,
        decoration: const BoxDecoration(
          color: Color(0xFFf4835a),
          shape: BoxShape.circle,
        ),
        child: Center(
          child: Text(
            _getAvatarText(user.name ?? user.fullName ?? "用户"),
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
        placeholder: (final context, final url) => Container(
          width: 64,
          height: 64,
          color: Colors.grey.shade200,
          child: const Center(child: CircularProgressIndicator(strokeWidth: 2)),
        ),
        errorWidget: (final context, final url, final error) => Container(
          width: 64,
          height: 64,
          decoration: const BoxDecoration(
            color: Color(0xFFf4835a),
            shape: BoxShape.circle,
          ),
          child: Center(
            child: Text(
              _getAvatarText(user.name ?? user.fullName ?? "用户"),
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
    final BuildContext context, {
    required final String label,
    required final String value,
    final bool copyable = false,
  }) => Padding(
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

  /// 复制到剪贴板
  void _copyToClipboard(final BuildContext context, final String text) {
    Clipboard.setData(ClipboardData(text: text));
    ScaffoldMessenger.of(context).showSnackBar(
      const SnackBar(
        content: Text("已复制到剪贴板"),
        duration: Duration(seconds: 1),
        behavior: SnackBarBehavior.floating,
      ),
    );
  }

  /// 获取头像文字
  String _getAvatarText(final String name) {
    if (name.isEmpty) return "用户";
    // 取第一个字符（如果是中文）或前两个字符
    var runes = name.runes.toList();
    if (runes.length == 1) {
      return String.fromCharCode(runes.first);
    }
    return name.substring(0, 2);
  }

  @override
  void debugFillProperties(DiagnosticPropertiesBuilder properties) {
    super.debugFillProperties(properties);
    properties.add(DiagnosticsProperty<UserDto>('user', user));
  }
}
