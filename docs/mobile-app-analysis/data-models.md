# 数据模型与实体关系

> Entity 层分析与实体关系总览，用于 Flutter App 开发数据对接。
> 更新时间: 2026-05-22

---

## 一、实体继承体系

```
AbpUser<TUser> (ABP框架)
  └── User (TtWork.Abp.Authorization.Users)
       扩展: HeadImgUrl, Balance, DepositBalance, Qq, Wx, SkipProfileCompletion

Entity<TPrimaryKey> (ABP 基础)
├── Entity<long>
│   ├── + ICreationAudited → BanedUser
│   ├── + IHasCreationTime + IHasModificationTime → ChatChannel (Id=long)
│   ├── ChatGroup (CreationAuditedEntity<long>)
│   ├── PushSubscription (CreationAuditedEntity<long>)
│   ├── GroupChatLevelSetting (Entity<int>)
│   ├── UserFriend (Entity<int>)
│   └── ChatListDelete (Entity<int> + IHasCreationTime)
├── Entity<Guid>
│   └── Message (Entity<Guid>)
├── CreationAuditedEntity<long>
│   ├── BidHistory
│   ├── ChatGroup
│   └── PushSubscription
├── AuditedEntity<long> (创建+修改)
│   ├── AuthRequest
│   └── SmsVerificationCode
├── FullAuditedEntity<long> (创建+修改+软删除)
│   └── Announce
├── FullAuditedAggregateRoot<long>
│   ├── AuctionItem
│   ├── CmsArticle
│   ├── CmsCategory
│   └── AppRelease
├── FullAuditedAggregateRoot<Ulid>
│   └── PayOrder (还实现了 IMustHaveTenant, IExtendableObject)
```

---

## 二、枚举定义汇总

### 枚举速查表（移动端需要处理的）

| 枚举 | 值范围 | 使用场景 | 移动端处理 |
|------|--------|---------|-----------|
| `AuctionStatusEnum` | 0-128 (位标志) | 拍卖品状态 | 展示对应状态文案 |
| `PayState` | -1, 0, 1, 3 | 支付订单状态 | 展示支付状态 |
| `OrderType` | 1, 2 | 订单类型(充值/保证金) | 区分用途 |
| `ChatMessageType` | Text/Image/System/Goods/Order | 消息类型 | 渲染不同UI |
| `ChatChannelType` | 1-3 | 频道类型(私聊/系统/群聊) | 会话列表区分 |
| `ChatChannelStatus` | 0-3 | 会话可见性 | 控制列表显示 |
| `AuthRequestStatus` | 0-3 | 扫码登录状态机 | 扫码页状态 |
| `SmsCodePurpose` | 1-3 | 验证码用途 | 登录/改密/绑手机 |
| `PayType` | 1, 2 | 支付方式 | 微信支付 |

### 各枚举详细值

```dart
// Dart 版本，供 Flutter App 参考

enum AuctionStatus {
  draft = 0,
  listed = 1,
  auctioning = 2,
  deal = 4,         // 已成交（有最高出价）
  tradeSuccess = 8, // 交易完成
  sellerDefault = 16,
  buyerDefault = 32,
  closed = 128,
}

enum PayState {
  cancelled = -1,
  unpaid = 0,
  paid = 1,
  refunded = 3,
}

enum OrderType {
  recharge = 1,     // 充值
  deposit = 2,      // 保证金
}

enum ChatMessageType {
  text = 0,
  image = 1,
  system = 2,
  goods = 3,
  order = 4,
}

enum ChatChannelType {
  private = 1,
  system = 2,
  group = 3,
}

enum ChatChannelStatus {
  normal = 0,
  deleted = 1,
  pinned = 2,    // 预留
  muted = 3,     // 预留
}

enum AuthRequestStatus {
  pending = 0,
  scanned = 1,
  confirmed = 2,
  expired = 3,
}

enum SmsCodePurpose {
  login = 1,
  bindPhone = 2,
  resetPassword = 3,
}
```

---

## 三、关键实体间关系

