# PC 前端模块 AI 指令扩展

## 技术栈
- Vue 3
- TypeScript 5+
- Vite
- UnoCSS
- Pinia (状态管理)
- Vue Router
- Axios
- Element Plus

## Vue 3 开发规范
- 优先使用 Composition API (`<script setup>`)
- TypeScript 严格模式，所有变量必须有类型
- 使用 Pinia 进行状态管理，避免 Vuex
- 使用 `<script setup lang="ts">` 语法
- 响应式数据优先使用 `ref`，对象使用 `reactive`

## 组件开发规范
- 组件命名采用 PascalCase (如: `UserProfile.vue`)
- Props 必须定义类型，使用 `defineProps<T>()`
- Emits 必须定义类型，使用 `defineEmits<T>()`
- 组件事件命名采用 kebab-case
- 单文件组件结构顺序: `<template>` → `<script setup>` → `<style>`
- 使用 `defineOptions` 设置组件名称

## TypeScript 规范
- 启用严格类型检查
- 接口命名使用 PascalCase，以 `I` 开头 (可选)
- 类型别名使用 PascalCase
- 避免使用 `any`，优先使用 `unknown`
- 使用 `as const` 创建只读字面量类型

## 样式规范
- 使用 UnoCSS 原子类，避免内联样式
- 样式作用域使用 `scoped`
- 响应式设计优先使用 UnoCSS 的响应式前缀
- 颜色值使用 CSS 变量
- 动画使用 CSS transitions 或 UnoCSS 的 transition 工具类

## 路由和页面
- 路由配置使用 TypeScript 类型安全
- 页面组件放在 `src/views/` 目录
- 路由懒加载: `component: () => import('@/views/Home.vue')`
- 使用路由守卫进行权限控制
- 页面传参使用 query 或 params，类型要明确

## API 调用规范
- 使用 Axios 封装 API 请求
- API 接口定义在 `src/api/` 目录
- 使用 TypeScript 定义请求和响应类型
- 统一错误处理和响应拦截
- 使用 async/await 处理异步请求

## 项目结构
```
src/
├── api/           # API 接口定义
├── assets/        # 静态资源
├── components/    # 通用组件
│   └── [name]/
│       ├── index.vue
│       └── types.ts
├── composables/   # 组合式函数
├── layouts/       # 布局组件
├── pages/         # 页面组件
├── router/        # 路由配置
├── stores/        # Pinia stores
├── types/         # TypeScript 类型定义
└── utils/         # 工具函数
```

## 状态管理 (Pinia)
- Store 定义使用 `defineStore`
- State 使用函数返回初始值
- Getters 用于计算属性
- Actions 用于异步操作和业务逻辑
- Store 命名: `useXxxStore`

## 性能优化
- 组件懒加载: `defineAsyncComponent`
- 图片懒加载使用 `loading="lazy"`
- 使用 `v-show` vs `v-if` 合理选择
- 长列表使用虚拟滚动
- 避免不必要的响应式数据

## 代码质量
- 使用 ESLint + Prettier 格式化代码
- 提交前运行 lint 检查
- 组件必须有默认导出
- 避免 `console.log`，使用 logger
- 合理拆分组件，保持单一职责

## 特定约定
- 图标使用 Element Plus 的图标组件
- 日期处理使用 dayjs
- 表单验证使用 Element Plus 的表单验证
- 消息提示使用 Element Plus 的 ElMessage
- 确认对话框使用 ElMessageBox