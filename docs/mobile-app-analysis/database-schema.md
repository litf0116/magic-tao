# 数据库 Schema 总览

> 基于 EF Core 实体和迁移文件分析，用于移动端 Flutter App 开发参考。
> 更新时间: 2026-05-22

---

## 一、总览

### ABP 框架基础表

| 表名 | 用途 |
|------|------|
| `AbpUsers` | 用户账号体系（继承自 `AbpUser`） |
| `AbpRoles` | 角色 |
| `AbpUserRoles` | 用户-角色关联 |
| `AbpUserLogins` | 第三方登录（微信 OpenId 等） |
| `AbpUserTokens` | 用户身份令牌 |
| `AbpSettings` | 系统设置键值存储 |
| `AbpPermissions` | 权限定义 |
| `AbpTenants` | 多租户（当前为单租户模式） |
| `AbpAuditLogs` | 审计日志 |
| `AbpBackgroundJobs` | ABP 后台作业队列 |
| `AbpLanguages` / `AbpLanguageTexts` | 多语言 |
| `AbpNotifications` / `AbpNotificationSubscriptions` | 通知 |
| `AbpOrganizationUnits` | 组织单元 |
| `AbpEntityChangeSets` / `AbpEntityChanges` | 实体变更审计 |

### 业务表一览

| # | 表名 | 中文名 | 模块 |
|---|------|--------|------|
| 1 | `T_AuctionItem` | 拍卖品 | 拍卖 |
| 2 | `T_BidHistory` | 出价记录 | 拍卖 |
| 3 | `T_Message` | 聊天消息 | 即时通讯 |
| 4 | `T_UserFriend` | 好友关系 | 即时通讯 |
| 5 | `T_ChatChannel` | 聊天频道 | 即时通讯 |
| 6 | `T_ChatListDelete` | 会话删除记录 | 即时通讯 |
| 7 | `T_ChatGroups` | 群聊 | 即时通讯 |
| 8 | `T_GroupChatLevelSettings` | 群聊等级设置 | 即时通讯 |
| 9 | `T_ChatEmoji` | 聊天表情 | 即时通讯 |
| 10 | `T_Announce` | 公告 | 内容 |
| 11 | `T_CmsArticle` | CMS 文章 | 内容 |
| 12 | `T_CmsCategory` | CMS 分类 | 内容 |
| 13 | `Pays_PayOrder` | 支付订单 | 支付 |
| 14 | `T_UserBalanceLog` | 余额变动记录 | 支付 |
| 15 | `T_UserDepositLog` | 保证金变动记录 | 支付 |
| 16 | `T_UserAvatarHistory` | 头像历史 | 支付 |
| 17 | `T_WechatPayNotify` | 微信支付回调 | 支付 |
| 18 | `T_AuthRequest` | 扫码登录授权 | 认证 |
| 19 | `T_SmsVerificationCode` | 短信验证码 | 认证 |
| 20 | `T_BanedUsers` | 禁言用户 | 聊天管理 |
| 21 | `T_PushSubscription` | WebPush 订阅 | 推送 |
| 22 | `AppReleases` | App 版本发布 | 运维 |
| 23 | `T_SensitiveWord` | 敏感词 | 审核 |
| 24 | `T_AuctionStartNotify` | 拍卖开始通知 | 通知 |

---

## 二、核心业务表详细结构

### 2.1 用户体系

#### `AbpUsers`

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `long` | 主键 | ✅ 用户标识 |
| `UserName` | `string(256)` | 用户名 | ✅ 显示/登录 |
| `Name` | `string(64)` | 姓名 | ✅ 昵称 |
| `Surname` | `string(64)` | 姓氏 | |
| `EmailAddress` | `string(256)` | 邮箱 | |
| `PhoneNumber` | `string(32)` | 手机号 | ✅ 登录/验证 |
| `IsActive` | `bool` | 是否激活 | ✅ 检查 |
| `IsEmailConfirmed` | `bool` | 邮箱验证 | |
| `IsPhoneNumberConfirmed` | `bool` | 手机验证 | |
| `Password` | — | 密码哈希 | |
| `LastLoginTime` | `datetime` | 最后登录时间 | |
| `LockoutEndDateUtc` | `datetime` | 锁定截止 | |
| `AccessFailedCount` | `int` | 登录失败次数 | |
| `TenantId` | `int` | 租户 ID | |

