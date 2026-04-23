# 好友申请系统测试用例

## 一、文档概述

本文档记录好友申请系统的功能测试用例、业务规则验证和API调用示例。

**测试环境**: localhost:12580
**测试账号**: 用户14（管理员）、用户7509（普通用户feifei）

---

## 二、数据模型

### 2.1 T_UserFriend 表结构

| 字段名 | 类型 | 说明 |
|--------|------|------|
| Id | int | 主键 |
| UserId | long | 接收方用户ID（收到请求的人） |
| FriendId | long | 申请方用户ID（发起请求的人） |
| Status | bool | true=已同意, false=等待同意 |
| Remark | string(64) | 备注 |

### 2.2 业务语义

```
好友申请流程：
用户A（申请方） → AddFriend(B的用户ID) → 创建记录(UserId=B, FriendId=A, Status=false)
                                                        ↓
用户B（接收方） → Agree(A的用户ID, true) → 更新记录(UserId=B, FriendId=A, Status=true)
                                                        ↓
                                              自动创建反向记录(UserId=A, FriendId=B, Status=true)
```

---

## 三、API 接口清单

| 接口 | 方法 | 权限 | 说明 |
|------|------|------|------|
| `/api/services/app/UserFriend/AddFriend` | GET | 登录用户 | 添加好友 |
| `/api/services/app/UserFriend/GetUserFriends` | GET | 登录用户 | 获取好友列表 |
| `/api/services/app/UserFriend/GetUserFriendCount` | GET | 登录用户 | 获取待处理请求数量 |
| `/api/services/app/UserFriend/Agree` | GET | 登录用户 | 同意/拒绝好友 |

---

## 四、测试用例

### 4.1 TC-001: 正常添加好友

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-001 |
| **用例名称** | 正常添加好友 |
| **前置条件** | 用户14和用户7509之间不存在好友关系 |
| **测试步骤** | 1. 用户7509调用 AddFriend(14) |
| **预期结果** | 成功创建好友请求记录：UserId=14, FriendId=7509, Status=false |
| **验证方式** | 用户14调用 GetUserFriendCount() 返回 1 |

### 4.2 TC-002: 重复添加好友

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-002 |
| **用例名称** | 重复添加好友 |
| **前置条件** | 用户7509已向用户14发送过好友请求（Status=false） |
| **测试步骤** | 1. 用户7509再次调用 AddFriend(14) |
| **预期结果** | 抛出异常："请不要重复发送好友请求" |
| **验证方式** | 检查异常消息 |

### 4.3 TC-003: 添加已是好友的用户

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-003 |
| **用例名称** | 添加已是好友的用户 |
| **前置条件** | 用户14和用户7509已是双向好友（Status=true） |
| **测试步骤** | 1. 用户7509调用 AddFriend(14) |
| **预期结果** | 抛出异常："对方已是你的好友" |
| **验证方式** | 检查异常消息 |

### 4.4 TC-004: 获取待处理好友请求

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-004 |
| **用例名称** | 获取待处理好友请求 |
| **前置条件** | 用户14有1条待处理请求（用户7509发送的） |
| **测试步骤** | 1. 用户14调用 GetUserFriends(14, false) |
| **预期结果** | 返回包含用户7509的列表，Count=1 |
| **验证方式** | 检查返回列表长度和用户信息 |

### 4.5 TC-005: 获取已同意好友列表

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-005 |
| **用例名称** | 获取已同意好友列表 |
| **前置条件** | 用户14和用户7509已是双向好友 |
| **测试步骤** | 1. 用户14调用 GetUserFriends(14, true) |
| **预期结果** | 返回包含用户7509的列表 |
| **验证方式** | 检查返回列表长度和用户信息 |

### 4.6 TC-006: 获取待处理请求数量

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-006 |
| **用例名称** | 获取待处理请求数量 |
| **前置条件** | 用户14有2条待处理请求 |
| **测试步骤** | 1. 用户14调用 GetUserFriendCount() |
| **预期结果** | 返回 {count: 2} |
| **验证方式** | 检查返回的count值 |

