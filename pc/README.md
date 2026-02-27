# PC 前端模块

> **说明**: 本文档是 PC 前端模块的说明文档。目前使用的是 Vite 生成的默认模板。
> **待更新**: 建议更新此文档，添加项目特定的模块说明和启动方式。
> **开发规范**: 详细的开发规范请参阅 [pc/CLAUDE.md](./CLAUDE.md)
> **项目文档**: 查看所有文档请参考 [docs/INDEX.md](../docs/INDEX.md)

## 模块概述

这是魔力淘项目的 PC 管理端，使用 Vue 3 + TypeScript + Vite 构建。

### 技术栈
- Vue 3 (Composition API with `<script setup>`)
- TypeScript 5+
- Vite
- UnoCSS
- Pinia
- Element Plus
- Axios

## 快速开始

### 安装依赖
```bash
npm install
```

### 开发模式
```bash
npm run dev
```

### 构建生产版本
```bash
npm run build
```

### 代码检查
```bash
npm run lint:fix
```

### 类型检查
```bash
vue-tsc --noEmit
```

## 开发指南

### IDE 设置

推荐使用 VS Code 并安装以下扩展：

- [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (必须)
- [TypeScript Vue Plugin (Volar)](https://marketplace.visualstudio.com/items?itemName=Vue.vscode-typescript-vue-plugin) (必须)
- ESLint
- Prettier

### Vue 文件类型支持

TypeScript 默认无法处理 `.vue` 文件的类型信息，因此我们使用 `vue-tsc` 进行类型检查。

如果 Volar 插件性能不够快，可以使用 **Take Over Mode**：

1. 禁用内置的 TypeScript 扩展
   - 在 VS Code 命令面板运行 `Extensions: Show Built-in Extensions`
   - 找到 `TypeScript and JavaScript Language Features`，右键选择 `Disable (Workspace)`
2. 重新加载 VS Code 窗口

## 目录结构

```
pc/
├── src/
│   ├── api/              # API 接口定义
│   ├── assets/           # 静态资源
│   ├── components/       # 通用组件
│   ├── composables/      # 组合式函数
│   ├── layouts/          # 布局组件
│   ├── pages/            # 页面组件
│   ├── router/           # 路由配置
│   ├── stores/           # Pinia stores
│   ├── types/            # TypeScript 类型定义
│   ├── utils/            # 工具函数
│   ├── App.vue
│   └── main.ts
├── public/               # 公共静态资源
├── docs/                # 模块文档
├── CLAUDE.md            # 开发规范
├── package.json
├── tsconfig.json
├── vite.config.mts
└── README.md            # 本文档
```

## 相关文档

- **开发规范**: [pc/CLAUDE.md](./CLAUDE.md)
- **项目文档索引**: [docs/INDEX.md](../docs/INDEX.md)
- **服务启动**: [docs/CLI-STARTUP-GUIDE.md](../docs/CLI-STARTUP-GUIDE.md)
- **功能文档**: [docs/auction-optimization.md](../docs/auction-optimization.md)

---

**最后更新**: 2025-01-14
