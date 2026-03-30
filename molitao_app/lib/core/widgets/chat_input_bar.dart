import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

/// 聊天输入栏组件
///
/// 提供消息输入功能，包括：
/// - 文本输入框
/// - 表情选择按钮
/// - 更多功能按钮（图片、文件等）
/// - 发送按钮
class ChatInputBar extends StatefulWidget {
  final Function(String) onSendText;
  final VoidCallback? onEmojiTap;
  final VoidCallback? onImagePick;
  final VoidCallback? onFilePick;
  final VoidCallback? onMoreTap;
  final String? placeholder;
  final int maxLength;

  const ChatInputBar({
    super.key,
    required this.onSendText,
    this.onEmojiTap,
    this.onImagePick,
    this.onFilePick,
    this.onMoreTap,
    this.placeholder,
    this.maxLength = 700,
  });

  @override
  State<ChatInputBar> createState() => _ChatInputBarState();
}

class _ChatInputBarState extends State<ChatInputBar> {
  final TextEditingController _controller = TextEditingController();
  final FocusNode _focusNode = FocusNode();
  bool _hasText = false;

  @override
  void initState() {
    super.initState();
    _controller.addListener(_onTextChanged);
  }

  @override
  void dispose() {
    _controller.removeListener(_onTextChanged);
    _controller.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  void _onTextChanged() {
    setState(() {
      _hasText = _controller.text.isNotEmpty;
    });
  }

  void _handleSend() {
    final text = _controller.text.trim();
    if (text.isNotEmpty) {
      widget.onSendText(text);
      _controller.clear();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 4,
            offset: const Offset(0, -1),
          ),
        ],
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 8),
          child: Row(
            children: [
              // 表情按钮
              _buildIconButton(
                icon: Icons.emoji_emotions_outlined,
                onTap: widget.onEmojiTap,
              ),

              // 输入框
              Expanded(
                child: Container(
                  constraints: const BoxConstraints(maxHeight: 100),
                  decoration: BoxDecoration(
                    color: Colors.grey.shade100,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: TextField(
                    controller: _controller,
                    focusNode: _focusNode,
                    maxLines: null,
                    maxLength: widget.maxLength,
                    decoration: InputDecoration(
                      hintText: widget.placeholder ?? '发送消息',
                      hintStyle: TextStyle(
                        color: Colors.grey.shade400,
                        fontSize: 15,
                      ),
                      border: InputBorder.none,
                      contentPadding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 10,
                      ),
                      counterText: '', // 隐藏字符计数
                    ),
                    style: const TextStyle(fontSize: 15),
                    textInputAction: TextInputAction.send,
                    onSubmitted: (_) => _handleSend(),
                  ),
                ),
              ),

              const SizedBox(width: 8),

              // 更多按钮（图片、文件等）
              _buildIconButton(
                icon: Icons.add_circle_outline,
                onTap: widget.onMoreTap,
              ),

              // 发送按钮
              if (_hasText)
                Padding(
                  padding: const EdgeInsets.only(left: 4),
                  child: Material(
                    color: const Color(0xFFf4835a),
                    borderRadius: BorderRadius.circular(18),
                    child: InkWell(
                      onTap: _handleSend,
                      borderRadius: BorderRadius.circular(18),
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 16,
                          vertical: 8,
                        ),
                        child: const Text(
                          '发送',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 14,
                            fontWeight: FontWeight.w500,
                          ),
                        ),
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

  Widget _buildIconButton({required IconData icon, VoidCallback? onTap}) {
    return IconButton(
      icon: Icon(icon),
      iconSize: 26,
      color: Colors.grey.shade600,
      onPressed: onTap,
    );
  }
}

/// 更多功能面板
class MoreActionsPanel extends StatelessWidget {
  final VoidCallback? onImagePick;
  final VoidCallback? onFilePick;
  final VoidCallback? onCameraPick;

  const MoreActionsPanel({
    super.key,
    this.onImagePick,
    this.onFilePick,
    this.onCameraPick,
  });

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 120,
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 16),
      decoration: BoxDecoration(
        color: Colors.grey.shade50,
        border: Border(top: BorderSide(color: Colors.grey.shade200)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _buildActionItem(icon: Icons.image, label: '图片', onTap: onImagePick),
          _buildActionItem(
            icon: Icons.camera_alt,
            label: '拍照',
            onTap: onCameraPick,
          ),
          _buildActionItem(
            icon: Icons.insert_drive_file,
            label: '文件',
            onTap: onFilePick,
          ),
        ],
      ),
    );
  }

  Widget _buildActionItem({
    required IconData icon,
    required String label,
    VoidCallback? onTap,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            width: 50,
            height: 50,
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(10),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.05),
                  blurRadius: 4,
                  offset: const Offset(0, 1),
                ),
              ],
            ),
            child: Icon(icon, color: const Color(0xFFf4835a), size: 28),
          ),
          const SizedBox(height: 8),
          Text(
            label,
            style: TextStyle(fontSize: 12, color: Colors.grey.shade700),
          ),
        ],
      ),
    );
  }
}
