# 待优化问题清单

> 最后更新：2026-04-17
> 
> 本文档记录项目中已实现但存在优化空间的功能点。

---

## 一、Flutter App 收藏表情功能优化

### 1. 重复收藏检查

**问题描述**：当前用户可以重复收藏同一张图片，没有去重校验。

**影响**：用户体验不佳，收藏列表可能出现重复项。

**建议方案**：
- 在 `addToEmoji` 方法中添加去重检查
- 如果 URL 已存在，提示"该表情已收藏"
- 后端 API 也应添加唯一性校验

**涉及文件**：
- `molitao_app/lib/presentation/providers/chat_emoji_store.dart`
- `molitao_app/lib/data/repositories/chat_emoji_repository.dart`

**预估工时**：1h

---

### 2. 网络请求防抖

**问题描述**：用户快速点击收藏/删除按钮可能触发多次 API 请求。

**影响**：服务器压力增加，可能产生数据不一致。

**建议方案**：
- 在收藏和删除操作时禁用按钮，显示加载状态
- 添加 500ms 防抖延迟
- 使用 `RefreshIndicator` 的刷新机制

**涉及文件**：
- `molitao_app/lib/presentation/pages/chat/auction_chat_page.dart`
- `molitao_app/lib/presentation/widgets/chat/emoji_picker.dart`

**预估工时**：2h

---

### 3. 收藏表情本地缓存

**问题描述**：每次打开表情面板都会请求 API 获取收藏列表。

**影响**：网络请求频繁，加载速度慢，用户体验不佳。

**建议方案**：
- 使用 `SharedPreferences` 或 `Hive` 缓存收藏表情列表
- 设置缓存过期时间（如 5 分钟）
- 提供手动刷新按钮

**涉及文件**：
- `molitao_app/lib/presentation/providers/chat_emoji_store.dart`
- 新建 `molitao_app/lib/data/services/emoji_cache_service.dart`

**预估工时**：3h

---

### 4. 表情 URL 失效处理

**问题描述**：如果收藏的表情图片 URL 失效，当前只显示占位图标，没有进一步处理。

**影响**：用户看到破损图片，无法清理失效表情。

**建议方案**：
- 定期检查收藏表情 URL 有效性
- 显示失效标识，提供批量清理功能
- 或自动过滤失效的表情

**涉及文件**：
- `molitao_app/lib/presentation/widgets/chat/emoji_picker.dart`

**预估工时**：2h

---

## 二、统计汇总

| 分类 | 问题数 | 预估工时 |
|------|:------:|:--------:|
| 用户体验优化 | 4 | 8h |

---

## 三、优先级建议

1. **高优先级**：重复收藏检查（影响用户数据质量）
2. **中优先级**：网络请求防抖、本地缓存（影响用户体验）
3. **低优先级**：表情 URL 失效处理（边界情况，发生概率低）

---

*本文档由开发团队维护，优化完成后请更新状态*
