# 保证金充值测试规范

## 1. 测试概述

### 1.1 测试目标
验证保证金充值功能的完整性，包括：
- 充值订单创建
- 支付成功后余额更新
- 充值记录生成
- 手续费扣除

### 1.2 测试范围

| 模块 | 测试内容 | 测试类型 |
|------|---------|---------|
| 支付 API | PayDeposit 接口 | API 测试 |
| 后台任务 | UserDepositJob | 业务测试 |
| 数据库 | 余额更新 | 数据验证 |

### 1.3 测试环境

```
后端服务: http://localhost:12580
数据库: MySQL 127.0.0.1:3306/www_molitao_top
Redis: 127.0.0.1:6379
测试用户: 7509 (feifei)
租户ID: 1
```

## 2. 测试数据

### 2.1 测试用户
```
用户ID: 7509
用户名: feifei
姓名: 粑粑&贝伊果我
初始保证金: 0.00
```

### 2.2 测试金额
```
充值金额: 100.00 元
实际到账: 100.00 元（充值无手续费）
订单类型: 充值
```

## 3. 测试用例

### 3.1 充值订单创建测试

**测试目标**: 验证充值订单创建功能

**前置条件**: 
- 用户已登录（有有效Token）
- 用户余额充足

**测试步骤**:
1. 调用充值API创建订单
2. 验证订单创建成功
3. 查询数据库验证订单信息

**API 调用**:
```bash
# 注意：充值需要通过其他接口，这里展示流程
POST http://localhost:12580/api/services/app/Client/CreateTopUpOrder
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "amount": 100.00
}
```

**数据库验证**:
```sql
SELECT Id, OutTradeNo, State, Total, HostType
FROM Pays_PayOrder
WHERE CreatorUserId = 7509
  AND HostType = 1  -- 充值类型
ORDER BY CreationTime DESC
LIMIT 1;
```

**验证点**:
- State = 0 (未支付)
- Total = 10000 (分)
- HostType = 1 (充值)
- OutTradeNo 格式正确

**预期结果**: 订单创建成功，金额正确

---

### 3.2 充值成功余额更新测试

**测试目标**: 验证充值成功后余额正确更新

**前置条件**: 
- 充值订单已创建
- 订单状态为未支付

**测试步骤**:
1. 查询用户初始余额
2. 模拟充值成功
3. 更新用户余额
4. 验证余额更新正确

**数据库操作**:
```sql
-- 查询初始余额
SELECT DepositBalance FROM AbpUsers WHERE Id = 7509;

-- 更新订单状态
UPDATE Pays_PayOrder 
SET State = 1, IsSuccessPay = 1, SuccessPayTime = NOW()
WHERE OutTradeNo = '{outTradeNo}';

-- 更新用户余额（充值无手续费）
UPDATE AbpUsers 
SET DepositBalance = DepositBalance + 100.00
WHERE Id = 7509;

-- 验证最终余额
SELECT DepositBalance FROM AbpUsers WHERE Id = 7509;
```

**验证点**:
- 订单状态更新为已支付
- 用户余额增加 100.00 元
- 余额计算正确

**预期结果**: 余额正确更新，无手续费扣除

---

### 3.3 充值记录测试

**测试目标**: 验证充值记录正确生成

**前置条件**: 充值成功

**测试步骤**:
1. 查询充值记录
2. 验证记录内容正确

**数据库验证**:
```sql
SELECT Id, Amount, Type, CreatorUserId, CreationTime
FROM UserDepositLogs
WHERE CreatorUserId = 7509
  AND Amount = 100.00
ORDER BY CreationTime DESC
LIMIT 1;
```

**验证点**:
- Amount = 100.00
- Type = 1 (支付)
- CreatorUserId = 7509
- CreationTime 正确

**预期结果**: 充值记录生成正确

---

## 4. 充值 vs 保证金支付对比

| 项目 | 充值 | 保证金支付 |
|------|------|-----------|
| 金额 | 自定义 | 固定 51元 |
| 手续费 | 无 | 1元 |
| 实际到账 | 全额 | 50元 |
| 订单类型 | 1 (充值) | 2 (保证金) |
| 余额类型 | DepositBalance | DepositBalance |

## 5. 测试验证清单

### 5.1 功能验证

- [ ] 充值订单创建成功
- [ ] 订单金额正确
- [ ] 订单类型正确（充值）
- [ ] 支付成功后订单状态更新
- [ ] 用户余额正确增加
- [ ] 无手续费扣除
- [ ] 充值记录生成正确

### 5.2 数据验证

- [ ] 订单数据完整
- [ ] 余额计算正确
- [ ] 日志记录完整

## 6. 测试清理

```sql
-- 删除测试充值订单
DELETE FROM Pays_PayOrder 
WHERE CreatorUserId = 7509 AND HostType = 1;

-- 恢复用户余额
UPDATE AbpUsers SET DepositBalance = 0 WHERE Id = 7509;

-- 删除充值记录
DELETE FROM UserDepositLogs 
WHERE CreatorUserId = 7509 AND Amount = 100.00;
```

---

**最后更新**: 2026-04-03  
**相关文档**: [支付测试规范](PAYMENT-TEST-SPEC.md)