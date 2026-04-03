# 竞拍流程测试规范

## 1. 测试概述

### 1.1 测试目标
验证拍卖竞拍功能的完整性，包括：
- 竞拍出价
- 价格更新
- 竞拍历史记录
- 保证金检查
- 竞拍资格验证

### 1.2 测试范围

| 模块 | 测试内容 | 测试类型 |
|------|---------|---------|
| 拍卖 API | 出价接口 | API 测试 |
| 业务逻辑 | 价格更新 | 业务测试 |
| 数据库 | 竞拍记录 | 数据验证 |

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
保证金余额: 50.00 元（需满足竞拍条件）
```

### 2.2 测试拍卖商品
```
商品ID: {拍卖商品ID}
商品名称: 测试商品
起拍价: 100.00 元
当前价: NULL
状态: 拍卖中
```

### 2.3 竞拍参数
```
出价金额: 150.00 元
加价幅度: 10.00 元
```

## 3. 测试前置条件

### 3.1 环境检查
- [ ] 后端服务运行正常
- [ ] 测试用户存在
- [ ] 用户保证金充足
- [ ] 拍卖商品存在且状态为拍卖中

### 3.2 数据准备

**创建测试拍卖商品**:
```sql
INSERT INTO AuctionItems (
  Id, Name, StartingPrice, CurrentPrice, Status, 
  TenantId, CreatorUserId, CreationTime
)
VALUES (
  {ID}, '测试商品', 100.00, NULL, 2,
  1, 7509, NOW()
);
```

**设置用户保证金**:
```sql
UPDATE AbpUsers 
SET DepositBalance = 50.00 
WHERE Id = 7509;
```

## 4. 测试用例

### 4.1 正常竞拍测试

**测试目标**: 验证正常竞拍流程

**前置条件**: 
- 用户已登录
- 用户保证金充足
- 拍卖商品存在

**测试步骤**:
1. 查询拍卖商品当前价
2. 用户出价竞拍
3. 验证价格更新
4. 验证竞拍历史记录

**API 调用**:
```bash
POST http://localhost:12580/api/services/app/Auction/Bid
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "auctionItemId": {商品ID},
  "bidPrice": 150.00
}
```

**验证点**:
- 返回 success: true
- 商品当前价更新为 150.00
- 当前价用户ID = 7509
- 竞拍历史记录创建

**数据库验证**:
```sql
-- 验证商品价格
SELECT CurrentPrice, CurrentPriceUserId, CurrentPriceUserName
FROM AuctionItems
WHERE Id = {商品ID};

-- 验证竞拍历史
SELECT BidPrice, BidUserName, CreatorUserId, CreationTime
FROM BidHistories
WHERE AuctionItemId = {商品ID}
ORDER BY CreationTime DESC
LIMIT 1;
```

**预期结果**: 
- 商品当前价 = 150.00
- 竞拍历史记录生成

---

### 4.2 保证金不足竞拍测试

**测试目标**: 验证保证金不足时的竞拍拒绝

**前置条件**: 
- 用户保证金不足（< 50元）
- 拍卖商品存在

**测试步骤**:
1. 设置用户保证金为 30.00 元
2. 尝试出价竞拍
3. 验证竞拍被拒绝

**数据准备**:
```sql
UPDATE AbpUsers SET DepositBalance = 30.00 WHERE Id = 7509;
```

**API 调用**:
```bash
POST http://localhost:12580/api/services/app/Auction/Bid
Authorization: Bearer {accessToken}

{
  "auctionItemId": {商品ID},
  "bidPrice": 150.00
}
```

**验证点**:
- 返回 success: false
- error.message 包含"保证金不足"
- 商品价格未更新

**预期结果**: 竞拍被拒绝，提示保证金不足

---

### 4.3 出价低于当前价测试

**测试目标**: 验证出价低于当前价时被拒绝

**前置条件**: 
- 商品当前价 = 150.00
- 用户保证金充足

**测试步骤**:
1. 尝试出价 100.00 元
2. 验证竞拍被拒绝

**API 调用**:
```bash
POST http://localhost:12580/api/services/app/Auction/Bid
Authorization: Bearer {accessToken}

{
  "auctionItemId": {商品ID},
  "bidPrice": 100.00
}
```

**验证点**:
- 返回 success: false
- error.message 包含"出价必须高于当前价"
- 商品价格保持 150.00

**预期结果**: 竞拍被拒绝

---

### 4.4 连续竞拍测试

**测试目标**: 验证连续多次出价的价格更新

**前置条件**: 
- 商品起拍价 100.00 元
- 用户保证金充足

**测试步骤**:
1. 用户A出价 110.00 元
2. 用户B出价 120.00 元
3. 用户A再次出价 130.00 元
4. 验证每次价格更新正确

**数据库验证**:
```sql
-- 查询竞拍历史
SELECT BidPrice, BidUserName, CreationTime
FROM BidHistories
WHERE AuctionItemId = {商品ID}
ORDER BY CreationTime ASC;

-- 验证商品当前价
SELECT CurrentPrice, CurrentPriceUserName
FROM AuctionItems
WHERE Id = {商品ID};
```

**验证点**:
- 当前价 = 130.00
- 当前价用户 = 用户A
- 竞拍历史记录3条

**预期结果**: 每次出价都正确更新价格

---

## 5. 竞拍状态流转

```
拍卖中 → 用户出价 → 价格更新 → 竞拍历史记录
                                    ↓
                              等待下一次出价
                                    ↓
                            拍卖结束 → 成交/流拍
```

## 6. 测试验证清单

### 6.1 功能验证

- [ ] 正常出价成功
- [ ] 价格更新正确
- [ ] 竞拍历史记录生成
- [ ] 保证金不足被拒绝
- [ ] 出价过低被拒绝
- [ ] 连续出价处理正确

### 6.2 数据验证

- [ ] 商品价格正确
- [ ] 竞拍历史完整
- [ ] 用户信息正确

## 7. 测试清理

```sql
-- 删除测试拍卖商品
DELETE FROM AuctionItems WHERE Name = '测试商品';

-- 删除竞拍历史
DELETE FROM BidHistories WHERE AuctionItemId IN (
  SELECT Id FROM AuctionItems WHERE Name = '测试商品'
);

-- 恢复用户保证金
UPDATE AbpUsers SET DepositBalance = 0 WHERE Id = 7509;
```

---

**最后更新**: 2026-04-03