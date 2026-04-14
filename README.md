# 魔力淘 (molitao)

## 项目结构

```
molitao/
├── backend/           # 后端 API (.NET 8 + ABP Framework)
├── pc/               # PC Web 端 (Vue 3 + TypeScript)
├── molitao_uniapp/  # UniApp 跨平台应用
├── molitao_app/      # Flutter App (开发中)
├── docs/             # 项目文档
├── scripts/          # 构建脚本
└── design/           # 设计文件
```

## 快速开始

### 后端
```bash
cd backend
dotnet run --project src/TtWork.Project.Web.Host
```

### PC Web
```bash
cd pc
npm install
npm run dev
```

### UniApp
```bash
cd molitao_uniapp
npm install
npm run dev:h5
```

## CI/CD

使用 Gitee Go 自动化构建部署，配置见 `.gitee/workflows/main.yml`

## 文档

详细文档请查看 `docs/` 目录
