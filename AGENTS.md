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

## Quick Commands

**Backend**: `cd backend && dotnet build` | `dotnet test --filter "FullyQualifiedName~TestClassName"` (single test)
**PC**: `cd pc && npm run dev` | `cd pc && npm run lint`
**UniApp**: `cd molitao_uniapp && npm run dev:h5`
**Lint/TypeCheck**: See CLAUDE.md in each module

## Code Style (KISS)

- **KISS**: Keep It Simple, Stupid - prefer simple, maintainable solutions
- **Naming**: PascalCase (classes/methods), camelCase (variables), SCREAMING_SNAKE_CASE (constants)
- **Types**: Use explicit types; avoid `var` except for obvious inference
- **Error Handling**: Use try-catch with meaningful messages; throw `UserFriendlyException` for business errors
- **Formatting**: Follow module-specific rules in CLAUDE.md (backend: C# style, pc: Vue/TS style)
- **Comments**: No comments unless required; code should be self-documenting

## Key Rules

- See `.cursor/rules/command.mdc` for RIPER-5 mode protocol
- See `CLAUDE.md` for detailed module-specific guidelines
- See subdirectory CLAUDE.md files for extended module instructions