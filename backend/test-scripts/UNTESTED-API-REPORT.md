# 未测试接口报告

**生成时间**: 2026-04-04
**总接口数**: 260
**已测试**: 37 (14.2%)
**未测试**: 223 (85.8%)

---

## 测试覆盖概览

### 已测试模块 ✅

| 模块 | 已测试 | 主要功能 |
|------|--------|---------|
| 用户认证 | 7/17 | 登录、密码管理、Token |
| 拍卖管理 | 6/19 | 列表、出价、开始/结束 |
| 用户管理 | 5/14 | 创建、查询、角色 |
| 角色管理 | 3/8 | 列表、权限 |
| 聊天群组 | 4/10 | 创建、查询、用户 |
| 公告管理 | 2/8 | 列表、最新 |
| 敏感词 | 1/10 | 创建 |
| 租户管理 | 1/5 | 查询 |

---

## 未测试接口分类

### 1. 核心业务接口 (高优先级) 

#### 1.1 拍卖相关 (13个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/AuctionItem/Create` | 创建拍卖品 | ⭐⭐⭐ |
| `/api/services/app/AuctionItem/Update` | 更新拍卖品 | ⭐⭐⭐ |
| `/api/services/app/AuctionItem/Delete` | 删除拍卖品 | ⭐⭐ |
| `/api/services/app/AuctionItem/GetAll` | 获取所有拍卖品 | ⭐⭐ |
| `/api/services/app/AuctionItem/GetCurrentAuctionItem` | 获取当前拍卖品 | ⭐⭐⭐ |
| `/api/services/app/AuctionItem/GetMySuccessList` | 获取我拍得的商品 | ⭐⭐⭐ |
| `/api/services/app/AuctionItem/GetAuctionMidList` | 获取拍卖中商品 | ⭐⭐ |
| `/api/services/app/AuctionItem/Callback` | 拍卖回调 | ⭐⭐⭐ |
| `/api/services/app/AuctionItem/SetKasecStatus` | 设置卡秒状态 | ⭐⭐ |
| `/api/services/app/AuctionItem/GetKasecStatus` | 获取卡秒状态 | ⭐⭐ |
| `/api/services/app/AuctionItem/SubStartNotify` | 订阅开始通知 | ⭐⭐ |
| `/api/services/app/AuctionItem/DateAnlayse` | 数据分析 | ⭐ |
| `/api/services/app/AuctionItem/DateAnlayse2` | 数据分析2 | ⭐ |

#### 1.2 出价历史 (8个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/BidHistory/GetMyBidHistory` | 我的出价历史 | ⭐⭐⭐ |
| `/api/services/app/BidHistory/GetAll` | 所有出价历史 | ⭐⭐ |
| `/api/services/app/BidHistory/Get` | 获取出价详情 | ⭐⭐ |
| `/api/services/app/BidHistory/Create` | 创建出价记录 | ⭐ |
| `/api/services/app/BidHistory/Update` | 更新出价 | ⭐ |
| `/api/services/app/BidHistory/Delete` | 删除出价 | ⭐ |
| `/api/services/app/BidHistory/GetForEdit` | 获取编辑信息 | ⭐ |
| `/api/services/app/BidHistory/DateAnlayse` | 数据分析 | ⭐ |

#### 1.3 支付相关 (6个未测试)

| 接口 | 功能 | 重要性 | 备注 |
|------|------|--------|------|
| `/api/services/app/Client/PayDeposit` | 充值保证金 | ⭐⭐⭐ | 需微信配置 |
| `/api/services/app/Client/PayDepositNative` | 原生充值 | ⭐⭐⭐ | 需微信配置 |
| `/api/services/app/Client/PayRefund` | 退款 | ⭐⭐⭐ | 需微信配置 |
| `/api/services/app/Client/TopUp` | 充值余额 | ⭐⭐⭐ | 需微信配置 |
| `/api/services/app/Client/GetPayOrderStatus` | 支付状态 | ⭐⭐ | |
| `/api/services/app/Client/GetMyCount` | 获取我的统计 | ⭐⭐ | |

