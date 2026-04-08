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

**Backend (C# .NET 8)**: `cd backend && dotnet build` | `dotnet test --filter "FullyQualifiedName~TestClassName"` (single test) | `dotnet test` (all tests)

**PC (Vue 3 + TypeScript)**: `cd pc && npm run dev` | `npm run build` | `npm run lint:fix` (ESLint) | `vue-tsc --noEmit` (typecheck)

**UniApp**: `cd molitao_uniapp && npm run dev:h5` | `npm run dev:mp-weixin` | `npm run build` | `npm run type-check` | `npm run lint:fix`

**Monitor UI**: `cd monitor-ui && npm run dev` | `npm run build`

## Code Style (KISS)

- **KISS**: Keep It Simple, Stupid - prefer simple, maintainable solutions over clever complex ones
- **Naming**: PascalCase (classes/methods), camelCase (variables), SCREAMING_SNAKE_CASE (constants)
- **Types**: Use explicit types; avoid `var` except for obvious inference; avoid `any`, prefer `unknown`
- **Error Handling**: Use try-catch with meaningful messages; throw `UserFriendlyException` (C#) or custom exceptions for business errors
- **Formatting**: Follow module-specific rules (backend: C# conventions, pc: Vue/TS, uniapp: UniApp/TS)
- **Comments**: No comments unless required; code should be self-documenting
- **Imports**: Use absolute imports from root; organize alphabetically; avoid deep relative paths

## Backend (C# .NET 8 + ABP Framework)

- Follow C# 10+ conventions and ABP Framework patterns
- Use dependency injection for all services; avoid static methods where DI possible
- Async/await: always use async with await; avoid `async void` (use `async Task`)
- Use Repository pattern (IRepository<T>) for data access
- Entity naming: inherit from AuditedAggregateRoot or FullAuditedAggregateRoot
- API design: RESTful, use DTOs with AutoMapper, ApplicationService for logic
- Database migrations: YYYYMMDD_Description format in EntityFrameworkCore
- UnitOfWork for transactions; soft delete with ISoftDelete
- Testing: xUnit + Shouldly; use `[Fact]` and `[Theory]`
- Private fields: _camelCase; constants: PascalCase; interfaces: IPascalCase
- File organization: namespace first, then using statements, then class

## PC (Vue 3 + TypeScript + Pinia)

- Use Composition API with `<script setup lang="ts">`
- TypeScript strict mode: all variables/functions must have types
- Component naming: PascalCase (e.g., `UserProfile.vue`)
- Use `ref` for primitives, `reactive` for objects; avoid unnecessary reactivity
- Route lazy loading: `() => import('@/views/Home.vue')`
- API calls: async/await with Axios; define types for request/response
- Styling: UnoCSS atomic classes prefer over inline styles; scoped styles
- Pinia stores: `useXxxStore` naming; State as function, Getters for computed, Actions for async

## UniApp (Vue 3 + TypeScript)

**项目定位：molitao_uniapp 仅负责微信小程序开发**

- **平台范围**：仅支持微信小程序 (`mp-weixin`)，不维护 H5、App、其他小程序平台
- **构建命令**：`npm run dev` 和 `npm run build` 默认使用 `mp-weixin` 平台
- **条件编译**：代码中使用 `#ifdef MP-WEIXIN` 进行小程序专属逻辑处理
- **移除的平台支持**：H5 PWA 推送、App 原生功能、其他小程序平台代码已逐步清理

**开发规范**：
- Use UniApp API (uni.*) over platform-specific APIs for cross-platform compatibility
- Conditional compilation for platform differences: `#ifdef MP-WEIXIN`
- Responsive units: use `rpx` for layout dimensions
- Page registration: configure in pages.json
- Lifecycle: use `onLoad`, `onShow`, `onReady` (UniApp specific)
- Navigation: `uni.navigateTo`, `uni.redirectTo`, `uni.switchTab`
- Storage: `uni.setStorageSync`/`uni.getStorageSync`
- Component naming: kebab-case for file names, PascalCase for component definitions

## General Guidelines

- File size formatting: Use `@lib/core/services/file_path_manager.dart` formatFileSize (if applicable)
- Branch naming: Use date prefix `YYYYMMDD_***` when creating branches
- Always run lint and typecheck before committing code
- Check module CLAUDE.md for detailed conventions (pc/CLAUDE.md, molitao_uniapp/CLAUDE.md, backend/CLAUDE.md)
- Use existing libraries and utilities; check package.json/dependencies before adding new packages

## Key Rules

- See `CLAUDE.md` in each module for detailed guidelines
- Language: Chinese for regular responses; English for mode declarations and code formatting

## Testing Guidelines

- **Backend tests**: Use xUnit + Shouldly; write unit tests for business logic and integration tests for API endpoints
- **Frontend tests**: Verify functionality manually or add tests as needed; component testing with Vitest if configured
- **Test naming**: Clear, descriptive names following pattern: `Method_State_ExpectedResult`
- **Test isolation**: Each test should be independent and run in any order

## Error Handling Patterns

- **Backend**: Catch exceptions at application boundaries; use specific exception types; log errors with context
- **Frontend**: Use try-catch around async operations; show user-friendly messages; handle API errors consistently
- **Validation**: Validate inputs early; use DTOs with data annotations (C#) or form validation schemas (Vue)

## File Organization

- **Backend**: Organize by feature/domain; keep related classes together; separate concerns (entities, services, DTOs)
- **Frontend**: Group by feature or type; keep components small and focused; use barrel exports for clean imports
- **Naming consistency**: Match file names to class/component names; use consistent patterns across the codebase

## Performance Considerations

- **Backend**: Use async/await correctly; avoid N+1 queries; implement caching where appropriate
- **Frontend**: Lazy load routes and components; use computed properties efficiently; optimize bundle size
- **Database**: Use appropriate indexes; limit query results; avoid unnecessary joins

## Security Best Practices

- **Backend**: Validate all inputs; use parameterized queries; implement proper authentication/authorization
- **Frontend**: Never store sensitive data in localStorage; validate data on server; use HTTPS
- **Secrets**: Never commit secrets or API keys; use environment variables or secret management

## Code Formatting

- **C#**: Use .editorconfig for consistent formatting; follow IDE auto-formatting
- **TypeScript**: Use Prettier for formatting; follow ESLint rules; max line length 100-120 chars
- **Vue**: Single-quotes for template strings; semicolons required; consistent indentation (2 or 4 spaces)

## Documentation

- **Code**: Minimal inline comments; prefer self-documenting code with clear naming
- **Complex Logic**: Add brief comments for non-obvious business logic or algorithms
- **APIs**: Document public APIs with JSDoc (JS/TS) or XML docs (C#) when interfaces are public-facing

## Git Conventions

- **Commit messages**: Clear, concise messages (imperative mood): "Add user authentication feature"
- **Branch naming**: Use date prefix: `YYYYMMDD_feature-description` or `YYYYMMDD_fix-bug-description`
- **PR/Reviews**: Focus on logic, security, and maintainability; don't nitpick style differences