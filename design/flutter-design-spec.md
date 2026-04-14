# 魔力淘 Flutter 设计规范

> 基于 HTML 设计稿和 UniApp 消息样式规范，为 Flutter 开发提供直接可用的设计规范。

---

## 一、设计 Token

### 1.1 颜色 (Color)

```dart
import 'package:flutter/material.dart';

class AppColors {
  // 主题色
  static const Color primary = Color(0xFFF4835a);
  static const Color primaryLight = Color(0xFFff7144);
  
  // 功能色
  static const Color success = Color(0xFF52c41a);
  static const Color warning = Color(0xFFfa8c16);
  static const Color info = Color(0xFF1890ff);
  static const Color error = Color(0xFFef4444);
  
  // 背景色
  static const Color background = Color(0xFFf6f6f6);
  static const Color surface = Color(0xFFffffff);
  static const Color surfaceVariant = Color(0xFFf5f5f5);
  
  // 文字色
  static const Color textPrimary = Color(0xFF333333);
  static const Color textSecondary = Color(0xFF999999);
  static const Color textHint = Color(0xFFcccccc);
  static const Color textOnPrimary = Color(0xFFffffff);
  
  // 边框色
  static const Color border = Color(0xFFe5e5e5);
  static const Color divider = Color(0xFFeeeeee);
  
  // === 消息相关颜色 ===
  
  // 出价消息
  static const Color msgBidBorder = Color(0xFFff7144);
  static const Color msgBidBackground = Color(0xFFffb673);
  
  // 开始秒杀消息
  static const Color msgStartBorder = Color(0xFFef4444);
  static const Color msgStartBackground = Color(0xFFfff5f5);
  
  // 秒杀结束消息
  static const Color msgEndBorder = Color(0xFFff9800);
  static const Color msgEndBackground = Color(0xFFffb673);
  
  // 成交消息
  static const Color msgDealBorder = Color(0xFF22c55e);
  static const Color msgDealBackground = Color(0xFF86efac);
  
  // 卡秒开启
  static const Color msgKasecEnabledBorder = Color(0xFFe53e3e);
  static const Color msgKasecEnabledBgStart = Color(0xFFfff5f5);
  static const Color msgKasecEnabledBgEnd = Color(0xFFfed7d7);
  
  // 卡秒关闭
  static const Color msgKasecDisabledBorder = Color(0xFF38a169);
  static const Color msgKasecDisabledBgStart = Color(0xFFf0fff4);
  static const Color msgKasecDisabledBgEnd = Color(0xFFc6f6d5);
  
  // 秒杀场商品颜色
  static const Color auctionItemText = Color(0xFF935F4E);
}
```

### 1.2 字体 (Typography)

```dart
class AppTextStyles {
  // 标题
  static const TextStyle heading1 = TextStyle(
    fontSize: 32,
    fontWeight: FontWeight.w700,
    color: AppColors.textPrimary,
  );
  
  static const TextStyle heading2 = TextStyle(
    fontSize: 24,
    fontWeight: FontWeight.w600,
    color: AppColors.textPrimary,
  );
  
  static const TextStyle heading3 = TextStyle(
    fontSize: 18,
    fontWeight: FontWeight.w600,
    color: AppColors.textPrimary,
  );
  
  // 正文
  static const TextStyle body1 = TextStyle(
    fontSize: 16,
    fontWeight: FontWeight.w400,
    color: AppColors.textPrimary,
  );
  
  static const TextStyle body2 = TextStyle(
    fontSize: 14,
    fontWeight: FontWeight.w400,
    color: AppColors.textPrimary,
  );
  
  static const TextStyle body3 = TextStyle(
    fontSize: 13,
    fontWeight: FontWeight.w400,
    color: AppColors.textPrimary,
  );
  
  // 辅助文字
  static const TextStyle caption = TextStyle(
    fontSize: 12,
    fontWeight: FontWeight.w400,
    color: AppColors.textSecondary,
  );
  
  static const TextStyle small = TextStyle(
    fontSize: 11,
    fontWeight: FontWeight.w400,
    color: AppColors.textSecondary,
  );
  
  // 导航栏标题
  static const TextStyle navTitle = TextStyle(
    fontSize: 17,
    fontWeight: FontWeight.w600,
    color: AppColors.textOnPrimary,
  );
  
  // === 消息相关字体 ===
  
  // 出价价格
  static const TextStyle bidPrice = TextStyle(
    fontSize: 24,
    fontWeight: FontWeight.w700,
    color: Colors.white,
  );
  
  // 商品名称
  static const TextStyle productName = TextStyle(
    fontSize: 13,
    fontWeight: FontWeight.w400,
    color: AppColors.textPrimary,
  );
  
  // 消息标签
  static const TextStyle messageTag = TextStyle(
    fontSize: 12,
    fontWeight: FontWeight.w500,
    color: Colors.white,
  );
}
```

