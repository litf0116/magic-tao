---
id: ANAL-PLAN-3
phase: 1
wave: 3
type: execute
status: pending
depends_on: [ANAL-PLAN-2]
requirements: [ANAL-01, ANAL-02]
files_modified:
  - docs/mobile-app-analysis/api-inventory.md
  - docs/mobile-app-analysis/tech-debt-log.md
---

# Plan: API Inventory & Tech Debt Log

## Objective

基于前两个计划的产出，整理 Flutter App 需要调用的 API 接口清单，汇总全部分析过程中发现的技术债和优化机会。

## Tasks

### Task 3.1: API 接口清单整理

<read_first>
- 各模块 Controller 文件（`backend/modules/*/src/*/Controllers/`）
- 已有的 codebase 地图: `.planning/codebase/`
- database-schema.md 和 business-flows.md 中的 API 引用
</read_first>

<action>
1. 从各模块的 Controller/AppService 中提取对外暴露的 API 端点
2. 按业务域分组整理（用户、商品、聊天、订单、推送）
3. 每条 API 记录：HTTP 方法、路径、请求参数、响应格式、认证要求
4. 特别标注：
   - Flutter App 前期需要的 API（P0-P2）
   - 需要传 Token 的接口
   - 需要特殊 Header 的接口
   - 分页接口的请求/响应格式
5. 输出到 `docs/mobile-app-analysis/api-inventory.md`
</action>

<acceptance_criteria>
- api-inventory.md 已创建
- 覆盖至少 30 个 API 端点
- 按业务域分组列出
- 每条 API 包含：方法、路径、认证要求
- 标注了 Flutter App 各阶段的依赖优先级（P0/P1/P2）
- 一致的 RESTful 风格模式已描述
</acceptance_criteria>

### Task 3.2: 技术债与优化机会汇总

<read_first>
- 已有的 codebase 地图中的 CONCERNS.md: `.planning/codebase/CONCERNS.md`
- 前两个分析计划中发现的潜在问题
- Session History 中已知的技术债记录
</read_first>

<action>
1. 汇总分析过程中发现的所有技术债、代码质量问题、潜在 Bug
2. 按严重等级分类：Critical / High / Medium / Low
3. 评估每项对 Flutter App 开发的影响（阻塞/有影响/无影响）
4. 给出修复建议和预估工作量
5. 输出到 `docs/mobile-app-analysis/tech-debt-log.md`
</action>

<acceptance_criteria>
- tech-debt-log.md 已创建
- 覆盖至少 10 个记录项
- 每项包含：描述、严重等级、影响范围、修复建议
- 标注了哪些项目需要在 Flutter 开发前修复（阻塞性）
- 与 CONCERNS.md 中的已有发现不重复
</acceptance_criteria>

## Verification

- [ ] api-inventory.md 完整且可用作 Flutter 开发的 API 参考
- [ ] tech-debt-log.md 已按优先级排序
- [ ] 所有文档均在 `docs/mobile-app-analysis/` 目录下
- [ ] 文档中包含指向源码的具体文件路径引用