#### 1.4 用户余额 (13个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/UserBalanceLog/GetMyAll` | 我的余额记录 | ⭐⭐⭐ |
| `/api/services/app/UserBalanceLog/GetAll` | 所有余额记录 | ⭐⭐ |
| `/api/services/app/UserBalanceLog/Get` | 获取详情 | ⭐⭐ |
| `/api/services/app/UserDepositLog/GetAll` | 所有保证金记录 | ⭐⭐ |
| `/api/services/app/UserDepositLog/Get` | 获取详情 | ⭐⭐ |
| `/api/services/app/UserDepositLog/Update` | 更新记录 | ⭐ |
| `/api/services/app/UserDepositLog/Delete` | 删除记录 | ⭐ |
| `/api/services/app/UserBalanceLog/Create` | 创建记录 | ⭐ |
| `/api/services/app/UserBalanceLog/Update` | 更新记录 | ⭐ |
| `/api/services/app/UserBalanceLog/Delete` | 删除记录 | ⭐ |

---

### 2. 用户功能接口 (中优先级)

#### 2.1 用户管理 (8个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/User/Delete` | 删除用户 | ⭐⭐ |
| `/api/services/app/User/GetForEdit` | 获取编辑信息 | ⭐⭐ |
| `/api/services/app/User/GetUserForEdit` | 获取用户编辑 | ⭐⭐ |
| `/api/services/app/User/GetUsersInRole` | 获取角色用户 | ⭐⭐ |
| `/api/services/app/User/ResetPassword` | 重置密码 | ⭐⭐⭐ |
| `/api/services/app/User/ChangeLanguage` | 更改语言 | ⭐ |
| `/api/services/app/User/ChangePassword` | 修改密码 | ⭐⭐ |
| `/api/services/app/User/CreateOrUpdateUser` | 创建或更新用户 | ⭐⭐ |

#### 2.2 好友功能 (3个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/UserFriend/AddFriend` | 添加好友 | ⭐⭐⭐ |
| `/api/services/app/UserFriend/Agree` | 同意好友 | ⭐⭐⭐ |
| `/api/services/app/UserFriend/GetUserFriendCount` | 好友数量 | ⭐⭐ |

#### 2.3 认证相关 (10个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/TokenAuth/LogOut` | 登出 | ⭐⭐⭐ |
| `/api/TokenAuth/RefreshToken` | 刷新Token | ⭐⭐⭐ |
| `/api/TokenAuth/WeixinMiniAuthenticate` | 微信小程序登录 | ⭐⭐⭐ |
| `/api/TokenAuth/WeixinMiniPhoneAuthenticate` | 微信手机登录 | ⭐⭐⭐ |
| `/api/TokenAuth/AuthenticateWeixinApp` | 微信APP登录 | ⭐⭐ |
| `/api/TokenAuth/QrLogin` | 二维码登录 | ⭐⭐ |
| `/api/TokenAuth/PubQrLogin` | 发布二维码 | ⭐⭐ |
| `/api/TokenAuth/QrToken` | 二维码Token | ⭐⭐ |
| `/api/TokenAuth/GenerateTokenForUser` | 生成用户Token | ⭐ |
| `/api/TokenAuth/GenerateHashedPassword` | 生成哈希密码 | ⭐ |

---

### 3. 内容管理接口 (中优先级)

#### 3.1 公告管理 (6个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/Announce/Create` | 创建公告 | ⭐⭐⭐ |
| `/api/services/app/Announce/Update` | 更新公告 | ⭐⭐ |
| `/api/services/app/Announce/Delete` | 删除公告 | ⭐⭐ |
| `/api/services/app/Announce/Get` | 获取公告 | ⭐⭐ |
| `/api/services/app/Announce/GetForEdit` | 获取编辑信息 | ⭐⭐ |
| `/api/services/app/Announce/GetAllPublic` | 公开公告列表 | ⭐⭐ |

#### 3.2 CMS文章 (7个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/CmsArticle/Create` | 创建文章 | ⭐⭐ |
| `/api/services/app/CmsArticle/Update` | 更新文章 | ⭐⭐ |
| `/api/services/app/CmsArticle/Delete` | 删除文章 | ⭐⭐ |
| `/api/services/app/CmsArticle/Get` | 获取文章 | ⭐⭐ |
| `/api/services/app/CmsArticle/GetAll` | 文章列表 | ⭐⭐ |
| `/api/services/app/CmsArticle/GetAllPublic` | 公开文章 | ⭐⭐ |
| `/api/services/app/CmsArticle/GetForEdit` | 编辑信息 | ⭐⭐ |

