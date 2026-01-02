<!-- OPENSPEC:START -->
# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `@/openspec/AGENTS.md` when the request:
- Mentions planning or proposals (words like proposal, spec, change, plan)
- Introduces new capabilities, breaking changes, architecture shifts, or big performance/security work
- Sounds ambiguous and you need the authoritative spec before coding

Use `@/openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guidelines

Keep this managed block so 'openspec update' can refresh the instructions.

<!-- OPENSPEC:END -->
- 删除方法的时候检查一下这个方式是否在前端或者小程序中有使用。如果存在使用则不能删除
- 登录账号 admin 密码 123456
- 我们调用登录接口执行之后获取的 token 信息及时更新到 @docs/Authorization.md 中，支持给后续接口调用使用

## 子目录 CLAUDE.md 支持

本项目支持在各模块子目录中创建独立的 CLAUDE.md 文件，用于添加模块特定的 AI 指令。

### 继承规则
- **仅扩展模式**: 子目录 CLAUDE.md 仅扩展根目录的全局指令，不覆盖
- 根目录的 OpenSpec 指令和项目特定说明对所有模块有效
- 子目录专注于模块特有的技术规范和开发约定

### 已配置模块
- `/backend/CLAUDE.md` - Backend 模块 C#/.NET/ABP 框架指令
- `/pc/CLAUDE.md` - PC 前端 Vue/TypeScript/UnoCSS 规范
- `/molitao_uniapp/CLAUDE.md` - 小程序 UniApp 开发规范

### 创建新模块 CLAUDE.md 指南
```markdown
# [模块名称] AI 指令扩展

## 技术栈
- 列出模块使用的主要技术和框架

## 开发规范
- 编码标准和最佳实践
- 命名约定
- 文件组织规范

## 特定指令
- 模块特有的开发约定
- 与其他模块的交互规范
- 测试和部署相关指令
```

## KISS 原则

KISS 是 "Keep It Simple, Stupid" 的缩写，是项目开发遵循的核心原则。

### 核心理念
- **简单优于复杂**：选择能解决问题的最简单方案
- **清晰优于晦涩**：代码和设计应该易于理解和维护
- **实用优于花哨**：避免过度工程化和不必要的抽象

### 实践指导
1. **代码实现**
   - 一个函数只做一件事
   - 使用清晰的命名和注释
   - 避免过度优化和炫技

2. **架构设计**
   - 选择成熟稳定的技术栈
   - 保持系统简单可维护
   - 减少不必要的依赖

3. **开发流程**
   - 先让功能正常工作，再考虑优化
   - 不为未来不确定的需求过度设计
   - 优先考虑可读性和可维护性

记住：简单应该是深思熟虑后的简单，而非偷懒。