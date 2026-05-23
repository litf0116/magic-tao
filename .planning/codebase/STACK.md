# Technology Stack

**Analysis Date:** 2026-05-22

## Languages

**Primary:**
- C# (.NET 8) - Backend API
- TypeScript 5.4+ - Frontend applications
- Vue 3.4+ - UI framework

**Secondary:**
- Dart (Flutter) - Mobile app (`molitao_app/`)
- SCSS/Less - CSS preprocessing

## Runtime

**Environment:**
- .NET 8 SDK (Backend)
- Node.js 20+ (Frontend build)

**Package Manager:**
- npm (PC, UniApp, H5)
- pnpm (PC - `.pnpm-approvals.json` present)
- NuGet (.NET packages)

## Frameworks

### Backend (ASP.NET Core / ABP Framework)

**Core:**
- ABP Framework 9.1.3 - Application framework
  - `Abp.AutoMapper` - Object mapping
  - `Abp.AspNetCore` - ASP.NET Core integration
  - `Abp.ZeroCore.EntityFrameworkCore` - EF Core integration
- Entity Framework Core 8.0.2 - ORM
- Pomelo.EntityFrameworkCore.MySql 8.0.2 - MySQL provider
- Castle.Windsor.MsDependencyInjection 4.1.0 - DI container

**Real-time:**
- SignalR - WebSocket communication (FreeIM module)

**Project Structure:**
```
backend/
├── src/
│   ├── TtWork.Project.Core/           # Domain layer
│   ├── TtWork.Project.Application/   # Application services
│   ├── TtWork.Project.EntityFrameworkCore/  # EF Core
│   └── TtWork.Project.Web.Host/      # API host
├── Modules/
│   ├── TtWork.Abp.Core/              # Core module
│   ├── TtWork.Abp.AppManagement/     # App settings
│   ├── TtWork.Abp.Oss.UpYun/          # OSS integration
│   ├── TtWork.Abp.Entity/             # Entity helpers
│   ├── TtWork.Lib/                    # Utilities (Redis)
│   └── TtWork.HttpClient.Weixin/       # WeChat API client
├── FreeIM/                            # Real-time messaging
└── Molitao.sln
```

### Frontend (Vue 3 + TypeScript)

**PC Web Admin (`pc/`):**
- Vue 3.4.26 + Composition API
- TypeScript 5.4
- Vite 4.5.3 - Build tool
- UnoCSS 0.59.4 - Atomic CSS
- Pinia 2.1.7 - State management
- Vue Router 4.3.2 - Routing
- Axios 1.6.8 - HTTP client
- Element Plus 2.7.2 - UI component library
- ECharts 5.5.0 - Charts
- VueQuill 1.2.0 - Rich text editor
- WangEditor 5.1.23 - WYSIWYG editor
- Playwright 1.59.1 - E2E testing

**UniApp MiniApp (`molitao_uniapp/`):**
- UniApp 3.0.0-alpha - Cross-platform framework
- Vue 3.4.21 + Composition API
- TypeScript 5.3
- Pinia 2.0.36 - State management
- uView UI 1.1.20 - UI component library
- z-paging 2.7.10 - Pagination
- Vue-i18n 9.1.9 - Internationalization
- dayjs 1.11.10 - Date handling

**H5 Application (`molitao_h5/`):**
- UniApp 3.0.0 - H5 platform
- Vue 3.4.21
- TypeScript 5.3
- Pinia 2.0.36
- uView UI 1.1.20
- html5-qrcode 2.3.8 - QR code scanning

## Key Dependencies

**Backend Infrastructure:**
| Package | Version | Purpose |
|---------|---------|---------|
| ABP Framework | 9.1.3 | Application framework |
| EF Core | 8.0.2 | ORM |
| Pomelo.MySql | 8.0.2 | MySQL provider |
| Redis | - | Cache (via StackExchange.Redis) |
| JWT Bearer | - | Authentication |

**Frontend Critical:**
| Package | Version | Purpose |
|---------|---------|---------|
| Vue | 3.4.26 | UI framework |
| Element Plus | 2.7.2 | PC UI library |
| uView UI | 1.1.20 | Mobile UI library |
| Pinia | 2.1.7 | State management |
| Axios | 1.6.8 | HTTP client |
| UnoCSS | 0.59.4 | Atomic CSS |

## Configuration

**Environment Configuration:**

Backend (`appsettings.json`):
- Connection strings: MySQL database
- Redis: `127.0.0.1:6379`
- JWT: Security key, issuer, audience (7-day expiry)
- WeChat Pay: App credentials per platform (pub, uniapp, openplatformapp, app)
- UpYun OSS: Bucket `molitao`, domain `image.molitao.top`
- JPush: AppKey + MasterSecret (production mode)
- WebPush: Vapid keys

**Frontend Build:**
- PC: `vite.config.mts`
- UniApp/H5: `vite.config.ts` + `project.config.json`

## Platform Requirements

**Development:**
- .NET 8 SDK
- Node.js 20+
- MySQL 8.0+
- Redis 6.0+
-微信开发者工具 (for mini-app)

**Production:**
- Linux server (centos-based - 8.130.178.251)
- Nginx (for H5, PC static hosting)
- Docker containers

---

*Stack analysis: 2026-05-22*