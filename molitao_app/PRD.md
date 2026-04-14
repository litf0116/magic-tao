# 魔力淘 - 产品需求文档 (PRD)

## 1. 产品概述

### 1.1 产品名称
**魔力淘** - 在线拍卖交易平台

### 1.2 产品定位
一款集实时拍卖、即时通讯、社交交易于一体的移动端应用，支持微信小程序、H5 和 APP 多端运行。

### 1.3 目标用户
- 游戏虚拟物品买卖双方
- 寻求快速交易的用户
- 社交电商爱好者

### 1.4 核心价值
- **实时秒杀**: 独特的卡秒拍卖模式，刺激竞拍体验
- **即时通讯**: 支持私聊、群聊、拍卖频道
- **社交交易**: 帖子交易站，用户自主发布交易信息

---

## 2. 功能架构

```
魔力淘
├── 首页
│   ├── 头部 Logo
│   ├── 交易站入口
│   ├── 秒杀场入口
│   ├── 文章轮播
│   └── 广告位展示
│
├── 消息中心
│   ├── 会话列表
│   ├── 私聊
│   ├── 群聊
│   └── 秒杀场聊天
│
├── 交易站
│   ├── 帖子列表
│   ├── 发布帖子
│   ├── 帖子详情
│   └── 分类筛选
│
├── 通讯录
│   ├── 好友列表
│   ├── 好友申请
│   └── 用户搜索
│
└── 个人中心
    ├── 用户信息
    ├── 魔力值明细
    ├── 余额明细
    └── 成交记录
```

---

## 3. 功能模块详细说明

### 3.1 首页模块

#### 3.1.1 页面结构
| 区域 | 功能 | 数据来源 |
|------|------|----------|
| Header | Logo + 背景展示 | 静态资源 |
| Banner 区 | 交易站/秒杀场入口 | 点击跳转 |
| 轮播图 | CMS 文章展示 | `/api/services/app/CmsArticle/GetAllPublic` |
| 广告位 | 广告展示 | `/api/AdvertisingSpace/GetTypeList/{type}` |

#### 3.1.2 交互说明
- 下拉刷新：重新加载文章和广告位数据
- 轮播自动播放：5 秒间隔
- Banner 点击：跳转对应功能页面

---

### 3.2 消息模块

#### 3.2.1 会话列表

**功能描述**: 展示所有聊天会话，包括私聊、群聊、系统消息

**数据结构**:
```typescript
interface ChatListItem {
  id: number              // 会话 ID（私聊为用户 ID，群聊为负数）
  name: string            // 会话名称
  type: ChatListItemType  // 类型：group(0) | user(1) | system(2)
  avatar?: string         // 头像
  lastMsg?: string        // 最后一条消息
  unread: number          // 未读数
  time?: number           // 最后消息时间
  order: number           // 排序权重
}
```

**系统会话**:
| ID | 名称 | 说明 |
|----|------|------|
| -1 | auction | 秒杀场 |
| -2 | lobby | 大厅（已禁用） |
| -3 | notice | 系统公告 |
| -4 | beginner | 新手群 |

**交互说明**:
- 点击会话：进入聊天页面
- 长按会话：删除会话（仅删除本地列表）

#### 3.2.2 私聊页面

**功能描述**: 一对一即时通讯

**消息类型**:
| 类型 | 说明 |
|------|------|
| Text | 文本消息 |
| Image | 图片消息 |
| Receipt | 回执消息 |
| Welcome | 加入欢迎 |
| Goodbye | 退出提示 |
| BanUser | 禁言通知 |
| Backout | 撤回消息 |

**API 接口**:
- 发送消息: `POST /ws/send-msg`
- 获取历史: `GET /api/services/app/Message/getPrivateHistory`
- 撤回消息: `POST /ws/backout`

#### 3.2.3 群聊页面

**功能描述**: 多人群组聊天

**与私聊差异**:
- 消息通过频道发送: `POST /ws/SendChannelMsg`
- 显示用户头像和昵称
- 支持管理员标签显示
- 历史消息: `GET /api/services/app/Message/getChanHistory`

#### 3.2.4 秒杀场聊天

**功能描述**: 拍卖频道聊天，集成出价功能

**特殊功能**:
- 实时出价消息
- 拍卖状态变更通知
- 卡秒模式状态展示

**消息类型**:
| 类型 | 说明 |
|------|------|
| AuctionStart | 拍卖开始 |
| AuctionBid | 用户出价 |
| AuctionEnd | 拍卖结束 |
| AuctionDeal | 成交通知 |
| KasecStatusChanged | 卡秒状态变更 |

---

### 3.3 拍卖模块

#### 3.3.1 拍卖商品列表