#### 3.3 CMS分类 (6个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/CmsCategory/Create` | 创建分类 | ⭐⭐ |
| `/api/services/app/CmsCategory/Update` | 更新分类 | ⭐⭐ |
| `/api/services/app/CmsCategory/Delete` | 删除分类 | ⭐⭐ |
| `/api/services/app/CmsCategory/Get` | 获取分类 | ⭐⭐ |
| `/api/services/app/CmsCategory/GetAll` | 分类列表 | ⭐⭐ |
| `/api/services/app/CmsCategory/GetForEdit` | 编辑信息 | ⭐⭐ |

---

### 4. 管理功能接口 (低优先级)

#### 4.1 角色管理 (5个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/Role/Create` | 创建角色 | ⭐⭐ |
| `/api/services/app/Role/Update` | 更新角色 | ⭐⭐ |
| `/api/services/app/Role/Delete` | 删除角色 | ⭐⭐ |
| `/api/services/app/Role/GetRoleForEdit` | 编辑信息 | ⭐⭐ |
| `/api/services/app/Role/GetRoles` | 角色列表 | ⭐⭐ |

#### 4.2 敏感词管理 (9个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/api/services/app/SensitiveWord/GetAll` | 获取列表 | ⭐⭐ |
| `/api/services/app/SensitiveWord/Get` | 获取详情 | ⭐⭐ |
| `/api/services/app/SensitiveWord/Update` | 更新敏感词 | ⭐⭐ |
| `/api/services/app/SensitiveWord/Delete` | 删除敏感词 | ⭐⭐ |
| `/api/services/app/SensitiveWord/BatchCreate` | 批量创建 | ⭐⭐ |
| `/api/services/app/SensitiveWord/GetCachedWords` | 缓存词列表 | ⭐⭐ |
| `/api/services/app/SensitiveWord/ReBuildCache` | 重建缓存 | ⭐⭐ |
| `/api/services/app/SensitiveWord/CheckCacheHealth` | 缓存健康检查 | ⭐ |
| `/api/services/app/SensitiveWord/GetForEdit` | 编辑信息 | ⭐ |

#### 4.3 禁言用户 (5个未测试)

| 接口 | 功能 | 重要性 | 备注 |
|------|------|--------|------|
| `/api/services/app/BanedUser/GetAll` | 禁言列表 | ⭐⭐ | |
| `/api/services/app/BanedUser/Get` | 获取详情 | ⭐⭐ | |
| `/api/services/app/BanedUser/Update` | 更新禁言 | ⭐⭐ | |
| `/api/services/app/BanedUser/Delete` | 删除禁言 | ⭐⭐ | |
| `/api/services/app/BanedUser/GetForEdit` | 编辑信息 | ⭐ | |

---

### 5. 第三方集成接口

#### 5.1 微信相关

| 接口 | 功能 | 需要配置 |
|------|------|---------|
| `/api/TokenAuth/WeixinMiniAuthenticate` | 小程序登录 | 微信小程序配置 |
| `/api/TokenAuth/WeixinMiniPhoneAuthenticate` | 手机号登录 | 微信小程序配置 |
| `/api/TokenAuth/AuthenticateWeixinApp` | APP登录 | 微信开放平台配置 |
| `/api/services/app/WxUserInfo/GetWechatUserinfos` | 微信用户信息 | 微信配置 |
| `/api/wx` | 微信入口 | 微信配置 |

#### 5.2 内容安全

| 接口 | 功能 | 需要配置 |
|------|------|---------|
| `/api/ContentSecurity/CheckContent` | 内容检测 | 微信内容安全API |
| `/api/ContentSecurity/CheckMedia` | 媒体检测 | 微信内容安全API |
| `/api/ContentSecurity/TestWeixinConnection` | 测试连接 | 微信配置 |

---

### 6. WebSocket 接口 (10个未测试)

| 接口 | 功能 | 重要性 |
|------|------|--------|
| `/ws/sub-channel` | 订阅频道 | ⭐⭐⭐ |
| `/ws/leave-channel` | 离开频道 | ⭐⭐⭐ |
| `/ws/send-msg` | 发送消息 | ⭐⭐⭐ |
| `/ws/SendChannelMsg` | 发送频道消息 | ⭐⭐⭐ |
| `/ws/get-channels` | 获取频道 | ⭐⭐ |
| `/ws/pre-connect` | 预连接 | ⭐⭐ |
| `/ws/offline` | 离线 | ⭐⭐ |
| `/ws/del-channel` | 删除频道 | ⭐⭐ |
| `/ws/ban-user` | 禁言用户 | ⭐⭐ |
| `/ws/backout` | 退出 | ⭐ |