### 3.1 用户 ↔ 聊天消息

```
User (1) ──────────→ Message (N)    用户发送的消息
User (1) ──────────→ UserFriend (N)  好友关系
User (1) ──────────→ ChatChannel (N) 参与的频道（通过 User1Id/User2Id）
User (1) ──────────→ ChatListDelete (N) 删除的会话记录
```

### 3.2 用户 ↔ 拍卖

```
User (1) ──────────→ AuctionItem (N)    作为卖家/出价人
AuctionItem (1) ────→ BidHistory (N)    出价记录
User (1) ──────────→ BidHistory (N)     出价记录（通过 CreatorUserId）
AuctionItem (1) ────→ AuctionStartNotify (N) 开拍通知
```

### 3.3 用户 ↔ 支付

```
User (1) ──────────→ PayOrder (N)       支付订单
PayOrder (1) ───────→ UserBalanceLog (N) 余额变动
PayOrder (1) ───────→ UserDepositLog (N) 保证金变动
PayOrder (1) ───────→ WechatPayNotify (N) 微信回调记录
```

### 3.4 内容分类

```
CmsCategory (1) ────→ CmsArticle (N)   文章按分类归属
```

---

## 四、ABP 审计与多租户

### 审计字段说明

```dart
// 创建审计接口 — ICreationAudited
int? creatorUserId;    // 创建者 ID（对应 AbpUsers.Id）
String creationTime;   // ISO 8601 格式

// 修改审计接口 — IModificationAudited  
int? lastModifierUserId;
String? lastModificationTime;

// 软删除审计接口 — IDeletionAudited
bool isDeleted = false;
int? deleterUserId;
String? deletionTime;
```

### 软删除处理

`FullAuditedAggregateRoot` 的实体（AuctionItem、CmsArticle、CmsCategory、PayOrder、Announce、AppRelease），在查询时需要过滤 `IsDeleted == false`。ABP Repository 默认会添加 `WHERE IsDeleted = 0` 条件。

### 多租户

`PayOrder` 实现了 `IMustHaveTenant`（`TenantId` 字段），当前为单租户运行，`TenantId` 通常为 1。

---

## 五、数据类型映射

| C# 类型 | 数据库类型 | Flutter/Dart 类型 | 备注 |
|---------|-----------|-------------------|------|
| `long` | `INTEGER` / `BIGINT` | `int` | 主键类型 |
| `int` | `INTEGER` | `int` | 枚举、金额(分) |
| `Guid` | `TEXT` / `CHAR(36)` | `String` | 消息 ID |
| `Ulid` | `TEXT` / `CHAR(26)` | `String` | 支付订单 ID |
| `string` | `TEXT` / `VARCHAR(N)` | `String` | 字符串 |
| `bool` | `INTEGER` (0/1) | `bool` | 布尔值 |
| `DateTime` | `TEXT` / `datetime` | `String` (ISO8601) | 时间 |
| `decimal(18,2)` | `TEXT` / `DECIMAL` | `double` / `int`(分) | 余额/保证金 |
| `long` | `INTEGER` / `BIGINT` | `int` | 时间戳(ms) |
| `string` (JSON) | `TEXT` | `Map<String, dynamic>` | 扩展数据 |

### 重要说明

1. **金额单位**: 拍卖出价和支付金额使用 **分**（整数），余额和保证金使用 **元**（decimal）
   - `AuctionItem.StartingPrice` — 分 (int)
   - `AuctionItem.CurrentPrice` — 分 (int?)
   - `PayOrder.Total` — 分 (int)
   - `User.Balance` — 元 (decimal)
   - `User.DepositBalance` — 元 (decimal)

2. **时间格式**: 两种形式共存
   - Unix 毫秒时间戳（`Message.Time`, `ChatChannel.LastMessageTime`）— `long` 类型
   - ISO 8601 字符串（ABP 审计字段）— `DateTime` 类型

3. **枚举传输**: 后端返回枚举时使用 `StringEnumConverter` JSON 序列化（如 `"PAID"`），接受时使用整数
