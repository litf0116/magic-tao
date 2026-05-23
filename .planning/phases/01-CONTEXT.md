# Phase 1: 项目梳理与分析 — Context

**Created:** 2026-05-23
**Source:** Phase 1 discuss session

## Decisions

### 1. 分析范围：先全量后聚焦

Phase 1 执行路径：**数据库底层 → 实体映射 → 业务服务 → API 接口**，逐层向上梳理。

- **Step 1 — 数据库 ER 模型**：从 EF Core 迁移文件和实体类定义出发，导出核心业务的 ER 模型（表结构、字段、主外键关系、索引）
- **Step 2 — 实体与领域模型**：梳理 `Domains/` 目录下的实体类（继承链、值对象、枚举）、了解 ABP 的审计字段（CreationTime/LastModificationTime/DeletionTime）和软删除（ISoftDelete）
- **Step 3 — 业务逻辑流转**：按功能域阅读 Application Services，理解完整的业务流转路径（含状态机、事件处理、后台 Job）
- **Step 4 — API 接口层**：从 Controller/AppService 反向整理对外暴露的 RESTful API 清单

### 2. 输出形式：综合文档包

每步产出对应的文档，沉淀到 `docs/mobile-app-analysis/` 目录：

| 文档 | 内容 | 对应步骤 |
|------|------|---------|
| `database-schema.md` | 核心 ER 模型、表结构、字段说明、索引 | Step 1 |
| `data-models.md` | 实体继承图、值对象、枚举定义、ABP 审计模式 | Step 2 |
| `business-flows.md` | 按功能域的业务流转描述、状态机、事件/Job 触发 | Step 3 |
| `api-inventory.md` | Flutter App 需要调用的 API 清单（路径、方法、参数、响应） | Step 4 |
| `tech-debt-log.md` | 技术债与优化机会清单（含优先级排序） | 贯穿全过程 |

### 3. 代码理解深度：全部细节

- **实体字段级**：每个核心实体的字段含义、数据类型、业务语义
- **数据流转路径**：用户操作 → API 请求 → Service 处理 → DB 写入的完整链路
- **状态机转换**：有状态流转的实体（订单、拍卖、消息）的状态转换图
- **外键关系**：哪些表关联哪些表，是否是级联删除

### 4. 优先级排序：识别 + 计划

分析过程中发现的技术债和优化机会：

1. 记录到 `tech-debt-log.md`，标注严重等级（Critical / High / Medium / Low）
2. 评估每项对 Flutter App 开发的影响（阻塞 / 有影响 / 无影响）
3. 此 Phase 不做代码修改 — 修复决策放到本里程碑内后续 Phase 或更远期

## 优先关注的业务域（移动端核心）

按分析顺序排列：

| 优先级 | 业务域 | 核心表/实体 | 说明 |
|--------|--------|------------|------|
| P0 | **用户认证** | `AbpUsers`, `Client` | 微信登录 + 手机号登录是 Flutter App 的入口 |
| P1 | **即时通讯** | `Messages`, `ChatChannels` | 核心社交功能，涉及 WebSocket/SignalR |
| P2 | **商品/服务** | `AuctionItems` 等 | 信息流展示和搜索 |
| P3 | **订单与支付** | `PayOrders`, `Refunds` | 展示订单状态，支付已集成微信支付 |
| P4 | **推送通知** | JPush 相关配置 | 消息推送配置和下发逻辑 |

## 执行约束

- 本 Phase **不修改代码** — 纯阅读 + 文档产出
- 分析发现的阻塞性问题记录到 tech-debt-log，不在此 Phase 修复
- 所有文档存入 `docs/mobile-app-analysis/`，与代码库一同版本管理
