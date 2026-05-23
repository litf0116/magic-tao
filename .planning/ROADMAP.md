# Roadmap: 魔力淘 (MoliTao)

**Milestone:** v1.0 项目梳理与移动端发布
**Defined:** 2026-05-22
**Phases:** 7 | **Requirements mapped:** 23/23 ✓

---

## Phase 1: 项目梳理与分析

**Goal:** 深入理解后端业务逻辑与数据模型，完成需求文档化，识别技术债与优化机会。

**Requirements:** ANAL-01, ANAL-02

**Success criteria:**
1. 核心业务流程（用户、商品、聊天、订单）的后端 API 和数据模型已梳理清楚
2. 需求文档已完成，可作为后续开发阶段的输入
3. 已识别并记录至少 5 个技术债/优化项
4. Flutter App 的数据库表和 API 接口清单已整理完成

**Suggested tasks:**
- 梳理用户相关 API（认证、个人信息）
- 梳理商品/服务相关 API 和数据模型
- 梳理即时通讯相关 API 和数据模型
- 梳理个人中心相关 API
- 检查推送通知相关 API
- 输出需求梳理文档
- 扫描当前技术债并记录

---

## Phase 2: Flutter — 用户登录注册

**Goal:** 实现 Flutter App 的微信登录和手机号验证码登录功能。

**Requirements:** AUTH-01, AUTH-02, AUTH-03

**Success criteria:**
1. 用户可点击微信登录按钮，拉起微信授权，完成登录
2. 用户可输入手机号，获取短信验证码并完成登录
3. 登录 Token 持久化，应用关闭后重新打开无需重新登录
4. 登录态失效时自动跳转到登录页
5. 现有后端认证 API 无需修改即可复用

**Key context:** 后端已有完整的微信登录和 SMS 验证码 API，Flutter App 仅需集成客户端逻辑。

---

## Phase 3: Flutter — 商品/服务浏览

**Goal:** 实现 Flutter App 端的商品/服务信息流浏览、分类筛选、搜索和详情页。

**Requirements:** BROWSE-01, BROWSE-02, BROWSE-03, BROWSE-04

**Success criteria:**
1. 首页展示商品/服务信息流列表（分页加载）
2. 用户可按分类标签筛选列表
3. 用户可输入关键词搜索商品/服务
4. 用户点击可进入商品/服务详情页，查看完整信息
5. 列表加载流畅，无卡顿
6. 网络异常时有友好提示

**Key context:** 后端已有商品/服务相关 API，参考 H5/PC 端的现有实现。

---

## Phase 4: Flutter — 即时通讯

**Goal:** 实现 Flutter App 端的一对一私聊和群聊功能。

**Requirements:** CHAT-01, CHAT-02, CHAT-03, CHAT-04

**Success criteria:**
1. 用户可发起和接收一对一私聊
2. 用户可参与群聊
3. 聊天消息实时送达（通过 WebSocket 或轮询）
4. 用户可查看聊天历史记录（分页加载）
5. 新消息时收到推送通知
6. 聊天界面消息发送状态可感知（发送中/已发送/失败）

**Key context:** 后端已有完整的 WebSocket/FreeIM 支持，小程序已有聊天实现可作为参考。

---

## Phase 5: Flutter — 个人中心与推送设置

**Goal:** 实现个人中心页面、推送通知设置和基础应用设置。

**Requirements:** PROF-01, PROF-02, PROF-03

**Success criteria:**
1. 用户可查看和编辑个人头像、昵称、手机号
2. 用户可开启/关闭推送通知
3. 用户可查看应用版本信息和关于页面
4. 退出登录功能正常
5. 个人信息修改后同步显示

**Key context:** 后端已有用户信息相关 API。

---

## Phase 6: 微信小程序体验优化

**Goal:** 优化微信小程序 UI/UX 体验，修复已知 Bug，提升性能。

**Requirements:** MP-01, MP-02, MP-03

**Success criteria:**
1. 至少修复 3 个小程序已知 Bug（如空 catch、定时器未清理）
2. 页面加载速度明显提升
3. UI 一致性改善（与设计规范对齐）
4. 无新的 Console Error
5. 小程序提交微信审核通过

**Key context:** 小程序已上线运营，修改需谨慎，避免影响现有用户。

---

## Phase 7: 测试验证与发布

**Goal:** 完成全功能回归测试，Flutter App 成功上架 iOS App Store 和 Android 应用市场。

**Requirements:** QA-01, QA-02, RELEASE-01, RELEASE-02

**Success criteria:**
1. Flutter App 全部 v1 功能通过回归测试
2. 微信小程序现有功能无退化
3. iOS 构建通过，IPA 可正常安装到真机
4. Android APK 构建通过，可正常安装
5. Flutter App 提交到 App Store Connect 审核
6. Flutter App 提交到 Android 应用市场

**Key context:** iOS 上架需要 Apple Developer 账号（已续费，Team ID: WX4RK78D62），需准备发布证书和 Provisioning Profile。

---

## Phase Dependencies

```
Phase 1 (项目梳理) ─┬─→ Phase 2 (Flutter 登录) ──→ Phase 3 (浏览)
                    │                                      │
                    ├─→ Phase 6 (小程序优化) ────────────→ Phase 7 (测试发布)
                    │
                    └─→ Phase 4 (聊天) ←───────────────────┘
                                        │
                                        └─→ Phase 5 (个人中心)
```

Phase 1 必须先完成（为所有开发阶段提供输入）。
Phase 2 → 3 → 4 → 5 按顺序推进（Flutter 模块依赖递进）。
Phase 6 可并行（小程序优化与 Flutter 开发互不依赖）。
Phase 7 全部完成后启动。

---

*Last updated: 2026-05-22 after milestone v1.0 initialization*
