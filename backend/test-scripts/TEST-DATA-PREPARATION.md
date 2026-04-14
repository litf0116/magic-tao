# 测试数据准备文档

**创建时间**: 2026-04-04
**目的**: 为 API 接口测试准备完整的测试数据

---

## 测试账号

| 账号 | 密码 | 角色 | 用户ID | 余额 | 保证金余额 |
|------|------|------|--------|------|-----------|
| 18012341234 | 123456 | 管理员 | 2 | 5000.00 | 0.00 |
| feifei | 123456 | 普通用户 | 7509 | 1000.00 | 50.00 |

---

## 测试数据清单

### 1. 公告数据 (t_announce)

| ID | 内容 | 排序 |
|----|------|------|
| 13 | 欢迎使用摩力淘拍卖系统，这是一个测试公告。 | 1 |
| 14 | 系统维护通知：本周日凌晨2点将进行系统维护。 | 2 |
| 15 | 新功能上线：支持微信支付充值。 | 3 |

### 2. 拍卖商品数据 (T_AuctionItem)

| ID | 名称 | 状态 | 起拍价 | 用途 |
|----|------|------|--------|------|
| 17394 | 测试拍卖商品_拍卖中_01 | 拍卖中 (2) | 100 | 出价测试 |
| 17395 | 测试拍卖商品_拍卖中_02 | 拍卖中 (2) | 200 | 出价测试 |
| 17396 | 测试拍卖商品_上架_01 | 上架 (1) | 300 | 开始拍卖测试 |

### 3. 分类数据 (t_cmscategory)

| ID | 标题 |
|----|------|
| 1 | 首页轮播图 |

---

## 数据准备 SQL 脚本

```sql
-- ============================================
-- 测试数据准备脚本
-- ============================================

-- 1. 设置用户余额
UPDATE AbpUsers SET Balance = 1000.00 WHERE Id = 7509;  -- feifei
UPDATE AbpUsers SET Balance = 5000.00 WHERE Id = 2;      -- admin

-- 2. 创建公告
INSERT INTO t_announce (CategoryId, Content, ImageUrl, Sort, CreationTime, CreatorUserId, IsDeleted)
VALUES 
  (1, '欢迎使用摩力淘拍卖系统，这是一个测试公告。', 'https://picsum.photos/seed/announce1/800/400', 1, NOW(), 2, 0),
  (1, '系统维护通知：本周日凌晨2点将进行系统维护。', 'https://picsum.photos/seed/announce2/800/400', 2, NOW(), 2, 0),
  (1, '新功能上线：支持微信支付充值。', 'https://picsum.photos/seed/announce3/800/400', 3, NOW(), 2, 0);

-- 3. 创建拍卖商品
-- 状态: 1=上架, 2=拍卖中, 3=已成交
INSERT INTO T_AuctionItem (
  Name, Status, ImageUrl, Description, StartingPrice, SellerInfo, SellerId, 
  `Order`, CreationTime, CreatorUserId, IsDeleted
) VALUES 
  ('测试拍卖商品_拍卖中_01', 2, 'https://picsum.photos/seed/auction101/400/300', '这是一个正在拍卖的商品，品质优良。', 100, '测试卖家信息', 2, 1, NOW(), 2, 0),
  ('测试拍卖商品_拍卖中_02', 2, 'https://picsum.photos/seed/auction102/400/300', '精美拍品，值得收藏。', 200, '测试卖家信息', 2, 2, NOW(), 2, 0),
  ('测试拍卖商品_上架_01', 1, 'https://picsum.photos/seed/auction201/400/300', '即将上架拍卖的商品。', 300, '测试卖家信息', 2, 3, NOW(), 2, 0);

-- 4. 创建缺失的表（如果不存在）
CREATE TABLE IF NOT EXISTS t_withdrawalamount (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    Amount DECIMAL(18,2) NOT NULL,
    UserId INT NOT NULL,
    Status INT NOT NULL DEFAULT 1,
    WithdrawalTime DATETIME(6) NOT NULL,
    CreationTime DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CreatorUserId BIGINT NULL,
    IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
    INDEX IX_t_withdrawalamount_UserId (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

---

## 验证测试数据 (通过 API)

### 1. 登录获取 Token

```bash
# 管理员登录
curl -X POST "http://127.0.0.1:12580/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress": "18012341234", "password": "123456"}'

# 普通用户登录
curl -X POST "http://127.0.0.1:12580/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress": "feifei", "password": "123456"}'
```

### 2. 获取拍卖中商品

```bash
TOKEN="your-access-token"

curl "http://127.0.0.1:12580/api/services/app/AuctionItem/GetPublicList?Status=2&MaxResultCount=5" \
  -H "Authorization: Bearer $TOKEN"
```

### 3. 出价测试

```bash
curl -X POST "http://127.0.0.1:12580/api/services/app/AuctionItem/Bid" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"auctionItemId": 17394, "bidPrice": 150}'
```

### 4. 提现测试

```bash
curl -X POST "http://127.0.0.1:12580/api/services/app/Client/PayWithdrawal" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"amount": 100}'
```

### 5. 获取公告列表

```bash
curl "http://127.0.0.1:12580/api/services/app/Announce/GetAll?MaxResultCount=5" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 测试结果验证

| 测试项 | 预期结果 | 实际结果 |
|--------|---------|---------|
| 用户登录 | 返回 accessToken | ✅ 通过 |
| 获取拍卖中商品 | 返回 2+ 条记录 | ✅ 通过 |
| 拍卖品详情 | 返回商品信息 | ✅ 通过 |
| 出价 | success: true | ✅ 通过 |
| 提现 | success: true | ✅ 通过 |
| 获取公告 | 返回 3 条记录 | ✅ 通过 |

---

## 数据状态说明

### 拍卖商品状态 (Status)

| 值 | 状态 | 说明 |
|----|------|------|
| 1 | 上架 | 商品已上架，等待开始拍卖 |
| 2 | 拍卖中 | 正在进行拍卖，可以出价 |
| 3 | 已成交 | 拍卖结束，有成交用户 |
| 4 | 流拍 | 拍卖结束，无人出价 |

### 用户余额说明

- **Balance**: 可用余额，可用于提现
- **DepositBalance**: 保证金余额，拍卖时冻结

---

## 注意事项

1. **测试数据清理**: 测试完成后可以删除测试数据
   ```sql
   DELETE FROM t_announce WHERE Id >= 13 AND Id <= 15;
   DELETE FROM T_AuctionItem WHERE Id >= 17394 AND Id <= 17396;
   DELETE FROM t_withdrawalamount WHERE UserId IN (2, 7509);
   ```

2. **余额重置**: 如需重新测试，可以重置用户余额
   ```sql
   UPDATE AbpUsers SET Balance = 1000.00, DepositBalance = 50.00 WHERE Id = 7509;
   UPDATE AbpUsers SET Balance = 5000.00, DepositBalance = 0.00 WHERE Id = 2;
   ```

3. **支付功能**: PayDeposit 需要微信支付配置，暂无法测试

4. **禁言功能**: BanedUser/Create 接口已禁用 (NOT SUPPORTED)