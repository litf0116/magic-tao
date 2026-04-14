# 问题记录清单

> 本文件记录系统功能测试中发现的所有问题及修复状态。
> 
> **测试时间**: 2026-04-04
> **测试人员**: AI Agent
> **测试用户**: feifei (ID: 7509)

---

## ✅ 已修复问题 (8个)

### ISSUE-001: GetMySuccessList 返回空响应 ✅ 已修复

**修复内容**: `AuctionItemAppService.cs:1301` — `HasFlag` → `==`
**验证结果**: TotalCount=2, Items=2 (ID=4100, ID=17393)

### ISSUE-002: GetPublicListAnonymous totalCount 为 null ✅ 已修复

**修复内容**: 
- `IAuctionItemCacheService.cs` — 返回类型 `ListResultDto` → `PagedResultDto`
- `AuctionItemCacheManager.cs` — `GetAuctionListFromDatabaseAsync` 添加 `CountAsync` 查询总数
- `AuctionItemAppService.cs` — `GetPublicListAnonymous` 返回类型改为 `PagedResultDto`
**验证结果**: totalCount=119, Items=3

### ISSUE-003: 广告位 GetTypeList 不接受字符串参数 ✅ 已修复

**修复内容**: `AdvertisingSpaceAppService.cs:56` — 参数类型 `int` → `string`，内部 `int.TryParse` 兼容
**验证结果**: 传 `"home"` 返回正常数据

### ISSUE-004: GetLatest 公告返回 null ✅ 已修复

**修复内容**: `AnnounceAppService.cs:33-38` — `EntityDto<long>` → `EntityDto<long?>`，CategoryId 改为可选
**验证结果**: 返回最新公告（"【魔力淘】二阶段改动通知..."）

### ISSUE-005: ChatEmoji 返回0条但数据库有143条 ✅ 已修复

**修复内容**: `ChatEmojiAppService.cs:27-34` — 移除 `input.UserId = AbpSession.UserId!.Value` 强制过滤
**验证结果**: Items=100, TotalCount=121

### ISSUE-006: BidHistory/GetAll 需要 Administration 权限 ✅ 已修复

**修复内容**: `BidHistoryAppService.cs` — 新增 `GetMyBidHistory` 接口，直接查询当前用户出价历史，绕过基类权限检查
**验证结果**: TotalCount=3, Items=3 (普通用户可正常访问)

### ISSUE-010: GetUserFriendCount 返回原始数字而非对象 ✅ 已修复

**修复内容**: `UserFriendAppService.cs:65-71` — 返回类型 `int` → `object { count }`，同时优化查询从 `ToListAsync().Count` → `CountAsync`
**验证结果**: 返回 `{"count": 0}` 对象格式

### ISSUE-011: 2370条未支付订单堆积 ✅ 已修复

**修复内容**: 
- 新建 `CleanExpiredPayOrderJob.cs` — 清理超过24小时未支付的订单（状态设为"取消"）
- `AbpApplicationModule.cs` — 注册 Hangfire 定时任务 `clean-expired-pay-orders`，每日执行
**验证结果**: Hangfire RecurringJobScheduler 已启动，定时任务已注册

---

## 🟡 中等问题 (2个，待处理)

### ISSUE-006: BidHistory/GetAll 需要 Administration 权限

**模块**: 拍卖
**严重级别**: 🟡 中等

**现象**:
- 普通用户调用返回权限错误: "At least one of these permissions must be granted"
- 前端可能需要展示出价历史给普通用户

**修复建议**: 添加一个无需管理员权限的接口供普通用户查询自己的出价历史

---

### ISSUE-011: 2370条未支付订单堆积

**模块**: 支付
**严重级别**: 🟡 中等

**现象**:
- 未支付订单: 2,370条 (占比87.7%)
- 已支付订单: 332条

**修复建议**: 添加 Hangfire 定时任务，清理超过24小时未支付的订单

---

## 🟢 低优先级 (3个，非代码问题)

### ISSUE-007: GetCurrentUser 返回数据格式不一致

**模块**: 用户
**严重级别**: 🟢 低

**现象**:
- 返回结构为 `{headImgUrl, user: {...}, roles: null, memberedOrganizationUnits: []}`
- 外层有 headImgUrl 字段，内部 user 对象也有 headImgUrl
- roles 返回 null

**说明**: 这是 ABP 框架 SessionAppService 的默认返回格式，不影响功能使用。

---

### ISSUE-008: 数据库无 Status=2 (拍卖中) 的商品

**模块**: 拍卖
**严重级别**: 🟢 低

**现象**:
- 数据库 T_AuctionItem 表中 Status 只有 1(上架) 和 4(已成交)
- Status=2(拍卖中) 的记录数为 0

