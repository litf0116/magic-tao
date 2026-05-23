# Architecture

**Analysis Date:** 2026-05-22

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend Clients                         │
├──────────────────┬──────────────────┬───────────────────────┤
│   PC Web (Vue3)  │  UniApp (Vue3)   │   Flutter App         │
│  `pc/src/`       │ `molitao_uniapp/`│   `molitao_app/lib/`  │
│  + H5 (Vue3)     │                  │                       │
└────────┬─────────┴────────┬─────────┴──────────┬────────────┘
         │                  │                    │
         ▼                  ▼                    ▼
┌─────────────────────────────────────────────────────────────┐
│                    Backend API (.NET 8)                      │
│         `backend/src/TtWork.Project.Web.Host/`               │
│              Port 5000 (Production) / 5001 (Dev)            │
└────────┬───────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│              Data Layer (EF Core + SQL Server)               │
│     `backend/src/TtWork.Project.EntityFrameworkCore/`       │
└─────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | Location |
|-----------|----------------|----------|
| PC Web | Admin management interface | `pc/src/` |
| UniApp | Cross-platform mobile app | `molitao_uniapp/src/` |
| H5 | Mobile web version | `molitao_h5/src/` |
| Flutter App | Native mobile app | `molitao_app/lib/` |
| Backend API | Business logic, auth, data API | `backend/src/TtWork.Project/` |
| WebSocket Server | Real-time messaging | `backend/src/TtWork.Project/Controllers/` |

## Pattern Overview

**Overall:** ABP Framework + Vue 3 SPA + Multi-client architecture

**Key Characteristics:**
- Multi-tenant SaaS with tenant-based configuration
- WebSocket for real-time chat and auction notifications
- JWT-based authentication across all clients
- Soft delete + audit fields on all entities
- SignalR for push notifications

## Layers

**Frontend Layer (PC/UniApp/H5):**
- Purpose: User interface and API consumption
- Location: `pc/src/`, `molitao_uniapp/src/`, `molitao_h5/src/`
- Contains: Vue 3 components, Pinia stores, Vue Router routes, API clients
- Depends on: Backend REST API, WebSocket for real-time
- Used by: Browser (PC/H5), WeChat mini-program (UniApp)

**API Layer (ABP Application Services):**
- Purpose: Business logic and orchestration
- Location: `backend/src/TtWork.Project/Applications/`
- Contains: `*AppService.cs` classes with `[RemoteService]` attribute
- Depends on: Domain entities, EF Core repositories
- Used by: All frontend clients

**Domain Layer (ABP Domain Services & Entities):**
- Purpose: Core business rules and entity definitions
- Location: `backend/src/TtWork.Project/Domains/`
- Contains: Entity classes (inherit from `AuditedAggregateRoot`), domain services
- Depends on: None (pure business logic)
- Used by: Application services

**Data Access Layer (Entity Framework Core):**
- Purpose: Database operations and migrations
- Location: `backend/src/TtWork.Project.EntityFrameworkCore/`
- Contains: `DbContext`, Repository implementations, EF migrations
- Depends on: SQL Server/MySQL
- Used by: Application services via ABP's IRepository<T>

**Infrastructure Layer:**
- Purpose: External integrations (WeChat, JPush, UpYun OSS)
- Location: `backend/Modules/`
- Contains: `Tt.HttpClient.Weixin`, `TtWork.Abp.Oss.UpYun`, `TtWork.Lib`

## Data Flow

### Primary API Request Path

