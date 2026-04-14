# 更新用户密码说明

## 用户信息
- **用户ID**: 7509
- **新密码**: 123456

## 密码哈希值
使用系统的 `GenerateHashedPassword` API 生成的哈希值：
```
AQAAAAIAAYagAAAAEELvIS7IF2FX8osRxav+DfM8eAosC/ra0xZqxbzSsyzJmWb0NBs7L4HxxELQtQx1zg==
```

## 更新方法

### 方法1: 直接执行 SQL

在数据库中执行以下 SQL：

```sql
UPDATE t_users 
SET Password = 'AQAAAAIAAYagAAAAEELvIS7IF2FX8osRxav+DfM8eAosC/ra0xZqxbzSsyzJmWb0NBs7L4HxxELQtQx1zg=='
WHERE Id = 7509;
```

### 方法2: 使用密码重置接口

如果有密码重置接口，可以使用以下方式：

```bash
# 获取管理员 token
ADMIN_TOKEN=$(curl -s -X POST "http://127.0.0.1:12580/api/TokenAuth/GenerateTokenForUser" \
    -H "Content-Type: application/json" \
    -d '{"userId": 14}' | jq -r '.result.accessToken')

# 调用密码重置接口（如果存在）
curl -X POST "http://127.0.0.1:12580/api/services/app/User/ResetPassword" \
    -H "Authorization: Bearer $ADMIN_TOKEN" \
    -H "Content-Type: application/json" \
    -d '{
        "userId": 7509,
        "newPassword": "123456"
    }'
```

### 方法3: 使用应用程序接口（推荐）

使用系统的登录接口验证：

```bash
# 验证密码是否更新成功
curl -X POST "http://127.0.0.1:12580/api/TokenAuth/Authenticate" \
    -H "Content-Type: application/json" \
    -d '{
        "userNameOrEmailAddress": "feifei",
        "password": "123456"
    }' | jq
```

## 生成新密码哈希

如果需要为其他密码生成哈希值：

```bash
curl -X GET "http://127.0.0.1:12580/api/TokenAuth/GenerateHashedPassword?plainPassword=your_password"
```

## 注意事项

1. **密码哈希算法**: ASP.NET Identity 使用的是 PBKDF2 with HMAC-SHA256
2. **哈希特性**: 每次生成的哈希值都不同（因为包含随机盐值），但都可以验证同一个密码
3. **数据库影响**: 只影响 `t_users` 表中 ID=7509 的记录
4. **安全建议**: 更新密码后建议用户首次登录后修改密码

## 验证步骤

1. 执行 SQL 更新密码
2. 使用用户名 `feifei` 和密码 `123456` 登录验证
3. 如果成功，获取 token
4. 使用 token 访问其他接口验证权限

## 回滚

如果需要回滚，可以：
1. 记录更新前的密码哈希值
2. 或者重新生成一个新的随机密码
3. 或者联系数据库管理员恢复备份