### 1.3 间距 (Spacing)

```dart
class AppSpacing {
  // 基础间距
  static const double xs = 4.0;
  static const double sm = 8.0;
  static const double md = 12.0;
  static const double lg = 16.0;
  static const double xl = 20.0;
  static const double xxl = 24.0;
  
  // 页面边距
  static const double pagePadding = 15.0;
  
  // 卡片内边距
  static const double cardPadding = 12.0;
  
  // 列表项间距
  static const double listItemGap = 10.0;
  
  // 消息间距
  static const double messageGap = 15.0;
}
```

### 1.4 圆角 (Radius)

```dart
class AppRadius {
  static const double xs = 4.0;
  static const double sm = 6.0;
  static const double md = 8.0;
  static const double lg = 12.0;
  static const double xl = 16.0;
  static const double xxl = 20.0;
  
  // 圆形
  static const double circle = 999.0;
  
  // 卡片圆角
  static BorderRadius get card => BorderRadius.circular(lg);
  
  // 按钮圆角
  static BorderRadius get button => BorderRadius.circular(22.0);
  
  // 输入框圆角
  static BorderRadius get input => BorderRadius.circular(18.0);
  
  // 消息气泡圆角
  static BorderRadius get messageBubble => BorderRadius.circular(6.0);
  
  // 消息卡片圆角
  static BorderRadius get messageCard => BorderRadius.circular(md);
}
```

### 1.5 阴影 (Shadow)

```dart
class AppShadows {
  static List<BoxShadow> get card => [
    BoxShadow(
      color: Colors.black.withOpacity(0.05),
      blurRadius: 10,
      offset: const Offset(0, 2),
    ),
  ];
  
  static List<BoxShadow> get button => [
    BoxShadow(
      color: Colors.black.withOpacity(0.1),
      blurRadius: 8,
      offset: const Offset(0, 2),
    ),
  ];
  
  static List<BoxShadow> get modal => [
    BoxShadow(
      color: Colors.black.withOpacity(0.15),
      blurRadius: 20,
      offset: const Offset(0, 4),
    ),
  ];
}
```

---

## 二、基础组件

### 2.1 主按钮 (PrimaryButton)

```dart
class PrimaryButton extends StatelessWidget {
  final String text;
  final VoidCallback? onPressed;
  final bool loading;
  final bool enabled;
  
  const PrimaryButton({
    super.key,
    required this.text,
    this.onPressed,
    this.loading = false,
    this.enabled = true,
  });
  
  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: enabled && !loading ? onPressed : null,
      child: Container(
        height: 44,
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [AppColors.primary, AppColors.primaryLight],
          ),
          borderRadius: AppRadius.button,
          boxShadow: enabled ? AppShadows.button : null,
        ),
        child: Center(
          child: loading
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    valueColor: AlwaysStoppedAnimation(Colors.white),
                  ),
                )
              : Text(
                  text,
                  style: AppTextStyles.body1.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w500,
                  ),
                ),
        ),
      ),
    );
  }
}
```

### 2.2 次要按钮 (SecondaryButton)

```dart
class SecondaryButton extends StatelessWidget {
  final String text;
  final VoidCallback? onPressed;
  
  const SecondaryButton({
    super.key,
    required this.text,
    this.onPressed,
  });
  
  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onPressed,
      child: Container(
        height: 44,
        decoration: BoxDecoration(
          color: AppColors.surfaceVariant,
          borderRadius: AppRadius.button,
        ),
        child: Center(
          child: Text(
            text,
            style: AppTextStyles.body1.copyWith(
              fontWeight: FontWeight.w500,
            ),
          ),
        ),
      ),
    );
  }
}
```

### 2.3 卡片容器 (AppCard)

```dart
class AppCard extends StatelessWidget {
  final Widget child;
  final EdgeInsetsGeometry? padding;
  final Color? backgroundColor;
  final VoidCallback? onTap;
  
  const AppCard({
    super.key,
    required this.child,
    this.padding,
    this.backgroundColor,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: padding ?? const EdgeInsets.all(AppSpacing.cardPadding),
        decoration: BoxDecoration(
          color: backgroundColor ?? AppColors.surface,
          borderRadius: AppRadius.card,
          boxShadow: AppShadows.card,
        ),
        child: child,
      ),
    );
  }
}
```

### 2.4 输入框 (AppTextField)

