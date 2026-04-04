# 未使用接口记录

**分析时间**: 2026-04-04
**分析范围**: PC 端 + UniApp 端

---

## 统计概览

| 项目 | 数量 |
|------|------|
| PC 端使用接口 | 219 |
| UniApp 端使用接口 | 65 |
| 合并去重后使用 | 234 |
| 后端总接口数 | 260 |
| **未使用接口** | **56** |

---

## 未使用接口列表

### 1. 广告位管理 (3个)

| 接口 | 说明 |
|------|------|
| `/api/AdvertisingSpace/Delete/{id}` | 删除广告位 |
| `/api/AdvertisingSpace/GetTypeList/{type}` | 按类型获取广告位列表 |
| `/api/AdvertisingSpace/UpdateState/{id}/{status}` | 更新广告位状态 |

### 2. 内容安全 (2个)

| 接口 | 说明 |
|------|------|
| `/api/ContentSecurity/CheckContent` | 内容检测 |
| `/api/ContentSecurity/TestWeixinConnection` | 测试微信连接 |

### 3. 群聊等级设置 (4个)

| 接口 | 说明 |
|------|------|
| `/api/GroupChatLevelSettings/DeleteGroupChatLevelSetting/{id}` | 删除群聊等级设置 |
| `/api/GroupChatLevelSettings/GetUserGroupLevel/{id}` | 获取用户群等级 |
| `/api/GroupChatLevelSettings/GetUserLevelInfo/{id}` | 获取用户等级信息 |
| `/api/GroupChatLevelSettings/TestDataStructure` | 测试数据结构 |

### 4. 热词管理 (2个)

| 接口 | 说明 |
|------|------|
| `/api/HotWords/Delete/{id}` | 删除热词 |
| `/api/HotWords/Detail/{id}` | 热词详情 |

### 5. 消息修复工具 (2个)

| 接口 | 说明 |
|------|------|
| `/api/message-repair/repair-payload` | 修复消息载荷 |
| `/api/message-repair/statistics` | 消息统计 |

### 6. 监控接口 (5个)

| 接口 | 说明 |
|------|------|
| `/api/Monitor/clear-stats` | 清除统计 |
| `/api/Monitor/errors` | 错误日志 |
| `/api/Monitor/health` | 健康检查 |
| `/api/Monitor/performance` | 性能监控 |
| `/api/Monitor/slow-requests` | 慢请求 |

### 7. 支付回调 (1个)

| 接口 | 说明 |
|------|------|
| `/api/PayNotify/TenPay/{appName}` | 微信支付回调 |

### 8. 帖子管理 (5个)

| 接口 | 说明 |
|------|------|
| `/api/Post/Delete/{id}` | 删除帖子 |
| `/api/Post/PostDetail/{id}` | 帖子详情 |
| `/api/Post/SetEssence/{id}` | 设为精华 |
| `/api/Post/SetTop/{id}` | 置顶 |
| `/api/Post/UpdateStatus` | 更新状态 |

### 9. 公告板 (1个)

| 接口 | 说明 |
|------|------|
| `/api/PostBulletin/Delete/{id}` | 删除公告板 |

### 10. 帖子分类 (2个)

| 接口 | 说明 |
|------|------|
| `/api/PostCategory/Delete/{id}` | 删除分类 |
| `/api/PostCategory/UpdateState/{id}/{status}` | 更新状态 |

### 11. 拍卖回调 (2个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/AuctionItem/Callback` | 拍卖回调 |
| `/api/services/app/AuctionItem/GetCurrentAuctionItem` | 获取当前拍卖品 |

### 12. 出价资格 (3个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/BidEligibility/CheckBidEligibility` | 检查出价资格 |
| `/api/services/app/BidEligibility/CheckUserBidCapability` | 检查用户出价能力 |
| `/api/services/app/BidEligibility/CheckUserBidCapabilityById` | 按ID检查出价能力 |