**状态类型**:
| 状态 | 说明 |
|------|------|
| 草稿 | 未上架 |
| 上架 | 待拍卖 |
| 秒杀中 | 正在拍卖 |
| 已成交 | 拍卖完成 |

**API 接口**:
- 获取列表: `GET /api/services/app/AuctionItem/GetPublicList`
- 我的成交: `GET /api/services/app/AuctionItem/GetMySuccessList`

#### 3.3.2 出价功能

**业务规则**:
1. **最低出价**: 5 R
2. **卡秒模式**: 需三倍加价
3. **新用户限制**: 需缴纳魔力值 (51 R)
4. **出价计算**: `calculateMinBidPrice(currentPrice, isKasec)`

**API 接口**:
- 出价: `POST /api/services/app/AuctionItem/Bid`
- 开始拍卖: `GET /api/services/app/AuctionItem/StartAuction`
- 结束拍卖: `GET /api/services/app/AuctionItem/EndAuction`
- 卡秒状态: `GET /api/services/app/AuctionItem/GetKasecStatus`

#### 3.3.3 开拍通知

**平台差异**:
- 小程序: 微信订阅消息
- APP: 极光推送

**API 接口**:
- 订阅通知: `POST /api/services/app/AuctionItem/SubStartNotify`

---

### 3.4 交易站模块

#### 3.4.1 帖子列表

**功能描述**: 用户发布的交易信息展示

**数据结构**:
```typescript
interface PostModel {
  id: number
  title: string
  content: string          // HTML 富文本
  imageUrl?: string
  userId: number
  userName: string
  userAvatar?: string
  wechat?: string          // 微信号
  qq?: string              // QQ 号
  categoryName?: string    // 分类标签
  creationTime: Date
  viewCount: number
  likeCount: number
  commentCount: number
}
```

**API 接口**:
- 列表: `GET /api/Post/GetList`
- 分类列表: `GET /api/PostCategory/GetCategoryList`
- 热词: `GET /api/HotWords/GetList`

#### 3.4.2 发布/编辑帖子

**必填字段**:
- 标题 (maxLength: 100)
- 内容 (maxLength: 5000)

**可选字段**:
- 图片
- 分类标签
- 微信号
- QQ 号

**API 接口**:
- 发布: `POST /api/Post/Add`
- 编辑: `POST /api/Post/Edit`
- 详情: `GET /api/Post/PostDetail/{id}`
- 删除: `GET /api/Post/Delete/{id}`

#### 3.4.3 帖子详情

**功能说明**:
- 富文本内容展示
- 图片预览
- 联系方式展示
- 留言按钮（跳转私聊）
- 作者可编辑/删除

---

### 3.5 通讯录模块

#### 3.5.1 好友列表

**功能描述**: 展示已添加的好友

**API 接口**:
- 获取好友: `GET /api/services/app/UserFriend/GetUserFriends`

#### 3.5.2 好友申请

**功能描述**: 处理好友申请

**API 接口**:
- 添加好友: `GET /api/services/app/UserFriend/AddFriend`
- 同意/拒绝: `GET /api/services/app/UserFriend/Agree`
- 申请数量: `GET /api/services/app/UserFriend/GetUserFriendCount`

---

### 3.6 个人中心模块

#### 3.6.1 用户信息

**数据结构**:
```typescript
interface UserDto {
  id: number
  userName: string
  name: string
  headImgUrl?: string
  phoneNumber?: string
  qq?: string
  wx?: string
  depositBalance: number  // 魔力值
  balance: number         // 余额
}
```

**API 接口**:
- 获取信息: `GET /api/services/app/User/Get`
- 更新信息: `PUT /api/services/app/User/Update`

#### 3.6.2 魔力值明细

**说明**: 记录魔力值（押金）变动

**API 接口**:
- `GET /api/services/app/UserDepositLog/GetMyAll`

#### 3.6.3 余额明细

**说明**: 记录账户余额变动

**API 接口**:
- `GET /api/services/app/UserBalanceLog/GetMyAll`

#### 3.6.4 成交记录

**说明**: 用户拍卖成交记录

**API 接口**:
- `GET /api/services/app/AuctionItem/GetMySuccessList`

---

## 4. 技术架构

### 4.1 前端技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| UniApp | - | 跨平台框架 |
| Vue 3 | - | 前端框架 |
| TypeScript | - | 类型系统 |
| Pinia | - | 状态管理 |
| uView UI | - | UI 组件库 |
| UnoCSS | - | 原子化 CSS |

### 4.2 后端接口

**基础地址**: `https://www.molitao.top`

**认证方式**: Bearer Token

**请求头**:
```
Authorization: Bearer {token}
Abp.Tenantid: 1
Content-Type: application/json
AppName: uniapp
AppVersion: {version}
```

### 4.3 WebSocket 通信