```dart
class AppTextField extends StatelessWidget {
  final String? placeholder;
  final TextEditingController? controller;
  final bool obscureText;
  final TextInputType? keyboardType;
  final ValueChanged<String>? onChanged;
  final VoidCallback? onEditingComplete;
  
  const AppTextField({
    super.key,
    this.placeholder,
    this.controller,
    this.obscureText = false,
    this.keyboardType,
    this.onChanged,
    this.onEditingComplete,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      height: 44,
      padding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
      decoration: BoxDecoration(
        color: AppColors.surfaceVariant,
        borderRadius: AppRadius.input,
      ),
      child: TextField(
        controller: controller,
        obscureText: obscureText,
        keyboardType: keyboardType,
        onChanged: onChanged,
        onEditingComplete: onEditingComplete,
        style: AppTextStyles.body1,
        decoration: InputDecoration(
          hintText: placeholder,
          hintStyle: AppTextStyles.body1.copyWith(color: AppColors.textHint),
          border: InputBorder.none,
          contentPadding: EdgeInsets.zero,
          isDense: true,
        ),
      ),
    );
  }
}
```

### 2.5 导航栏 (AppNavBar)

```dart
class AppNavBar extends StatelessWidget implements PreferredSizeWidget {
  final String title;
  final bool showBack;
  final String? actionText;
  final VoidCallback? onBack;
  final VoidCallback? onAction;
  final bool light;
  
  const AppNavBar({
    super.key,
    required this.title,
    this.showBack = true,
    this.actionText,
    this.onBack,
    this.onAction,
    this.light = false,
  });
  
  @override
  Size get preferredSize => const Size.fromHeight(44);
  
  @override
  Widget build(BuildContext context) {
    final bgColor = light ? AppColors.surface : AppColors.primary;
    final textColor = light ? AppColors.textPrimary : AppColors.textOnPrimary;
    
    return Container(
      height: 44,
      color: bgColor,
      child: Stack(
        children: [
          // 返回按钮
          if (showBack)
            Positioned(
              left: 15,
              top: 0,
              bottom: 0,
              child: GestureDetector(
                onTap: onBack ?? () => Navigator.pop(context),
                child: Center(
                  child: Text('←', style: TextStyle(fontSize: 20, color: textColor)),
                ),
              ),
            ),
          // 标题
          Center(
            child: Text(
              title,
              style: AppTextStyles.navTitle.copyWith(color: textColor),
            ),
          ),
          // 右侧操作按钮
          if (actionText != null)
            Positioned(
              right: 15,
              top: 0,
              bottom: 0,
              child: GestureDetector(
                onTap: onAction,
                child: Center(
                  child: Text(
                    actionText!,
                    style: TextStyle(fontSize: 14, color: textColor),
                  ),
                ),
              ),
            ),
        ],
      ),
    );
  }
}
```

### 2.6 底部标签栏 (AppTabBar)

```dart
class AppTabBar extends StatelessWidget {
  final int currentIndex;
  final ValueChanged<int>? onTap;
  
  const AppTabBar({
    super.key,
    required this.currentIndex,
    this.onTap,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      height: 83,
      decoration: const BoxDecoration(
        color: AppColors.surface,
        border: Border(top: BorderSide(color: AppColors.border)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: [
          _buildTabItem(0, '🏠', '首页'),
          _buildTabItem(1, '💬', '会话'),
          _buildTabItem(2, '👥', '通讯录'),
          _buildTabItem(3, '👤', '我的'),
        ],
      ),
    );
  }
  
  Widget _buildTabItem(int index, String icon, String label) {
    final isSelected = currentIndex == index;
    return GestureDetector(
      onTap: () => onTap?.call(index),
      behavior: HitTestBehavior.opaque,
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(icon, style: const TextStyle(fontSize: 22)),
          const SizedBox(height: 4),
          Text(
            label,
            style: AppTextStyles.small.copyWith(
              color: isSelected ? AppColors.primary : AppColors.textSecondary,
            ),
          ),
        ],
      ),
    );
  }
}
```

---

## 三、消息组件

### 3.1 消息类型枚举

```dart
enum ChatMessageType {
  text,
  image,
  auctionStart,
  auctionBid,
  auctionEnd,
  auctionDeal,
  kasecStatus,
  welcome,
  goodbye,
  system,
}
```

### 3.2 消息数据模型

```dart
class ChatMessage {
  final String id;
  final ChatMessageType type;
  final String? senderId;
  final String? senderName;
  final String? senderAvatar;
  final bool isSelf;
  final DateTime? timestamp;
  final Map<String, dynamic> data;
  
  const ChatMessage({
    required this.id,
    required this.type,
    this.senderId,
    this.senderName,
    this.senderAvatar,
    this.isSelf = false,
    this.timestamp,
    this.data = const {},
  });
}
```

### 3.3 消息气泡基类

```dart
class MessageBubble extends StatelessWidget {
  final Widget child;
  final bool isSelf;
  
  const MessageBubble({
    super.key,
    required this.child,
    this.isSelf = false,
  });
  
  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: isSelf ? MainAxisAlignment.end : MainAxisAlignment.start,
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (!isSelf) ...[
          // 头像
          _buildAvatar(),
          const SizedBox(width: 10),
        ],
        Flexible(child: child),
        if (isSelf) ...[
          const SizedBox(width: 10),
          // 头像
          _buildAvatar(),
        ],
      ],
    );
  }
  
  Widget _buildAvatar() {
    return Container(
      width: 36,
      height: 36,
      decoration: BoxDecoration(
        color: AppColors.primary,
        borderRadius: BorderRadius.circular(18),
      ),
      child: const Center(
        child: Text('头', style: TextStyle(color: Colors.white, fontSize: 12)),
      ),
    );
  }
}
```

