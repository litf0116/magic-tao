# 出价资格检查服务使用说明

## 概述

`BidEligibilityService` 是一个独立的出价资格检查服务，提供了完整的出价资格验证功能。该服务可以检查用户是否具备出价条件，并支持根据用户名称或用户 ID 进行出价能力检测。

## 服务功能

### 1. 完整出价资格检查 (`CheckBidEligibilityAsync`)

检查用户在特定拍卖商品上的出价资格，包括：

- 用户保证金检查
- 用户名格式验证
- 禁言状态检查
- 商品状态验证
- 最低出价计算
- 卡秒状态处理
- 并发控制检查

### 2. 用户出价能力检查 (`CheckUserBidCapabilityAsync`)

根据用户名称或用户 ID 检查用户的基础出价能力，包括：

- 用户基本信息验证
- 保证金余额检查
- 用户名格式验证
- 禁言状态检查
- 管理员权限检查

## API 接口

### 1. 检查出价资格

**POST** `/api/BidEligibility/CheckBidEligibility`

```json
{
  "auctionItemId": 123,
  "bidUserName": "张三",
  "bidUserId": "456",
  "bidPrice": 100
}
```

**响应示例：**

```json
{
  "canBid": true,
  "reason": "可以出价",
  "minBidPrice": 101,
  "currentPrice": 100,
  "depositBalance": 200.0,
  "userLevel": 1,
  "isKasec": false,
  "auctionStatus": 1,
  "isBanned": false,
  "banEndTime": null
}
```

### 2. 根据用户名称检查出价能力

**GET** `/api/BidEligibility/CheckUserBidCapability?userName=张三`

**响应示例：**

```json
{
  "userId": 456,
  "userName": "张三",
  "userAvatar": "http://example.com/avatar.jpg",
  "canBid": true,
  "reason": "用户具备出价资格",
  "depositBalance": 200.0,
  "userLevel": 1,
  "isBanned": false,
  "banEndTime": null,
  "isAdmin": false,
  "adminTag": ""
}
```

### 3. 根据用户 ID 检查出价能力

**GET** `/api/BidEligibility/CheckUserBidCapabilityById?userId=456`

**响应示例：**

```json
{
  "userId": 456,
  "userName": "张三",
  "userAvatar": "http://example.com/avatar.jpg",
  "canBid": true,
  "reason": "用户具备出价资格",
  "depositBalance": 200.0,
  "userLevel": 1,
  "isBanned": false,
  "banEndTime": null,
  "isAdmin": false,
  "adminTag": ""
}
```

## 服务注册

在 `AbpWebCoreModule.cs` 中注册服务：

```csharp
public override void Initialize()
{
    IocManager.RegisterAssemblyByConvention(typeof(AbpWebCoreModule).GetAssembly());

    // 注册出价资格检查服务
    IocManager.Register<IBidEligibilityService, BidEligibilityService>(DependencyLifeStyle.Transient);
}
```

## 使用场景

### 1. 前端预检查

在用户输入出价金额时，可以调用 `CheckBidEligibility` 接口进行预检查，提前发现可能的问题。

### 2. 用户资格验证

在用户进入拍卖页面时，可以调用 `CheckUserBidCapability` 接口检查用户的基础出价资格。

### 3. 管理员工具

管理员可以使用这些接口来检查特定用户的出价能力和状态。

## 错误处理

服务会返回详细的错误信息，常见的错误包括：

- **保证金不足**：0 级用户需要至少 50 元保证金
- **用户名格式错误**：不能使用默认用户名格式（玩家 XXXXX）
- **用户被禁言**：禁言用户不能出价
- **商品状态错误**：商品不在拍卖中
- **出价金额过低**：不满足最低加价要求
- **并发处理中**：后台正在处理其他出价请求

## 注意事项

1. 所有接口都支持匿名访问（`[AllowAnonymous]`）
2. 服务使用缓存来提高性能
3. 支持卡秒状态的三倍加价规则
4. 包含完整的并发控制机制
5. 提供详细的错误信息和指导