**连接地址**: `wss://www.molitao.top/ws`

**连接流程**:
1. 调用 `/ws/pre-connect` 获取 websocketId
2. 建立 WebSocket 连接，携带 connectionId
3. 订阅频道: `/ws/sub-channel`
4. 接收消息: 监听 `ReceiveMessage` / `ReceiveChannelMessage`

**消息格式** (SignalR 协议):
```json
{
  "type": 1,
  "target": "ReceiveMessage",
  "arguments": [{ ...messageData }]
}
```

---

## 5. 平台差异

### 5.1 功能差异

| 功能 | 小程序 | H5 | APP |
|------|:------:|:--:|:---:|
| 微信登录 | ✅ | ✅(扫码) | ✅ |
| 微信支付 | ✅ | ❌ | ✅ |
| 推送通知 | 订阅消息 | ❌ | 极光推送 |
| 底部 Tab 数量 | 4 | 5 | 5 |
| 中间发布按钮 | ❌ | ✅ | ✅ |
| 交易站 | ❌ | ✅ | ✅ |

### 5.2 条件编译

```javascript
// 小程序专属
#ifdef MP-WEIXIN
// 微信小程序代码
#endif

// APP/H5 专属
#ifndef MP-WEIXIN
// APP 和 H5 代码
#endif

// H5 专属
#ifdef H5
// H5 代码
#endif

// APP 专属
#ifdef APP-PLUS
// APP 代码
#endif
```

---

## 6. 数据字典

### 6.1 消息类型枚举

```typescript
enum ChatMessageType {
  Text = 'Text',
  Image = 'Image',
  File = 'File',
  Receipt = 'Receipt',
  Welcome = 'Welcome',
  Goodbye = 'Goodbye',
  BanUser = 'BanUser',
  Backout = 'Backout',
  AuctionStart = 'AuctionStart',
  AuctionBid = 'AuctionBid',
  AuctionEnd = 'AuctionEnd',
  AuctionDeal = 'AuctionDeal',
  Error = 'Error',
  KasecStatusChanged = 'KasecStatusChanged',
}
```

### 6.2 拍卖状态枚举

```typescript
enum AuctionStatusEnum {
  草稿 = '草稿',
  上架 = '上架',
  秒杀中 = '秒杀中',
  已成交 = '已成交',
}
```

### 6.3 会话类型枚举

```typescript
enum ChatListItemType {
  group = 0,    // 群聊
  user = 1,     // 私聊
  system = 2,   // 系统消息
}
```

---

## 7. 安全规范

### 7.1 内容审核

**图片审核**: `POST /api/ContentSecurity/CheckMedia`

调用时机:
- 发送图片消息前
- 发布帖子图片时

### 7.2 用户权限

**魔力值限制**:
- 新用户参与秒杀需魔力值 ≥ 51
- 卡秒模式需三倍加价

**管理员权限**:
- 禁言用户: `POST /ws/ban-user`
- 结束拍卖
- 发送拍卖消息

---

## 8. 版本规划

### 8.1 当前版本功能

| 模块 | 功能 | 状态 |
|------|------|:----:|
| 首页 | Banner + 轮播 + 广告位 | ✅ |
| 消息 | 私聊 + 群聊 + 秒杀场 | ✅ |
| 拍卖 | 列表 + 出价 + 通知 | ✅ |
| 交易站 | 帖子 CRUD | ✅ |
| 通讯录 | 好友管理 | ✅ |
| 个人中心 | 信息 + 明细 | ✅ |

### 8.2 后续规划

| 功能 | 优先级 | 说明 |
|------|:------:|------|
| 表情包 | 中 | 自定义表情发送 |
| 语音消息 | 中 | 语音聊天支持 |
| 视频消息 | 低 | 短视频分享 |
| 直播拍卖 | 低 | 实时直播竞拍 |

---

## 9. 附录

### 9.1 设计规范

**主色调**: `#f4835a` (橙色)

**颜色使用**:
- 主色: 按钮、选中状态、强调文字
- 灰色: 未选中状态、次要文字
- 红色: 删除、警告
- 绿色: 成功、微信相关

### 9.2 图标资源

| 图标 | 文件名 | 用途 |
|------|--------|------|
| 首页 | tab1.png / tab1_b.png | Tab 图标 |
| 会话 | tab2.png / tab2_b.png | Tab 图标 |
| 交易站 | add.png | Tab 图标 |
| 通讯录 | tab3.png / tab3_b.png | Tab 图标 |
| 个人中心 | tab4.png / tab4_b.png | Tab 图标 |

### 9.3 单位规范

- 尺寸: rpx (响应式像素)
- 字体: rpx
- 间距: rpx

---

**文档版本**: v1.0
**更新日期**: 2026-03-30
**维护者**: 开发团队