---

### 7. 测试/工具接口 (9个)

| 接口 | 功能 |
|------|------|
| `/api/test/message/send-channel` | 发送频道测试消息 |
| `/api/test/message/chat-list` | 测试聊天列表 |
| `/api/test/message/channels` | 测试频道 |
| `/api/test/push/broadcast` | 推送广播 |
| `/api/test/push/send` | 发送推送 |
| `/api/test/push/send-by-alias` | 按别名推送 |
| `/api/test/push/send-by-registration-id` | 按注册ID推送 |
| `/api/message-repair/repair-payload` | 消息修复 |
| `/api/message-repair/statistics` | 消息统计 |

---

### 8. 其他接口

#### 8.1 社区论坛

| 接口 | 功能 |
|------|------|
| `/api/Post/GetList` | 帖子列表 |
| `/api/Post/Add` | 发帖 |
| `/api/Post/PostDetail/{id}` | 帖子详情 |
| `/api/Post/Edit` | 编辑帖子 |
| `/api/Post/Delete/{id}` | 删除帖子 |
| `/api/Post/SetEssence/{id}` | 设为精华 |
| `/api/Post/SetTop/{id}` | 置顶 |
| `/api/PostCategory/*` | 帖子分类管理 |
| `/api/PostBulletin/*` | 公告板管理 |

#### 8.2 广告位

| 接口 | 功能 |
|------|------|
| `/api/AdvertisingSpace/GetList` | 广告位列表 |
| `/api/AdvertisingSpace/Add` | 添加广告位 |
| `/api/AdvertisingSpace/Edit` | 编辑广告位 |
| `/api/AdvertisingSpace/Delete/{id}` | 删除广告位 |

#### 8.3 热词

| 接口 | 功能 |
|------|------|
| `/api/HotWords/GetList` | 热词列表 |
| `/api/HotWords/Add` | 添加热词 |
| `/api/HotWords/Edit` | 编辑热词 |
| `/api/HotWords/Delete/{id}` | 删除热词 |

#### 8.4 监控

| 接口 | 功能 |
|------|------|
| `/api/Monitor/health` | 健康检查 |
| `/api/Monitor/errors` | 错误日志 |
| `/api/Monitor/performance` | 性能监控 |
| `/api/Monitor/slow-requests` | 慢请求 |
| `/api/Monitor/clear-stats` | 清除统计 |

---

## 测试优先级建议

### P0 - 核心业务流程 (必须测试)

1. 拍卖品管理: Create, Update, Delete, GetAll
2. 出价历史: GetMyBidHistory, GetAll
3. 支付相关: PayDeposit, GetPayOrderStatus
4. 用户余额: GetMyAll
5. 认证: LogOut, RefreshToken

### P1 - 重要功能 (建议测试)

1. 用户管理: Delete, ResetPassword, GetUsersInRole
2. 好友功能: AddFriend, Agree
3. 公告管理: Create, Update, Delete
4. WebSocket: sub-channel, send-msg, get-channels
5. 微信登录: WeixinMiniAuthenticate

### P2 - 管理功能 (可选测试)

1. 角色管理: Create, Update, Delete
2. 敏感词管理: GetAll, Update, Delete
3. CMS管理: Create, Update, Delete
4. 租户管理: Create, Update, Delete

### P3 - 第三方集成 (需配置)

1. 微信支付 (需配置微信商户)
2. 内容安全 (需配置微信内容安全API)
3. 推送服务 (需配置极光推送)

---

## 测试阻塞因素

| 因素 | 影响接口数 | 解决方案 |
|------|-----------|---------|
| 微信支付未配置 | 6 | 配置微信商户号 |
| 微信小程序未配置 | 3 | 配置微信小程序 |
| 极光推送未配置 | 4 | 配置极光推送 |
| WebSocket测试环境 | 10 | 需要WebSocket客户端 |

---

## 建议下一步

1. **优先完成 P0 核心业务测试** (约 30 个接口)
2. **配置微信支付** 以测试支付流程
3. **准备 WebSocket 测试环境** 以测试实时通信
4. **编写自动化测试脚本** 覆盖核心业务流程