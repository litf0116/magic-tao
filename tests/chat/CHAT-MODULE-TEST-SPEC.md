# 聊天/消息模块测试规范

## 1. 测试概述

### 1.1 测试目标
验证聊天室、消息历史、表情、好友等聊天相关功能。

### 1.2 测试环境
```
后端服务: http://localhost:12580
数据库: MySQL 127.0.0.1:3306/www_molitao_top
测试用户: feifei (ID: 7509)
```

## 2. API接口

### 2.1 获取公开频道列表
```bash
GET /api/services/app/ChatGroup/GetAllPublic
Authorization: Bearer {token}
```

### 2.2 获取频道消息历史
```bash
GET /api/services/app/Message/GetChanHistory?chan=-1_auction&maxId=0&count=5
Authorization: Bearer {token}
```

### 2.3 获取频道最后消息ID
```bash
GET /api/services/app/Message/GetChanLastId?chan=-1_auction
Authorization: Bearer {token}
```

### 2.4 获取私聊消息历史
```bash
GET /api/services/app/Message/GetPrivateHistory?targetUserId={userId}&maxId=0&count=20
Authorization: Bearer {token}
```

### 2.5 获取表情列表
```bash
GET /api/services/app/ChatEmoji/GetAll
Authorization: Bearer {token}
```

### 2.6 获取聊天列表
```bash
GET /api/services/app/Client/GetChatList
Authorization: Bearer {token}
```

### 2.7 删除聊天记录
```bash
GET /api/services/app/Client/DeleteChatList?toUserId={userId}
Authorization: Bearer {token}
```

## 3. 数据库表

### 3.1 T_Message (消息表)
- 总记录数: 722,980
- 关键字段: Id, Chan, FromUserId, MsgType, Payload, CreationTime

### 3.2 T_ChatChannel (频道表)
- 总记录数: 5
- 关键字段: Id, Name, Chan

### 3.3 t_chatemoji (表情表)
- 总记录数: 143

### 3.4 t_chatlistdelete (聊天删除记录表)
- 总记录数: 2,214

## 4. 测试用例

### 4.1 频道消息测试
- ✅ 获取频道消息历史
- ✅ 获取频道最后消息ID
- ✅ 获取公开频道列表

### 4.2 聊天列表测试
- ✅ 获取聊天列表 (返回3条)
- ❌ 删除聊天记录 (待测试)

### 4.3 表情测试
- ⚠️ 获取表情列表返回0条 (数据库有143条，需排查)

## 5. 测试结果

| 测试项 | 状态 | 备注 |
|-------|------|------|
| 获取频道列表 | ✅ 通过 | 返回1个频道 |
| 获取频道消息历史 | ✅ 通过 | 返回1条消息 |
| 获取频道最后消息ID | ✅ 通过 | 返回UUID |
| 获取表情列表 | ❌ 异常 | 返回0条，数据库有143条 |
| 获取聊天列表 | ✅ 通过 | 返回3条 |

## 6. 数据库验证

```sql
-- 验证频道消息
SELECT COUNT(*) FROM T_Message WHERE Chan = '-1_auction';

-- 验证表情
SELECT COUNT(*) FROM t_chatemoji;

-- 验证聊天频道
SELECT * FROM T_ChatChannel;
```

---
**最后更新**: 2026-04-04