**自定义扩展字段**（在 `TtWork.Abp.Authorization.Users.User` 中）:

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `HeadImgUrl` | `string(256)` | 头像 URL | ✅ 用户头像显示 |
| `Qq` | `string(32)` | QQ号 | |
| `Wx` | `string(32)` | 微信号 | ✅ 联系信息 |
| `Balance` | `decimal(18,2)` | 账户余额 | ✅ 钱包显示 |
| `DepositBalance` | `decimal(18,2)` | 保证金 | ✅ 拍卖场景 |
| `SkipProfileCompletion` | `bool` | 跳过完善引导 | |

### 2.2 聊天/即时通讯

#### `T_Message` — 聊天消息

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `Guid` | 主键 | ✅ |
| `Type` | `ChatMessageType` (int) | 消息类型枚举 | ✅ 解析类型 |
| `Chan` | `string(64)` | 频道 ID | ✅ 定位会话 |
| `From` | `long` | 发送者 ID | ✅ |
| `FromName` | `string(64)` | 发送者名称 | ✅ 显示 |
| `FromAdmin` | `bool` | 是否管理员 | |
| `FromTag` | `string(32)` | 发送者标签 | ✅ 身份标识 |
| `TagClass` | `string(32)` | 标签样式类 | |
| `Avatar` | `string(128)` | 发送者头像 | ✅ |
| `To` | `long?` | 接收者 ID | |
| `Time` | `long` | 时间戳(ms) | ✅ 时间显示 |
| `Msg` | `string(2048)` | 消息内容 | ✅ 核心内容 |
| `Payload` | `text` | JSON 扩展数据 | ✅ 图片/商品卡片 |
| `Receipt` | `string(64)` | 回执 ID | |
| `Ip` | `string(64)` | 发送者 IP | |
| `SequenceNumber` | `long` | 序列号(顺序保证) | ✅ 消息排序 |

**`ChatMessageType` 枚举**（源自 FreeIM 库）:

| 值 | 说明 | Payload 格式 |
|----|------|-------------|
| `Text` | 文本消息 | — |
| `Image` | 图片消息 | `{url: string}` |
| `System` | 系统消息 | — |
| `Goods` | 商品卡片 | 商品 JSON |
| `Order` | 订单信息 | 订单 JSON |

**频道格式说明**:
- 私聊: `private_{smallerId}_{largerId}`
- 系统: `auction`, `lobby` 等

#### `T_UserFriend` — 好友关系

| 字段 | 类型 | 说明 |
|------|------|------|
| `UserId` | `long` | 用户 ID |
| `FriendId` | `long` | 好友 ID |
| `Remark` | `string(64)` | 备注名 |
| `Status` | `bool` | true=已接受, false=待验证 |

#### `T_ChatChannel` — 聊天频道

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `long` | 主键 | |
| `ChannelId` | `string(128)` | 频道唯一标识 | ✅ 连接 SignalR |
| `ChannelType` | `ChatChannelType` | 1=私聊, 2=系统, 3=群聊 | ✅ |
| `ChannelName` | `string(128)` | 频道名称 | ✅ 标题 |
| `User1Id` / `User2Id` | `long?` | 私聊参与者 | |
| `User1Status` / `User2Status` | `ChatChannelStatus` | 各用户会话状态 | ✅ 删除状态 |
| `LastMessageId` | `Guid?` | 最后消息 ID | |
| `LastMessageContent` | `string(2048)` | 最后消息摘要 | ✅ 列表预览 |
| `LastMessageFromId` | `long?` | 最后消息发送者 | |
| `LastMessageFromName` | `string(64)` | 最后消息发送者名 | |
| `LastMessageFromAvatar` | `string(512)` | 最后消息发送者头像 | |
| `LastMessageTime` | `long` | 最后消息时间戳 | ✅ 排序 |
| `MessageCount` | `int` | 消息总数 | |
| `IsActive` | `bool` | 有效状态 | |
| `SortOrder` | `int` | 排序权重 | |

**`ChatChannelStatus`**:
- `Normal = 0` — 正常显示
- `Deleted = 1` — 已删除（隐藏）
- `Pinned = 2` — 已置顶（预留）
- `Muted = 3` — 已静音（预留）

