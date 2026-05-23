# Codebase Structure

**Analysis Date:** 2026-05-22

## Directory Layout

```
magic-tao/                    # Project root
├── backend/                  # .NET 8 + ABP Framework API
│   ├── Modules/             # Reusable ABP modules
│   │   ├── TtWork.Abp.Core/
│   │   ├── TtWork.Abp.Entity/
│   │   ├── TtWork.Abp.Oss.UpYun/
│   │   ├── TtWork.Abp.AppManagement/
│   │   └── Tt.HttpClient.Weixin/
│   ├── src/
│   │   ├── TtWork.Project/           # Main application
│   │   │   ├── Applications/          # App services (*AppService.cs)
│   │   │   ├── Controllers/           # API controllers
│   │   │   ├── Domains/               # Domain entities
│   │   │   ├── Services/              # Business services
│   │   │   └── ...
│   │   ├── TtWork.Project.Core/      # Core abstractions
│   │   ├── TtWork.Project.EntityFrameworkCore/
│   │   ├── TtWork.Project.Web.Core/
│   │   ├── TtWork.Project.Web.Host/   # API entry point
│   │   └── TtWork.Project.Migrator/   # DB migrations
│   ├── test/                  # Unit/integration tests
│   └── FreeIM/                # WebSocket server (separate)
├── pc/                        # PC Web Admin (Vue 3 + TypeScript)
│   ├── src/
│   │   ├── api/               # API client definitions
│   │   ├── components/        # Vue components
│   │   ├── composables/       # Vue composables
│   │   ├── layouts/           # Layout components
│   │   ├── routes/           # Vue Router config
│   │   ├── stores/            # Pinia stores
│   │   ├── types/             # TypeScript types
│   │   ├── utils/             # Utilities (request.ts, etc.)
│   │   ├── views/             # Page components
│   │   ├── App.vue
│   │   └── main.ts            # Entry point
│   └── package.json
├── molitao_uniapp/           # UniApp (Vue 3, cross-platform)
│   ├── src/
│   │   ├── pages/            # UniApp pages
│   │   ├── components/        # Components
│   │   ├── stores/            # Pinia stores
│   │   ├── utils/             # Utilities
│   │   └── main.ts
│   └── pages.json
├── molitao_h5/               # H5 Web (Vue 3)
│   └── src/
├── molitao_app/              # Flutter App
│   └── lib/
│       ├── main.dart
│       ├── models/           # Data models
│       ├── services/          # Business logic
│       └── pages/             # Flutter pages
├── docs/                      # Project documentation
├── scripts/                   # Build/deploy scripts
└── design/                    # Design files (.pen)
```

## Directory Purposes

**Backend Structure (`backend/src/`):**

| Directory | Purpose | Key Files |
|-----------|---------|-----------|
| `TtWork.Project/Applications/` | Application services (business logic) | `*AppService.cs` |
| `TtWork.Project/Controllers/` | API controllers (thin, routing only) | `WebsocketController.cs` |
| `TtWork.Project/Domains/` | Domain entities (business model) | `AuctionItem.cs`, `BidHistory.cs` |
| `TtWork.Project/Services/` | Domain services | |
| `TtWork.Project.Core/` | Core abstractions, session, permissions | `AbpSessionExtension.cs` |
| `TtWork.Project.EntityFrameworkCore/` | EF Core DbContext, migrations | |
| `TtWork.Project.Web.Host/` | API host, Startup configuration | `Program.cs` |

**PC Frontend Structure (`pc/src/`):**

