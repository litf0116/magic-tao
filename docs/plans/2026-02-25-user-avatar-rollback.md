# 用户头像历史记录与回退功能实现计划

> **For Claude:** REQUIRED SUB-SKILL: Use @superpowers:executing-plans to implement this plan task-by-task.

**目标:** 实现用户头像修改历史记录功能，支持回退到上一个头像（最多保留 5 条历史记录）

**架构:**
- 在用户修改头像时自动保存旧头像到历史记录表
- 每个用户最多保留 5 条历史记录，超过时自动删除最旧的
- 提供管理员专用的回退接口，回退到最近的一条历史记录
- 回退后删除该条历史记录

**技术栈:**
- ABP Framework (ApplicationService, Repository)
- Entity Framework Core (Migration)
- MySQL 数据库

---

## Task 1: 创建实体类和数据表

**文件:**
- Create: `backend/src/TtWork.Project/Domains/Pays/UserAvatarHistory.cs`

**Step 1: 创建实体类**

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains.Pays;

/// <summary>
/// 用户头像修改历史（最多保留 5 条）
/// </summary>
[Table("Pays_UserAvatarHistory")]
public class UserAvatarHistory : Entity<long>, IMustHaveTenant
{
    public UserAvatarHistory()
    {
        ChangeTime = DateTime.Now;
    }

