# 用户模块测试规范

## 1. 测试概述

### 1.1 测试目标
验证用户管理相关功能，包括用户信息获取、好友管理、个人资料修改等。

### 1.2 测试环境
```
后端服务: http://localhost:12580
数据库: MySQL 127.0.0.1:3306/www_molitao_top
测试用户: feifei (ID: 7509) / admin
```

## 2. 测试账号

| 角色 | 用户名 | 密码 | 用户ID |
|------|--------|------|--------|
| 普通用户 | feifei | 123456 | 7509 |
| 管理员 | admin | 123456 | - |

## 3. API接口

### 3.1 用户登录
```bash
POST /api/TokenAuth/Authenticate
Content-Type: application/json

{"userNameOrEmailAddress": "feifei", "password": "123456"}
```

### 3.2 获取当前登录信息
```bash
GET /api/services/app/Session/GetCurrentLoginInformations
Authorization: Bearer {token}
```

### 3.3 获取用户详情
```bash
GET /api/services/app/User/Get?Id={userId}
Authorization: Bearer {token}
```

### 3.4 获取当前用户
```bash
GET /api/services/app/User/GetCurrentUser
Authorization: Bearer {token}
```

### 3.5 获取好友列表
```bash
GET /api/services/app/UserFriend/GetUserFriends
Authorization: Bearer {token}
```

### 3.6 获取好友申请数量
```bash
GET /api/services/app/UserFriend/GetUserFriendCount
Authorization: Bearer {token}
```

## 4. 测试用例

### 4.1 登录测试
- ✅ 正确用户名密码登录成功
- ❌ 错误密码应返回明确错误
- ❌ 不存在的用户应返回明确错误

### 4.2 用户信息测试
- ✅ GetCurrentUser 返回当前用户信息
- ✅ Get 获取指定用户信息
- ✅ Session 获取登录信息

### 4.3 好友功能测试
- ✅ 获取好友列表
- ✅ 获取好友申请数量

## 5. 测试结果

| 测试项 | 状态 | 备注 |
|-------|------|------|
| 用户登录 | ✅ 通过 | Token有效期7天 |
| 获取当前登录信息 | ✅ 通过 | 返回用户+租户信息 |
| 获取用户详情 | ✅ 通过 | DepositBalance=0 |
| 获取当前用户 | ⚠️ 异常 | 返回数据格式不一致（见问题记录） |
| 好友列表 | ✅ 通过 | 返回0条 |
| 好友申请数量 | ⚠️ 异常 | 返回原始数字0而非对象 |

## 6. 数据库验证

```sql
-- 验证用户信息
SELECT Id, UserName, Name, DepositBalance, HeadImgUrl 
FROM AbpUsers WHERE Id = 7509;

-- 验证好友关系
SELECT * FROM t_userfriend WHERE UserId = 7509 OR TargetUserId = 7509;
```

---
**最后更新**: 2026-04-04