### 3.4 文本消息

```dart
class TextMessage extends StatelessWidget {
  final String content;
  final bool isSelf;
  
  const TextMessage({
    super.key,
    required this.content,
    this.isSelf = false,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: AppRadius.messageBubble,
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.03),
            blurRadius: 4,
            offset: const Offset(0, 1),
          ),
        ],
      ),
      child: Text(
        content,
        style: AppTextStyles.body2,
      ),
    );
  }
}
```

### 3.5 出价消息 (AuctionBidMessage)

```dart
class AuctionBidMessage extends StatelessWidget {
  final String productName;
  final int price;
  final String? senderName;
  
  const AuctionBidMessage({
    super.key,
    required this.productName,
    required this.price,
    this.senderName,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: const BoxConstraints(minWidth: 200, maxWidth: 280),
      decoration: BoxDecoration(
        color: AppColors.msgBidBackground,
        border: Border.all(color: AppColors.msgBidBorder, width: 2),
        borderRadius: AppRadius.messageCard,
      ),
      child: Stack(
        children: [
          // 右上角标签
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: const BoxDecoration(
                color: AppColors.msgBidBorder,
                borderRadius: BorderRadius.only(
                  topRight: Radius.circular(6),
                  bottomLeft: Radius.circular(8),
                ),
              ),
              child: Text('出价', style: AppTextStyles.messageTag),
            ),
          ),
          // 内容
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('商品: $productName', style: AppTextStyles.productName),
                const SizedBox(height: 4),
                Text('当前出价：￥$price', style: AppTextStyles.bidPrice),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
```

### 3.6 开始秒杀消息 (AuctionStartMessage)

```dart
class AuctionStartMessage extends StatelessWidget {
  final String productName;
  final String? description;
  
  const AuctionStartMessage({
    super.key,
    required this.productName,
    this.description,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: const BoxConstraints(minWidth: 200, maxWidth: 280),
      decoration: BoxDecoration(
        color: AppColors.msgStartBackground,
        border: Border.all(color: AppColors.msgStartBorder, width: 2),
        borderRadius: AppRadius.messageCard,
      ),
      child: Stack(
        children: [
          // 右上角标签
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: BoxDecoration(
                color: AppColors.msgStartBorder,
                borderRadius: const BorderRadius.only(
                  topRight: Radius.circular(6),
                  bottomLeft: Radius.circular(8),
                ),
              ),
              child: Text('开始秒杀', style: AppTextStyles.messageTag),
            ),
          ),
          // 内容
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('商品名称: $productName', style: AppTextStyles.productName),
                if (description != null) ...[
                  const SizedBox(height: 4),
                  Text(description!, style: AppTextStyles.caption),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}
```

### 3.7 秒杀结束消息 (AuctionEndMessage)

```dart
class AuctionEndMessage extends StatelessWidget {
  final String productName;
  final String winnerName;
  final int finalPrice;
  final DateTime dealTime;
  
  const AuctionEndMessage({
    super.key,
    required this.productName,
    required this.winnerName,
    required this.finalPrice,
    required this.dealTime,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: const BoxConstraints(minWidth: 200, maxWidth: 280),
      decoration: BoxDecoration(
        border: Border.all(color: AppColors.msgEndBorder, width: 2),
        borderRadius: AppRadius.messageCard,
      ),
      child: Stack(
        children: [
          // 右上角标签
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: BoxDecoration(
                color: AppColors.msgEndBorder,
                borderRadius: const BorderRadius.only(
                  topRight: Radius.circular(6),
                  bottomLeft: Radius.circular(8),
                ),
              ),
              child: Text('成功秒杀', style: AppTextStyles.messageTag),
            ),
          ),
          // 内容
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                RichText(
                  text: TextSpan(
                    style: AppTextStyles.body2,
                    children: [
                      const TextSpan(text: '恭喜 '),
                      TextSpan(
                        text: winnerName,
                        style: const TextStyle(fontWeight: FontWeight.bold),
                      ),
                      const TextSpan(text: ' 最终以 '),
                      TextSpan(
                        text: '￥$finalPrice',
                        style: const TextStyle(
                          color: AppColors.error,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      const TextSpan(text: ' 秒得商品'),
                    ],
                  ),
                ),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: AppColors.msgEndBackground,
                    border: Border.all(color: AppColors.msgEndBorder),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(productName, style: AppTextStyles.body3),
                ),
                const SizedBox(height: 4),
                Text(
                  '${dealTime.year}-${dealTime.month.toString().padLeft(2, '0')}-${dealTime.day.toString().padLeft(2, '0')} '
                  '${dealTime.hour.toString().padLeft(2, '0')}:${dealTime.minute.toString().padLeft(2, '0')}:${dealTime.second.toString().padLeft(2, '0')}',
                  style: AppTextStyles.small,
                ),
                const SizedBox(height: 4),
                Text(
                  '双方私聊秒杀主持确认交易!\n认准星标小心冒充\n有请下一件拍品',
                  style: AppTextStyles.small.copyWith(height: 1.4),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
```