    /// <summary>
    /// 租户ID
    /// </summary>
    public int TenantId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 修改前的头像URL（用于回退）
    /// </summary>
    [StringLength(512)]
    public string PreviousHeadImgUrl { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ChangeTime { get; set; }

    /// <summary>
    /// 修改来源：User=用户上传, Admin=管理员修改, System=系统修正
    /// </summary>
    [StringLength(32)]
    public string ChangeSource { get; set; }
}
```

**Step 2: 添加到 DbContext**

修改文件: `backend/src/TtWork.Project.EntityFrameworkCore/TtWorkProjectDbContext.cs`

```csharp
// 在 DbSet 属性区域添加
public virtual DbSet<UserAvatarHistory> UserAvatarHistories { get; set; }
```

**Step 3: 创建数据库迁移**

运行命令:
```bash
cd backend
dotnet ef migrations add AddUserAvatarHistoryTable --startup-project src/TtWork.Project.Web.Host
```

**Step 4: 验证生成的迁移文件**

检查文件: `backend/src/TtWork.Project.EntityFrameworkCore/Migrations/YYYYMMDDHHmmss_AddUserAvatarHistoryTable.cs`

确认包含:
- `Pays_UserAvatarHistory` 表创建
- `UserId`, `PreviousHeadImgUrl`, `ChangeTime`, `ChangeSource`, `TenantId` 字段

**Step 5: 应用迁移到数据库**

运行命令:
```bash
cd backend/src/TtWork.Project.Web.Host
dotnet run --migrate-database
```

**Step 6: 验证表创建**

使用 MySQL 客户端检查:
```sql
SHOW CREATE TABLE Pays_UserAvatarHistory;
```

**Step 7: 提交变更**

```bash
git add backend/src/TtWork.Project/Domains/Pays/UserAvatarHistory.cs
git add backend/src/TtWork.Project.EntityFrameworkCore/
git commit -m "feat: 添加用户头像历史记录实体"
```

---

## Task 2: 创建 Repository

**文件:**
- Create: `backend/src/TtWork.Project/Core/Pays/IUserAvatarHistoryRepository.cs`

**Step 1: 创建 Repository 接口**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Core.Pays;

public interface IUserAvatarHistoryRepository : IRepository<UserAvatarHistory, long>
{
    /// <summary>
    /// 获取用户最近的头像历史记录
    /// </summary>
    Task<List<UserAvatarHistory>> GetRecentHistoryAsync(long userId, int count = 5);

    /// <summary>
    /// 获取用户最近的一条历史记录（用于回退）
    /// </summary>
    Task<UserAvatarHistory> GetLastHistoryAsync(long userId);

    /// <summary>
    /// 删除用户超过指定数量的旧历史记录
    /// </summary>
    Task DeleteOldHistoryAsync(long userId, int keepCount = 5);
}
```

**Step 2: 实现 Repository**

创建文件: `backend/src/TtWork.Project.EntityFrameworkCore/Core/Pays/UserAvatarHistoryRepository.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Core.Pays;

public class UserAvatarHistoryRepository : TtWorkProjectRepositoryBase<UserAvatarHistory, long>, IUserAvatarHistoryRepository
{
    public UserAvatarHistoryRepository(IDbContextProvider<TtWorkProjectDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<UserAvatarHistory>> GetRecentHistoryAsync(long userId, int count = 5)
    {
        return await GetAll()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ChangeTime)
            .Take(count)
            .ToListAsync();
    }

    public async Task<UserAvatarHistory> GetLastHistoryAsync(long userId)
    {
        return await GetAll()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ChangeTime)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteOldHistoryAsync(long userId, int keepCount = 5)
    {
        var historiesToDelete = await GetAll()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ChangeTime)
            .Skip(keepCount)
            .ToListAsync();

        if (historiesToDelete.Any())
        {
            await HardDeleteManyAsync(historiesToDelete);
        }
    }
}
```

**Step 3: 注册到依赖注入**

修改文件: `backend/src/TtWork.Project/TtWorkProjectModule.cs`

```csharp
// 在 ConfigureServices 方法中添加
services.AddTransient<IUserAvatarHistoryRepository, UserAvatarHistoryRepository>();
```

**Step 4: 提交变更**

```bash
git add backend/src/TtWork.Project/Core/Pays/
git add backend/src/TtWork.Project.EntityFrameworkCore/Core/Pays/
git add backend/src/TtWork.Project/TtWorkProjectModule.cs
git commit -m "feat: 添加用户头像历史 Repository"
```

---

## Task 3: 创建 DTO

**文件:**
- Create: `backend/src/TtWork.Project/Applications/Pays/Dto/UserAvatarHistoryDto.cs`
- Create: `backend/src/TtWork.Project/Applications/Pays/Dto/RollbackAvatarInput.cs`

**Step 1: 创建历史记录 DTO**

```csharp
using System;
using Abp.Application.Services.Dto;

namespace TtWork.Project.Applications.Pays.Dto;

/// <summary>
/// 用户头像历史记录 DTO
/// </summary>
public class UserAvatarHistoryDto : EntityDto<long>
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 修改前的头像URL
    /// </summary>
    public string PreviousHeadImgUrl { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ChangeTime { get; set; }

    /// <summary>
    /// 修改来源
    /// </summary>
    public string ChangeSource { get; set; }
}
```

**Step 2: 创建回退输入 DTO**

```csharp
using Abp.Application.Services.Dto;

namespace TtWork.Project.Applications.Pays.Dto;

/// <summary>
/// 回退头像输入参数
/// </summary>
public class RollbackAvatarInput
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }
}
```

**Step 3: 添加 AutoMapper 配置**

修改文件: `backend/src/TtWork.Project/Applications/Pays/Dto/UserAvatarHistoryProfile.cs`

```csharp
using AutoMapper;
using TtWork.Project.Domains.Pays;
using TtWork.Project.Applications.Pays.Dto;

namespace TtWork.Project.Applications.Pays.Dto;

public class UserAvatarHistoryProfile : Profile
{
    public UserAvatarHistoryProfile()
    {
        CreateMap<UserAvatarHistory, UserAvatarHistoryDto>();
        CreateMap<UserAvatarHistoryDto, UserAvatarHistory>();
    }
}
```

**Step 4: 提交变更**

```bash
git add backend/src/TtWork.Project/Applications/Pays/Dto/
git commit -m "feat: 添加头像历史相关 DTO"
```

---

## Task 4: 创建 AppService

**文件:**
- Create: `backend/src/TtWork.Project/Applications/Pays/UserAvatarHistoryAppService.cs`

**Step 1: 创建 ApplicationService 接口**

```csharp
using System.Threading.Tasks;
using Abp.Application.Services;
using TtWork.Project.Applications.Pays.Dto;

namespace TtWork.Project.Applications.Pays;

public interface IUserAvatarHistoryAppService : IApplicationService
{
    /// <summary>
    /// 回退用户头像到上一个状态
    /// </summary>
    Task<string> RollbackAvatar(RollbackAvatarInput input);

    /// <summary>
    /// 获取用户头像历史记录
    /// </summary>
    Task<UserAvatarHistoryDto> GetLastHistory(EntityDto<long> input);
}
```

**Step 2: 实现 ApplicationService**

```csharp
using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.UI;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Core.Pays;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Applications.Pays;

[AbpAuthorize]
public class UserAvatarHistoryAppService : TtWorkProjectAppServiceBase, IUserAvatarHistoryAppService
{
    private readonly IUserAvatarHistoryRepository _historyRepository;
    private readonly UserManager _userManager;
    private readonly ILogger<UserAvatarHistoryAppService> _logger;

    public UserAvatarHistoryAppService(
        IUserAvatarHistoryRepository historyRepository,
        UserManager userManager,
        ILogger<UserAvatarHistoryAppService> logger)
    {
        _historyRepository = historyRepository;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// 回退用户头像到上一个状态（仅管理员）
    /// </summary>
    [AbpAuthorize(AppPermissions.Administration)]
    public async Task<string> RollbackAvatar(RollbackAvatarInput input)
    {
        // 获取用户
        var user = await _userManager.GetUserByIdAsync(input.UserId);
        if (user == null)
        {
            throw new UserFriendlyException("用户不存在");
        }

        // 获取最近的历史记录
        var lastHistory = await _historyRepository.GetLastHistoryAsync(input.UserId);
        if (lastHistory == null)
        {
            throw new UserFriendlyException("没有可回退的头像记录");
        }

        var oldAvatar = user.HeadImgUrl;
        var newAvatar = lastHistory.PreviousHeadImgUrl;

        // 恢复头像
        user.HeadImgUrl = newAvatar;
        await _userManager.UpdateAsync(user);

        // 删除已使用的历史记录
        await _historyRepository.DeleteAsync(lastHistory);

        _logger.LogInformation("用户头像已回退: UserId={UserId}, 从={OldAvatar}, 到={NewAvatar}",
            input.UserId, oldAvatar, newAvatar);

        return newAvatar;
    }

    /// <summary>
    /// 获取用户最近的头像历史记录
    /// </summary>
    public async Task<UserAvatarHistoryDto> GetLastHistory(EntityDto<long> input)
    {
        var history = await _historyRepository.GetLastHistoryAsync(input.Id);
        if (history == null)
        {
            return null;
        }

        return ObjectMapper.Map<UserAvatarHistoryDto>(history);
    }
}
```

**Step 3: 添加 HTTP 路由**

修改文件: `backend/src/TtWork.Project.Web.Host/Startup/Startup.cs`

```csharp
// 在 Configure 方法中添加
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "UserAvatarHistory",
        pattern: "api/services/app/UserAvatarHistory/{action=Index}/{id?}");
});
```

**Step 4: 提交变更**

```bash
git add backend/src/TtWork.Project/Applications/Pays/UserAvatarHistoryAppService.cs
git add backend/src/TtWork.Project.Web.Host/Startup/Startup.cs
git commit -m "feat: 添加头像历史 AppService 和 API 接口"
```

---

## Task 5: 修改 UserAppService - 记录头像历史

**文件:**
- Modify: `backend/src/TtWork.Project/Applications/Core/Users/UserAppService.cs`

**Step 1: 注入依赖**

在构造函数中添加:

```csharp
private readonly IUserAvatarHistoryRepository _avatarHistoryRepository;

public UserAppService(
    // ... 现有参数
    IUserAvatarHistoryRepository avatarHistoryRepository
)
{
    // ... 现有赋值
    _avatarHistoryRepository = avatarHistoryRepository;
}
```

**Step 2: 创建记录历史的方法**

```csharp
/// <summary>
/// 记录头像修改历史
/// </summary>
private async Task RecordAvatarHistoryAsync(long userId, string oldAvatarUrl, string changeSource = "User")
{
    if (string.IsNullOrEmpty(oldAvatarUrl))
    {
        return; // 如果没有旧头像，不记录
    }

    var history = new UserAvatarHistory
    {
        UserId = userId,
        PreviousHeadImgUrl = oldAvatarUrl,
        ChangeTime = DateTime.Now,
        ChangeSource = changeSource
    };

    await _avatarHistoryRepository.InsertAsync(history);

    // 清理超过 5 条的旧记录
    await _avatarHistoryRepository.DeleteOldHistoryAsync(userId, keepCount: 5);
}
```

**Step 3: 在 UpdateAsync 方法中调用记录**

在 `UserAppService.UpdateAsync` 方法中，找到更新头像的代码位置（大约在行 340-420 之间）：

```csharp
public override async Task<UserDto> UpdateAsync(UserEditDto input)
{
    // ... 现有验证代码

    var user = await _userManager.GetUserByIdAsync(input.Id);

    // ===== 新增：记录头像历史 =====
    string oldAvatarUrl = user.HeadImgUrl;
    bool avatarChanged = !string.IsNullOrEmpty(input.HeadImgUrl) &&
                         input.HeadImgUrl != user.HeadImgUrl;
    // ============================

    // ... 现有昵称验证代码

    // URL格式验证（现有代码）
    if (!string.IsNullOrEmpty(input.HeadImgUrl) && input.HeadImgUrl != user.HeadImgUrl)
    {
        // ... 现有验证逻辑
    }

    // ===== 新增：在更新前记录历史 =====
    if (avatarChanged)
    {
        await RecordAvatarHistoryAsync(user.Id, oldAvatarUrl, "User");
    }
    // ============================

    // 🔐 头像安全检查（现有代码）
    if (!string.IsNullOrEmpty(input.HeadImgUrl) && input.HeadImgUrl != user.HeadImgUrl)
    {
        // ... 现有安全检查代码
    }

    // ... 后续代码
}
```

**Step 4: 提交变更**

```bash
git add backend/src/TtWork.Project/Applications/Core/Users/UserAppService.cs
git commit -m "feat: 用户修改头像时自动记录历史"
```

---

## Task 6: 编写单元测试

**文件:**
- Create: `backend/test/TtWork.SoMall.Tests/Pays/UserAvatarHistoryTests.cs`

**Step 1: 创建测试类**

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using TtWork.Project.Domains.Pays;
using TtWork.Project.Core.Pays;
using Xunit;

namespace TtWork.SoMall.Tests.Pays;

public class UserAvatarHistoryTests : SoMallTestBase
{
    private readonly IUserAvatarHistoryRepository _historyRepository;

