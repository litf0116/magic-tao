# 魔力淘 (MoliTao)

## What This Is

信息撮合平台，连接商品/服务供求双方。买家发布需求，卖家进行报价，双方通过在线沟通达成交易意向。平台不参与交易担保，交易由用户线下完成。

全平台覆盖：PC 管理后台 + H5 移动端网页 + 微信小程序 + Flutter App（iOS/Android）。

Flutter App 已于 2026-05 完成 iOS App Store + Android 应用市场双端发布。微信小程序持续运营优化中。

## Core Value

用户可以快速发布需求、找到合适的服务方，并通过即时通讯高效沟通促成交易。

## Requirements

### Validated

<!-- Shipped and confirmed valuable. -->

- ✓ PC 管理后台 — 运营/管理员使用的后台管理系统（已在线上稳定运行）
- ✓ H5 移动端网页 — 用户端 H5 页面（已在线上稳定运行）
- ✓ 微信小程序 — 用户端小程序（已上线运营，有真实用户）
- ✓ 后端 API（.NET 8 + ABP Framework）— 全功能 API 服务
- ✓ Flutter App（iOS/Android）— 首次双端发布完成 —— v1.0
- ✓ 用户认证系统 — 微信登录 + 手机号验证码登录 —— v1.0
- ✓ 即时通讯 — 私聊/群聊实时消息 —— v1.0
- ✓ 商品/服务信息发布与浏览 —— v1.0
- ✓ 微信支付集成 — 支付处理
- ✓ 极光推送 — iOS/Android 推送通知
- ✓ 短信服务 — 阿里云短信官方 SDK —— v1.0
- ✓ 项目技术债识别与分析（18项）—— v1.0
- ✓ 微信小程序体验优化 —— v1.0
- ✓ SMS 服务并发安全改造 —— v1.0
- ✓ 全项目时间统一为北京时间 —— v1.0
- ✓ 2 项 Critical 技术债修复（AppSecret 硬编码 + HttpClient 反模式）—— v1.0

### Active

<!-- Current scope. Building toward these. -->

下一个里程碑规划中...

### Out of Scope

<!-- Explicit boundaries. Includes reasoning to prevent re-adding. -->

- 交易担保/担保支付 — 平台定位为信息撮合，不提供交易担保
- 海外发布 — 仅面向中国大陆用户

## Context

- **开发周期近 1 年**（2025-05 至今），968+ 次提交，61+ 个分支
- **后端**：.NET 8 + ABP Framework 9.1.3 + MySQL + Redis
- **PC 端**：Vue 3 + TypeScript + Pinia + Element Plus（管理后台）
- **H5**：Vue 3 + UniApp（移动端网页）
- **微信小程序**：Vue 3 + UniApp（已上线运营，已体验优化）
- **Flutter App**：Flutter + Dart（iOS + Android 双端已发布）
- 项目为 monorepo 结构
- **v1.0 已交付**（2026-05-23）：Flutter 双端上架、SMS 并发安全改造、时间统一、2 项 Critical 技术债修复
- **剩余技术债**：13 项（7 High / 5 Medium / 2 Low）

## Constraints

- **时区**：所有时间记录使用北京时间（UTC+8），`DateTime.Now`
- **后端时间**：仅 3 处 Unix 时间戳计算场景保留 `DateTime.UtcNow`（Message.cs、MessageSequenceService.cs、RedisDistributedCache.cs）
- **空 catch 禁止**：不允许使用空 catch 块
- **类型安全**：禁止使用 `as any`、`@ts-ignore`、`@ts-expect-error`
- **分支命名**：`YYYYMMDD_***` 日期前缀格式

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| 全项目使用 DateTime.Now（北京时间） | 仅在中国运营，无需 UTC 存储 | ✓ Good |
| 阿里云短信 SDK 替代手动 HMAC 签名 | 官方 SDK 更稳定，减少 80+ 行手动代码 | ✓ Good |
| 平台定位为信息撮合，非交易担保 | 降低法律合规风险，聚焦核心体验 | ✓ Good |
| SMS 业务先发后存 | 防止验证码发送失败产生脏数据 | ✓ Good |
| 微信凭证 IOptions 注入替代硬编码 | 消除密钥泄露风险 | ✓ Good |
| HttpClient IHttpClientFactory 替代裸注入 | 消除 Socket 泄漏风险 | ✓ Good |
| iOS + Android 同时首发 | 统一市场策略 | ✓ Good |
| iOS 审核前禁用更新弹窗 | 避免违反 App Store 审核准则 | ✓ Good |
| 提现入口审核前隐藏 | 合规要求，后续迭代再开放 | ✓ Good |
| 微信支付走 H5 引导不接入 IAP | 降低技术复杂度，不走苹果支付分成 | ✓ Good |

---

*Last updated: 2026-05-23 after v1.0 milestone*

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state
