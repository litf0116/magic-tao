# PC 端统一响应格式方案

## 问题背景

### 现象
- 某些 API（如扫码登录）返回的 `result` 是字符串（URL）
- 某些 API（如列表查询）返回的 `result` 是对象（`{items: [...], totalCount: N}`）
- 原有代码假设 `result` 总是对象，导致字符串类型的 `result` 无法正确处理

### 根本原因
`appService.ts` 的 `axios` 函数总是执行 `resolve(res.data)`，假设 `res` 是 axios 响应对象。

但当响应拦截器已经提取了 `result` 后：
- `result` 是对象 → `res.data` 可能仍然存在（导致重复取 data）
- `result` 是字符串 → `res.data` 是 `undefined`（导致数据丢失）

### ABP 响应格式
```json
{
  "result": <任意类型>,
  "__abp": true,
  "success": true,
  "error": null,
  "targetUrl": null,
  "unAuthorizedRequest": false
}
```

## 解决方案

### 方案：拦截器统一提取 result，axios 函数直接返回

**核心思路**：
1. 响应拦截器负责识别 ABP 格式并提取 `result`
2. `appService.ts` 的 `axios` 函数不再重复取 `.data`，直接返回拦截器处理结果
3. 调用方直接拿到业务数据（`result` 的值）

### 数据流对比

**旧流程**：
```
axios 原始响应: {data: {result: {...}, __abp: true}}
    ↓
appService.ts: resolve(res.data) → {result: {...}}
    ↓
调用方: response.result.items ❌ (多取了一次)
```

**新流程**：
```
axios 原始响应: {data: {result: {...}, __abp: true}}
    ↓
拦截器 normalizeResponse(): 提取 result = {...}
    ↓
appService.ts: resolve(res) → {...} (直接返回)
    ↓
调用方: response.items ✅
```

**字符串 result 的情况（旧流程会出错）**：
```
axios 原始响应: {data: {result: "url", __abp: true}}
    ↓
旧流程 appService.ts: resolve(res.data) → "url" (字符串)
调用方: response.data → undefined ❌
    ↓
新流程 appService.ts: resolve(res) → "url"
调用方: response → "url" ✅
```

## 核心代码

### request.ts - 统一响应处理

```typescript
// ABP 标准响应格式
interface AbpResponse<T = any> {
    __abp?: boolean
    success: boolean
    result?: T
    error?: { message: string; code?: number; details?: any }
    targetUrl?: string | null
    unAuthorizedRequest?: boolean
}

// 简化响应格式（部分接口使用）
interface SimpleResponse<T = any> {
    success: boolean
    result?: T
    message?: string
    data?: T
}

function isAbpResponse(response: any): boolean {
    if (!response?.data) return false
    if (response.data.__abp === true) return true
    return false
}

function isSimpleResponse(response: any): boolean {
    if (!response?.data) return false
    if (typeof response.data.success !== 'boolean') return false
    if (response.data.result !== undefined || response.data.data !== undefined) return true
    return false
}

// 统一响应解析函数
export function normalizeResponse<T = any>(response: any): T {
    if (isAbpResponse(response)) {
        return response.data?.result as T
    }
    if (isSimpleResponse(response)) {
        return response.data?.result ?? response.data?.data as T
    }
    if (response?.data !== undefined) {
        return response.data as T
    }
    return response as T
}

// 响应拦截器
service.interceptors.response.use(
    (response: any) => {
        if (!isSuccessResponse(response)) {
            const errorMsg = getErrorMessage(response) || '操作失败'
            ElMessage.error(errorMsg)
            return Promise.reject(response.data?.error || response.data)
        }
        return normalizeResponse(response)
    },
    // error handler...
)
```

### appService.ts - axios 函数简化

```typescript
export function axios(configs, resolve, reject): Promise<any> {
    if (serviceOptions.axios) {
        return serviceOptions.axios
            .request(configs)
            .then((res) => {
                resolve(res)  // 直接返回拦截器处理后的结果
            })
            .catch((err) => {
                reject(err)
            })
    }
}
```

## 修改的文件

| 文件 | 修改内容 |
|------|----------|
| `pc/src/utils/request.ts` | 添加统一响应类型定义和 normalizeResponse 函数 |
| `pc/src/api/appService.ts` | axios 函数简化为直接 resolve(res) |
| `pc/src/views/home/components/TradingPostCard.vue` | `response.data.items` → `response.items` |
| `pc/src/views/home/components/AdvertisementBanner.vue` | `res.data.items` → `res.items` |
| `pc/src/views/home/components/AuctionCard.vue` | 已有 fallback 逻辑，无需修改 |
| `pc/src/views/home/tradingPost.vue` | `res.data.items` → `res.items` |
| `pc/src/components/paged-table/index.vue` | 已有 fallback 逻辑，无需修改 |
| `pc/src/components/Chat/auctionItemDetail.vue` | `res.data.items` → `res.items` |

## 注意事项

### 1. 调用方数据访问变化

**之前**：
```typescript
const res = await api.post.GetList(params)
if (res.data && res.data.items) {
    list.value = res.data.items
}
```

**现在**：
```typescript
const res = await api.post.GetList(params)
if (res && res.items) {
    list.value = res.items
}
```

### 2. result 为基本类型的情况

当 `result` 是字符串、数字等基本类型时（如扫码登录返回的 URL）：
- 旧流程：`res.data` 返回 `undefined`
- 新流程：`res` 直接返回字符串

### 3. 非 ABP 格式接口

对于不使用 ABP 格式的接口（如外部 API），`normalizeResponse` 会直接返回 `response.data`。

## 待办事项

- [ ] 阶段二：推动后端接口统一使用 ABP 标准格式
- [ ] 清理遗留的兼容代码（如 paged-table 的 fallback 逻辑）
- [ ] 添加 TypeScript 类型支持

## 相关文档

- [ABP Framework 响应格式](https://docs.abp.io/zh-Hans/abp/latest/AspNet-Core/Mvc/Ajax-Responses)