    public UserAvatarHistoryTests()
    {
        _historyRepository = Resolve<IUserAvatarHistoryRepository>();
    }

    [Fact]
    public async Task Should_Create_Avatar_History()
    {
        // Arrange
        var history = new UserAvatarHistory
        {
            UserId = 1,
            PreviousHeadImgUrl = "https://old-avatar.jpg",
            ChangeSource = "User"
        };

        // Act
        await _historyRepository.InsertAsync(history);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Assert
        var savedHistory = await _historyRepository.GetAsync(history.Id);
        savedHistory.ShouldNotBeNull();
        savedHistory.PreviousHeadImgUrl.ShouldBe("https://old-avatar.jpg");
    }

    [Fact]
    public async Task Should_Get_Last_History()
    {
        // Arrange
        var userId = 1;
        await _historyRepository.InsertAsync(new UserAvatarHistory
        {
            UserId = userId,
            PreviousHeadImgUrl = "https://avatar1.jpg",
            ChangeSource = "User"
        });
        await _historyRepository.InsertAsync(new UserAvatarHistory
        {
            UserId = userId,
            PreviousHeadImgUrl = "https://avatar2.jpg",
            ChangeSource = "User"
        });
        await CurrentUnitOfWork.SaveChangesAsync();

        // Act
        var lastHistory = await _historyRepository.GetLastHistoryAsync(userId);

        // Assert
        lastHistory.ShouldNotBeNull();
        lastHistory.PreviousHeadImgUrl.ShouldBe("https://avatar2.jpg"); // 最新的
    }