### 4.7 TC-007: 同意好友请求（双向记录已存在）

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-007 |
| **用例名称** | 同意好友请求（双向记录已存在） |
| **前置条件** | 用户7509向用户14发送请求，用户14曾向用户7509发送过请求（记录存在但Status=false） |
| **测试步骤** | 1. 用户14调用 Agree(7509, true) |
| **预期结果** | 1. 记录(UserId=14, FriendId=7509) Status更新为true |
| | 2. 记录(UserId=7509, FriendId=14) Status更新为true |
| **验证方式** | 用户14和用户7509调用 GetUserFriends(?, true)都能看到对方 |

### 4.8 TC-008: 同意好友请求（反向记录不存在）

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-008 |
| **用例名称** | 同意好友请求（反向记录不存在） |
| **前置条件** | 用户7509向用户14发送请求，用户14从未向用户7509发送过请求 |
| **测试步骤** | 1. 用户14调用 Agree(7509, true) |
| **预期结果** | 1. 记录(UserId=14, FriendId=7509) Status更新为true |
| | 2. 自动创建新记录(UserId=7509, FriendId=14, Status=true) |
| **验证方式** | 用户14和用户7509调用 GetUserFriends(?, true)都能看到对方 |

### 4.9 TC-009: 拒绝好友请求

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-009 |
| **用例名称** | 拒绝好友请求 |
| **前置条件** | 用户7509向用户14发送了好友请求 |
| **测试步骤** | 1. 用户14调用 Agree(7509, false) |
| **预期结果** | 1. 记录(UserId=14, FriendId=7509)被删除 |
| | 2. 用户14调用 GetUserFriendCount() 返回 0 |
| **验证方式** | 记录被删除，好友数量为0 |

### 4.10 TC-010: 同意不存在的记录

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-010 |
| **用例名称** | 同意不存在的记录 |
| **前置条件** | 用户14和用户7509之间不存在任何好友关系 |
| **测试步骤** | 1. 用户14调用 Agree(7509, true) |
| **预期结果** | 抛出异常："记录不存在" |
| **验证方式** | 检查异常消息 |

### 4.11 TC-011: 自己给自己发送好友请求

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-011 |
| **用例名称** | 自己给自己发送好友请求 |
| **前置条件** | 无 |
| **测试步骤** | 1. 用户14调用 AddFriend(14) |
| **预期结果** | 无异常也不创建记录（代码中 if (id != AbpSession.UserId)） |
| **验证方式** | 用户14调用 GetUserFriendCount() 返回 0 |

### 4.12 TC-012: 未登录用户添加好友

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-012 |
| **用例名称** | 未登录用户添加好友 |
| **前置条件** | 无 |
| **测试步骤** | 1. 不带Token调用 AddFriend(14) |
| **预期结果** | 返回401未授权 |
| **验证方式** | 检查HTTP状态码 |

### 4.13 TC-013: 查看别人的好友列表

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-013 |
| **用例名称** | 查看别人的好友列表 |
| **前置条件** | 用户14有已同意的好友 |
| **测试步骤** | 1. 用户7509调用 GetUserFriends(14, true) |
| **预期结果** | 正常返回用户14的好友列表（不限制只能看自己的） |
| **验证方式** | 检查返回列表 |

### 4.14 TC-014: 多次拒绝同一请求

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-014 |
| **用例名称** | 多次拒绝同一请求 |
| **前置条件** | 用户7509向用户14发送好友请求，用户14已拒绝过一次 |
| **测试步骤** | 1. 用户14调用 Agree(7509, false)（第二次） |
| **预期结果** | 抛出异常："记录不存在" |
| **验证方式** | 检查异常消息 |

### 4.15 TC-015: A申请B，B申请A，然后A同意B

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-015 |
| **用例名称** | 双向申请后一方同意 |
| **前置条件** | 无 |
| **测试步骤** | 1. 用户14调用 AddFriend(7509)（14申请7509） |
| | 2. 用户7509调用 AddFriend(14)（7509申请14） |
| | 3. 用户14调用 Agree(7509, true)（14同意7509） |
| **预期结果** | 两人的所有记录 Status 都为 true |
| **验证方式** | 用户14和7509调用 GetUserFriends(?, true) 都能看到对方 |

### 4.16 TC-016: A申请B，B拒绝后A再次申请

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-016 |
| **用例名称** | 拒绝后重新申请 |
| **前置条件** | 用户7509向用户14发送请求，用户14拒绝 |
| **测试步骤** | 1. 用户14调用 Agree(7509, false) |
| | 2. 用户7509再次调用 AddFriend(14) |
| **预期结果** | 成功创建新的好友请求记录 |
| **验证方式** | 用户14调用 GetUserFriendCount() 返回 1 |

