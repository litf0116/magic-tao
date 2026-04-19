# Magic-Tao 项目 - Multica 使用指南

## 📋 项目概览

**魔力桃系统** - 全栈电商管理系统，包含 5 个核心组件：

| 组件 | 技术栈 | 推荐Agent |
|------|--------|-----------|
| Backend API | .NET 8 + ABP Framework | backend-engineer |
| PC 管理后台 | Vue 3 + TypeScript + UnoCSS | frontend-engineer |
| 微信小程序 | UniApp + Vue 3 | mobile-developer |
| Flutter App | Flutter + Dart | mobile-developer |
| H5 应用 | UniApp + Vue 3 | frontend-engineer |

## 🚀 快速开始

### 1. 初始化项目上下文

```bash
cd /Users/mac/workspace/magic-tao
```

### 2. 给 Agent 分配任务

#### 后端开发任务

```bash
# 创建商品管理 API
multica task create \
  --agent-id a3d4cd84-bf2c-412a-993e-c554f3bfc435 \
  --title '创建商品管理 API' \
  --description '在 backend 模块创建商品 CRUD 接口，包含商品列表、详情、创建、更新、删除功能。使用 ABP Framework 的 ApplicationService 模式。项目路径: /Users/mac/workspace/magic-tao/backend'
```

#### PC 前端任务

```bash
# 创建商品管理页面
multica task create \
  --agent-id bc528226-efd9-462f-8ebb-8fbd87b47921 \
  --title '创建商品管理页面' \
  --description '在 pc 模块创建商品管理页面，使用 Vue 3 Composition API + TypeScript + UnoCSS。包含商品列表、搜索、筛选、新增、编辑功能。项目路径: /Users/mac/workspace/magic-tao/pc'
```

#### 小程序任务

```bash
# 创建商品详情页
multica task create \
  --agent-id 5af91523-934b-49d5-947e-e268a1203593 \
  --title '创建商品详情页' \
  --description '在 molitao_uniapp 模块创建微信小程序商品详情页，使用 UniApp + Vue 3。仅支持 mp-weixin 平台，使用条件编译处理平台差异。项目路径: /Users/mac/workspace/magic-tao/molitao_uniapp'
```

## 🔧 开发命令

### Backend (.NET 8)

```bash
# 构建
cd /Users/mac/workspace/magic-tao/backend && dotnet build

# 运行测试
dotnet test

# 运行单个测试
dotnet test --filter "FullyQualifiedName~TestClassName"
```

### PC (Vue 3)

```bash
# 开发环境
cd /Users/mac/workspace/magic-tao/pc && npm run dev

# 构建生产版本
npm run build:prod

# 代码检查
npm run lint:fix

# 类型检查
vue-tsc --noEmit
```

### UniApp 小程序

```bash
# 开发环境（仅微信小程序）
cd /Users/mac/workspace/magic-tao/molitao_uniapp && npm run dev

# 构建
npm run build

# 类型检查
npm run type-check
```

### Flutter App

```bash
# 获取依赖
cd /Users/mac/workspace/magic-tao/molitao_app && flutter pub get

# 运行
flutter run

# 构建
flutter build
```

## 📦 部署流程

### PC 端部署

```bash
# 1. 构建生产版本
cd /Users/mac/workspace/magic-tao/pc && pnpm run build:prod

# 2. 备份（可选）
ssh molitao "cd /www/wwwroot/www.molitao.top && tar -czf ../www.molitao.top-bak-$(date +%Y%m%d_%H%M%S).tar.gz *"

# 3. 上传
scp -r pc/dist/* molitao:/www/wwwroot/www.molitao.top/

# 4. 重载 Nginx
ssh molitao "nginx -t && nginx -s reload"

# 5. 验证
curl -I https://www.molitao.top/
```

### H5 部署