#### `T_ChatListDelete` — 会话删除记录

| 字段 | 类型 | 说明 |
|------|------|------|
| `UserId` | `long` | 操作者 |
| `ToUserId` | `long` | 对方用户 |
| `CreationTime` | `datetime` | 删除时间 |

> 当发送新消息时自动清除对应的删除记录。

#### `T_ChatGroups` — 群聊

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `Title` | `string(64)` | 群名称 |
| `Limit` | `int` | 人数上限（2-5人） |
| `IsHidden` | `bool` | 是否隐藏 |

#### `T_GroupChatLevelSettings` — 群聊等级

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `int` | 主键 |
| `Name` | `string` | 等级名称 |
| `Level` | `int` | 等级 |
| `AmountRequired` | `decimal(18,2)` | 所需金额 |
| `BorderColor` / `RightBorderColor` | `string?` | 边框颜色 |

### 2.3 拍卖

#### `T_AuctionItem` — 拍卖品

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `long` | 主键 | ✅ |
| `Name` | `string(128)` | 标题 | ✅ 列表/详情 |
| `Status` | `AuctionStatusEnum` (int) | 状态枚举 | ✅ 状态显示 |
| `ImageUrl` | `string(256)` | 图片 | ✅ 缩略图 |
| `Description` | `text` | 描述 | ✅ 详情 |
| `StartingPrice` | `int` | 起拍价（分） | ✅ 显示 |
| `CurrentPrice` | `int?` | 当前出价 | ✅ 竞价显示 |
| `CurrentPriceUserId` | `long?` | 当前出价人 ID | |
| `CurrentPriceUserName` | `string(64)` | 当前出价人 | |
| `FinalPrice` | `int?` | 成交价 | ✅ 成交页 |
| `DealTime` | `datetime?` | 成交时间 | |
| `DealUserId` | `long?` | 成交人 ID | |
| `DealUserName` | `string(64)` | 成交人 | |
| `SellerInfo` | `string(256)` | 卖家信息 | ✅ 联系方式 |
| `SellerId` | `long?` | 卖家 ID | |
| `Order` | `int` | 排序 | |
| ABP 审计字段 | — | `CreationTime`, `CreatorUserId` 等 | ✅ 发布时间 |

**`AuctionStatusEnum`（位标志风格枚举）**:

| 值 | 名称 | 说明 |
|----|------|------|
| 0 | 草稿 | 未发布 |
| 1 | 上架 | 可浏览未开始 |
| 2 | 拍卖中 | 正在竞价 |
| 4 | 已成交 | 有最高出价 |
| 8 | 交易成功 | 双方完成交易 |
| 16 | 卖家失约 | 卖家违约 |
| 32 | 买家失约 | 买家违约 |
| 128 | 交易关闭 | 取消 |

#### `T_BidHistory` — 出价记录

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `long` | 主键 | |
| `AuctionItemId` | `long` | 拍卖品 ID | ✅ |
| `BidPrice` | `int` | 出价金额（分） | ✅ 显示 |
| `BidTime` | `datetime` | 出价时间 | ✅ 时间线 |
| `BidUserName` | `string(64)` | 出价人 | |
| `BidUserAvatar` | `string(256)` | 出价人头像 | |
| `IsRollBack` | `bool` | 是否回滚 | |

### 2.4 支付

#### `Pays_PayOrder` — 支付订单

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `Ulid` | 主键 | ✅ |
| `TenantId` | `int` | 租户 ID | |
| `Total` | `int` | 金额（分） | ✅ 显示 |
| `OutTradeNo` | `string(48)` | 商户订单号 | |
| `OpenId` | `string(48)` | 微信 OpenId | |
| `MchId` | `string(32)` | 商户号 | |
| `AppId` | `string(32)` | AppId | |
| `HostType` | `OrderType` | 1=充值, 2=保证金 | ✅ 显示用途 |
| `HostId` | `string(48)` | 关联业务 ID | |
| `PayType` | `PayType` | 1=微信, 2=微信扫码 | |
| `State` | `PayState` | -1=取消, 0=未付, 1=已付, 3=已退款 | ✅ 支付状态 |
| `IsSuccessPay` | `bool` | 是否成功支付 | ✅ |
| `SuccessPayTime` | `datetime?` | 支付时间 | |
| `IsRefund` | `bool` | 是否退款 | |
| `RefundTime` | `datetime?` | 退款时间 | |
| `RefundPrice` | `int?` | 退款金额 | |
| `ShareFromUserId` | `int?` | 推荐人 | |
| `AppName` | `string(32)` | App 来源 | |
| `ExtensionData` | `string(512)` | JSON 扩展数据 | |
| ABP 审计字段 | — | 创建/修改/删除 | ✅ 创建时间 |