### 13. 出价历史 (1个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/BidHistory/GetMyBidHistory` | 我的出价历史 |

### 14. 支付退款 (1个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/Client/PayRefund` | 退款 |

### 15. 敏感词 (2个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/SensitiveWord/CheckCacheHealth` | 检查缓存健康 |
| `/api/services/app/SensitiveWord/GetCachedWords` | 获取缓存敏感词 |

### 16. 版本控制 (2个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/VersionControl/GetLatestStableVersion` | 获取最新稳定版本 |
| `/api/services/app/VersionControl/UpdateLatestStableVersion` | 更新最新稳定版本 |

### 17. 提现审批 (1个)

| 接口 | 说明 |
|------|------|
| `/api/services/app/WithdrawalAmountService/ApproveWithdrawal` | 审批提现 |

### 18. 测试接口 (9个)

| 接口 | 说明 |
|------|------|
| `/api/test/message/channels` | 测试频道 |
| `/api/test/message/channels/sync-all` | 同步所有频道 |
| `/api/test/message/channels/sync-user` | 同步用户频道 |
| `/api/test/message/chat-list` | 测试聊天列表 |
| `/api/test/message/send-channel` | 发送测试频道消息 |
| `/api/test/push/broadcast` | 推送广播测试 |
| `/api/test/push/send` | 发送推送测试 |
| `/api/test/push/send-by-alias` | 按别名推送测试 |
| `/api/test/push/send-by-registration-id` | 按注册ID推送测试 |

### 19. Token 工具 (2个)

| 接口 | 说明 |
|------|------|
| `/api/TokenAuth/GenerateHashedPassword` | 生成哈希密码 |
| `/api/TokenAuth/GenerateTokenForUser` | 生成用户Token |

### 20. 后台任务 (4个)

| 接口 | 说明 |
|------|------|
| `/GetChatChannelStats` | 获取聊天频道统计 |
| `/MigrateChatData` | 迁移聊天数据 |
| `/SyncAllUserDeleteStatus` | 同步所有用户删除状态 |
| `/SyncUserDeleteStatus` | 同步用户删除状态 |

### 21. WebSocket 接口 (10个)

| 接口 | 说明 |
|------|------|
| `/ws/backout` | 退出 |
| `/ws/ban-user` | 禁言用户 |
| `/ws/del-channel` | 删除频道 |
| `/ws/get-channels` | 获取频道 |
| `/ws/leave-channel` | 离开频道 |
| `/ws/offline` | 离线 |
| `/ws/pre-connect` | 预连接 |
| `/ws/send-msg` | 发送消息 |
| `/ws/SendChannelMsg` | 发送频道消息 |
| `/ws/sub-channel` | 订阅频道 |

---

## 分类汇总

| 分类 | 数量 | 说明 |
|------|------|------|
| 测试/工具接口 | 9 | 开发调试用，可跳过测试 |
| WebSocket 接口 | 10 | 需要 WebSocket 客户端测试 |
| 监控接口 | 5 | 后台监控，可跳过测试 |
| 后台任务 | 4 | 定时任务，可跳过测试 |
| 功能未使用 | 26 | 前端未调用的业务接口 |

---

## 建议

### 可以跳过测试的接口

1. **测试接口** (9个) - 仅用于开发调试
2. **监控接口** (5个) - 后台监控使用
3. **后台任务** (4个) - 定时任务或手动触发
4. **WebSocket** (10个) - 需要 WebSocket 测试环境

### 需要评估的功能接口 (26个)

这些接口前端未调用，建议与产品确认是否：

1. **保留** - 未来功能规划
2. **删除** - 废弃功能
3. **测试** - 隐藏功能但需要验证

---

## 跳过测试的接口汇总

共 **56 个** 接口可跳过测试：

- 测试接口: 9 个
- WebSocket: 10 个
- 监控接口: 5 个
- 后台任务: 4 个
- 未使用业务接口: 28 个