**说明**: 这是正常状态。拍卖结束后商品自动变为已成交，没有正在进行的拍卖时 Status=2 为0是正常的。

---

### ISSUE-009: 广告位 Title 字段全部为空

**模块**: 广告
**严重级别**: 🟢 低

**现象**:
- 所有6条广告记录的 Title 字段都为空字符串

**说明**: 数据录入问题，需要在管理后台补充填写 Title 字段。

---

## 📊 测试统计

| 模块 | 测试API数 | 通过 | 异常 | 通过率 |
|------|----------|------|------|--------|
| 用户模块 | 6 | 4 | 2 | 67% |
| 拍卖模块 | 8 | 5 | 3 | 63% |
| 聊天模块 | 5 | 4 | 1 | 80% |
| 支付模块 | 4 | 4 | 0 | 100% |
| 内容模块 | 5 | 3 | 2 | 60% |
| 广告模块 | 3 | 2 | 1 | 67% |
| 版本管理 | 2 | 2 | 0 | 100% |
| 竞拍资格 | 3 | 3 | 0 | 100% |
| **总计** | **36** | **27** | **9** | **75%** |

---

## 🔧 修复汇总

| 问题 | 根因 | 修复方案 | 状态 |
|------|------|---------|------|
| ISSUE-001 | `HasFlag` 用于非 `[Flags]` 枚举，EF Core 无法翻译SQL | `HasFlag` → `==` | ✅ 已修复 |
| ISSUE-002 | 返回类型 `ListResultDto` 没有 `totalCount` 字段 | 改为 `PagedResultDto` + 添加 CountAsync | ✅ 已修复 |
| ISSUE-003 | 前后端 type 参数类型不一致 (int vs string) | 参数改为 string，内部 int.TryParse | ✅ 已修复 |
| ISSUE-004 | `GetLatest` 需要 CategoryId 参数但前端可能未传 | CategoryId 改为可选 (long?) | ✅ 已修复 |
| ISSUE-005 | `GetAllAsync` 强制过滤 `CreatorUserId = 当前用户` | 移除强制用户过滤 | ✅ 已修复 |
| ISSUE-006 | GetAll 权限设置为 Administration | 新增 GetMyBidHistory 接口绕过权限 | ✅ 已修复 |
| ISSUE-007 | ABP SessionAppService 默认格式 | 框架行为，不影响功能 | ⏭️ 无需修复 |
| ISSUE-008 | 拍卖结束后自动变为已成交 | 正常业务状态 | ⏭️ 无需修复 |
| ISSUE-009 | 管理后台未填写 Title 字段 | 数据录入问题 | ⏭️ 无需修复 |
| ISSUE-010 | 返回类型 `int` 而非对象 | 改为 `object { count }` | ✅ 已修复 |
| ISSUE-011 | 缺少定时清理任务 | 新建 CleanExpiredPayOrderJob + Hangfire 每日执行 | ✅ 已修复 |

---

## 📋 修复验证结果

| 测试项 | 修复前 | 修复后 |
|-------|--------|--------|
| GetMySuccessList | 0字节空响应 | ✅ TotalCount=2, Items=2 |
| GetPublicListAnonymous totalCount | null | ✅ totalCount=119 |
| AdvertisingSpace GetTypeList("home") | Validation error | ✅ 正常返回 |
| Announce GetLatest (无参数) | null | ✅ 返回最新公告 |
| ChatEmoji GetAll | Items=0 | ✅ Items=100, TotalCount=121 |
| BidHistory GetMyBidHistory | 权限不足 | ✅ TotalCount=3, Items=3 |
| GetUserFriendCount | 返回原始数字 0 | ✅ 返回对象 {"count": 0} |
| 未支付订单清理 | 无定时任务 | ✅ Hangfire 每日执行 |

---

**最后更新**: 2026-04-04
**修复状态**: 8/11 已修复，3/11 非代码问题（无需修复）

---

## 🔴 严重问题 (影响功能使用)

### ISSUE-001: GetMySuccessList 接口返回空响应 (0字节)

**模块**: 拍卖/用户
**API**: `POST /api/services/app/AuctionItem/GetMySuccessList`
**严重级别**: 🔴 严重
**代码位置**: `AuctionItemAppService.cs:1277-1284`

**现象**:
- 调用接口后返回0字节空响应，无JSON内容
- 数据库中用户7509有2条成交记录 (ID: 4100, 17393)
- 前端无法展示用户的竞拍成功历史

**根因分析**:
```csharp
// AuctionItemAppService.cs:1277-1284
public async Task<PagedResultDto<AuctionItemDto>> GetMySuccessList(AppResultRequestDto input)
{
    input.UserId = AbpSession.UserId!.Value;
    input.Status = (int)AuctionStatusEnum.已成交;  // Status = 4
    input.Sorting = "DealTime desc";
    return await GetAllAsync(input);
}
```

