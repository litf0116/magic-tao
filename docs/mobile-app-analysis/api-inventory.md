# API 接口清单（Flutter App 侧）

> 本文档列出 Flutter App 需要调用的后端 API 接口，按业务域分组。
>
> **基础路径**: `/api`
> **认证方式**: JWT Bearer Token（`/TokenAuth` 接口除外）
> **统一请求格式**: `AppResultRequestDto`（继承 `PagedResultRequestDto`）
> - `MaxResultCount`: 分页大小（默认 20）
> - `SkipCount`: 跳过的记录数
> - `Pid`: 父 ID（可选过滤）
> - `UserId`: 用户 ID（可选过滤）
> - `From`/`To`: 时间范围（可选过滤）
> - `Keyword`: 搜索关键字

---

## 1. 用户认证 (`/api/TokenAuth`)

| 方法 | 路径 | 说明 | Flutter 端 |
|------|------|------|-----------|
| POST | `/TokenAuth/Authenticate` | 账号密码登录 | `AuthenticateAsync` |
| POST | `/TokenAuth/PhoneAuthenticate` | 手机号验证码登录 | — |
| POST | `/TokenAuth/SendSmsCode` | 发送短信验证码 | `sendSmsCode` |
| POST | `/TokenAuth/Register` | 用户注册 | — |
| POST | `/TokenAuth/RegisterV2` | 用户注册 v2（含推荐码） | — |
| POST | `/TokenAuth/RefreshToken` | 刷新 JWT Token | — |
| GET | `/TokenAuth/GetInfo` | 获取当前用户信息（含权限） | `getUserInfo` |
| GET | `/TokenAuth/About` | 获取应用配置信息 | — |
| POST | `/TokenAuth/SendResetPwdSmsCode` | 发送重置密码验证码 | — |
| POST | `/TokenAuth/PhoneVerification` | 手机号验证（改密流程） | — |
| POST | `/TokenAuth/UpdatePassword` | 修改密码 | — |
| POST | `/TokenAuth/ResetPassword` | 重置密码 | — |
| POST | `/TokenAuth/BindPhoneNumber` | 绑定手机号 | — |

**关键 DTO**:

```typescript
// 登录请求
interface AuthenticateInput {
  userNameOrEmailAddress: string;
  password: string;
  rememberClient?: boolean;
}

// 手机号验证码登录
interface PhoneAuthenticateInput {
  phoneNumber: string;  // 符合正则: ^1[3-9]\\d{9}$
  code: string;         // 6 位验证码
  refreshToken?: string;
}

// 登录响应
interface AuthenticateResult {
  accessToken: string;
  encryptedAccessToken: string;
  expireInSeconds: number;
  userId: number;
  refreshToken: string;
  refreshTokenExpireInSeconds: number;
}
```

---

## 2. 用户管理 (`/api/services/app/User`)

| 方法 | 路径 | 说明 | Flutter 端 |
|------|------|------|-----------|
| GET | `/User/GetAll` | 用户列表（管理端） | — |
| GET | `/User/Get` | 获取用户详情 | `getUserInfo` |
| GET | `/User/GetUserInfoByIds` | 批量获取用户信息 | — |
| POST | `/User/Create` | 创建用户 | — |
| PUT | `/User/Update` | 更新用户信息 | `updateProfile` |
| DELETE | `/User/Delete` | 删除用户 | — |
| POST | `/User/UploadProfile` | 上传头像 | `uploadAvatar` |
| GET | `/User/GetWeChatUserInfoByCode` | 微信静默登录 | `wechatLogin` |
| GET | `/User/GetWeChatUserInfoByCodeFull` | 微信完整授权登录 | `wechatFullLogin` |
| POST | `/User/BindPhoneNumber` | 绑定手机号 | `bindPhone` |
| POST | `/User/UnBindPhoneNumber` | 解绑手机号 | — |
| POST | `/User/UpdatePassword` | 修改密码 | `changePassword` |
| GET | `/User/GetMyUserInfo` | 获取我的个人信息 | `getProfile` |
| GET | `/User/GetUserLevel` | 获取用户等级（VIP） | `getUserLevel` |
| GET | `/User/GetChatUser` | 获取聊天用户信息 | — |

**关键 DTO**:

```typescript
// 用户基本信息（Flutter 端常用）
interface UserProfile {
  id: number;
  name: string;
  phoneNumber: string;
  headImgUrl: string;            // 头像 URL
  age: number;
  gender: string;                // male / female
  birthday: string;
  cityName: string;
  personality: string;           // 个性签名
  realName: string;
  level: string;
  balance: number;
  isBanned: boolean;
  passwordSet: boolean;
}
```

---

## 3. 好友关系 (`/api/services/app/UserFriend`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/UserFriend/AddFriend` | 发送好友请求 |
| GET | `/UserFriend/GetUserFriends` | 获取好友列表（`id`, `status` 参数） |
| GET | `/UserFriend/GetUserFriendCount` | 获取待处理好友请求数量 |
| GET | `/UserFriend/Agree` | 同意/拒绝好友请求 (`id`, `status`) |