    [Fact]
    public async Task Should_Delete_Old_History_When_Exceed_Limit()
    {
        // Arrange - 创建 7 条历史记录
        var userId = 2;
        for (int i = 1; i <= 7; i++)
        {
            await _historyRepository.InsertAsync(new UserAvatarHistory
            {
                UserId = userId,
                PreviousHeadImgUrl = $"https://avatar{i}.jpg",
                ChangeSource = "User"
            });
        }
        await CurrentUnitOfWork.SaveChangesAsync();

        // Act - 只保留 5 条
        await _historyRepository.DeleteOldHistoryAsync(userId, keepCount: 5);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Assert - 应该只剩 5 条
        var remainingHistories = await _historyRepository.GetRecentHistoryAsync(userId, count: 10);
        remainingHistories.Count.ShouldBe(5);
    }
}
```

**Step 2: 运行测试**

```bash
cd backend
dotnet test test/TtWork.SoMall.Tests/TtWork.SoMall.Tests.csproj --filter "FullyQualifiedName~UserAvatarHistoryTests"
```

**Step 3: 验证测试通过**

预期输出: 所有测试 PASS

**Step 4: 提交测试**

```bash
git add backend/test/TtWork.SoMall.Tests/Pays/UserAvatarHistoryTests.cs
git commit -m "test: 添加头像历史功能单元测试"
```

---

## Task 7: API 测试

**Step 1: 启动后端服务**

```bash
cd backend/src/TtWork.Project.Web.Host
dotnet run
```

**Step 2: 测试回退接口（需要管理员权限）**

```bash
# 登录获取 token
curl -X POST http://localhost:5000/api/TokenAuth/Authenticate \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmailAddress":"admin","password":"你的密码"}'

# 使用 token 测试回退接口
curl -X POST http://localhost:5000/api/services/app/UserAvatarHistory/RollbackAvatar \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer 你的token" \
  -d '{"userId": 123}'
```

**Step 3: 测试获取历史记录接口**

```bash
curl -X GET "http://localhost:5000/api/services/app/UserAvatarHistory/GetLastHistory?id=123" \
  -H "Authorization: Bearer 你的token"
```

**Step 4: 验证数据库记录**

```sql
-- 查看用户头像历史
SELECT * FROM Pays_UserAvatarHistory WHERE UserId = 123 ORDER BY ChangeTime DESC LIMIT 5;

-- 查看用户当前头像
SELECT HeadImgUrl FROM AbpUsers WHERE Id = 123;
```

**Step 5: 提交 API 测试脚本**

创建文件: `backend/test_user_avatar_rollback.sh`

```bash
#!/bin/bash
# 用户头像回退 API 测试脚本

BASE_URL="http://localhost:5000"
TOKEN="your-admin-token"

echo "测试 1: 回退用户头像"
curl -X POST "$BASE_URL/api/services/app/UserAvatarHistory/RollbackAvatar" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"userId": 1}' \
  | jq .

