# API 统一响应格式标准

## 背景

为了解决 PC 前端响应处理复杂性问题，我们定义了一套简化的 API 统一响应格式，作为魔力淘项目所有 API 的标准响应规范。

## 标准响应格式

### 成功响应

```json
{
  "success": true,
  "result": <业务数据>,
  "code": 200
}
```

**说明**：
- `success`: 必填，标识请求是否成功
- `result`: 必填，业务数据，可以是任意类型（对象、数组、字符串、数字等）
- `code`: 可选，HTTP 状态码或业务码，默认 200

### 失败响应

```json
{
  "success": false,
  "error": {
    "code": 400,
    "message": "错误描述"
  }
}
```

**说明**：
- `success`: 必填，标识请求失败
- `error`: 必填，错误信息对象
- `error.code`: 错误码
- `error.message`: 错误描述

### 分页响应（列表数据）

```json
{
  "success": true,
  "result": {
    "items": [...],
    "totalCount": 100
  },
  "code": 200
}
```

**说明**：
- `result.items`: 数据列表
- `result.totalCount`: 总记录数

## 示例

### 用户信息

```json
{
  "success": true,
  "result": {
    "id": 1,
    "name": "张三",
    "avatar": "https://..."
  }
}
```

### 列表数据

```json
{
  "success": true,
  "result": {
    "items": [
      {"id": 1, "title": "商品1"},
      {"id": 2, "title": "商品2"}
    ],
    "totalCount": 100
  }
}
```

### 简单字符串

```json
{
  "success": true,
  "result": "https://mp.weixin.qq.com/cgi-bin/showqrcode?ticket=..."
}
```

### 空响应

```json
{
  "success": true,
  "result": null
}
```

### 错误响应

```json
{
  "success": false,
  "error": {
    "code": 401,
    "message": "未授权，请登录"
  }
}
```

### 业务错误

```json
{
  "success": false,
  "error": {
    "code": 1001,
    "message": "库存不足"
  }
}
```

## 字段说明

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| `success` | boolean | ✅ | 请求是否成功 |
| `result` | any | ✅（成功时） | 业务数据 |
| `code` | number | ❌ | 状态码，默认 200 |
| `error` | object | ❌（失败时） | 错误信息 |
| `error.code` | number | ✅（失败时） | 错误码 |
| `error.message` | string | ✅（失败时） | 错误描述 |

## PC 前端处理逻辑

```typescript
// request.ts 响应拦截器
service.interceptors.response.use((response) => {
    if (response.success === false) {
        ElMessage.error(response.error?.message || '操作失败')
        return Promise.reject(response.error)
    }
    return response.result
})
```

调用方直接使用：

```typescript
const data = await api.getUserInfo()
console.log(data.name)  // 直接使用业务数据
```

## 后端实现参考

### C# 实现

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Result { get; set; }
    public int Code { get; set; } = 200;
    public ApiError Error { get; set; }
}

public class ApiError
{
    public int Code { get; set; }
    public string Message { get; set; }
}

// 成功响应
public static ApiResponse<T> Success<T>(T result, int code = 200)
{
    return new ApiResponse<T> { Success = true, Result = result, Code = code };
}

// 失败响应
public static ApiResponse<T> Error(int code, string message)
{
    return new ApiResponse<T> 
    { 
        Success = false, 
        Error = new ApiError { Code = code, Message = message }
    };
}
```

### 使用示例

```csharp
// 返回列表
return Ok(ApiResponse<List<UserDto>>.Success(new { items = users, totalCount = 100 }));

// 返回简单值
return Ok(ApiResponse<string>.Success(token));

// 返回错误
return BadRequest(ApiResponse<object>.Error(400, "参数错误"));
```

## 迁移计划

### 阶段一（已完成）

- PC 前端添加 `normalizeResponse` 函数，兼容多种响应格式
- 修改 `appService.ts` 直接返回结果
- 更新组件调用方式

### 阶段二（待完成）

- [ ] 后端接口统一使用标准格式
- [ ] 移除兼容代码
- [ ] 添加 TypeScript 类型定义

## 相关文档

- [[PC-统一响应格式|PC 统一响应格式]]
- [[../Projects/Magic-Tao-项目概览|Magic Tao 项目概览]]