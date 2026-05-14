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

### API 响应数据处理规范

#### 1. ABP 标准响应格式
后端使用 ABP Framework，返回统一封装格式：
```typescript
// ABP 标准响应结构
interface AbpResponse<T = any> {
    __abp: boolean
    success: boolean
    result?: T           // 业务数据（关键字段）
    error?: {
        code?: number
        message: string
        details?: any
        validationErrors?: { message: string; members?: string[] }[]
    }
    targetUrl?: string | null
    unAuthorizedRequest?: boolean
}
```

#### 2. normalizeResponse 函数详解

**函数位置**：`src/utils/request.ts`

**职责**：从 axios 响应中提取业务数据，处理三种响应格式：
1. ABP 标准格式 `{__abp: true, success: true, result: {...}}`
2. 简化格式 `{success: true, result: {...}}`
3. 普通格式 `{data: {...}}`

**源码实现**：
```typescript
// 判断是否为 ABP 标准响应
function isAbpResponse(response: any): boolean {
    if (!response?.data) return false
    if (response.data.__abp === true) return true
    return false
}

// 从 ABP 响应中提取业务数据
function extractAbpResult<T>(response: any): T | undefined {
    return response.data?.result as T | undefined
}

// 统一响应解析函数
export function normalizeResponse<T = any>(response: any): T {
    if (isAbpResponse(response)) {
        return extractAbpResult<T>(response)  // 返回 response.data.result
    }

    if (isSimpleResponse(response)) {
        return extractSimpleResult<T>(response)
    }

    if (response?.data !== undefined) {
        return response.data as T
    }

    return response as T
}
```

#### 3. 响应拦截器数据流转流程

```
后端返回原始 JSON
    ↓
axios 接收 response 对象
    ↓
响应拦截器处理（request.ts 第 212-220 行）
    ↓
normalizeResponse(response) 提取 result
    ↓
返回 response.data.result（业务数据）
    ↓
API 函数直接使用该数据（不再提取 .data）
```

**关键代码**（request.ts）：
```typescript
service.interceptors.response.use(
    (response: any) => {
        if (!isSuccessResponse(response)) {
            const errorMsg = getErrorMessage(response) || '操作失败'
            ElMessage.error(errorMsg)
            return Promise.reject(response.data?.error || response.data)
        }
        // 关键：已解包，直接返回业务数据
        return normalizeResponse(response)  // ✅ 返回 result 内容
    },
    (err: any) => { /* 错误处理 */ }
)
```

#### 4. API 函数编写规范（重要）

❌ **错误写法**：在 API 函数中再次提取 `.data`
```typescript
// ❌ 错误：二次提取导致 undefined
export function getPaymentStatus(query: PaymentQuery): Promise<PaymentResult> {
    return request({
        method: 'get',
        url,
        params: { outTradeNo: query.outTradeNo },
    }).then((response) => response.data)  // response.data 已是业务数据，再 .data = undefined
}
```

✅ **正确写法**：直接返回拦截器处理后的数据
```typescript
// ✅ 正确：直接返回
export async function getPaymentStatus(query: PaymentQuery): Promise<PaymentResult> {
    const response = await request({
        method: 'get',
        url,
        params: { outTradeNo: query.outTradeNo },
    })
    // 拦截器已返回 normalizeResponse 后的业务数据（result 字段内容）
    return response as unknown as PaymentResult
}
```

#### 5. 后端接口返回数据结构

**PayOrderStatusDto（支付订单状态）**
```typescript
interface PayOrderStatusDto {
    orderId: string      // 订单ID
    outTradeNo: string   // 商户订单号
    status: string       // 状态：'已支付' | '未支付' | '失败' | '退款中' | '已退款'
    amount: number       // 金额（元）
    paidTime?: string    // 支付时间
    tradeNo?: string     // 交易流水号
    message: string      // 状态消息
}
```

**CreatePaymentOrderResponse（创建支付订单）**
```typescript
interface CreatePaymentOrderResponse {
    code_url: string     // 支付二维码 URL
    outTradeNo: string   // 商户订单号
    amount: number       // 支付金额
}
```

#### 6. 完整数据流示例

以 `GetPayOrderStatus` 接口为例：

**后端返回**：
```json
{
  "__abp": true,
  "success": true,
  "result": {
    "orderId": "123",
    "outTradeNo": "1KRK26KKOX0TGG3XDGQHZA0WF",
    "status": "已支付",
    "amount": 51,
    "paidTime": "2025-01-10T12:00:00Z",
    "tradeNo": "TRADE_SUCCESS",
    "message": "支付成功"
  }
}
```

**流转过程**：
1. `axios response` = `{data: {__abp, success, result:{...}}}`
2. `normalizeResponse(response)` → `{orderId, outTradeNo, status, ...}`（提取 result）
3. `getPaymentStatus()` 返回 → `{orderId, outTradeNo, status, ...}`（业务数据）
4. 调用方直接使用 `result.status === '已支付'` ✅

#### 7. 注意事项
- 拦截器已经完成响应解包，API 函数中**不要**再次调用 `.data`
- 使用 `as unknown as T` 类型断言解决拦截器返回类型与业务 DTO 的不匹配
- 所有 API 函数统一使用 async/await 语法
- `isSuccessResponse` 判断的是 `response.data.success`，不是 `response.success`

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