#### `T_UserBalanceLog` — 余额变动记录

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `UserId` | `long` | 用户 ID |
| `PayOrderId` | `Ulid?` | 关联支付单 |
| `Type` | `string` | 变动类型（充值/消费等） |
| `BeforeAmount` / `AfterAmount` | `decimal(18,2)` | 变动前后金额 |
| `ChangeAmount` | `decimal(18,2)` | 变动金额 |
| `Remark` | `string(256)` | 备注 |
| `CreationTime` | `datetime` | 创建时间 |

#### `T_UserDepositLog` — 保证金变动记录

结构与 `UserBalanceLog` 类似，记录保证金的变化历史。

### 2.5 认证

#### `T_AuthRequest` — 扫码登录授权

| 字段 | 类型 | 说明 | 移动端需要 |
|------|------|------|-----------|
| `Id` | `long` | 主键 | |
| `Code` | `string(64)` | 二维码随机码 | ✅ 扫码后传参 |
| `UserId` | `long` | PC 端用户 ID | |
| `Status` | `AuthRequestStatus` | 0=待扫描, 1=已扫描, 2=已确认, 3=已过期 | ✅ |
| `ScannedAt` | `datetime?` | 扫描时间 | |
| `ConfirmedAt` | `datetime?` | 确认时间 | |
| `ExpiresAt` | `datetime` | 过期时间 | |
| `CreationTime` | `datetime` | 创建时间 | |

> 扫码登录流程：App 扫描 PC 二维码 → 标记 Scanned → 用户确认 → 标记 Confirmed → PC 轮询获取 token

#### `T_SmsVerificationCode` — 短信验证码

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `PhoneNumber` | `string(20)` | 手机号 |
| `Code` | `string(6)` | 验证码 |
| `Purpose` | `SmsCodePurpose` | 1=登录, 2=绑定手机, 3=重置密码 |
| `IsUsed` | `bool` | 是否已使用 |
| `ExpireTime` | `datetime` | 过期时间（5分钟） |
| `TenantId` | `int` | |
| `CreationTime` | `datetime` | |
| `CreatorUserId` | `long?` | |

### 2.6 内容/公告

#### `T_Announce` — 公告

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `CategoryId` | `long` | 分类 ID |
| `Content` | `string(2048)` | 内容 |
| `ImageUrl` | `string(256)` | 图片 |
| `Sort` | `int` | 排序 |

#### `T_CmsArticle` — CMS 文章

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `CategoryId` | `long` | 分类 ID |
| `Title` | `string(128)` | 标题 |
| `TitleImageUrl` | `string(128)` | 标题图 |
| `Content` | `text` | 内容（富文本） |
| `Sort` | `int` | 排序 |
| `Status` | `AlticleStatusEnum` | 0=草稿, 1=已发布 |

#### `T_CmsCategory` — CMS 分类

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `Title` | `string(128)` | 分类名 |
| `TitleImageUrl` | `string(128)` | 图标 |
| `Sort` | `int` | 排序 |

### 2.7 管理/辅助

#### `T_BanedUsers` — 禁言用户

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `UserId` | `long` | 用户 ID |
| `EndTime` | `datetime` | 禁言截止时间 |
| `Chan` | `string` | 禁言频道 (null=全频道) |
| `CreationTime` | `datetime` | 创建时间 |
| `CreatorUserId` | `long?` | 操作者 |

#### `T_SensitiveWord` — 敏感词

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `Word` | `string` | 敏感词内容 |

#### `T_PushSubscription` — WebPush 订阅

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `UserId` | `long` | 用户 ID |
| `Endpoint` | `text` | 订阅端点 |
| `P256Dh` | `text` | 公钥 |
| `Auth` | `text` | 认证密钥 |
| `DeviceName` | `string?` | 设备名称 |