echo -e "\n测试 2: 获取头像历史"
curl -X GET "$BASE_URL/api/services/app/UserAvatarHistory/GetLastHistory?id=1" \
  -H "Authorization: Bearer $TOKEN" \
  | jq .
```

```bash
git add backend/test_user_avatar_rollback.sh
git commit -m "test: 添加头像回退 API 测试脚本"
```

---

## Task 8: 文档编写

**文件:**
- Create: `backend/docs/user-avatar-rollback-feature.md`

**Step 1: 创建功能文档**

```markdown
# 用户头像历史记录与回退功能

## 功能说明

当用户修改头像时，系统自动记录旧头像 URL，每个用户最多保留 5 条历史记录。

管理员可以通过 API 回退用户头像到上一个状态。

## API 接口

### 1. 回退头像

**接口:** `POST /api/services/app/UserAvatarHistory/RollbackAvatar`

**权限:** 需要管理员权限

**请求参数:**
\`\`\`json
{
  "userId": 123
}
\`\`\`

**返回:** 回退后的头像 URL

### 2. 获取头像历史

**接口:** `GET /api/services/app/UserAvatarHistory/GetLastHistory?id={userId}`

**权限:** 需要管理员权限

**返回:**
\`\`\`json
{
  "result": {
    "id": 1,
    "userId": 123,
    "previousHeadImgUrl": "https://old-avatar.jpg",
    "changeTime": "2026-02-25T10:30:00",
    "changeSource": "User"
  }
}
\`\`\`

## 数据表

\`\`\`sql
CREATE TABLE `Pays_UserAvatarHistory` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `TenantId` int NOT NULL,
  `UserId` bigint NOT NULL,
  `PreviousHeadImgUrl` varchar(512) DEFAULT NULL,
  `ChangeTime` datetime NOT NULL,
  `ChangeSource` varchar(32) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
\`\`\`

## 使用场景

1. **用户上传错误头像** - 用户误上传了错误图片，管理员可以回退
2. **系统修正后恢复** - 系统自动修正 wxfile:// 临时路径后，如果修正错误可以回退
3. **批量操作失误** - 管理员批量修改头像后发现问题，可以逐个回退
\`\`\`

**Step 2: 提交文档**

```bash
git add backend/docs/user-avatar-rollback-feature.md
git commit -m "docs: 添加头像回退功能文档"
```

---

## Task 9: 数据库清理脚本（可选）

**文件:**
- Create: `backend/scripts/clean_old_avatar_history.sql`

**Step 1: 创建手动清理脚本**

\`\`\`sql
-- 手动清理超过 5 条的头像历史记录（保留最新的 5 条）
DELETE h1 FROM Pays_UserAvatarHistory h1
INNER JOIN (
    SELECT Id, ROW_NUMBER() OVER (
        PARTITION BY UserId ORDER BY ChangeTime DESC
    ) AS rn
    FROM Pays_UserAvatarHistory
) h2 ON h1.Id = h2.Id
WHERE h2.rn > 5;

-- 查看清理结果
SELECT UserId, COUNT(*) as HistoryCount
FROM Pays_UserAvatarHistory
GROUP BY UserId
ORDER BY HistoryCount DESC;
\`\`\`

**Step 2: 提交脚本**

```bash
git add backend/scripts/clean_old_avatar_history.sql
git commit -m "scripts: 添加头像历史清理脚本"
```

---

## 完成检查清单

- [ ] 数据库迁移已应用
- [ ] 实体类创建完成
- [ ] Repository 实现完成
- [ ] DTO 创建完成
- [ ] AppService 实现完成
- [ ] UserAppService 集成完成
- [ ] 单元测试通过
- [ ] API 测试通过
- [ ] 文档编写完成

---

## 执行顺序建议

1. **Task 1-2**: 数据层（实体 + Repository）→ 测试
2. **Task 3-4**: 服务层（DTO + AppService）→ 测试
3. **Task 5**: 集成到 UserAppService
4. **Task 6-7**: 测试验证
5. **Task 8-9**: 文档和脚本

**总预计时间:** 2-3 小时

---

## 风险与注意事项

1. **回退后 CDN 图片可能已被删除** - 回退时需要验证图片是否仍可访问
2. **并发修改** - 用户快速连续修改头像时，历史记录可能不按预期顺序
3. **权限控制** - 确保只有管理员可以调用回退接口
4. **数据迁移** - 如果有现有用户，首次修改头像时 `oldAvatarUrl` 可能为空