### 3.8 成交通知消息 (AuctionDealMessage)

```dart
class AuctionDealMessage extends StatelessWidget {
  final String productName;
  final int finalPrice;
  final DateTime dealTime;
  
  const AuctionDealMessage({
    super.key,
    required this.productName,
    required this.finalPrice,
    required this.dealTime,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      constraints: const BoxConstraints(minWidth: 200, maxWidth: 280),
      decoration: BoxDecoration(
        border: Border.all(color: AppColors.msgDealBorder, width: 2),
        borderRadius: AppRadius.messageCard,
      ),
      child: Stack(
        children: [
          // 右上角标签
          Positioned(
            top: 0,
            right: 0,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
              decoration: BoxDecoration(
                color: AppColors.msgDealBorder,
                borderRadius: const BorderRadius.only(
                  topRight: Radius.circular(6),
                  bottomLeft: Radius.circular(8),
                ),
              ),
              child: Text('交易通知', style: AppTextStyles.messageTag),
            ),
          ),
          // 内容
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 8, 16, 12),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  '🎉 恭喜您成功拍得商品！',
                  style: AppTextStyles.body2.copyWith(color: AppColors.success),
                ),
                const SizedBox(height: 8),
                Container(
                  padding: const EdgeInsets.all(8),
                  decoration: BoxDecoration(
                    color: AppColors.msgDealBackground,
                    border: Border.all(color: AppColors.msgDealBorder),
                    borderRadius: BorderRadius.circular(4),
                  ),
                  child: Text(productName, style: AppTextStyles.body3),
                ),
                const SizedBox(height: 4),
                Text(
                  '成交价: ￥$finalPrice',
                  style: AppTextStyles.body2.copyWith(
                    color: AppColors.error,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  '${dealTime.year}-${dealTime.month.toString().padLeft(2, '0')}-${dealTime.day.toString().padLeft(2, '0')} '
                  '${dealTime.hour.toString().padLeft(2, '0')}:${dealTime.minute.toString().padLeft(2, '0')}:${dealTime.second.toString().padLeft(2, '0')}',
                  style: AppTextStyles.small,
                ),
                const SizedBox(height: 4),
                Text(
                  '请联系秒杀主持确认交易详情\n认准星标，小心冒充\n感谢您的参与！',
                  style: AppTextStyles.small.copyWith(height: 1.4),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
```

### 3.9 卡秒状态消息 (KasecStatusMessage)

```dart
class KasecStatusMessage extends StatelessWidget {
  final bool enabled;
  final String? message;
  
  const KasecStatusMessage({
    super.key,
    required this.enabled,
    this.message,
  });
  
  @override
  Widget build(BuildContext context) {
    final borderClr = enabled ? AppColors.msgKasecEnabledBorder : AppColors.msgKasecDisabledBorder;
    final textClr = enabled ? const Color(0xFFc53030) : const Color(0xFF2f855a);
    
    return Container(
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: enabled
              ? [AppColors.msgKasecEnabledBgStart, AppColors.msgKasecEnabledBgEnd]
              : [AppColors.msgKasecDisabledBgStart, AppColors.msgKasecDisabledBgEnd],
        ),
        border: Border.all(color: borderClr, width: 2),
        borderRadius: BorderRadius.circular(10),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          Text('⚡', style: const TextStyle(fontSize: 18)),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              message ?? (enabled ? '秒杀主持已开启卡秒模式，需三倍加价！' : '卡秒已关闭，恢复正常加价'),
              style: AppTextStyles.body2.copyWith(
                color: textClr,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
```

### 3.10 消息工厂 (MessageFactory)

