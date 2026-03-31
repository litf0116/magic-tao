# 魔力淘 UI 设计规范

本目录包含所有 UI 设计相关的资料和规范文档。

## 目录结构

```
design/
├── README.md                      # 本文件
├── mobile-ui-design-v2.html       # 移动端 UI 设计稿（推荐）⭐
├── mobile-ui-design.html          # 移动端 UI 设计稿（初版）
├── app-ui-design-checklist.md     # App UI 设计检查清单
├── message-styles-spec.md         # 消息样式规范
├── flutter-design-spec.md         # Flutter 设计规范
└── equipment-ux-ui-guide.md       # 装备查询系统 UX/UI 设计指南
```

## 设计稿预览

### 移动端 UI 设计稿

```bash
# 打开 v2 版本（推荐）
open design/mobile-ui-design-v2.html

# 打开初版
open design/mobile-ui-design.html
```

### v2 版本包含内容（13 个页面）

| 类型 | 页面/组件 |
|------|----------|
| 核心页面 | 首页、秒杀场、会话列表、个人中心、交易站、登录页 |
| 消息样式 | 文本、图片、出价、成交通知、卡秒状态、秒杀开始/结束 |
| 弹窗设计 | 拍品详情、出价规则、出价输入 |
| 列表样式 | 秒杀榜列表、我的已成交列表 |

## 设计规范

### 主题色

| 名称 | 色值 | 用途 |
|------|------|------|
| Primary | `#F4835a` | 主色调、按钮、强调 |
| Primary Light | `#ff7144` | 悬浮状态 |
| Primary Dark | `#e06a3a` | 按下状态 |
| Success | `#52c41a` | 成功状态 |
| Warning | `#fa8c16` | 警告状态 |
| Info | `#1890ff` | 信息状态 |

### 品质颜色（装备系统）

| 品质 | 颜色 | 十六进制 |
|------|------|---------|
| 普通 | 灰色 | `#9E9E9E` |
| 优秀 | 绿色 | `#4CAF50` |
| 精良 | 蓝色 | `#2196F3` |
| 传说 | 橙色 | `#FF9800` |
| 史诗 | 紫色 | `#9C27B0` |

## 组件规范

详见各规范文档：

- [消息样式规范](./message-styles-spec.md)
- [Flutter 设计规范](./flutter-design-spec.md)
- [装备查询系统 UX/UI 设计指南](./equipment-ux-ui-guide.md)
- [App UI 设计检查清单](./app-ui-design-checklist.md)

## 相关链接

- [UniApp 消息样式实现](../molitao_uniapp/src/components/message/)
- [PC 端组件库](../pc/src/components/)
- [后端 API 文档](../backend/)

---

**更新日期**: 2026-04-01