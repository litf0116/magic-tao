---
id: ANAL-PLAN-1
phase: 1
wave: 1
type: execute
status: pending
depends_on: []
requirements: [ANAL-01]
files_modified:
  - docs/mobile-app-analysis/database-schema.md
  - docs/mobile-app-analysis/data-models.md
---

# Plan: Database Schema & Data Model Mapping

## Objective

从 EF Core 迁移文件和实体类定义出发，完成后端数据库表结构、实体关系、字段含义的全面梳理。产出数据库 schema 文档和数据模型文档。

## Scope

- 读取所有 EF Core 迁移文件（`backend/**/EntityFrameworkCore/Migrations/`）了解表结构演变
- 梳理核心实体类（`Domains/` 目录）的字段定义、继承链、值对象、枚举
- 理解 ABP 审计字段（CreationTime/LastModificationTime/DeletionTime）和软删除（ISoftDelete）模式
- 按业务域整理外键关系和索引
- 识别 Flutter App 开发需要关注的表结构

**不包含**：业务流转逻辑分析（在 ANAL-PLAN-2 中处理）

## Tasks

### Task 1.1: 数据库表结构梳理

<read_first>
- 后端模块目录结构: `backend/modules/` 下的 EntityFrameworkCore 项目
- 已有的 codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 遍历所有 EF Core 迁移文件，提取当前数据库表的最终 Schema
2. 按业务域（用户/商品/聊天/订单/支付/推送）分组列出核心表
3. 每张表记录：表名、主要字段及类型、主键、外键关系、索引
4. 特别关注与移动端展示相关的字段（图片 URL、状态字段、时间字段）
5. 输出到 `docs/mobile-app-analysis/database-schema.md`
</action>

<acceptance_criteria>
- database-schema.md 已创建
- 覆盖至少 15 张核心业务表
- 每张表包含：表名、字段清单、主键、外键关系
- 表按业务域分组（用户、商品、聊天、订单、支付）
- 已标注与移动端相关的关键字段
</acceptance_criteria>

### Task 1.2: 实体与数据模型映射

<read_first>
- 各模块的 Domain 项目: `backend/modules/*/src/*/Domains/`
- 已有的 codebase 地图: `.planning/codebase/`
</read_first>

<action>
1. 梳理核心实体类的继承链（AggregateRoot / AuditedAggregateRoot / FullAuditedAggregateRoot 等）
2. 识别值对象（ValueObject）和枚举（Enum）定义
3. 整理 ABP 审计字段在每张表的映射情况
4. 记录软删除（ISoftDelete）和多语言（IMultiLingual）等接口实现
5. 输出到 `docs/mobile-app-analysis/data-models.md`
</action>

<acceptance_criteria>
- data-models.md 已创建
- 包含实体类继承关系图（文本描述）
- 包含所有核心枚举的定义和取值
- 标识了值为对象和复杂类型字段
- 标注了哪些实体实现了 ISoftDelete 等 ABP 接口
</acceptance_criteria>

## Verification

- [ ] database-schema.md 完整且准确
- [ ] data-models.md 覆盖所有核心实体
- [ ] 文档中包含指向源码的具体文件路径引用
