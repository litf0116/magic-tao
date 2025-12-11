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