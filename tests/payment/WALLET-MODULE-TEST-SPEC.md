# 支付/钱包模块测试规范

## 1. 测试概述

### 1.1 测试目标
验证保证金支付、充值、提现、余额记录等支付相关功能。

### 1.2 测试环境
```
后端服务: http://localhost:12580
数据库: MySQL 127.0.0.1:3306/www_molitao_top
测试用户: feifei (ID: 7509)
```

## 2. API接口

### 2.1 获取用户统计
```bash
GET /api/services/app/Client/GetMyCount
Authorization: Bearer {token}
```

### 2.2 获取我的魔力值记录
```bash
GET /api/services/app/UserDepositLog/GetMyAll?MaxResultCount=5
Authorization: Bearer {token}
```

### 2.3 获取我的余额记录
```bash
GET /api/services/app/UserBalanceLog/GetMyAll?MaxResultCount=5
Authorization: Bearer {token}
```

### 2.4 保证金支付 (Native扫码)
```bash
GET /api/services/app/Client/PayDepositNative
Authorization: Bearer {token}
```

### 2.5 用户充值
```bash
GET /api/services/app/Client/TopUp
Authorization: Bearer {token}
```

### 2.6 用户提现
```bash
POST /api/services/app/Client/PayWithdrawal
Authorization: Bearer {token}
```

## 3. 数据库表

### 3.1 pays_payorder (支付订单表)
- 总记录数: 2,702
- 未支付: 2,370 | 已支付: 332 | 已退款: 0

### 3.2 Pays_UserDepositLog (魔力值记录表)
- 总记录数: 290

### 3.3 Pays_UserBalanceLog (余额记录表)
- 总记录数: 47

### 3.4 pays_wechatpaymentnotification (微信支付回调表)
- 总记录数: 348

## 4. 测试用例

### 4.1 用户统计测试
- ✅ 获取用户统计 (返回auctionSuccess, friend, balance, depositBalance)

### 4.2 记录查询测试
- ✅ 获取魔力值记录 (返回0条 - 用户无记录)
- ✅ 获取余额记录 (返回0条 - 用户无记录)

### 4.3 支付流程测试
- 详见 payment/PAYMENT-TEST-SPEC.md

## 5. 测试结果

| 测试项 | 状态 | 备注 |
|-------|------|------|
| 获取用户统计 | ✅ 通过 | 返回4个字段 |
| 获取魔力值记录 | ✅ 通过 | 用户无记录 |
| 获取余额记录 | ✅ 通过 | 用户无记录 |
| 保证金支付 | ✅ 通过 | 见支付测试文档 |

---
**最后更新**: 2026-04-04
