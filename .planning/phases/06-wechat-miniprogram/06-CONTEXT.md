# Phase 6: 微信小程序体验优化 - Context

**Gathered:** 2026-05-23
**Status:** Ready for planning
**Source:** Session context + codebase exploration

<domain>
## Phase Boundary

优化已有线上微信小程序（UniApp，`molitao_uniapp/`）的用户体验。**不增加新功能**，聚焦：
- 修复现有 Bug（空 catch、定时器未清理、异常处理）
- 清理生产环境调试日志
- 性能优化（setInterval 清理、页面渲染优化）
- UI/UX 体验细节优化

小程序与 Flutter App 共享后端 API。
</domain>

<decisions>
## Implementation Decisions

### Bug 修复范围
- 检查并修复所有空 catch 块（项目要求禁止空 catch 块）
- 检查 3 个 setInterval 调用是否在页面卸载时清理（避免内存泄漏和后台执行）
- 修复异常处理中仅 `console.log` 不处理实际错误的情况

### 调试日志清理
- 清理生产代码中的 `console.log` 和 `debugPrint` 调用
- 保留有意义的错误日志（catch 块中的错误输出）
- 保留第三库的外部日志

### 性能优化
- 检查 setInterval 在页面不可见时是否清理
- 优化图片懒加载

### UI/UX 优化
- 检查 UI 一致性，修复明显的不对齐、间距问题

### 排除范围
- 不新增功能页面
- 不修改后端 API
- 不改动小程序基础框架（pages.json、manifest.json 中的核心配置）

</decisions>

<specifics>
## 代码库现状

通过代码分析发现的已知问题：

| 类别 | 数量 | 详情 |
|------|------|------|
| `console.log` 调用 | 155 处 / 21 文件 | 大量调试日志在 src/ 中 |
| `setInterval` | 3 处 | chatMain.vue, tabbar/index.vue, signalr.ts |
| `any` 类型 | 138 处 / 31 文件 | TypeScript 类型安全 |
| 空 catch | 0 处(正式) | 但有 30 个 catch 块需确认处理完整性 |

</specifics>

<canonical_refs>
## Canonical References

### 小程序代码
- `molitao_uniapp/src/` — 所有小程序源代码
- `molitao_uniapp/src/pages/` — 页面组件
- `molitao_uniapp/src/components/` — 通用组件
- `molitao_uniapp/src/stores/` — Pinia 状态管理
- `molitao_uniapp/src/utils/` — 工具函数

### 编码规范
- `AGENTS.md` — 项目编码规范（禁止空 catch 等）
- `molitao_uniapp/CLAUDE.md` — UniApp 模块规范

</canonical_refs>

<deferred>
## Deferred Ideas

- 添加新功能或新页面（超出 Phase 6 范围）
- 后端 API 改造
- 大规模 TypeScript 重构（any → 精确类型）
- UniApp 框架版本升级

</deferred>

---

*Phase: 06-wechat-miniprogram*
*Context gathered: 2026-05-23 via session context + code scan*
