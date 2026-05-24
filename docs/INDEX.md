# 魔力淘项目文档索引

> 📚 **完整知识库**: 项目资料已整理到 Obsidian 知识库
> 位置: `~/Documents/Obsidian/magic-tao-kb/`

本项目文档索引。完整的项目知识库请使用 Obsidian 打开。

## 🔗 快速链接

| 链接 | 说明 |
|------|------|
| [Obsidian 知识库](~/Documents/Obsidian/magic-tao-kb/) | 完整项目知识库（推荐） |
| [账号密码](../secrets/账号密码.md) | 敏感信息，仅本地保存 |
| [AGENTS.md](../AGENTS.md) | AI 开发指南 |
| [CLAUDE.md](../CLAUDE.md) | 全局 AI 指令 |

## 📚 文档分类

### 🎯 核心开发文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **AGENTS.md** | `/AGENTS.md` | AI 代理开发指南（构建、测试、代码规范） |
| **CLAUDE.md** | `/CLAUDE.md` | 全局 AI 指令和 KISS 原则 |
| **CLAUDE.md** | `/backend/CLAUDE.md` | Backend 模块 C#/.NET/ABP 框架指令 |
| **CLAUDE.md** | `/pc/CLAUDE.md` | PC 前端 Vue/TypeScript/UnoCSS 规范 |
| **CLAUDE.md** | `/molitao_uniapp/CLAUDE.md` | 小程序 UniApp 开发规范 |

### 🚀 服务启动文档（存在部分重复）

| 文档 | 位置 | 侧重点 |
|------|------|--------|
| **CLI-STARTUP-GUIDE.md** | `/docs/CLI-STARTUP-GUIDE.md` | 🌟 命令行启动指南（推荐，更详细） |
| **README-SERVICES.md** | `/docs/README-SERVICES.md` | 服务快速启动指南（基础版） |
| **Docker-README.md** | `/docs/Docker-README.md` | Docker 部署指南 |

**说明**：
- `CLI-STARTUP-GUIDE.md` 是主要文档，包含完整的命令行启动和故障排除
- `README-SERVICES.md` 是快速参考版本，适合新手入门
- 两个文档覆盖相同的服务（API 服务 5000、IM 服务 6001）

### 🐳 Docker 部署文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **Docker-README.md** | `/docs/Docker-README.md` | 🌟 Docker 完整部署指南 |
| **dev.md** | `/dev.md` | 包含 Docker 构建命令和调试笔记 |

**说明**：
- `Docker-README.md` 是官方部署文档，包含完整的 Docker Compose 配置
- `dev.md` 是开发调试笔记，包含生产环境的实际部署命令
- 两个文档有部分 Docker 命令重复

### 📋 项目规划文档（2025-11-08）

| 文档 | 位置 | 内容 |
|------|------|------|
| **项目功能清单_20251108.md** | `/项目功能清单_20251108.md` | 功能清单表格（28项功能） |
| **项目需求修改清单_20251108.md** | `/项目需求修改清单_20251108.md` | 需求变更说明 |
| **项目修改内容详细说明_20251108.md** | `/项目修改内容详细说明_20251108.md` | 具体实现细节 |

**说明**：
- 三个文档描述 2025-11-08 的项目优化工作
- 按功能清单 → 需求说明 → 实现细节的逻辑组织
- 建议按顺序阅读，或根据需要查阅特定文档

### 🔧 特定功能文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **auction_bid_logic_update_plan.md** | `/docs/auction_bid_logic_update_plan.md` | 拍卖出价逻辑更新计划 |
| **auction_kasec_feature.md** | `/docs/auction_kasec_feature.md` | 拍卖 Kasec 功能说明 |
| **auction_message_interaction.md** | `/docs/auction_message_interaction.md` | 拍卖消息交互机制 |
| **auction_status_unification.md** | `/docs/auction_status_unification.md` | 拍卖状态统一规范 |
| **auction-optimization.md** | `/pc/docs/auction-optimization.md` | PC 端拍卖优化实现 |
| **BID_ELIGIBILITY_SERVICE_USAGE.md** | `/docs/BID_ELIGIBILITY_SERVICE_USAGE.md` | 出价资格服务使用指南 |
| **min-bid-price-util.md** | `/docs/min-bid-price-util.md` | 最低出价价格工具 |
| **auction-api-testing-guide.md** | `/docs/auction-api-testing-guide.md` | 🌟 拍卖 API 测试指南 | 

