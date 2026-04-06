# 支付功能测试规范

## 1. 测试概述

### 1.1 测试目标
验证扫码 Native 支付功能的完整性和正确性，包括：
- 用户认证流程
- 支付订单创建
- 支付回调处理
- 保证金余额更新
- 异常情况处理

### 1.2 测试范围

| 模块 | 测试内容 | 测试类型 |
|------|---------|---------|
| 后端 API | PayDepositNative 接口 | API 测试 |
| 支付回调 | PayNotify 处理流程 | 集成测试 |
| 数据库 | 订单状态流转 | 数据验证 |
| 业务逻辑 | 保证金充值 | 业务测试 |

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

### 2.2 测试订单
```
订单金额: 51.00 元
实际到账: 50.00 元
手续费: 1.00 元
订单类型: 保证金支付
支付方式: 微信 Native 扫码
```

### 2.3 应用配置
```
AppId: wxfb7bd5b5f94a8805
MchId: 1669900694
AppName: pub
NotifyUrl: http://8j4yg3.natappfree.cc/api/PayNotify/TenPay/pub
```

## 3. 测试前置条件

### 3.1 环境检查
- [ ] 后端服务运行正常 (http://localhost:12580)
- [ ] MySQL 数据库可连接
- [ ] Redis 服务正常
- [ ] 测试用户数据存在

### 3.2 数据准备
- [ ] 用户 7509 存在且激活
- [ ] 应用配置（pub）完整
- [ ] 微信支付证书文件存在

## 4. 测试用例

### 4.1 用户认证测试

**测试目标**: 验证用户 Token 生成功能

**测试步骤**:
1. 调用 GenerateTokenForUser 接口
2. 传入用户ID: 7509
3. 验证返回的 Token 格式

**API 调用**:
```bash
POST http://localhost:12580/api/TokenAuth/GenerateTokenForUser
Content-Type: application/json

{
  "userId": 7509
}
```

**验证点**:
- 返回 success: true
- accessToken 不为空
- userId = 7509
- userName = "feifei"
- expireInSeconds > 0

**预期结果**: Token 生成成功，包含正确的用户信息

---

### 4.2 支付订单创建测试

**测试目标**: 验证 Native 支付订单创建功能

**前置条件**: 已获取有效 Token

**测试步骤**:
1. 使用 Token 调用 PayDepositNative 接口
2. 传入金额: 51
3. 验证返回的订单信息
4. 查询数据库验证订单创建

**API 调用**:
```bash
GET http://localhost:12580/api/services/app/Client/PayDepositNative?amount=51
Authorization: Bearer {accessToken}
```

**验证点**:
- 返回 success: true
- code_url 不为空（格式: weixin://wxpay/bizpayurl?pr=xxx）
- outTradeNo 不为空（格式: Ulid，最长48字符）
- amount = 51

**数据库验证**:
```sql
SELECT Id, OutTradeNo, State, Total, HostType, PayType, AppName
FROM Pays_PayOrder
WHERE OutTradeNo = '{outTradeNo}'
ORDER BY CreationTime DESC
LIMIT 1;
```

**预期结果**:
- State = 0 (未支付)
- Total = 5100 (分)
- HostType = 2 (保证金)
- PayType = 1 (微信)
- AppName = 'pub'

---

### 4.3 支付成功处理测试

**测试目标**: 验证支付成功后的订单状态更新和余额充值

**前置条件**: 订单已创建（步骤 4.2）

**测试步骤**:
1. 模拟支付成功，更新订单状态
2. 创建充值记录
3. 更新用户保证金余额
4. 验证所有数据变更

**数据库操作**:

**步骤 1: 更新订单状态**
```sql
UPDATE Pays_PayOrder 
SET State = 1,
    IsSuccessPay = 1,
    SuccessPayTime = NOW(),
    TransactionId = 'TEST_TX_{timestamp}'
WHERE OutTradeNo = '{outTradeNo}';
```

**步骤 2: 创建充值记录**
```sql
INSERT INTO UserDepositLogs (Id, Amount, Type, CreatorUserId, TenantId, CreationTime)
VALUES (UUID(), 50.00, 1, 7509, 1, NOW());
```

**步骤 3: 更新保证金余额**
```sql
UPDATE AbpUsers 
SET DepositBalance = DepositBalance + 50.00
WHERE Id = 7509;
```

**验证点**:
- 订单状态更新为已支付 (State=1)
- 用户保证金余额增加 50.00
- 充值记录创建成功

**查询验证**:
```sql
-- 验证订单状态
SELECT State, IsSuccessPay, TransactionId 
FROM Pays_PayOrder 
WHERE OutTradeNo = '{outTradeNo}';

-- 验证用户余额
SELECT DepositBalance 
FROM AbpUsers 
WHERE Id = 7509;

-- 验证充值记录
SELECT Amount, Type, CreationTime 
FROM UserDepositLogs 
WHERE CreatorUserId = 7509 
ORDER BY CreationTime DESC 
LIMIT 1;
```

---

### 4.4 订单状态流转测试

**测试目标**: 验证订单状态机流转正确性

**测试步骤**:
1. 创建订单 → 未支付
2. 支付成功 → 已支付
3. 申请退款 → 退款中
4. 退款完成 → 已退款

**数据库验证**:
```sql
-- 初始状态
SELECT State FROM Pays_PayOrder WHERE OutTradeNo = '{outTradeNo}';
-- 预期: 0 (未支付)

-- 支付成功后
UPDATE Pays_PayOrder SET State = 1, IsSuccessPay = 1 WHERE OutTradeNo = '{outTradeNo}';
SELECT State FROM Pays_PayOrder WHERE OutTradeNo = '{outTradeNo}';
-- 预期: 1 (已支付)

-- 退款申请
UPDATE Pays_PayOrder SET State = 2, IsRefund = 1 WHERE OutTradeNo = '{outTradeNo}';
SELECT State FROM Pays_PayOrder WHERE OutTradeNo = '{outTradeNo}';
-- 预期: 2 (退款中)

-- 退款完成
UPDATE Pays_PayOrder SET State = 3, RefundComplateTime = NOW() WHERE OutTradeNo = '{outTradeNo}';
SELECT State FROM Pays_PayOrder WHERE OutTradeNo = '{outTradeNo}';
-- 预期: 3 (已退款)
```

---

### 4.5 异常场景测试

#### 4.5.1 无效用户测试

**测试目标**: 验证无效用户的错误处理

**测试数据**: userId = 99999 (不存在)

**API 调用**:
```bash
POST http://localhost:12580/api/TokenAuth/GenerateTokenForUser
Content-Type: application/json

{
  "userId": 99999
}
```

**预期结果**:
- 返回 success: false
- error.message 包含"用户不存在"

---

#### 4.5.2 未授权访问测试

**测试目标**: 验证未授权请求被拒绝

**API 调用**:
```bash
GET http://localhost:12580/api/services/app/Client/PayDepositNative?amount=51
# 无 Authorization header
```

**预期结果**:
- HTTP 401 Unauthorized
- 或返回 unAuthorizedRequest: true

---

#### 4.5.3 无效金额测试

**测试目标**: 验证金额参数验证

**测试数据**: amount = -1

**API 调用**:
```bash
GET http://localhost:12580/api/services/app/Client/PayDepositNative?amount=-1
Authorization: Bearer {accessToken}
```

**预期结果**:
- 返回 success: false
- 或订单金额为 0

---

## 5. 完整测试流程

### 5.1 端到端测试流程

```
步骤 1: 生成用户 Token
   ↓
步骤 2: 查询用户初始余额
   ↓
步骤 3: 创建支付订单
   ↓
步骤 4: 验证订单创建成功
   ↓
步骤 5: 模拟支付成功
   ↓
步骤 6: 更新用户余额
   ↓
步骤 7: 验证最终状态
   ↓
步骤 8: 生成测试报告
```

### 5.2 测试清理

**测试完成后执行**:
```sql
-- 删除测试订单
DELETE FROM Pays_PayOrder WHERE OutTradeNo LIKE 'TEST_%';

-- 恢复用户余额
UPDATE AbpUsers SET DepositBalance = 0 WHERE Id = 7509;

-- 删除测试充值记录
DELETE FROM UserDepositLogs WHERE CreatorUserId = 7509 AND Amount = 50.00;
```

## 6. 测试验证清单

### 6.1 功能验证

- [ ] Token 生成成功
- [ ] 订单创建成功
- [ ] code_url 格式正确
- [ ] outTradeNo 唯一且格式正确
- [ ] 订单初始状态为未支付
- [ ] 支付成功后状态更新为已支付
- [ ] 用户保证金余额正确更新
- [ ] 充值记录创建成功

### 6.2 数据验证

- [ ] 订单金额正确（分转元）
- [ ] 手续费扣除正确
- [ ] 订单类型正确（保证金）
- [ ] 支付方式正确（微信）
- [ ] 应用配置正确（pub）

### 6.3 异常处理

- [ ] 无效用户返回错误
- [ ] 未授权请求被拒绝
- [ ] 数据库异常有日志记录
- [ ] API 错误信息清晰

## 7. 测试输出

### 7.1 测试报告格式

```markdown
# 支付功能测试报告

**测试时间**: {timestamp}
**测试人员**: AI Agent
**测试环境**: 本地开发环境

## 测试结果摘要
- 总用例数: X
- 通过数: X
- 失败数: X
- 通过率: X%

## 详细结果

### 4.1 用户认证测试
- 状态: ✅ 通过
- Token 生成成功
- 用户信息正确

### 4.2 支付订单创建测试
- 状态: ✅ 通过
- 订单号: {outTradeNo}
- 订单状态: 未支付

### 4.3 支付成功处理测试
- 状态: ✅ 通过
- 充值前余额: ¥0.00
- 充值后余额: ¥50.00
- 充值金额: ¥50.00

## 数据验证
- 订单记录: 已创建
- 充值记录: 已创建
- 余额更新: 已完成

## 问题记录
- 无

## 建议
- 后续添加真实支付测试
- 添加并发支付测试
```

## 8. 附录

### 8.1 API 接口列表

| 接口 | 方法 | 路径 | 说明 |
|------|------|------|------|
| 生成 Token | POST | /api/TokenAuth/GenerateTokenForUser | 管理员生成用户 Token |
| 创建订单 | GET | /api/services/app/Client/PayDepositNative | Native 扫码支付 |
| 支付回调 | POST | /api/PayNotify/TenPay/{appName} | 微信支付回调 |

### 8.2 数据库表结构

**Pays_PayOrder**: 支付订单表
- Id: 订单ID (Ulid)
- OutTradeNo: 商户订单号
- State: 订单状态
- Total: 订单金额（分）
- HostType: 订单类型
- IsSuccessPay: 是否支付成功

**AbpUsers**: 用户表
- Id: 用户ID
- DepositBalance: 保证金余额

**UserDepositLogs**: 保证金日志表
- Id: 日志ID
- Amount: 金额
- Type: 类型
- CreatorUserId: 创建用户ID

### 8.3 状态码说明

**PayState (订单状态)**:
- -1: 取消
- 0: 未支付
- 1: 已支付
- 2: 退款中
- 3: 已退款
- 4: 部分退款

**OrderType (订单类型)**:
- 1: 充值
- 2: 保证金

**PayType (支付方式)**:
- 1: 微信
- 2: 微信扫码

---

## 执行说明

本文档供 AI Agent 执行测试使用：

1. **读取文档**: 解析测试规范和测试用例
2. **检查环境**: 验证测试前置条件
3. **执行测试**: 按顺序执行测试用例
4. **记录结果**: 记录每个步骤的执行结果
5. **生成报告**: 输出标准化的测试报告

**注意事项**:
- 严格按照测试步骤执行
- 每个验证点都要检查
- 异常情况要详细记录
- 测试完成后清理数据