```dart
class MessageFactory extends StatelessWidget {
  final ChatMessage message;
  
  const MessageFactory({super.key, required this.message});
  
  @override
  Widget build(BuildContext context) {
    Widget content;
    
    switch (message.type) {
      case ChatMessageType.text:
        content = TextMessage(
          content: message.data['content'] ?? '',
          isSelf: message.isSelf,
        );
        break;
        
      case ChatMessageType.auctionBid:
        content = AuctionBidMessage(
          productName: message.data['productName'] ?? '',
          price: message.data['price'] ?? 0,
          senderName: message.senderName,
        );
        break;
        
      case ChatMessageType.auctionStart:
        content = AuctionStartMessage(
          productName: message.data['productName'] ?? '',
          description: message.data['description'],
        );
        break;
        
      case ChatMessageType.auctionEnd:
        content = AuctionEndMessage(
          productName: message.data['productName'] ?? '',
          winnerName: message.data['winnerName'] ?? '',
          finalPrice: message.data['finalPrice'] ?? 0,
          dealTime: message.timestamp ?? DateTime.now(),
        );
        break;
        
      case ChatMessageType.auctionDeal:
        content = AuctionDealMessage(
          productName: message.data['productName'] ?? '',
          finalPrice: message.data['finalPrice'] ?? 0,
          dealTime: message.timestamp ?? DateTime.now(),
        );
        break;
        
      case ChatMessageType.kasecStatus:
        content = KasecStatusMessage(
          enabled: message.data['enabled'] ?? false,
          message: message.data['message'],
        );
        break;
        
      default:
        content = TextMessage(content: '未知消息类型');
    }
    
    return MessageBubble(
      isSelf: message.isSelf,
      child: content,
    );
  }
}
```

---

## 四、页面结构

### 4.1 页面基础结构

```dart
/// 标准页面结构
/// 
/// - 状态栏高度: 44pt
/// - 导航栏高度: 44pt
/// - 底部 TabBar 高度: 83pt
/// - 安全区域底部: 34pt (iPhone X 及以上)
class PageScaffold extends StatelessWidget {
  final String title;
  final bool showTabBar;
  final int tabBarIndex;
  final bool showBack;
  final String? actionText;
  final VoidCallback? onAction;
  final Widget body;
  final bool lightNavBar;
  final Color? backgroundColor;
  
  const PageScaffold({
    super.key,
    required this.title,
    this.showTabBar = true,
    this.tabBarIndex = 0,
    this.showBack = false,
    this.actionText,
    this.onAction,
    required this.body,
    this.lightNavBar = false,
    this.backgroundColor,
  });
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: backgroundColor ?? AppColors.background,
      body: Column(
        children: [
          // 状态栏
          Container(
            height: 44,
            color: lightNavBar ? AppColors.surface : AppColors.primary,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Text(
                    '9:41', // 实际使用时应获取系统时间
                    style: TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: lightNavBar ? AppColors.textPrimary : Colors.white,
                    ),
                  ),
                  Row(
                    children: [
                      Text('📶', style: TextStyle(fontSize: 14)),
                      SizedBox(width: 5),
                      Text('🔋', style: TextStyle(fontSize: 14)),
                    ],
                  ),
                ],
              ),
            ),
          ),
          // 导航栏
          AppNavBar(
            title: title,
            showBack: showBack,
            actionText: actionText,
            onAction: onAction,
            light: lightNavBar,
          ),
          // 内容区域
          Expanded(child: body),
          // 底部 TabBar
          if (showTabBar) AppTabBar(currentIndex: tabBarIndex),
        ],
      ),
    );
  }
}
```

### 4.2 屏幕尺寸常量

```dart
class ScreenSize {
  /// 设计稿宽度
  static const double designWidth = 375.0;
  
  /// 设计稿高度
  static const double designHeight = 812.0;
  
  /// 状态栏高度
  static const double statusBarHeight = 44.0;
  
  /// 导航栏高度
  static const double navBarHeight = 44.0;
  
  /// TabBar 高度
  static const double tabBarHeight = 83.0;
  
  /// 安全区域底部高度
  static const double safeAreaBottom = 34.0;
  
  /// 内容区域高度（有 TabBar）
  static double contentHeightWithTabBar = designHeight - statusBarHeight - navBarHeight - tabBarHeight;
  
  /// 内容区域高度（无 TabBar）
  static double contentHeightWithoutTab = designHeight - statusBarHeight - navBarHeight;
}
```

---

## 五、弹窗组件

### 5.1 拍品详情弹窗