| Directory | Purpose | Key Files |
|-----------|---------|-----------|
| `api/` | API client definitions (generated from backend) | `appService.ts`, `index.ts` |
| `components/` | Reusable Vue components | `Chat/`, `Payment/` |
| `composables/` | Vue Composition API utilities | `usePayment.ts` |
| `layouts/` | Page layout components | `Layout.vue`, `SideBar/` |
| `routes/` | Vue Router configuration | `index.ts`, `adminRoute.ts` |
| `stores/` | Pinia state management | `userStore.ts`, `chatStore.ts` |
| `types/` | TypeScript type definitions | `payment.ts` |
| `utils/` | Utility functions | `request.ts` (axios wrapper) |
| `views/` | Page components | `home/`, `admin/`, `chat/` |

## Key File Locations

**Entry Points:**
- Backend: `backend/src/TtWork.Project.Web.Host/Startup/Program.cs`
- PC Web: `pc/src/main.ts`
- UniApp: `molitao_uniapp/src/main.ts`
- Flutter: `molitao_app/lib/main.dart`

**Configuration:**
- Backend: `backend/src/TtWork.Project.Web.Host/appsettings.json`
- PC: `pc/vite.config.mts`, `pc/tsconfig.json`
- UniApp: `molitao_uniapp/manifest.json`

**Core Logic:**
- Backend API: `backend/src/TtWork.Project/Applications/` (30 AppService files)
- PC API client: `pc/src/api/index.ts` (aggregates all service clients)

**Testing:**
- Backend: `backend/test/TtWork.Project.Tests/`
- PC: `pc/tests/e2e/`

## Naming Conventions

**Backend (C#):**
- Namespace: `TtWork.Project.Applications.{Feature}`
- Class: `XxxAppService`, `XxxController`, `XxxEntity`
- Private fields: `_camelCase`
- Constants: `PascalCase`
- Interfaces: `IPascalCase`

**Frontend (TypeScript/Vue):**
- Files: `PascalCase.vue`, `camelCase.ts` for utilities
- Components: `PascalCase.vue` (e.g., `UserProfile.vue`)
- Composables: `useXxx.ts` (e.g., `usePayment.ts`)
- Stores: `useXxxStore.ts` (e.g., `useUserStore.ts`)
- API functions: `camelCase` in service objects
- Routes: `kebab-case` in paths, `PascalCase` for component names

**Database:**
- Tables: `PascalCase` (e.g., `AuctionItems`)
- Migrations: `YYYYMMDD_Description.cs`
- Foreign keys: `{Entity}Id` (e.g., `AuctionItemId`)

## Where to Add New Code

**New Backend Feature:**
1. Create entity in `Domains/` (inherit `AuditedAggregateRoot`)
2. Add Application Service in `Applications/{Feature}/`
3. Add DTOs and AutoMapper mapping in same feature folder
4. Create controller if needed in `Controllers/`
5. Add permissions in `ProjectNameAuthorizationProvider.cs`
6. Create EF migration in `EntityFrameworkCore/Migrations/`

**New PC Web Feature:**
1. Add API methods in `api/` (or add to existing service file)
2. Add TypeScript types in `types/`
3. Create component in `components/` or `views/`
4. Add route in `routes/` (adminRoute.ts for admin pages)
5. Add Pinia store if shared state needed in `stores/`

**New Mobile Client Feature:**
1. Add to UniApp first (cross-platform shared)
2. Mirror to H5 and Flutter as needed
3. Use `#ifdef MP-WEIXIN` for WeChat-specific code

## Special Directories

**Modules (Reusable):**
- Purpose: Shared ABP modules (WeChat integration, OSS storage, etc.)
- Location: `backend/Modules/`
- Not committed to main repo as NuGet packages

**FreeIM WebSocket Server:**
- Purpose: Real-time chat server (separate process)
- Location: `backend/FreeIM/`
- Communication: SignalR over WebSocket

**Design Files:**
- Purpose: UI/UX design in .pen format
- Location: `design/`, `docs/*.pen`
- Tool: OpenCode's pencil tool

**Docs:**
- Purpose: Project documentation, specs, migration guides
- Location: `docs/`
- Contains: API standards, PRD, test cases, database guides

---

*Structure analysis: 2026-05-22*