```bash
# 1. 备份 + 上传
ssh molitao "mv /www/wwwroot/molitao-h5 /www/wwwroot/molitao-h5.bak-$(date +%Y%m%d_%H%M%S) && mkdir -p /www/wwwroot/molitao-h5"
scp -r molitao_h5/dist/build/h5/* molitao:/www/wwwroot/molitao-h5/

# 2. 重载 Nginx
ssh molitao "nginx -t && nginx -s reload"

# 3. 验证
curl -I https://www.molitao.top/h5/
```

## 📝 代码规范

### KISS 原则
- **简单优于复杂**：选择能解决问题的最简单方案
- **清晰优于晦涩**：代码和设计应该易于理解和维护
- **实用优于花哨**：避免过度工程化和不必要的抽象

### 命名规范

| 类型 | Backend (C#) | PC/Vue | UniApp |
|------|-------------|--------|--------|
| 类/组件 | PascalCase | PascalCase.vue | PascalCase |
| 方法 | PascalCase | camelCase | camelCase |
| 变量 | camelCase | camelCase | camelCase |
| 常量 | PascalCase | UPPER_CASE | UPPER_CASE |
| 私有字段 | _camelCase | - | - |

### 重要规则

1. ✅ 删除方法前检查是否在前端/小程序中使用
2. ✅ 登录后及时更新 token 到 `docs/Authorization.md`
3. ✅ 创建分支使用日期前缀：`YYYYMMDD_feature-name`
4. ✅ 提交代码前运行 lint 和 typecheck
5. ✅ UniApp 项目仅维护微信小程序平台

## 🧪 测试账号

- **用户名**: admin
- **密码**: 123456

## 📚 相关文档

- `AGENTS.md` - AI 助手指令
- `backend/CLAUDE.md` - Backend 开发规范
- `pc/CLAUDE.md` - PC 前端开发规范
- `molitao_uniapp/CLAUDE.md` - 小程序开发规范
- `ROADMAP.md` - 项目路线图
- `TODO.md` - 待办事项
- `docs/` - 详细文档

## 🤝 协作示例

### 全栈功能开发

```bash
# 1. 后端 API（backend-engineer）
multica task create \
  --agent-id a3d4cd84-bf2c-412a-993e-c554f3bfc435 \
  --title '订单管理 API' \
  --description '创建订单管理 API，包含订单创建、支付、取消、退款等功能'

# 2. PC 前端（frontend-engineer）
multica task create \
  --agent-id bc528226-efd9-462f-8ebb-8fbd87b47921 \
  --title '订单管理页面' \
  --description '创建订单管理页面，集成后端 API'

# 3. 小程序（mobile-developer）
multica task create \
  --agent-id 5af91523-934b-49d5-947e-e268a1203593 \
  --title '小程序订单功能' \
  --description '创建小程序订单列表、详情、支付页面'

# 4. 测试（qa-engineer）
multica task create \
  --agent-id 9e2e6f56-f8b2-48b3-a09b-e6d0b96a1dbd \
  --title '订单功能测试' \
  --description '编写订单功能自动化测试用例'
```

## 🎯 常见任务模板

### 添加新功能模块

```bash
multica task create \
  --agent-id <agent-id> \
  --title '<功能名称>' \
  --description '在 <模块名> 中实现 <功能描述>。
  
## 要求
- 遵循 KISS 原则
- 代码符合项目规范
- 添加必要的测试
- 更新相关文档

## 项目路径
/Users/mac/workspace/magic-tao/<模块名>
'
```

## 💡 提示

- 每个任务描述中明确指定模块路径
- 提供清晰的业务需求和技术要求
- 指定需要遵循的项目规范
- 大任务拆分为多个小任务
- 使用合适的 agent（backend/frontend/mobile）

## 📞 支持

遇到问题？查看：
- 项目 `AGENTS.md` 和各模块 `CLAUDE.md`
- `docs/` 目录下的详细文档
- `ROADMAP.md` 了解项目规划
