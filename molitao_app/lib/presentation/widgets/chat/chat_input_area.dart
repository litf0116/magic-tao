import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:molitao_app/presentation/widgets/chat/emoji_picker.dart';
import '../../../data/models/chat_emoji_model.dart';

/// 聊天输入区域组件
/// 包含：输入框、表情按钮、更多按钮、发送按钮
class ChatInputArea extends StatefulWidget {
  /// 发送文本消息回调
  final void Function(String text)? onSendText;

  /// 选择图片回调
  final VoidCallback? onSelectImage;

  /// 选择表情回调
  final void Function(String emojiCode)? onSelectEmoji;

  /// 选择收藏表情回调（发送收藏的图片表情）
  final void Function(ChatEmojiDto emoji)? onSelectFavoriteEmoji;

  /// 是否显示收藏标签
  final bool showFavoriteTab;

  /// 输入框占位符
  final String placeholder;

  /// 最大输入长度
  final int maxLength;

  const ChatInputArea({
    Key? key,
    this.onSendText,
    this.onSelectImage,
    this.onSelectEmoji,
    this.onSelectFavoriteEmoji,
    this.showFavoriteTab = false,
    this.placeholder = '发送消息',
    this.maxLength = 700,
  }) : super(key: key);

  @override
  State<ChatInputArea> createState() => _ChatInputAreaState();
}

class _ChatInputAreaState extends State<ChatInputArea> {
  final TextEditingController _textController = TextEditingController();
  final FocusNode _focusNode = FocusNode();

  bool _showSendButton = false;
  bool _showEmojiPanel = false;
  bool _showMorePanel = false;

  @override
  void initState() {
    super.initState();
    _textController.addListener(_onTextChanged);
  }

  @override
  void dispose() {
    _textController.removeListener(_onTextChanged);
    _textController.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  void _onTextChanged() {
    final hasText = _textController.text.trim().isNotEmpty;
    if (hasText != _showSendButton) {
      setState(() {
        _showSendButton = hasText;
      });
    }
  }

  void _sendTextMessage() {
    final text = _textController.text.trim();
    if (text.isEmpty) return;

    widget.onSendText?.call(text);
    _textController.clear();
    setState(() {
      _showSendButton = false;
    });
  }

  void _toggleEmojiPanel() {
    setState(() {
      _showEmojiPanel = !_showEmojiPanel;
      _showMorePanel = false;

      if (_showEmojiPanel) {
        // 显示表情面板时，收起键盘
        _focusNode.unfocus();
      } else {
        // 关闭表情面板时，弹出键盘
        _focusNode.requestFocus();
      }
    });
  }

  void _toggleMorePanel() {
    setState(() {
      _showMorePanel = !_showMorePanel;
      _showEmojiPanel = false;

      if (_showMorePanel) {
        _focusNode.unfocus();
      }
    });
  }

  void _selectEmoji(String emojiCode) {
    final text = _textController.text;
    final selection = _textController.selection;
    final newText = text.replaceRange(
      selection.start,
      selection.end,
      emojiCode,
    );

    _textController.text = newText;
    _textController.selection = TextSelection.collapsed(
      offset: selection.start + emojiCode.length,
    );

    widget.onSelectEmoji?.call(emojiCode);
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: Color(0xFFF6F6F6),
        border: Border(top: BorderSide(color: Color(0xFFECECEC), width: 1)),
      ),
      child: SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // 输入行
            _buildInputRow(),

            // 表情面板
            if (_showEmojiPanel) _buildEmojiPanel(),

            // 更多面板
            if (_showMorePanel) _buildMorePanel(),
          ],
        ),
      ),
    );
  }

  Widget _buildInputRow() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      child: Row(
        children: [
          // 输入框
          Expanded(
            child: Container(
              height: 40,
              padding: const EdgeInsets.symmetric(horizontal: 12),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(6),
              ),
              child: TextField(
                controller: _textController,
                focusNode: _focusNode,
                maxLength: widget.maxLength,
                maxLengthEnforcement: MaxLengthEnforcement.enforced,
                decoration: InputDecoration(
                  hintText: widget.placeholder,
                  hintStyle: const TextStyle(
                    color: Color(0xFF999999),
                    fontSize: 14,
                  ),
                  border: InputBorder.none,
                  counterText: '', // 隐藏字数统计
                  contentPadding: const EdgeInsets.symmetric(vertical: 10),
                ),
                style: const TextStyle(fontSize: 14),
                textInputAction: TextInputAction.send,
                onSubmitted: (_) => _sendTextMessage(),
                onTap: () {
                  // 点击输入框时，关闭表情和更多面板
                  if (_showEmojiPanel || _showMorePanel) {
                    setState(() {
                      _showEmojiPanel = false;
                      _showMorePanel = false;
                    });
                  }
                },
              ),
            ),
          ),

          const SizedBox(width: 8),

          // 表情按钮
          GestureDetector(
            onTap: _toggleEmojiPanel,
            child: Container(
              width: 32,
              height: 32,
              padding: const EdgeInsets.all(4),
              child: Icon(
                _showEmojiPanel
                    ? Icons.keyboard
                    : Icons.emoji_emotions_outlined,
                size: 24,
                color: const Color(0xFF666666),
              ),
            ),
          ),

          const SizedBox(width: 4),

          // 更多按钮
          GestureDetector(
            onTap: _toggleMorePanel,
            child: Container(
              width: 32,
              height: 32,
              padding: const EdgeInsets.all(4),
              child: const Icon(
                Icons.add_circle_outline,
                size: 24,
                color: Color(0xFF666666),
              ),
            ),
          ),

          // 发送按钮（仅在有文本时显示）
          if (_showSendButton) ...[
            const SizedBox(width: 8),
            GestureDetector(
              onTap: _sendTextMessage,
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 16,
                  vertical: 8,
                ),
                decoration: BoxDecoration(
                  color: const Color(0xFFF4835A),
                  borderRadius: BorderRadius.circular(6),
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
          ],
        ],
      ),
    );
  }

  Widget _buildEmojiPanel() {
    return SizedBox(
      height: 200,
      child: EmojiPicker(
        onEmojiSelected: _selectEmoji,
        onFavoriteEmojiSelected: widget.onSelectFavoriteEmoji,
        showFavoriteTab: widget.showFavoriteTab,
      ),
    );
  }

  Widget _buildMorePanel() {
    return Container(
      height: 120,
      color: Colors.white,
      padding: const EdgeInsets.all(16),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // 图片
          _buildMoreItem(
            icon: Icons.image_outlined,
            label: '图片',
            onTap: () {
              widget.onSelectImage?.call();
              setState(() {
                _showMorePanel = false;
              });
            },
          ),
        ],
      ),
    );
  }

  Widget _buildMoreItem({
    required IconData icon,
    required String label,
    VoidCallback? onTap,
  }) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 60,
        margin: const EdgeInsets.only(right: 20),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 50,
              height: 50,
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(8),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withOpacity(0.05),
                    blurRadius: 4,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: Icon(icon, size: 28, color: const Color(0xFF666666)),
            ),
            const SizedBox(height: 8),
            Text(
              label,
              style: const TextStyle(fontSize: 12, color: Color(0xFF666666)),
            ),
          ],
        ),
      ),
    );
  }
}