**实体关系**:
- `UserFriend` 使用双向记录模式：A→B 记录 `UserId=A, FriendId=B`，B→A 记录 `UserId=B, FriendId=A`
- `Status=false` 表示待确认，`Status=true` 表示已是好友

---

## 4. 即时通讯（SignalR / FreeIM）

**连接端点**:
- WebSocket 连接通过 FreeIM 的 `ImHelper.Initialization` 配置
- 开发环境: `ws://127.0.0.1:6001`
- 生产环境: `wss://ws.molitao.top`

**消息格式**:

```typescript
// 聊天消息（Message 实体）
interface ChatMessage {
  id: string;                    // GUID
  content: string;               // 消息内容
  sendUserId: number;           // 发送者 ID
  receiveUserId: number;        // 接收者 ID
  auctionItemId?: number;       // 关联拍卖商品 ID
  auctionItemName?: string;     // 关联拍卖商品名称
  messageType: number;          // 消息类型（见下方）
  sendTime: string;             // 发送时间
  isRead: boolean;              // 是否已读
  isSystem: boolean;            // 是否系统消息
  extra: string;                // 扩展字段（JSON，用于图片URL等）
}

// 消息类型枚举
enum ChatMessageType {
  文字 = 1,
  图片 = 2,
  语音 = 3,
  系统 = 100,
}
```

**API 接口**:

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/services/app/Message/GetAll` | 获取历史消息列表（支持分页，按 `AuctionItemId` 过滤） |
| GET | `/api/services/app/Message/Get` | 获取单条消息详情 |
| GET | `/api/services/app/Message/GetOfflineMessages` | 获取离线消息 |
| POST | `/api/services/app/Message/ReadMessage` | 标记消息已读 |
| POST | `/api/services/app/Message/Create` | 发送消息（也可通过 WebSocket 实时发送） |
| GET | `/api/services/app/Message/GetAuctionNavigation` | 获取最近聊天的拍卖列表 |

**系统消息（ChatMessageType.System = 100）**:
系统消息通过 `MessageSendingService` 发送，包括交易通知、拍卖推送等。

**联系人列表**:
- 联系人列表通过 `ChatListDelete` 实体管理：未删除的聊天记录显示在列表中
- 搜索用户: `User/GetUserInfoByIds` 或 `User/GetChatUser`

**好友关系**:
- 好友通过 `UserFriendAppService` 管理
- 非好友也可以聊天（平台使用场景）— 信息撮合模式

---

## 5. 拍卖/商品 (`/api/services/app/AuctionItem`)

| 方法 | 路径 | 说明 | Flutter 端 |
|------|------|------|-----------|
| GET | `/AuctionItem/GetAll` | 拍卖列表（支持分类/关键字/状态过滤） | `getAuctionList` |
| GET | `/AuctionItem/Get` | 拍卖详情 | `getAuctionDetail` |
| GET | `/AuctionItem/GetHomeItems` | 首页推荐列表 | `getHomeItems` |
| GET | `/AuctionItem/GetLatest` | 最新上架 | `getLatest` |
| POST | `/AuctionItem/Create` | 发布拍卖 | `createAuction` |
| PUT | `/AuctionItem/Update` | 更新拍卖 | `updateAuction` |
| DELETE | `/AuctionItem/Delete` | 删除拍卖 | `deleteAuction` |
| POST | `/AuctionItem/Bid` | 出价 | `bid` |
| GET | `/AuctionItem/GetMyItems` | 我发布的拍卖列表 | `myItems` |
| POST | `/AuctionItem/SubStartNotify` | 订阅开拍通知 | `subscribeAuction` |
| POST | `/AuctionItem/TestSendAuctionStartNotify` | 测试开拍通知（管理端） | — |
| POST | `/AuctionItem/GetMaxBidPrice` | 获取当前最高出价 | — |
| POST | `/AuctionItem/UploadFile` | 上传商品图片/文件 | `uploadFile` |
| POST | `/AuctionItem/UpdateStatus` | 更新拍卖状态（管理端） | — |
| POST | `/AuctionItem/DeleteImgs` | 删除商品图片 | — |

**拍卖状态枚举**:

```typescript
enum AuctionItemStatus {
  Pending = 0,        // 待审核
  Active = 1,         // 拍卖中
  Sold = 2,           // 已售出
  Cancelled = 3,      // 已取消
  Expired = 4,        // 已过期（无人出价）
}
```

**出价历史 API**:

```typescript
interface BidHistory {
  id: number;
  auctionItemId: number;
  bidPrice: number;
  bidUserName: string;
  bidUserAvatar: string;
  creationTime: string;
}

