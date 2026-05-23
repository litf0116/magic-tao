# Requirements: 魔力淘 (MoliTao)

**Defined:** 2026-05-22
**Core Value:** 用户可以快速发布需求、找到合适的服务方，并通过即时通讯高效沟通促成交易。

## v1 Requirements

### 项目梳理与分析

- [ ] **ANAL-01**: 完成现有后端 API 的业务逻辑梳理与需求文档化
- [ ] **ANAL-02**: 识别并记录当前系统的技术债与优化机会

### 用户登录认证

- [ ] **AUTH-01**: Flutter App 用户可使用微信登录
- [ ] **AUTH-02**: Flutter App 用户可使用手机号验证码登录
- [ ] **AUTH-03**: 用户登录状态持久化，应用重启后无需重新登录

### 商品/服务浏览

- [ ] **BROWSE-01**: 用户可在 Flutter App 浏览商品/服务信息流
- [ ] **BROWSE-02**: 用户可按分类筛选商品/服务
- [ ] **BROWSE-03**: 用户可搜索商品/服务关键词
- [ ] **BROWSE-04**: 用户可查看商品/服务详情

### 即时通讯

- [ ] **CHAT-01**: 用户可进行一对一私聊
- [ ] **CHAT-02**: 用户可参与群聊
- [ ] **CHAT-03**: 用户收到新消息时可收到推送通知
- [ ] **CHAT-04**: 用户可查看聊天历史记录

### 个人中心

- [ ] **PROF-01**: 用户可查看和编辑个人信息（头像、昵称、手机号等）
- [ ] **PROF-02**: 用户可配置推送通知开关
- [ ] **PROF-03**: 用户可查看应用设置（关于、版本等）

### 微信小程序体验优化

- [ ] **MP-01**: 优化小程序 UI/UX 用户体验
- [ ] **MP-02**: 修复小程序现有已知 Bug
- [ ] **MP-03**: 提升小程序性能表现

### 测试与发布

- [ ] **QA-01**: 完成 Flutter App 全功能回归测试
- [ ] **QA-02**: 完成微信小程序回归测试
- [ ] **RELEASE-01**: Flutter App 成功上架 iOS App Store
- [ ] **RELEASE-02**: Flutter App 成功上架 Android 应用市场

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### 支付与交易

- **PAY-01**: 用户可在 Flutter App 内完成微信支付
- **PAY-02**: 用户可查看订单状态
- **PAY-03**: 用户可管理退款流程

### 需求发布

- **POST-01**: 用户可发布商品/服务需求
- **POST-02**: 用户可管理已发布的需求

### 更多功能

- **FEAT-01**: 用户收藏/关注功能
- **FEAT-02**: 用户评价系统
- **FEAT-03**: 消息已读/未读状态同步

## Out of Scope

| Feature | Reason |
|---------|--------|
| PC 端功能改造 | 本里程碑以移动端为主，PC 后台保持现有功能不变 |
| 交易担保/担保支付 | 平台定位为信息撮合，不提供交易担保 |
| 海外应用商店发布 | 仅面向中国大陆用户 |
| Android 厂商推送通道 | 极光推送基础版足够，厂商通道配置复杂，可后续优化 |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| ANAL-01 | Phase 1 | Pending |
| ANAL-02 | Phase 1 | Pending |
| AUTH-01 | Phase 2 | Pending |
| AUTH-02 | Phase 2 | Pending |
| AUTH-03 | Phase 2 | Pending |
| BROWSE-01 | Phase 3 | Pending |
| BROWSE-02 | Phase 3 | Pending |
| BROWSE-03 | Phase 3 | Pending |
| BROWSE-04 | Phase 3 | Pending |
| CHAT-01 | Phase 4 | Pending |
| CHAT-02 | Phase 4 | Pending |
| CHAT-03 | Phase 4 | Pending |
| CHAT-04 | Phase 4 | Pending |
| PROF-01 | Phase 5 | Pending |
| PROF-02 | Phase 5 | Pending |
| PROF-03 | Phase 5 | Pending |
| MP-01 | Phase 6 | Pending |
| MP-02 | Phase 6 | Pending |
| MP-03 | Phase 6 | Pending |
| QA-01 | Phase 7 | Pending |
| QA-02 | Phase 7 | Pending |
| RELEASE-01 | Phase 7 | Pending |
| RELEASE-02 | Phase 7 | Pending |

**Coverage:**
- v1 requirements: 23 total
- Mapped to phases: 23
- Unmapped: 0 ✓

---
*Requirements defined: 2026-05-22*
*Last updated: 2026-05-22 after milestone v1.0 initialization*