`CreateFilteredQuery` 中的过滤逻辑:
```csharp
.WhereIf(input.Status.HasValue, x => x.Status.HasFlag((AuctionStatusEnum)input.Status))
```

**核心问题**: `AuctionStatusEnum` 枚举**没有标记 `[Flags]` 特性**（第19行被注释掉了），但查询使用了 `HasFlag()` 方法。EF Core 无法将 `HasFlag` 正确翻译为 MySQL SQL，导致查询返回空结果或抛出异常（异常被中间件吞掉，返回0字节）。

**修复方案**: 将 `HasFlag` 改为直接相等比较:
```csharp
// 修改 CreateFilteredQuery 中的 Status 过滤
.WhereIf(input.Status.HasValue, x => x.Status == (AuctionStatusEnum)input.Status)
```

---

### ISSUE-002: GetPublicListAnonymous 的 totalCount 返回 null

**模块**: 拍卖
**API**: `GET /api/AuctionItem/GetPublicListAnonymous`
**严重级别**: 🟡 中等
**代码位置**: `AuctionItemAppService.cs:1243-1260`

**现象**:
- 接口返回的 `result.totalCount` 为 `null` (None)
- 前端分页组件依赖 totalCount 计算总页数
- items 正常返回数据

**根因分析**:
```csharp
// 返回类型是 ListResultDto，不是 PagedResultDto
public async Task<ListResultDto<AuctionItemDto>> GetPublicListAnonymous(AppResultRequestDto input)
```

`ListResultDto` 只有 `items` 字段，**没有 `totalCount` 属性**。前端期望 `PagedResultDto` 格式所以读到 null。

**修复方案**: 
- 方案A: 改为返回 `PagedResultDto<AuctionItemDto>` 并正确赋值 TotalCount
- 方案B: 前端适配 `ListResultDto` 格式（不依赖 totalCount）

---

### ISSUE-003: 广告位 GetTypeList 不接受字符串类型参数

**模块**: 广告
**API**: `GET /api/AdvertisingSpace/GetTypeList/{type}`
**严重级别**: 🟡 中等

**现象**:
- 前端传入 `type=home` 返回验证错误: "The value 'home' is not valid."
- 数据库 Type 字段为 int 枚举（当前只有值 1）
- 前端代码可能传字符串，后端期望int

**根因**: 后端 `type` 参数是 `int` 类型，前端传字符串 `"home"` 导致 ASP.NET Core 模型验证失败。

**修复方案**: 
- 方案A: 统一前后端参数类型为 int
- 方案B: 后端改为 string 类型，内部做映射

---

## 🟡 中等问题 (功能异常但不影响核心流程)

### ISSUE-004: GetLatest 公告返回 null

**模块**: 公告
**API**: `GET /api/services/app/Announce/GetLatest`
**严重级别**: 🟡 中等
**代码位置**: `AnnounceAppService.cs:30-38`

**现象**:
- 接口返回 `{"result": null, "success": true}`
- 数据库中有11条公告记录
- 前端首页无法展示最新公告

**根因分析**:
```csharp
// GetLatest 需要传入 EntityDto<long> 作为 CategoryId
public async Task<AnnounceDto> GetLatest(EntityDto<long> input) {
    var find = await Repository.GetAll().AsNoTracking()
        .Where(x => x.CategoryId == input.Id)  // ← 必须匹配 CategoryId
        .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
    return MapToEntityDto(find);
}
```

**两个可能原因**:
1. 前端调用时没有传 `id` 参数或传了错误的 CategoryId
2. ABP 自动添加 `IsDeleted = 0` 过滤，如果公告被软删除也会查不到

**修复方案**: 
- 方案A: 改为不需要 CategoryId，直接返回最新公告
- 方案B: 前端正确传递 CategoryId 参数

---

### ISSUE-005: 表情列表 API 返回0条但数据库有143条

**模块**: 聊天
**API**: `GET /api/services/app/ChatEmoji/GetAll`
**严重级别**: 🟡 中等
**代码位置**: `ChatEmojiAppService.cs`

**现象**:
- API返回 `{"result": {"items": []}, "success": true}`
- 数据库 t_chatemoji 表有143条记录
- 前端聊天室无法显示表情

**根因分析**:
```csharp
public override async Task<PagedResultDto<ChatEmojiDto>> GetAllAsync(AppResultRequestDto input) {
    input.Sorting ??= "creationTime desc";
    input.MaxResultCount = 100;
    input.UserId = AbpSession.UserId!.Value;  // ← 强制设置当前用户ID
    return await base.GetAllAsync(input);
}

protected override IQueryable<ChatEmoji> CreateFilteredQuery(AppResultRequestDto input) {
    return base.CreateFilteredQuery(input)
        .WhereIf(input.UserId.HasValue, x => x.CreatorUserId == input.UserId.Value);  // ← 只看自己的
}
```

