# 好友申请功能测试用例

## 概述

本文档记录好友申请功能的完整测试用例，用于回归测试和功能验证。

## 测试环境

| 配置 | 值 |
|------|-----|
| 本地后端 | `http://localhost:12580` |
| 测试用户 | 用户 14、用户 7509 |
| 数据库 | MySQL `www_molitao_top` |

### 获取测试 Token

```bash
# 获取用户 14 的 Token
TOKEN_14=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}' | python3 -c "import sys,json; print(json.load(sys.stdin)['result']['accessToken'])")

# 获取用户 7509 的 Token
TOKEN_7509=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 7509}' | python3 -c "import sys,json; print(json.load(sys.stdin)['result']['accessToken'])")
```

### 清理测试数据

```bash
mysql -h 127.0.0.1 -u root -proot -D www_molitao_top -e "DELETE FROM T_UserFriend WHERE UserId IN (14, 7509) OR FriendId IN (14, 7509);"
```

---

## 测试用例

### TC-001: 用户申请添加好友

**前置条件**: 清理好友关系

**操作步骤**:
```bash
# 用户14 申请添加用户7509为好友
curl -s "http://localhost:12580/api/services/app/UserFriend/AddFriend?id=7509" \
  -H "Authorization: Bearer $TOKEN_14"
```

**预期结果**:
1. 接口返回 `{"success": true}`
2. 数据库插入记录: `UserId=7509, FriendId=14, Status=0`

**验证 SQL**:
```sql
SELECT * FROM T_UserFriend WHERE UserId=7509 AND FriendId=14;
-- 应返回 Status=0
```

---

### TC-002: 查看好友申请数量

**前置条件**: TC-001 已执行

**操作步骤**:
```bash
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriendCount" \
  -H "Authorization: Bearer $TOKEN_7509"
```

**预期结果**:
```json
{"result":{"count":1}}
```

---

### TC-003: 查看待处理的好友申请列表

**前置条件**: TC-001 已执行

**操作步骤**:
```bash
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=7509&status=false" \
  -H "Authorization: Bearer $TOKEN_7509"
```

**预期结果**:
1. 返回用户 14 的信息
2. 用户 ID 为 14

---

### TC-004: 同意好友申请

**前置条件**: TC-001 已执行

**操作步骤**:
```bash
curl -s "http://localhost:12580/api/services/app/UserFriend/Agree?id=14&status=true" \
  -H "Authorization: Bearer $TOKEN_7509"
```

**预期结果**:
1. 接口返回 `{"success": true}`
2. 数据库生成双向记录:
   - `UserId=14, FriendId=7509, Status=1`
   - `UserId=7509, FriendId=14, Status=1`

**验证 SQL**:
```sql
SELECT * FROM T_UserFriend WHERE UserId IN (14, 7509) OR FriendId IN (14, 7509);
-- 应返回 2 条 Status=1 的记录
```

---

### TC-005: 查看已同意的好友列表

**前置条件**: TC-004 已执行

**操作步骤**:
```bash
# 用户14 查看好友列表
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=14&status=true" \
  -H "Authorization: Bearer $TOKEN_14"

# 用户7509 查看好友列表
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=7509&status=true" \
  -H "Authorization: Bearer $TOKEN_7509"
```

**预期结果**:
1. 用户14 的好友列表包含用户 7509
2. 用户7509 的好友列表包含用户 14

---

### TC-006: 好友之间发送消息

**前置条件**: TC-004 已执行

**操作步骤**:
```bash
# 用户7509 给用户14发送消息（7509非管理员，需校验好友关系）
curl -s -X POST "http://localhost:12580/ws/send-msg" \
  -H "Authorization: Bearer $TOKEN_7509" \
  -H "Content-Type: application/json" \
  -d '{"from": 7509, "to": 14, "message": {"type": 1, "msg": "测试消息"}}'
```

**预期结果**:
```json
{"success": true, "result": {"code": 0, "data": {...}}}
```

---

### TC-007: 非好友发送消息被拒绝

**前置条件**: 清理好友关系

**操作步骤**:
```bash
# 用户7509 给用户14发送消息（非好友）
curl -s -X POST "http://localhost:12580/ws/send-msg" \
  -H "Authorization: Bearer $TOKEN_7509" \
  -H "Content-Type: application/json" \
  -d '{"from": 7509, "to": 14, "message": {"type": 1, "msg": "测试消息"}}'
```

