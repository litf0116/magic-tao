# External Integrations

**Analysis Date:** 2026-05-22

## APIs & External Services

**WeChat/Weixin Integration:**
- **WeChat Pay** - Payment processing
  - SDK: `Tt.HttpClient.Weixin` module
  - Apps: pub, uniapp, openplatformapp, app
  - Merchant ID: 1669900694
  - API: V3 pay + traditional TenPay
  - Certificates: `certs/apiclient_cert.p12`, `.pem`, `.key`
- **WeChat Auth** - User authentication
  - WechatPubAuthProviderApi - Public account login
  - WechatMiniOpenidProviderApi - Mini program login
  - WechatOpenidProviderApi - Open platform auth
- **WeChat API Client**
  - Module: `Modules/Tt.HttpClient.Weixin/`
  - Features: Access token, User info, JS-SDK parameters

**Push Notifications:**
- **JPush (极光推送)**
  - Service: `JPushService`
  - Config: AppKey `4e91398522bb1286f6452efb`, MasterSecret `43b487d8b4f1c907bc0d37b5`
  - Platforms: iOS, Android
- **WebPush**
  - Service: `WebPushService`
  - Vapid keys configured in appsettings.json
  - Subject: `mailto:admin@molitao.top`

## Data Storage

**Database:**
- **MySQL** (via Pomelo.EntityFrameworkCore.MySql 8.0.2)
  - Connection: `127.0.0.1:3306` (local dev)
  - Database: `www_molitao_top`
  - ORM: Entity Framework Core 8.0.2
  - Migrations: `YYYYMMDD_Description` format

**Cache:**
- **Redis**
  - Connection: `127.0.0.1:6379` (local dev)
  - Database ID: 0
  - Usage: Distributed cache, session, pub/sub
  - Implementation: StackExchange.Redis (via `TtWork.Lib/Redis/RedisClient.cs`)

**File Storage:**
- **UpYun OSS** (又拍云存储)
  - Module: `TtWork.Abp.Oss.UpYun`
  - Bucket: `molitao`
  - Domain: `http://image.molitao.top`
  - API: RESTful upload/download

## Authentication & Identity

**Backend Auth:**
- **JWT Bearer Authentication** (ABP Framework)
  - SecurityKey: Configured in appsettings.json
  - Issuer: `Abp`
  - Audience: `Abp`
  - Expiry: 7 days
  - Config: `src/TtWork.Project.Web.Host/Startup/AuthConfigurer.cs`

**External Auth Providers:**
- WeChat (Public Account, Mini Program, Open Platform)
- External login helpers: `ExternalLoginInfoHelper.cs`

**Multi-tenancy:**
- ABP multi-tenancy support
- Tenant resolution via subdomain/header

## Monitoring & Observability

**Logging:**
- Seq (structured logging)
  - URI: `http://localhost:5341`
  - Integration: Serilog with Seq sink

**Health Checks:**
- Redis health check: `RedisHealthCheckEnhanced.cs`
- Custom health checks: `HealthChecks.cs`

## CI/CD & Deployment

**Hosting:**
- **Backend**: Linux server (8.130.178.251)
  - Docker containers via docker-compose
  - nginx reverse proxy
- **PC Web**: `www.molitao.top` (nginx)
- **H5**: `www.molitao.top/h5/` (nginx)

**CI Pipeline:**
- Gitee Go (`.gitee/workflows/main.yml`)
- Automated build and deployment

## Environment Configuration

**Required env vars / secrets (backend `appsettings.json`):**

| Variable | Description |
|----------|-------------|
| `ConnectionStrings:Default` | MySQL connection string |
| `Redis:ConnectionString` | Redis connection string |
| `Authentication:JwtBearer:SecurityKey` | JWT signing key |
| `App:OssSetting:Upyun:Password` | UpYun API password |
| `Apps:pub:appsec` | WeChat Pay app secret |
| `Apps:uniapp:appsec` | WeChat Pay uniapp secret |
| `JPush:MasterSecret` | JPush master secret |

**Secrets Location:**
- Local dev: `appsettings.json` (dev) / `appsettings.Production.json` (prod)
- Production: Environment-specific configuration

## Webhooks & Callbacks

**WeChat Pay Notifications:**
- Endpoint: `/api/PayNotify/TenPay/{platform}`
- Platforms: pub, uniapp
- Job processing: `TenPayNotifyJob.cs`
- URL (dev): `http://8j4yg3.natappfree.cc/api/PayNotify/TenPay/pub`

**Backend API Endpoints:**
- Swagger UI: Enabled at root (`appsettings.json`: `SwaggerUiEnabled: true`)
- CORS origins configured for localhost:4200, 8j4yg3.natappfree.cc

---

*Integration audit: 2026-05-22*