// 出价请求
interface BidHistoryCreateDto {
  auctionItemId: number;
  bidPrice: number;
  bidUserName: string;
  bidUserAvatar: string;
}
```

**关键查询参数**:
- `Keyword`: 搜索标题
- `Pid`: 按分类 ID 过滤（`CmsCategory`）
- `UserId`: 按用户 ID 过滤
- `From`/`To`: 时间范围
- 默认按 `CreationTime` 降序排列

---

## 6. 分类浏览 (`/api/services/app/CmsCategory`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/CmsCategory/GetAll` | 获取全部分类列表（树形结构） |
| GET | `/CmsCategory/Get` | 获取分类详情 |
| POST | `/CmsCategory/Create` | 创建分类（管理端） |
| PUT | `/CmsCategory/Update` | 更新分类（管理端） |

**分类树结构**:
- `ParentId` 实现多级分类（支持 3-4 级）
- `Code` 字段用于排序

---

## 7. 文章内容 (`/api/services/app/CmsArticle`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/CmsArticle/GetAll` | 文章列表（按分类过滤） |
| GET | `/CmsArticle/Get` | 文章详情 |
| POST | `/CmsArticle/Create` | 创建文章（管理端） |

---

## 8. 支付/订单 (`/api/services/app/PayOrder`)

> **注意**: 支付服务使用 `PayOrderAppService`，当前代码路径待确认。

**支付流程**:
1. 用户发起支付请求 → 后端创建 `PayOrder`
2. 后端返回微信支付参数（prepay_id 等）
3. Flutter 端调起微信支付 SDK
4. 微信异步通知结果 → 后端更新订单状态
5. 前端轮询/推送获取结果

**关键实体**:

```typescript
interface PayOrder {
  id: string;                    // ULID
  outTradeNo: string;           // 商户订单号
  transactionId?: string;       // 微信支付单号
  totalFee: number;             // 订单金额（分）
  payState: PayState;
  body: string;                  // 商品描述
  successTime?: string;         // 支付成功时间
  extra: string;                // 扩展字段 JSON
}

enum PayState {
  UnPaid = 0,         // 待支付
  Paid = 1,           // 已支付
  Cancelled = -1,     // 已取消
}
```

---

## 9. 钱包/余额 (`/api/services/app/UserDepositLog`, `UserBalanceLog`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/UserDepositLog/GetMyAllAsync` | 我的充值记录 |
| GET | `/UserBalanceLog/GetMyAllAsync` | 我的余额变动记录 |

---

## 10. 敏感词 (`/api/services/app/SensitiveWord`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/SensitiveWord/GetAll` | 获取敏感词列表（管理端） |

---

## 11. 用户等级 (`/api/services/app/UserLevel`)

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/UserLevel/GetAll` | 获取全部等级定义 |

---

## 12. 文件上传

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/services/app/File/Upload` | 通用文件上传 |
| POST | `/api/services/app/AuctionItem/UploadFile` | 拍卖商品图片上传 |
| POST | `/api/services/app/User/UploadProfile` | 用户头像上传 |

- 文件存储使用 **又拍云 (UpYun)** OSS
- 模块: `TtWork.Abp.Oss.UpYun` / `UpYunModule`
- 上传图片支持裁剪/压缩

---

## 13. 微信 API（后端封装）

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/WeChatJsSdk/Config` | 获取 JS-SDK 配置 |
| GET | `/api/WeChatJsSdk/Signature` | URL 签名 |
| GET | `/api/WeChatCode2Session/OnLogin` | 微信小程序 code2session 登录 |

---

## 14. 系统/通用接口

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/services/app/Configuration/GetAll` | 系统配置 |
| GET | `/TokenAuth/GetInfo` | 当前登录信息+权限 |
| GET | `/TokenAuth/About` | 关于/版本信息 |

---

## 15. 宠物算档器

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/pet-calculator/monsters` | 获取魔物列表 |
| POST | `/api/pet-calculator/calculate` | 计算宠物档位 |
| GET | `/api/admin/pet-data` | 管理端魔物数据管理（管理端） |

---

## 汇总表

| 分组 | 接口数 | 认证 | 移动端关键性 |
|------|--------|------|-------------|
| 用户认证 | 12 | 部分 | ⭐⭐⭐⭐⭐ |
| 用户管理 | 14 | 是 | ⭐⭐⭐⭐⭐ |
| 好友关系 | 4 | 是 | ⭐⭐⭐⭐ |
| 即时通讯 | 7 | 是 | ⭐⭐⭐⭐⭐ |
| 拍卖商品 | 15 | 部分 | ⭐⭐⭐⭐⭐ |
| 分类/文章 | 6 | 部分 | ⭐⭐⭐ |
| 支付 | 3+ | 是 | ⭐⭐⭐⭐⭐ |
| 钱包/余额 | 2 | 是 | ⭐⭐⭐ |
| 文件上传 | 3 | 是 | ⭐⭐⭐⭐ |
| 微信小程序 | 3 | 否 | ⭐⭐⭐ |
| 敏感词/等级 | 2 | 是 | ⭐⭐ |
| 宠物算档 | 3 | 否 | ⭐⭐ |