```dart
class ItemDetailPopup extends StatelessWidget {
  final String productName;
  final int currentPrice;
  final String? description;
  final VoidCallback? onClose;
  final VoidCallback? onBid;
  
  const ItemDetailPopup({
    super.key,
    required this.productName,
    required this.currentPrice,
    this.description,
    this.onClose,
    this.onBid,
  });
  
  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          // 标题栏
          Padding(
            padding: const EdgeInsets.all(16),
            child: Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text('拍品详情', style: AppTextStyles.heading3),
                GestureDetector(
                  onTap: onClose,
                  child: Text('×', style: TextStyle(fontSize: 24, color: AppColors.textSecondary)),
                ),
              ],
            ),
          ),
          const Divider(height: 1),
          // 商品图片
          Container(
            height: 180,
            margin: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: AppColors.surfaceVariant,
              borderRadius: BorderRadius.circular(12),
            ),
            child: const Center(
              child: Text('🖼️ 商品图片', style: AppTextStyles.caption),
            ),
          ),
          // 商品信息
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(productName, style: AppTextStyles.body1.copyWith(color: AppColors.primary)),
                const SizedBox(height: 8),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text('当前价格', style: AppTextStyles.body2.copyWith(color: AppColors.textSecondary)),
                    Text('￥$currentPrice', style: AppTextStyles.heading2.copyWith(color: AppColors.error)),
                  ],
                ),
              ],
            ),
          ),
          // 商品描述
          if (description != null)
            Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('商品描述', style: AppTextStyles.body1.copyWith(fontWeight: FontWeight.w600)),
                  const SizedBox(height: 8),
                  Text(description!, style: AppTextStyles.body3),
                ],
              ),
            ),
          // 提示
          Container(
            margin: const EdgeInsets.symmetric(horizontal: 16),
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: const Color(0xFFfff7e6),
              borderRadius: BorderRadius.circular(8),
            ),
            child: Row(
              children: [
                Text('💡', style: const TextStyle(fontSize: 16)),
                const SizedBox(width: 8),
                Text('点击图片可查看大图', style: AppTextStyles.small.copyWith(color: AppColors.warning)),
              ],
            ),
          ),
          const SizedBox(height: 16),
          // 底部按钮
          Padding(
            padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
            child: Row(
              children: [
                Expanded(
                  child: SecondaryButton(text: '关闭', onPressed: onClose),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: PrimaryButton(text: '立即出价', onPressed: onBid),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
  
  /// 显示弹窗
  static Future<void> show(BuildContext context, {
    required String productName,
    required int currentPrice,
    String? description,
    VoidCallback? onBid,
  }) {
    return showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (context) => ItemDetailPopup(
        productName: productName,
        currentPrice: currentPrice,
        description: description,
        onBid: onBid,
      ),
    );
  }
}
```

### 5.2 出价输入弹窗

```dart
class BidInputPopup extends StatelessWidget {
  final int minPrice;
  final int? currentPrice;
  final ValueChanged<int>? onConfirm;
  final VoidCallback? onCancel;
  
  const BidInputPopup({
    super.key,
    required this.minPrice,
    this.currentPrice,
    this.onConfirm,
    this.onCancel,
  });
  
  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('提示', style: AppTextStyles.heading3),
            const SizedBox(height: 12),
            Text('请输入您的出价金额', style: AppTextStyles.body2.copyWith(color: AppColors.textSecondary)),
            const SizedBox(height: 16),
            // 输入框
            Container(
              height: 44,
              padding: const EdgeInsets.symmetric(horizontal: 12),
              decoration: BoxDecoration(
                color: Colors.white,
                border: Border.all(color: AppColors.border),
                borderRadius: BorderRadius.circular(8),
              ),
              child: TextField(
                keyboardType: TextInputType.number,
                textAlign: TextAlign.center,
                style: AppTextStyles.body1,
                decoration: InputDecoration(
                  hintText: '￥$minPrice',
                  hintStyle: AppTextStyles.body1.copyWith(color: AppColors.textHint),
                  border: InputBorder.none,
                ),
              ),
            ),
            const SizedBox(height: 20),
            // 按钮
            Row(
              children: [
                Expanded(
                  child: SecondaryButton(text: '取消', onPressed: onCancel ?? () => Navigator.pop(context)),
                ),
                const SizedBox(width: 16),
                Expanded(
                  child: PrimaryButton(
                    text: '确定',
                    onPressed: () => onConfirm?.call(minPrice),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
  
  /// 显示弹窗
  static Future<int?> show(BuildContext context, {
    required int minPrice,
    int? currentPrice,
  }) async {
    final controller = TextEditingController();
    final result = await showDialog<int>(
      context: context,
      builder: (context) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        contentPadding: const EdgeInsets.all(24),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('提示', style: AppTextStyles.heading3),
            const SizedBox(height: 12),
            Text('请输入您的出价金额', style: AppTextStyles.body2.copyWith(color: AppColors.textSecondary)),
            const SizedBox(height: 16),
            Container(
              height: 44,
              padding: const EdgeInsets.symmetric(horizontal: 12),
              decoration: BoxDecoration(
                color: Colors.white,
                border: Border.all(color: AppColors.border),
                borderRadius: BorderRadius.circular(8),
              ),
              child: TextField(
                controller: controller,
                keyboardType: TextInputType.number,
                textAlign: TextAlign.center,
                decoration: InputDecoration(
                  hintText: '￥$minPrice',
                  hintStyle: AppTextStyles.body1.copyWith(color: AppColors.textHint),
                  border: InputBorder.none,
                ),
              ),
            ),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context),
            child: Text('取消', style: AppTextStyles.body1),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, int.tryParse(controller.text)),
            child: Text('确定', style: AppTextStyles.body1.copyWith(color: AppColors.info)),
          ),
        ],
      ),
    );
    return result;
  }
}
```

