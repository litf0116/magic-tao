# 我的好友数据问题重新分析

## 问题重新定义
用户期望获取"我的好友"列表，但当前接口返回空数组。需要重新分析业务逻辑。

## 业务逻辑重新梳理

### 1. 当前接口行为
- `GetUserFriends(long id, bool status)` 
- 查询条件：`UserId == id && Status == status`
- 只查找用户作为发起方的好友记录

### 2. 问题分析
**好友关系的双向性**：
- 当A添加B为好友，创建记录：(UserId=A, FriendId=B, Status=false)
- 当B同意后，创建反向记录：(UserId=B, FriendId=A, Status=true)
- A的原始记录也会被更新为Status=true

**当前查询的局限性**：
- 只查询`UserId == 7509`的记录
- 忽略了用户作为接收方的好友关系
- 可能导致漏掉部分好友

### 3. 正确的"我的好友"查询逻辑

应该查询所有涉及该用户的好友关系，无论用户是发起方还是接收方：

```csharp
// 获取用户的好友（无论用户是发起方还是接收方）
var myFriends = await repository.GetAll().AsNoTracking()
    .Where(x => (x.UserId == userId || x.FriendId == userId) && x.Status == true)
    .ToListAsync();
```

### 4. 可能的问题原因

#### ✅ 最可能：查询逻辑不完整
- 当前只查询`UserId == 7509`的记录
- 可能遗漏了`FriendId == 7509`的好友关系

#### ⚠️ 缓存过滤问题
- 代码中使用`userCache.GetAsync(u.FriendId)`过滤
- 如果好友缓存不存在，会被过滤掉

#### ❌ 数据问题
- 用户7509确实没有任何好友

## 建议的修复方案

### 方案1：修改查询逻辑（推荐）
```csharp
public async Task<ListResultDto<UserDtoBase>> GetMyFriends(long userId)
{
    // 获取所有涉及该用户的已接受好友关系
    var friendships = await repository.GetAll().AsNoTracking()
        .Where(x => (x.UserId == userId || x.FriendId == userId) && x.Status == true)
        .ToListAsync();

    List<UserDtoBase> result = [];
    foreach (var friendship in friendships)
    {
        // 获取好友ID（排除用户自己）
        long friendId = friendship.UserId == userId ? friendship.FriendId : friendship.UserId;
        
        var cache = await userCache.GetAsync(friendId);
        if (cache != null)
            result.Add(ObjectMapper.Map<UserDtoBase>(cache));
    }
    
    return new ListResultDto<UserDtoBase>(result);
}
```

### 方案2：添加新的API端点
保留原有接口，添加新的专用接口：
```csharp
[HttpGet]
public async Task<ListResultDto<UserDtoBase>> GetMyFriends(long userId)
{
    // 新的实现逻辑
}

[HttpGet]
public async Task<ListResultDto<UserDtoBase>> GetUserFriends(long id, bool status)
{
    // 保持原有逻辑不变
}
```

## 验证步骤

1. **数据库验证**：
```sql
-- 查看用户7509的所有好友关系（双向）
SELECT * FROM T_UserFriend 
WHERE (UserId = 7509 OR FriendId = 7509) AND Status = 1;
```

2. **API测试**：
```bash
# 测试新的获取我的好友接口
GET /api/services/app/UserFriend/GetMyFriends?userId=7509
```

3. **缓存验证**：
检查`userCache.GetAsync()`是否能正确获取好友的用户信息

## 结论
当前查询逻辑过于局限，只考虑了用户作为发起方的情况，忽略了双向好友关系的特性。需要修改查询逻辑来获取完整的"我的好友"列表。