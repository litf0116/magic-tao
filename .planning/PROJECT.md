# 魔力淘 (MoliTao)

## What This Is

信息撮合平台，连接商品/服务供求双方。买家发布需求，卖家进行报价，双方通过在线沟通达成交易意向。平台不参与交易担保，交易由用户线下完成。

全平台覆盖：PC 管理后台 + H5 移动端网页 + 微信小程序 + Flutter App（iOS/Android）。

## Core Value

用户可以快速发布需求、找到合适的服务方，并通过即时通讯高效沟通促成交易。

## Requirements

### Validated

<!-- Shipped and confirmed valuable. -->

- ✓ PC 管理后台 — 运营/管理员使用的后台管理系统（已在线上稳定运行）
- ✓ H5 移动端网页 — 用户端 H5 页面（已在线上稳定运行）
- ✓ 微信小程序 — 用户端小程序（已上线运营，有真实用户）
- ✓ 后端 API（.NET 8 + ABP Framework）— 全功能 API 服务
- ✓ 用户认证系统 — 微信登录 + 手机号验证码登录
- ✓ 即时通讯 — 私聊/群聊实时消息
- ✓ 商品/服务信息发布 — 发布与展示
- ✓ 微信支付集成 — 支付处理
- ✓ 极光推送 — iOS/Android 推送通知
- ✓ 短信服务 — 阿里云短信验证码

### Active

<!-- Current scope. Building toward these. -->

- [ ] **ANAL-01**: 完成现有后端 API 的业务逻辑梳理与需求文档化
- [ ] **ANAL-02**: 识别并记录当前系统的技术债与优化机会
- [ ] **FLUTTER-01**: Flutter App 完成核心用户功能开发并发布到 iOS App Store
- [ ] **FLUTTER-02**: Flutter App 完成核心用户功能开发并发布到 Android 应用市场
- [ ] **MP-01**: 微信小程序现有功能优化迭代

### Out of Scope

<!-- Explicit boundaries. Includes reasoning to prevent re-adding. -->

- PC 端功能改造 — 本里程碑以移动端为主，PC 后台保持现有功能不变
- 交易担保/担保支付 — 平台定位为信息撮合，不提供交易担保
- 海外发布 — 仅面向中国大陆用户

## Context

- **开发周期近 1 年**（2025-05 至今），968 次提交，61 个分支
- **后端**：.NET 8 + ABP Framework 9.1.3 + MySQL + Redis
- **PC 端**：Vue 3 + TypeScript + Pinia + Element Plus（管理后台）
- **H5**：Vue 3 + UniApp（移动端网页）
- **微信小程序**：Vue 3 + UniApp（已上线运营）
- **Flutter App**：Flutter + Dart（开发中，未上架）
- 项目为 monorepo 结构，包含后端、PC、H5、小程序、Flutter App 五个子项目
- 最近完成的技术改造包括：阿里云短信官方 SDK 集成、全项目时间统一为北京时间

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

---

*Last updated: 2026-05-22 after milestone v1.0 initialization*

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