### 🔄 迁移规划文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **backend-migration-plan.md** | `/docs/backend-migration-plan.md` | 后端迁移计划（.NET → Java）——迁移策略、模块优先级、流程规范、API 设计标准 |
| **java-architecture-design.md** | `/docs/java-architecture-design.md` | Java 后端架构设计与迁移分析——目录结构、API 映射、外部依赖方案、ABP 横切关注点复现方案、Nginx 路由分流 |

### 📖 系统理解文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **system-understanding-guide.md** | `/docs/system-understanding-guide.md` | 后端系统理解指南——路由体系、双 ORM、事件总线、缓存结构、审计字段、危险代码模式、项目结构速览、快速参考卡 |

### 💾 技术实现文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **database-migration-chatchannel-user-status.md** | `/backend/docs/database-migration-chatchannel-user-status.md` | 数据库迁移：聊天频道用户状态 |
| **sensitive-words-cache-management.md** | `/backend/docs/sensitive-words-cache-management.md` | 敏感词缓存管理 |
| **performance-monitoring-guide.md** | `/performance-monitoring-guide.md` | 性能监控方案指南 |

### 🏗️ 模块文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **README.md** | `/pc/README.md` | PC 模块说明（待更新） |
| **README.md** | `/monitor-ui/README.md` | Monitor UI 模块说明（待更新） |

**说明**：
- 这两个 README 目前是 Vite 生成的默认模板
- 建议更新为项目特定的模块说明文档

### 📝 任务文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **2025-01-14_1_optimize-logging-config.md** | `/.tasks/` | 日志配置优化任务 |
| **2025-01-14_1_fix-message-order.md** | `/.tasks/` | 消息顺序修复任务 |

### 🔐 敏感信息文档

| 文档 | 位置 | 用途 |
|------|------|------|
| **魔力淘各种帐号(绝密,千万不要对外).md** | `/` | 🔒 账号信息（主文件） |
| **魔力淘各种帐号(绝密,千万不要对外).md** | `/pc/src/types/` | 🔒 账号信息（副本，已弃用） |

**说明**：
- 根目录的账号文件是主文件
- `pc/src/types/` 中的副本已废弃，建议删除

## 📖 文档使用建议

### 新手入门流程
1. 阅读 `docs/CLI-STARTUP-GUIDE.md` 启动服务
2. 参考 `/AGENTS.md` 了解开发规范
3. 查看 `/CLAUDE.md` 了解项目结构
4. 根据模块需求查看对应的子目录 `CLAUDE.md`

### Docker 部署流程
1. 主要参考 `docs/Docker-README.md`（官方文档）
2. 查看生产环境实际命令时参考 `dev.md`

### 开发调试
1. 使用 `docs/CLI-STARTUP-GUIDE.md` 进行本地开发
2. 参考 `performance-monitoring-guide.md` 了解性能监控
3. 查看特定功能文档（如拍卖相关文档）

### 项目规划参考
1. 阅读 `项目功能清单_20251108.md` 了解功能范围
2. 查看 `项目需求修改清单_20251108.md` 了解需求变更
3. 参考 `项目修改内容详细说明_20251108.md` 了解实现细节

## ⚠️ 注意事项

1. **重复文档**：部分文档内容有重叠，已在上方标注主要文档（🌟 标记）
2. **版本信息**：部分文档包含版本信息（如 20251108），请注意时间戳
3. **敏感信息**：账号信息文档绝密，切勿外传
4. **文档更新**：修改功能时，请同步更新相关文档

## 🔗 相关链接

- **项目根目录**: `/`
- **文档目录**: `/docs/`
- **后端文档**: `/backend/docs/`
- **PC 端文档**: `/pc/docs/`
- **任务文档**: `/.tasks/`

---

**最后更新**: 2026-04-05
