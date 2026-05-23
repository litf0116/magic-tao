---
id: ANAL-PLAN-2
phase: 1
wave: 2
type: execute
status: pending
depends_on: [ANAL-PLAN-1]
requirements: [ANAL-01]
files_modified:
  - docs/mobile-app-analysis/business-flows.md
---

# Plan: Business Flow Analysis

## Objective

按业务域阅读核心 Application Services，理解完整的业务流转路径（含状态机、事件处理、后台 Job），产出业务流转文档。

## Scope

按优先级顺序覆盖以下业务域：

| 优先级 | 业务域 | 核心模块 | 移动端关联 |
|--------|--------|---------|-----------|
| P0 | 用户认证 | TtWork.Abp.Identity | 登录注册入口 |
| P1 | 即时通讯 | TtWork.Abp.Message | 核心社交功能 |
| P2 | 商品/服务 | TtWork.Abp.Auction | 信息流展示 |
| P3 | 订单与支付 | TtWork.Abp.Pay | 订单状态展示 |
| P4 | 推送通知 | TtWork.Abp.Push | 消息推送 |

## Tasks

### Task 2.1: 用户认证业务流

<read_first>
- TtWork.Abp.Identity 模块的 Application Services
- TokenAuthController.cs
- 已有的 SmsVerificationCodeService 代码更新（Session History）
- codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 梳理用户注册/登录流程（手机号验证码 + 微信 OAuth）
2. 梳理 JWT 签发和验证逻辑
3. 梳理用户信息管理（个人资料修改、密码重置）
4. 梳理角色权限体系（如果存在）
</action>

<acceptance_criteria>
- 用户登录注册的完整流程已文档化（含时序）
- 微信 OAuth 集成方式已记录
- JWT Token 的生命周期和刷新机制已描述
- 涉及的核心 API 端点已记录路径和参数
</acceptance_criteria>

### Task 2.2: 即时通讯业务流

<read_first>
- TtWork.Abp.Message 模块的 Application Services 和 Domain 层
- WebSocket/SignalR Hub 实现
- 消息相关实体（Message.cs, ChatChannels 等）
- codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 梳理消息发送、接收、已读状态流转
2. 梳理聊天频道创建与管理流程
3. 梳理 WebSocket/SignalR 实时通信实现
4. 梳理消息序列号生成及顺序保证机制
</action>

<acceptance_criteria>
- 消息发送到接收的完整链路已文档化
- 聊天频道类型（私聊/群聊）的区别和处理逻辑已记录
- WebSocket/SignalR 连接建立和重连机制已描述
- 涉及的核心表（Messages, ChatChannels 等）的字段变化已追溯
</acceptance_criteria>

### Task 2.3: 商品/服务业务流

<read_first>
- TtWork.Abp.Auction 模块的 Application Services
- 商品相关实体（AuctionItems 等）
- 商品浏览、搜索、发布相关 Controller/Service
- codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 梳理商品的 CRUD 流程（发布、编辑、下架）
2. 梳理商品搜索/筛选的实现方式
3. 梳理商品状态机转换
4. 梳理图片上传和管理的实现
</action>

<acceptance_criteria>
- 商品从发布到下架的完整状态流已文档化
- 搜索功能的实现方式（关键词/分类/标签）已记录
- 图片上传的处理流程已描述
- 涉及的核心表和 API 已列出
</acceptance_criteria>

### Task 2.4: 订单与支付业务流

<read_first>
- TtWork.Abp.Pay 模块的 Application Services
- PayOrder 相关实体和处理逻辑
- 微信支付集成代码
- codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 梳理订单创建、支付、退款的流转
2. 梳理微信支付集成流程（统一下单、回调处理）
3. 梳理支付状态机
4. 梳理订单与商品/用户的关联关系
</action>

<acceptance_criteria>
- 订单从创建到完成的完整状态流已文档化
- 微信支付集成涉及的回调和签名验证流程已记录
- 退款流程已描述
- 涉及的核心表和 API 已列出
</acceptance_criteria>

### Task 2.5: 推送通知业务流

<read_first>
- TtWork.Abp.Push 模块
- 极光推送（JPush）集成代码
- 推送相关后台 Job
- codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 梳理推送通知的触发时机和发送逻辑
2. 梳理极光推送的集成方式（Tag/Alias 使用）
3. 梳理推送设置和开关逻辑
4. 梳理消息推送与即时通讯的关系
</action>

<acceptance_criteria>
- 推送通知的触发事件和发送流程已文档化
- JPush alias 和 tag 的使用策略已记录
- 推送开关和免打扰设置的逻辑已描述
- 涉及的后台 Job 已列出
</acceptance_criteria>

## Verification

- [ ] business-flows.md 完整覆盖 5 个业务域
- [ ] 每个业务域包含：数据流转路径、状态机（如有）、API 端点
- [ ] 文档中包含指向源码的具体文件路径引用
- [ ] 重点关注移动端 App 需要处理的逻辑边界