```
Frontend (Vue) 
    → Axios Request (`pc/src/utils/request.ts`)
    → Backend Controller (`WebsocketController.cs`, etc.)
    → Application Service (`*AppService.cs`)
    → Domain Entity (`Domains/*.cs`)
    → EF Core Repository
    → SQL Server Database
```

### Authentication Flow

```
Client → POST /api/TokenAuth/Authenticate
    → TokenAuthService.Validate()
    → JWT Token issued
    → Subsequent requests include Authorization: Bearer {token}
    → AbpSession populated with UserId, TenantId
```

### WebSocket Real-time Flow

```
Client connects to /websocket
    → WebsocketController handles
    → ChatChannel entities track channels
    → SignalR broadcasts to subscribers
    → Frontend receives via socket.io client
```

## Key Abstractions

**ABP Session (`IAbpSession`):**
- Purpose: Current user and tenant context
- Examples: `AbpSession.UserId`, `AbpSession.TenantId`
- File: `backend/src/TtWork.Project.Core/AbpSessionExtension.cs`

**Repository Pattern (`IRepository<TEntity, TPrimaryKey>`):**
- Purpose: Generic data access
- Examples: `IRepository<AuctionItem, int>`, `IRepository<Message, long>`
- Pattern: ABP provides implementation, use via dependency injection

**Application Services:**
- Purpose: Orchestrate domain operations, expose DTOs
- Examples: `AuctionItemAppService`, `ClientAppService`, `PayNotifyAppService`
- Location: `backend/src/TtWork.Project/Applications/`

**Domain Entities:**
- Purpose: Business objects with identity and behavior
- Examples: `AuctionItem`, `BidHistory`, `Message`, `ChatGroup`
- Base: `AuditedAggregateRoot<TKey>` with soft delete (`ISoftDelete`)

**Permission System:**
- Purpose: Authorization based on permissions
- Examples: `"Pages.Admin"`, `"Pages.Auction"`
- File: `backend/src/TtWork.Project.Core/ProjectNameAuthorizationProvider.cs`

## Entry Points

**Backend API:**
- Location: `backend/src/TtWork.Project.Web.Host/`
- Main: `TtWork.Project.Web.Host/Startup/Program.cs` (line 11)
- Port: 5000 (prod), 5001 (dev) via `ASPNETCORE_URLS`

**PC Web:**
- Location: `pc/src/main.ts` (line 22)
- Mount: `app.mount('#app')` (line 37)
- Router: `pc/src/routes/index.ts`

**UniApp:**
- Location: `molitao_uniapp/src/main.ts`
- Config: `pages.json` for page routing

**Flutter App:**
- Location: `molitao_app/lib/main.dart`

## Architectural Constraints

- **Threading:** .NET thread pool configured with `SetMinThreads(200, 200)` in `Program.cs:13`
- **Global state:** Pinia stores in `pc/src/stores/` (userStore, chatStore, auctionStore, permissionStore)
- **Circular imports:** None detected in ABP layers; frontend has standard Vue circular dependency patterns
- **Multi-tenancy:** Enabled via `IMultiTenant` on entities; tenant ID stored in `Client.TenantId`

## Anti-Patterns

### Direct Controller Logic

**What happens:** Controllers like `WebsocketController.cs` (23287 bytes) contain significant business logic
**Why it's wrong:** ABP's Application Service layer is designed for business orchestration; controllers should be thin
**Do this instead:** Move complex logic to Application Services, keep controllers for HTTP routing

### Large Entity Services

**What happens:** `ClientAppService.cs` is 37677 bytes with extensive functionality
**Why it's wrong:** Violates single responsibility; harder to test and maintain
**Do this instead:** Split into focused services (e.g., ClientAuthService, ClientConfigService)

### Static Service Locator Usage

**What happens:** Some code uses static accessors instead of DI
**Why it's wrong:** Makes testing harder, creates hidden dependencies
**Do this instead:** Inject dependencies via constructor

## Error Handling

**Strategy:** ABP exception handling middleware + custom `BusinessException`

**Patterns:**
- Validation errors → `validationErrors` array in response
- Business errors → `BusinessException` with error code
- HTTP errors → `AbpValidationException` or `UserFriendlyException`
- All wrapped in ABP's `AjaxResponse` with `success`/`error` fields

## Cross-Cutting Concerns

**Logging:** ABP's `ILogger<T>` injected via DI; configured in `appsettings.json`

**Validation:** Data annotations on DTOs + `AutoMapper` validation

**Authentication:** JWT Bearer tokens via ABP's `TokenAuthService`; custom permission provider `ProjectNameAuthorizationProvider`

**Caching:** `IDistributedCache` for session/token caching; configured in `AppSettings`

**Background Jobs:** `IBackgroundJobManager` for async tasks (message delivery, push notifications)

---

*Architecture analysis: 2026-05-22*