**预期结果**:
```json
{
  "success": false,
  "error": {"message": "对方不是你的好友，无法发送消息"}
}
```

---

### TC-008: 拒绝好友申请

**前置条件**: 重新执行 TC-001

**操作步骤**:
```bash
curl -s "http://localhost:12580/api/services/app/UserFriend/Agree?id=14&status=false" \
  -H "Authorization: Bearer $TOKEN_7509"
```

**预期结果**:
1. 接口返回 `{"success": true}`
2. 数据库记录被删除

**验证 SQL**:
```sql
SELECT * FROM T_UserFriend WHERE UserId=7509 AND FriendId=14;
-- 应无记录
```

---

### TC-009: 重复申请好友

**前置条件**: TC-001 已执行

**操作步骤**:
```bash
# 用户14 再次申请添加用户7509
curl -s "http://localhost:12580/api/services/app/UserFriend/AddFriend?id=7509" \
  -H "Authorization: Bearer $TOKEN_14"
```

**预期结果**:
```json
{
  "success": false,
  "error": {"message": "请不要重复发送好友请求"}
}
```

---

### TC-010: 查看对方已是自己好友

**前置条件**: TC-004 已执行

**操作步骤**:
```bash
# 用户14 再次申请添加用户7509（已是好友）
curl -s "http://localhost:12580/api/services/app/UserFriend/AddFriend?id=7509" \
  -H "Authorization: Bearer $TOKEN_14"
```

**预期结果**:
```json
{
  "success": false,
  "error": {"message": "对方已是你的好友"}
}
```

---

## 快速回归测试脚本

```bash
#!/bin/bash

# 获取 Token
TOKEN_14=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 14}' | python3 -c "import sys,json; print(json.load(sys.stdin)['result']['accessToken'])")
TOKEN_7509=$(curl -s -X POST "http://localhost:12580/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId": 7509}' | python3 -c "import sys,json; print(json.load(sys.stdin)['result']['accessToken'])")

# 清理数据
mysql -h 127.0.0.1 -u root -proot -D www_molitao_top -e "DELETE FROM T_UserFriend WHERE UserId IN (14, 7509) OR FriendId IN (14, 7509);" 2>/dev/null

echo "=== TC-001: 用户申请添加好友 ==="
curl -s "http://localhost:12580/api/services/app/UserFriend/AddFriend?id=7509" -H "Authorization: Bearer $TOKEN_14"
echo -e "\n"

echo "=== TC-002: 查看好友申请数量 ==="
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriendCount" -H "Authorization: Bearer $TOKEN_7509"
echo -e "\n"

echo "=== TC-004: 同意好友申请 ==="
curl -s "http://localhost:12580/api/services/app/UserFriend/Agree?id=14&status=true" -H "Authorization: Bearer $TOKEN_7509"
echo -e "\n"

echo "=== TC-005: 查看好友列表 ==="
echo "用户14:"
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=14&status=true" -H "Authorization: Bearer $TOKEN_14" | python3 -c "import sys,json; d=json.load(sys.stdin); print('好友数:', len(d['result']['items']))"
echo "用户7509:"
curl -s "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=7509&status=true" -H "Authorization: Bearer $TOKEN_7509" | python3 -c "import sys,json; d=json.load(sys.stdin); print('好友数:', len(d['result']['items']))"
echo -e "\n"

echo "=== TC-006: 好友之间发送消息 ==="
curl -s -X POST "http://localhost:12580/ws/send-msg" -H "Authorization: Bearer $TOKEN_7509" \
  -H "Content-Type: application/json" \
  -d '{"from": 7509, "to": 14, "message": {"type": 1, "msg": "回归测试消息"}}' | python3 -c "import sys,json; d=json.load(sys.stdin); print('成功!' if d.get('success') else d)"
```

---

## 相关文件

| 文件 | 说明 |
|------|------|
| `docs/friend-request-business-logic.md` | 好友申请业务逻辑文档 |
| `backend/src/TtWork.Project/Applications/UserFriendAppService.cs` | 核心服务 |
| `backend/src/TtWork.Project/Controllers/WebsocketController.cs` | 消息发送校验 |

## 更新记录

| 日期 | 版本 | 说明 |
|------|------|------|
| 2026-04-23 | v1.0 | 初始测试用例文档 |