#### `AppReleases` — App 版本发布

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `VersionName` | `string(50)` | 版本名 (如 "1.0.0") |
| `VersionCode` | `int` | 版本号 (如 1) |
| `Description` | `string(500)` | 更新说明 |
| `DownloadUrl` | `string(500)` | 下载地址 |
| `FileName` | `string(50)` | 文件名 |
| `FileSize` | `long` | 文件大小(字节) |
| `IsForceUpdate` | `bool` | 是否强制更新 |
| `Platform` | `string(20)` | 平台 (Android/iOS) |
| `ReleaseDate` | `datetime` | 发布时间 |
| `IsActive` | `bool` | 是否启用 |

#### `T_AuctionStartNotify` — 拍卖开始通知

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `UserId` | `long` | 用户 ID |
| `AuctionItemId` | `long` | 拍卖品 ID |
| `IsNotified` | `bool` | 是否已通知 |
| `CreationTime` | `datetime` | 创建时间 |

#### `T_UserGroupLevel` — 用户群聊等级

| 字段 | 类型 | 说明 |
|------|------|------|
| `Id` | `long` | 主键 |
| `UserId` | `long` | 用户 ID |
| `GroupId` | `long` | 群 ID |
| `Level` | `int` | 等级 |
| `TotalAmount` | `decimal` | 累计金额 |

---

## 三、ABP 审计字段模式

大部分业务实体继承自 ABP 审计基类，自动包含以下字段：

### `Entity<TPrimaryKey>` 
- `Id` — 主键

### `ICreationAudited` (CreationAuditedEntity / CreationAuditedAggregateRoot)
- `CreationTime` — 创建时间
- `CreatorUserId` — 创建者 ID

### `IHasModificationTime` (AuditedEntity / AuditedAggregateRoot)
- `LastModificationTime` — 最后修改时间

### `IModificationAudited` (新增)
- `LastModifierUserId` — 最后修改者 ID

### `IDeletionAudited` (FullAuditedEntity / FullAuditedAggregateRoot → 软删除)
- `IsDeleted` — 是否删除
- `DeleterUserId` — 删除者 ID
- `DeletionTime` — 删除时间

**系统中使用的实体基类**：

| 基类 | 包含 | 使用实体 |
|------|------|---------|
| `FullAuditedAggregateRoot<long>` | 创建+修改+软删除 | AuctionItem, CmsArticle, CmsCategory, PayOrder, AppRelease, Announce |
| `CreationAuditedEntity<long>` | 创建 | BidHistory, ChatGroup, PushSubscription |
| `AuditedEntity<long>` | 创建+修改 | AuthRequest, SmsVerificationCode |
| `Entity<long>` + `ICreationAudited` | 创建 | BanedUser |
| `Entity<long>` | 无审计 | Message, ChatChannel, ChatListDelete |
| `Entity<Guid>` | 无审计（Guid 主键） | Message |
| `Entity<int>` | 无审计（int 主键） | GroupChatLevelSetting, ChatListDelete, UserFriend |

---

## 四、索引与约束

### 复合索引
- `SmsVerificationCode` — `IX_PhoneNumber_Purpose_CreationTime` (PhoneNumber, Purpose, CreationTime DESC)
- `ChatChannel` — `Chan` 字段唯一索引?（SQLite 生产环境，由 EF Core 迁移中定义）

### 外键关系
- `BidHistory.AuctionItemId` → `AuctionItem.Id`
- `UserFriend.UserId` / `FriendId` → `AbpUsers.Id`（逻辑外键）
- 大部分为逻辑关联（通过 Application Service 层处理），非数据库物理外键

---

## 五、关键技术决策

1. **消息主键**: `Guid` 类型，适合分布式生成
2. **支付订单主键**: `Ulid` 类型，时间有序且适合分布式
3. **聊天时间**: Unix 毫秒时间戳 (`long`)，存储在 `Message.Time`
4. **金额单位**: `分`（整数存储，避免浮点误差）
5. **余额/保证金**: `decimal(18,2)`（精确小数）
6. **数据库**: 生产环境使用 SQLite（当前配置），迁移兼容 MySQL
7. **软删除**: 使用 `FullAuditedAggregateRoot` 支持恢复
8. **扩展数据**: 使用 `Json`/`IExtendableObject` 存储灵活扩展字段