---

## 六、使用示例

### 6.1 秒杀场页面示例

```dart
class AuctionPage extends StatelessWidget {
  const AuctionPage({super.key});
  
  @override
  Widget build(BuildContext context) {
    return PageScaffold(
      title: '秒杀场',
      showTabBar: false,
      showBack: true,
      actionText: '榜单',
      onAction: () {
        // 跳转到秒杀榜
      },
      body: Column(
        children: [
          // 公告栏
          _buildNoticeBar(),
          // 消息列表
          Expanded(
            child: ListView.builder(
              padding: const EdgeInsets.all(AppSpacing.pagePadding),
              itemCount: messages.length,
              itemBuilder: (context, index) {
                return Padding(
                  padding: const EdgeInsets.only(bottom: AppSpacing.messageGap),
                  child: MessageFactory(message: messages[index]),
                );
              },
            ),
          ),
          // 输入框
          _buildInputArea(),
        ],
      ),
    );
  }
  
  Widget _buildNoticeBar() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 15, vertical: 10),
      color: AppColors.primaryLight,
      child: Row(
        children: [
          Text('📢', style: const TextStyle(fontSize: 16)),
          const SizedBox(width: 8),
          Expanded(
            child: Text(
              '欢迎进入秒杀场，请遵守秒杀规则！',
              style: AppTextStyles.caption.copyWith(color: Colors.white),
            ),
          ),
        ],
      ),
    );
  }
  
  Widget _buildInputArea() {
    return Container(
      height: 54,
      padding: const EdgeInsets.symmetric(horizontal: 15, vertical: 10),
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(top: BorderSide(color: AppColors.border)),
      ),
      child: Row(
        children: [
          Expanded(
            child: Container(
              height: 36,
              padding: const EdgeInsets.symmetric(horizontal: 15),
              decoration: BoxDecoration(
                color: AppColors.surfaceVariant,
                borderRadius: AppRadius.input,
              ),
              child: const TextField(
                decoration: InputDecoration(
                  hintText: '说点什么...',
                  hintStyle: TextStyle(color: AppColors.textHint),
                  border: InputBorder.none,
                ),
              ),
            ),
          ),
          const SizedBox(width: 10),
          Container(
            width: 60,
            height: 36,
            decoration: BoxDecoration(
              color: AppColors.primary,
              borderRadius: AppRadius.input,
            ),
            child: const Center(
              child: Text('发送', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w500)),
            ),
          ),
        ],
      ),
    );
  }
}
```

---

## 七、资源清单

### 7.1 设计稿对应

| HTML 页面 | Flutter 页面 | 主要组件 |
|----------|-------------|---------|
| 首页 | HomePage | EntryCard, Banner, AdGrid |
| 秒杀场 | AuctionPage | NoticeBar, MessageFactory, InputArea |
| 会话列表 | ChatListPage | ChatListItem |
| 个人中心 | ProfilePage | UserCard, MenuSection |
| 交易站 | TradingPage | CategoryScroll, PostList |
| 登录页 | LoginPage | LoginCard, SocialLogin |
| 秒杀榜 | AuctionRankPage | TabHeader, AuctionItemList |
| 我的已成交 | MyDealsPage | DealCardList |

### 7.2 文件结构建议

```
lib/
├── core/
│   ├── constants/
│   │   ├── app_colors.dart
│   │   ├── app_spacing.dart
│   │   ├── app_radius.dart
│   │   ├── app_text_styles.dart
│   │   └── app_shadows.dart
│   └── theme/
│       └── app_theme.dart
├── shared/
│   └── widgets/
│       ├── buttons/
│       │   ├── primary_button.dart
│       │   └── secondary_button.dart
│       ├── cards/
│       │   └── app_card.dart
│       ├── inputs/
│       │   └── app_text_field.dart
│       ├── navigation/
│       │   ├── app_nav_bar.dart
│       │   └── app_tab_bar.dart
│       └── messages/
│           ├── message_bubble.dart
│           ├── text_message.dart
│           ├── auction_bid_message.dart
│           ├── auction_start_message.dart
│           ├── auction_end_message.dart
│           ├── auction_deal_message.dart
│           ├── kasec_status_message.dart
│           └── message_factory.dart
├── features/
│   ├── auction/
│   │   ├── pages/
│   │   │   ├── auction_page.dart
│   │   │   └── auction_rank_page.dart
│   │   └── widgets/
│   │       ├── notice_bar.dart
│   │       └── input_area.dart
│   ├── chat/
│   │   └── pages/
│   │       └── chat_list_page.dart
│   ├── profile/
│   │   └── pages/
│   │       └── profile_page.dart
│   └── trading/
│       └── pages/
│           └── trading_page.dart
└── main.dart
```

---

*文档创建时间: 2026-03-31*  
*基于魔力淘移动端 HTML 设计稿和 UniApp 消息样式规范*