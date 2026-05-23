# FIX-PLAN-2 Summary: 代码清理

## 完成

- **审核合规完成**: 提现入口隐藏、iOS 更新弹窗禁用
  - `profile_page.dart`: "魔力值减少" → "即将上线" + 敬请期待提示
  - `home_page.dart`: 更新弹窗仅在非 iOS 平台显示
- **代码清理完成**: 移除 6 处纯调试 debugPrint 调用
  - `private_chat_page.dart`: 移除 "消息被点击" debug (×1)
  - `group_chat_page.dart`: 移除 "消息被点击" debug (×1)
  - `chat_message_preview_page.dart`: 移除 "选择了表情" / "消息被点击" debug (×2)
  - `trading_post_page.dart`: 移除 "[TradingPostPage] Building page" debug (×1)
  - 保留结构化 `[Tag]` 格式的 service/provider debugPrint（Release 模式自动静音）

## 验证

| Check | Result |
|-------|--------|
| `flutter analyze` | 0 errors, 0 warnings ✓ |
| 编译警告 | 无新增 ✓ |
| 业务逻辑 | 未变更 ✓ |
