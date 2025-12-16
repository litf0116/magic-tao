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

## 开发原则

### KISS原则 (Keep It Simple, Stupid)

KISS原则是软件工程中的重要设计理念，强调保持系统简单和简洁。

**核心思想：**
- 大多数系统最好保持简单，避免不必要的复杂性
- 简单的设计更容易理解、维护和扩展

**主要优点：**
1. **易于理解** - 简单的代码更容易被团队成员理解
2. **便于维护** - 减少bug，降低维护成本
3. **提高效率** - 开发和调试更加高效
4. **降低错误** - 复杂性往往导致更多的错误

**实际应用指南：**
- 选择简单直接的解决方案而非复杂的方案
- 避免过度设计和不必要的抽象
- 优先使用清晰可读的代码而非技巧性的代码
- 在满足需求的前提下，选择最简单的实现方式
- 记住：简单往往是最好的解决方案

**适用范围：**
不仅适用于编程，也适用于系统设计、架构决策等各个方面。