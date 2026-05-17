# PC 端接口请求处理标准

> 本文档定义 PC 端与后端 API 交互的标准规范，包括请求封装、响应处理、错误处理等。

## 目录

- [1. 请求封装](#1-请求封装)
- [2. 响应格式标准](#2-响应格式标准)
- [3. 响应拦截处理](#3-响应拦截处理)
- [4. 错误处理](#4-错误处理)
- [5. API 调用模式](#5-api-调用模式)
- [6. Store 中的数据处理](#6-store-中的数据处理)
- [7. 类型定义](#7-类型定义)

---

## 1. 请求封装

### 1.1 请求拦截器 (`service.interceptors.request`)

所有请求自动添加以下 Header：

| Header | 来源 | 说明 |
|--------|------|------|
| `Abp.Tenantid` | 固定值 `1` | ABP 多租户标识 |
| `Authorization` | `Bearer ${getToken()}` | JWT 认证 Token |
| `Content-Type` | `application/json` | 请求内容类型 |
| `Appname` | `VITE_APP_AppName` | 应用名称 |
| `AppVersion` | `VITE_APP_VERSION` | 应用版本 |

```typescript
// 请求拦截器代码 (request.ts)
request.headers['Abp.Tenantid'] = 1
request.headers['Authorization'] = `Bearer ${getToken() || ''}`
request.headers['Content-Type'] = 'application/json'
request.headers['Appname'] = import.meta.env.VITE_APP_AppName
request.headers['AppVersion'] = import.meta.env.VITE_APP_VERSION || '20260224@1.0.0'
```

### 1.2 代理配置 (`vite.config.mts`)

开发环境通过 Vite 代理转发请求：

| 路径 | 开发环境目标 | 生产环境 |
|------|-------------|----------|
| `/api` | `http://127.0.0.1:12580` | `https://www.molitao.top` |
| `/ws` | `http://127.0.0.1:12580` | WebSocket 直连 |

---

## 2. 响应格式标准

### 2.1 ABP 标准格式（主推）

```json
{
  "__abp": true,
  "success": true,
  "result": { /* 业务数据 */ },
  "error": null,
  "unAuthorizedRequest": false
}
```

**失败响应：**
```json
{
  "__abp": true,
  "success": false,
  "result": null,
  "error": {
    "code": 500,
    "message": "错误描述",
    "details": null,
    "validationErrors": [
      { "message": "字段1不能为空", "members": ["field1"] }
    ]
  }
}
```

### 2.2 简化格式（部分接口使用）

```json
{
  "success": true,
  "result": { /* 业务数据 */ }
}
```

### 2.3 分页响应格式

```json
{
  "__abp": true,
  "success": true,
  "result": {
    "items": [ /* 数据数组 */ ],
    "totalCount": 100
  }
}
```

### 2.4 统一响应类型定义

```typescript
// ABP 标准响应格式
export interface AbpResponse<T = any> {
    __abp?: boolean
    success: boolean
    result?: T
    error?: {
        code?: number
        message: string
        details?: any
        validationErrors?: { message: string; members?: string[] }[]
    }
    targetUrl?: string | null
    unAuthorizedRequest?: boolean
}

// 简化响应格式
export interface SimpleResponse<T = any> {
    success: boolean
    result?: T
    message?: string
    data?: T
}
```

---

## 3. 响应拦截处理

### 3.1 响应解析函数

```typescript
// 判断是否为 ABP 标准响应
function isAbpResponse(response: any): boolean {
    if (!response?.data) return false
    if (response.data.__abp === true) return true
    return false
}

// 判断是否为简化响应格式
function isSimpleResponse(response: any): boolean {
    if (!response?.data) return false
    if (typeof response.data.success !== 'boolean') return false
    if (response.data.result !== undefined || response.data.data !== undefined) return true
    return false
}

// 统一响应解析 - 按优先级提取业务数据
export function normalizeResponse<T = any>(response: any): T {
    if (isAbpResponse(response)) {
        return response.data?.result as T
    }
    if (isSimpleResponse(response)) {
        return response.data?.result as T
    }
    if (response?.data !== undefined) {
        return response.data as T
    }
    return response as T
}
```

### 3.2 响应处理优先级

| 优先级 | 格式 | 提取方式 |
|--------|------|----------|
| 1 | ABP 标准格式 | `response.data.result` |
| 2 | 简化格式 | `response.data.result` |
| 3 | 普通格式 | `response.data` |
| 4 | 其他 | 原始数据 |

### 3.3 成功响应处理

```typescript
// 响应拦截器 - 成功路径
service.interceptors.response.use(
    (response: any) => {
        if (!isSuccessResponse(response)) {
            const errorMsg = getErrorMessage(response) || '操作失败'
            ElMessage.error(errorMsg)
            return Promise.reject(response.data?.error || response.data)
        }
        // 关键：返回 normalizeResponse(response)，即提取 result 后的数据
        return normalizeResponse(response)
    },
    // ...
)
```

---

## 4. 错误处理

### 4.1 错误分类处理

| HTTP 状态码 | 场景 | 处理方式 |
|-------------|------|----------|
| 401 | Token 过期/未授权 | 弹窗提示"请重新登录" → `location.reload()` |
| 403 | 权限不足 | 弹窗提示"权限不足" → 跳转首页 |
| 422 | 表单验证失败 | HTML 渲染验证错误列表 |
| 其他 4xx | 客户端错误 | 显示错误消息 |
| 5xx | 服务器错误 | 显示"请求失败" |

### 4.2 验证错误渲染

```typescript
if (validationErrors && validationErrors.length > 0) {
    const message = []
    message.push(`<div class="ml-2">`)
    message.push(`<h4 class="mb-4 font-bold">表单未能通过验证</h4>`)
    message.push('<ul class="ml-4 list-disc">')
    validationErrors.forEach((errItem: any) =>
        message.push(`<li class="leading-5">${errItem.message}</li>`)
    )
    message.push('</ul>')
    message.push(`</div>`)
    ElMessage({
        dangerouslyUseHTMLString: true,
        message: message.join(''),
        type: 'error',
    })
}
```

### 4.3 错误拦截器代码

```typescript
service.interceptors.response.use(
    (response) => { /* 成功处理 */ },
    (err: any) => {
        const errResponse = err.response
        if (errResponse?.data) {
            const errorData = errResponse.data
            const validationErrors = errorData?.error?.validationErrors
            const status = errResponse.status

            // 1. 表单验证错误
            if (validationErrors?.length > 0) { /* HTML 渲染 */ }

            // 2. 401 未授权
            else if (status === 401 || errorData.unAuthorizedRequest) {
                Tips.confirm('请重新登录', '错误', 'error').then(() => {
                    location.reload()
                })
            }

            // 3. 403 权限不足
            else if (status === 403) {
                Tips.confirm('权限不足，无法访问此资源', '错误', 'error').then(() => {
                    location.href = '/'
                })
            }

            // 4. 其他错误
            else {
                const errorMsg = errorData?.error?.message || errorData?.message || '请求失败'
                if (errorData?.error?.code) {
                    Tips.confirm(errorMsg, '错误', 'error')
                } else {
                    ElMessage.error(errorMsg)
                }
            }
            return Promise.reject(errorData.error || errorData)
        }
        ElMessage.error(err.message || '网络请求失败')
        return Promise.reject(err)
    }
)
```

---

## 5. API 调用模式

### 5.1 两种调用模式

#### 模式 1：使用 `useRequest()` 封装（推荐）

```typescript
// api/userFriendAPI.ts
import { useRequest } from '@/utils/request'
const axios = useRequest()

export function GetUserFriendCount() {
    return axios({
        url: '/api/services/app/UserFriend/GetUserFriendCount',
        method: 'get',
    })
}
```

**特点：**
- 自动携带认证 Header
- 自动处理响应标准化
- 自动处理错误提示

#### 模式 2：使用默认 `service`（appService.ts 自动生成）

```typescript
// appService.ts 自动生成的服务
import { AuctionItemService } from '@/api/appService'

// 调用时返回完整响应，需自行处理
const res = await AuctionItemService.get({ id: 1 })
// res 已经是 normalizeResponse 后的数据，即 response.data.result
```

### 5.2 调用示例

```typescript
// POST 请求
export function Add(params) {
    return axios({
        url: '/api/Post/Add',
        method: 'post',
        data: params,
    })
}

// GET 请求带参数
export function GetDetail(id) {
    return axios({
        url: '/api/AuctionItem/GetDetail?id=' + id,
        method: 'get',
    })
}
```

### 5.3 请求超时配置

```typescript
const service = axios.create({
    baseURL: BASE_API_URL,
    timeout: 3000000,  // 5分钟超时
})
```

---

## 6. Store 中的数据处理

### 6.1 标准调用方式

```typescript
// store 中调用 API
const res = await api.auctionItem.getPublicList({ maxResultCount })
// res 已经是 normalizeResponse 后的数据，直接使用

list.value = res.items || res  // 分页数据
```

### 6.2 处理 normalize 后的数据

```typescript
// 由于拦截器已经提取了 result，直接使用即可
const res = await GetDetail(auctionItemId)
// res 已经是拍品详情对象，不需要再 .data 或 .result

const auctionItem = res.data || res  // 兼容处理（部分接口可能返回原始数据）
```

### 6.3 分页数据处理

```typescript
// ABP 分页响应会被 normalizeResponse 提取 result
const res = await AuctionItemService.getAll({ maxResultCount: 10, skipCount: 0 })
// res = { items: [...], totalCount: 100 }

const { items, totalCount } = res
```

---

## 7. 类型定义

### 7.1 统一响应类型

```typescript
// 列表响应
export interface IListResult<T> {
    items?: T[]
}

export class ListResultDto<T> implements IListResult<T> {
    items?: T[]
}

// 分页响应
export interface IPagedResult<T> extends IListResult<T> {
    totalCount?: number
    items?: T[]
}

export class PagedResultDto<T = any> implements IPagedResult<T> {
    totalCount?: number
    items?: T[]
}
```

### 7.2 请求配置类型

```typescript
export interface IRequestOptions extends AxiosRequestConfig {
    loading?: boolean      // 是否显示加载状态
    showError?: boolean    // 是否显示错误提示
}
```

---

## 附录：完整响应流程图

```
HTTP 请求
    │
    ▼
┌─────────────────────────────────────┐
│  请求拦截器                          │
│  - 添加 TenantId/Authorization       │
│  - 添加 Content-Type/AppName        │
└─────────────────┬───────────────────┘
                  │
                  ▼
         ┌────────────────┐
         │  后端 API     │
         └───────┬────────┘
                 │
                 ▼
    ┌────────────────────────┐
    │  HTTP 响应             │
    │  { success, result,     │
    │    error, __abp }       │
    └────────────┬───────────┘
                 │
    ┌────────────▼────────────┐
    │  响应拦截器              │
    │  1. 检查 success        │
    │  2. 失败 → 错误处理    │
    │  3. 成功 → normalize   │
    └────────────┬────────────┘
                 │
                 ▼
    ┌────────────────────────┐
    │  normalizeResponse()  │
    │  提取 result 字段      │
    └────────────┬───────────┘
                 │
                 ▼
         ┌────────────────┐
         │  返回业务数据   │
         │  直接使用      │
         └────────────────┘
```

---

## 更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-04-15 | 1.0 | 初始文档，定义接口请求处理标准 |
