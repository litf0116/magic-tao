# 好友申请业务逻辑

## 概述

好友申请功能用于管理用户之间的好友关系，包括发送申请、查看申请列表、处理申请（同意/拒绝）等操作。

## 数据表结构

### t_userfriend 表

| 字段 | 类型 | 说明 |
|------|------|------|
| UserId | bigint | 接收方（被添加的人） |
| FriendId | bigint | 申请方（发起添加的人） |
| Status | tinyint(1) | 状态：0=待处理，1=已同意 |
| Remark | varchar(64) | 备注（可选） |

## 核心理解

### 一句话定义

> **FriendId 申请成为 UserId 的好友**

### 字段语义

| 字段 | 角色 | 说明 |
|------|------|------|
| **UserId** | 接收方 | 接收好友申请的人（被添加的人） |
| **FriendId** | 申请方 | 发起好友申请的人（主动添加的人） |

## API 接口

### 1. 发送好友申请

```
POST /api/services/app/UserFriend/AddFriend?id={用户ID}
```

- 当前登录用户向指定用户发送好友申请
- 如果对方已经发送过申请给我，则抛出异常

### 2. 获取好友申请列表

```
GET /api/services/app/UserFriend/GetUserFriends?id={用户ID}&status={状态}
```

- 查询条件：`FriendId = 当前用户 AND Status = 指定状态`
- 返回：符合条件的好友申请列表

### 3. 获取好友申请数量（红点数量）

```
GET /api/services/app/UserFriend/GetUserFriendCount
```

- 查询条件：`FriendId = 当前用户 AND Status = false`
- 返回：待处理的好友申请数量

### 4. 同意/拒绝好友申请

```
GET /api/services/app/UserFriend/Agree?id={申请人ID}&status={处理结果}
```

- 查询条件：`FriendId = 当前用户 AND UserId = 申请人ID`
- 处理逻辑：
  - **同意（status=true）**：更新 Status = true，并创建反向好友关系
  - **拒绝（status=false）**：删除该申请记录

## 业务流程图

```
用户A 添加 用户B：
┌─────────────────────────────────────────────────┐
│  UserId = B（接收方，被添加的人）                │
│  FriendId = A（申请方，主动添加的人）           │
│  Status = false（待处理）                      │
└─────────────────────────────────────────────────┘

用户B 打开好友申请列表：
→ 查询 FriendId = B 的记录 → 找到 A 发来的申请
→ 显示申请人信息：从 UserId 获取（A的用户信息）
```

## 常见问题

### Q: 为什么查询申请列表用 FriendId 而不是 UserId？

A: 因为 `FriendId = 申请方`。当用户B查看"发给我的申请"时，需要查找 `FriendId = B` 的记录，即 FriendId 为 B 的记录表示"有人向 B 发起申请"。

### Q: 好友关系是双向的吗？

A: 是的。当 A 申请添加 B，B 同意后，系统会创建两条记录：
1. `UserId=B, FriendId=A, Status=true` - B 的好友列表显示 A
2. `UserId=A, FriendId=B, Status=true` - A 的好友列表显示 B

## 相关文件

| 文件 | 说明 |
|------|------|
| `backend/src/TtWork.Project/Applications/UserFriendAppService.cs` | 好友申请核心服务 |
| `molitao_uniapp/src/views/chat/contacts.vue` | 小程序好友申请页面 |
| `molitao_uniapp/src/api/userFriendAPI.ts` | 小程序 API 调用 |
| `molitao_uniapp/src/stores/chatStore.ts` | 好友状态管理 |

## 更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-04-23 | v1.0 | 初始文档，定义 FriendId/UserId 语义 |