### 4.17 TC-017: 边界值测试 - 获取好友数量为0

| 项目 | 内容 |
|------|------|
| **用例ID** | TC-017 |
| **用例名称** | 获取好友数量为0 |
| **前置条件** | 用户14没有任何好友请求 |
| **测试步骤** | 1. 用户14调用 GetUserFriendCount() |
| **预期结果** | 返回 {count: 0} |
| **验证方式** | 检查返回的count值 |

---

## 五、API 调用示例

### 5.1 添加好友

```bash
# 用户7509添加用户14为好友
curl -X GET "http://localhost:12580/api/services/app/UserFriend/AddFriend?id=14" \
  -H "Authorization: Bearer $TOKEN_7509"
```

### 5.2 获取待处理好友请求

```bash
# 用户14获取自己收到的待处理好友请求
curl -X GET "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=14&status=false" \
  -H "Authorization: Bearer $TOKEN_14"
```

### 5.3 获取已同意好友列表

```bash
# 用户14获取已同意的好友列表
curl -X GET "http://localhost:12580/api/services/app/UserFriend/GetUserFriends?id=14&status=true" \
  -H "Authorization: Bearer $TOKEN_14"
```

### 5.4 获取待处理请求数量

```bash
# 用户14获取待处理好友请求数量
curl -X GET "http://localhost:12580/api/services/app/UserFriend/GetUserFriendCount" \
  -H "Authorization: Bearer $TOKEN_14"
```

### 5.5 同意好友请求

```bash
# 用户14同意用户7509的好友请求
curl -X GET "http://localhost:12580/api/services/app/UserFriend/Agree?id=7509&status=true" \
  -H "Authorization: Bearer $TOKEN_14"
```

### 5.6 拒绝好友请求

```bash
# 用户14拒绝用户7509的好友请求
curl -X GET "http://localhost:12580/api/services/app/UserFriend/Agree?id=7509&status=false" \
  -H "Authorization: Bearer $TOKEN_14"
```

---

## 六、测试脚本

### 6.1 完整流程测试脚本

```bash
#!/bin/bash

# 测试配置
BASE_URL="http://localhost:12580"
TOKEN_14=$(curl -s -X POST "$BASE_URL/api/TokenAuth/GenerateTokenForUser" \
  -H "Content-Type: application/json" \
  -d '{"userId":14}' | jq -r '.result.accessToken')

TOKEN_7509=$(curl -s -X POST "$BASE_URL/api/TokenAuth/Authenticate" \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"feifei","password":"123456"}' | jq -r '.result.accessToken')

echo "=== TC-001: 添加好友 ==="
curl -X GET "$BASE_URL/api/services/app/UserFriend/AddFriend?id=14" \
  -H "Authorization: Bearer $TOKEN_7509"
echo ""

echo "=== TC-006: 获取待处理数量 ==="
curl -X GET "$BASE_URL/api/services/app/UserFriend/GetUserFriendCount" \
  -H "Authorization: Bearer $TOKEN_14"
echo ""

echo "=== TC-007: 同意好友请求 ==="
curl -X GET "$BASE_URL/api/services/app/UserFriend/Agree?id=7509&status=true" \
  -H "Authorization: Bearer $TOKEN_14"
echo ""

echo "=== TC-005: 获取已同意好友列表 ==="
curl -X GET "$BASE_URL/api/services/app/UserFriend/GetUserFriends?id=14&status=true" \
  -H "Authorization: Bearer $TOKEN_14"
echo ""
```

---

## 七、业务规则总结

1. **好友申请单向性**：好友请求由申请方发起，接收方决定是否同意
2. **双向好友关系**：同意后自动建立双向好友记录
3. **状态语义**：
   - `Status=false` 在 GetUserFriends 中表示"等待我同意的请求"
   - `Status=true` 在 GetUserFriends 中表示"已同意的好友"
4. **防重复**：已发送请求或已是好友时拒绝重复操作
5. **无自添加**：不能给自己发送好友请求

---

## 八、相关文档

| 文档 | 说明 |
|------|------|
| [auction-api-testing-guide.md](./auction-api-testing-guide.md) | 拍卖API测试文档 |
| [profile-completion-guide.md](./profile-completion-guide.md) | 个人信息完善测试指南 |

---

**最后更新**: 2026-04-24