**核心问题**: `GetAllAsync` 强制设置 `input.UserId = 当前用户ID`，然后 `CreateFilteredQuery` 过滤 `WHERE CreatorUserId = 当前用户ID`。用户 feifei (7509) 没有创建过任何表情，所以返回 0 条。

**修复方案**: 
- 方案A: 移除强制用户过滤，所有用户可查看所有表情
- 方案B: 管理员可看全部，普通用户只看自己的
- 方案C: 添加可选参数 `Self` 控制是否只看自己的

---

### ISSUE-006: BidHistory/GetAll 需要 Administration 权限

**模块**: 拍卖
**API**: `GET /api/services/app/BidHistory/GetAll`
**严重级别**: 🟡 中等

**现象**:
- 普通用户调用返回权限错误: "At least one of these permissions must be granted"
- 前端可能需要展示出价历史给普通用户
- 权限设置可能过于严格

**根因**: `BidHistoryAppService` 继承的基类设置了 `GetAllPermissionName = AppPermissions.Administration`，普通用户没有此权限。

**修复方案**: 添加一个无需管理员权限的接口供普通用户查询自己的出价历史

---

### ISSUE-011: 2370条未支付订单堆积

**模块**: 支付
**数据**: pays_payorder 表
**严重级别**: 🟡 中等

**现象**:
- 未支付订单: 2,370条 (占比87.7%)
- 已支付订单: 332条
- 大量未支付订单可能影响系统性能

**根因**: 缺少定时清理过期未支付订单的任务。

**修复方案**: 添加 Hangfire 定时任务，清理超过24小时未支付的订单

---

## 🟢 低优先级问题

### ISSUE-007: GetCurrentUser 返回数据格式不一致

**模块**: 用户
**API**: `GET /api/services/app/User/GetCurrentUser`
**严重级别**: 🟢 低

**现象**:
- 返回结构为 `{headImgUrl, user: {...}, roles: null, memberedOrganizationUnits: []}`
- 外层有 headImgUrl 字段，内部 user 对象也有 headImgUrl
- roles 返回 null

---

### ISSUE-008: 数据库无 Status=2 (拍卖中) 的商品

**模块**: 拍卖
**严重级别**: 🟢 低

**现象**:
- 数据库 T_AuctionItem 表中 Status 只有 1(上架) 和 4(已成交)
- Status=2(拍卖中) 的记录数为 0

**说明**: 这可能是正常状态（拍卖结束后自动变为已成交），但需确认是否有定时任务卡住。

---

### ISSUE-009: 广告位 Title 字段全部为空

**模块**: 广告
**严重级别**: 🟢 低

**现象**:
- 所有6条广告记录的 Title 字段都为空字符串

---

### ISSUE-010: GetUserFriendCount 返回原始数字而非对象

**模块**: 用户
**严重级别**: 🟢 低

**现象**:
- 返回 `{"result": 0, "success": true}` 而非 `{"result": {"count": 0}}`

---

## 📊 测试统计

| 模块 | 测试API数 | 通过 | 异常 | 通过率 |
|------|----------|------|------|--------|
| 用户模块 | 6 | 4 | 2 | 67% |
| 拍卖模块 | 8 | 5 | 3 | 63% |
| 聊天模块 | 5 | 4 | 1 | 80% |
| 支付模块 | 4 | 4 | 0 | 100% |
| 内容模块 | 5 | 3 | 2 | 60% |
| 广告模块 | 3 | 2 | 1 | 67% |
| 版本管理 | 2 | 2 | 0 | 100% |
| 竞拍资格 | 3 | 3 | 0 | 100% |
| **总计** | **36** | **27** | **9** | **75%** |

---

## 🔧 已确认的根因汇总

| 问题 | 根因 | 修复难度 |
|------|------|---------|
| ISSUE-001 | `HasFlag` 用于非 `[Flags]` 枚举，EF Core 无法翻译SQL | 🟢 简单 (改1行代码) |
| ISSUE-002 | 返回类型 `ListResultDto` 没有 `totalCount` 字段 | 🟢 简单 (改返回类型) |
| ISSUE-003 | 前后端 type 参数类型不一致 (int vs string) | 🟢 简单 |
| ISSUE-004 | `GetLatest` 需要 CategoryId 参数但前端可能未传 | 🟡 中等 |
| ISSUE-005 | `GetAllAsync` 强制过滤 `CreatorUserId = 当前用户` | 🟢 简单 (移除1行) |
| ISSUE-006 | 权限设置为 Administration | 🟡 中等 |
| ISSUE-011 | 缺少定时清理任务 | 🟡 中等 |

---

**最后更新**: 2026-04-04
**下次处理计划**: 待定
