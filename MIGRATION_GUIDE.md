# Master 分支功能迁移指南

## 概述

本文档指导如何将 Master 分支的功能安全地迁移到当前分支 `20260225_progressive-migration`，同时避免引入缓存架构问题。

## 迁移策略

### ✅ 直接迁移（低风险）
- 前端文件修改
- 文档文件
- 配置文件

### ⚠️ 手动合并（需要解决冲突）
- 后端业务逻辑修改
- 需要适配当前分支的 Redis 缓存架构

### ❌ 暂不处理（高风险）
- 缓存架构重构（Redis → Memory）
- 依赖新缓存架构的性能优化

## 阶段 1: P0 安全修复

### 1.1 修复小程序用户头像上传数据完整性漏洞 (8426847)

#### 文件清单
- ✅ `molitao_uniapp/src/pages/user/info.vue` - 前端验证逻辑
- ⚠️ `backend/src/TtWork.Project/Applications/Core/Users/UserAppService.cs` - 后端安全检查（需手动合并）
- ✅ `数据库清理操作指南.md` - 新增文档
- ✅ `用户信息复制操作指南.md` - 新增文档

#### 前端修改（已完成）
```bash
git checkout master -- molitao_uniapp/src/pages/user/info.vue
```

#### 后端修改（手动合并）

**添加的安全检查代码**：

```csharp
// 在 UserAppService.cs 的 UpdateAsync 方法中
// 检查点：在用户名重复检查之后，更新用户信息之前

// 🔒 URL格式验证 - 阻止本地临时文件路径
if (!string.IsNullOrEmpty(input.HeadImgUrl) && 
    input.HeadImgUrl != user.HeadImgUrl)
{
    // 检查是否为本地临时文件路径
    if (input.HeadImgUrl.StartsWith("wxfile://", StringComparison.OrdinalIgnoreCase) ||
        input.HeadImgUrl.StartsWith("http://tmp_", StringComparison.OrdinalIgnoreCase) ||
        input.HeadImgUrl.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
    {
        _logger.LogWarning("检测到非法头像URL: UserId={UserId}, HeadImgUrl={HeadImgUrl}", 
            user.Id, input.HeadImgUrl);
        throw new UserFriendlyException("头像地址格式错误，请重新上传头像");
    }
    
    // 检查是否为CDN地址（允许的格式）
    if (!input.HeadImgUrl.StartsWith("https://cdn.molitao.top", StringComparison.OrdinalIgnoreCase))
    {
        _logger.LogWarning("头像URL不是CDN地址: UserId={UserId}, HeadImgUrl={HeadImgUrl}", 
            user.Id, input.HeadImgUrl);
        throw new UserFriendlyException("头像地址不正确，请使用CDN地址");
    }
}
```

**依赖的新方法**：

```csharp
/// <summary>
/// 下载图片用于安全检查
/// </summary>
private async Task<byte[]> DownloadImageAsync(string url)
{
    using var httpClient = new HttpClient();
    httpClient.Timeout = TimeSpan.FromSeconds(10);
    
    try
    {
        return await httpClient.GetByteArrayAsync(url);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "下载图片失败: {Url}", url);
        return null;
    }
}

/// <summary>
/// 获取微信配置
/// </summary>
private (string appId, string appSecret) GetWeixinConfig()
{
    // 从配置或数据库读取
    var appId = "your_app_id";  // TODO: 从配置读取
    var appSecret = "your_app_secret";  // TODO: 从配置读取
    return (appId, appSecret);
}
```

**注意**：
1. 当前分支可能还没有内容安全检测功能（`e9507ac`），需要先执行那个提交
2. 或者暂时只保留URL格式验证，跳过图片审核部分

#### 文档文件（待添加）
```bash
# 需要从 Master 分支手动复制这两个文件
# 数据库清理操作指南.md
# 用户信息复制操作指南.md
```

---

### 1.2 合并微信内容安全检测功能 (e9507ac)

#### 依赖关系
**前置条件**: 需要先执行此提交，然后再完成 8426847 的图片审核部分

#### 文件清单
- `backend/Modules/Tt.HttpClient.Weixin/IWeixinApi.cs` - API 接口定义
- `backend/Modules/Tt.HttpClient.Weixin/WeixinApi.cs` - 微信API实现
- `backend/src/TtWork.Project/Applications/ContentSecurityAppService.cs` - 新增服务
- `backend/src/TtWork.Project/Applications/Core/Users/UserAppService.cs` - 头像审核集成
- `molitao_uniapp/src/pages/user/info.vue` - 前端调用
- `molitao_uniapp/src/utils/api.ts` - API 定义

#### 实施步骤

**步骤 1**: 添加 ContentSecurityAppService
```bash
# 从 Master 复制 ContentSecurityAppService.cs
git show master:backend/src/TtWork.Project/Applications/ContentSecurityAppService.cs > backend/src/TtWork.Project/Applications/ContentSecurityAppService.cs
```

**步骤 2**: 更新 WeixinApi
```bash
# 查看 Master 的修改，手动合并
git diff master 20260225_progressive-migration -- backend/Modules/Tt.HttpClient.Weixin/WeixinApi.cs
```

**步骤 3**: 在 UserAppService 中集成审核逻辑
- 在头像更新前调用图片审核
- 审核失败时抛出异常

---

### 1.3 修复头像上传CDN域名问题 (80a9b58)

#### 简单配置修改
- 修改CDN域名为 `http://image.molitao.top`
- 影响文件：配置文件和硬编码的URL

---

## 阶段 2: P1 核心功能修复

### 2.1 修复拍卖成交后聊天窗口丢失问题 (5ceb8b2)

**注意**: 此提交与当前分支的 `1a357f8` 有功能重叠

#### 策略
- 对比两个提交的修改
- 保留双方的改进
- 合并成一个完整的修复

#### 关键修改点
1. 支持同时处理 `AuctionEnd` 和 `AuctionDeal` 消息类型
2. 优先使用实际拍卖成交时间 `dealTime`
3. 为拍卖师和中拍用户同时创建聊天会话

---

### 2.2 修复帖子编辑功能400错误 (0721bd0)
### 2.3 修复小程序运行时错误 (919d52f)

这两个修复可以直接 cherry-pick：
```bash
git cherry-pick 0721bd0
git cherry-pick 919d52f
```

---

## 阶段 4: P3 功能增强

### 4.1 添加用户头像历史记录与回退功能 (8927a75)

#### 数据库迁移
- 创建 `UserAvatarHistory` 实体
- 添加数据库迁移文件

#### API 服务
- 新增 `UserAvatarHistoryAppService`
- 在 `UserAppService.UpdateAsync` 中集成历史记录

---

### 4.2 优化拍卖系统日志记录和异常处理 (34b7b20)

#### 改动
- 添加 Serilog.Sinks.File 支持
- 配置双文件日志
- 修复异常处理问题

---

## 迁移执行检查清单

### 提交前
- [ ] 确认工作区干净
- [ ] 创建备份分支
- [ ] 拉取最新 Master 分支变更

### 提交后
- [ ] 编译通过（Backend + Frontend）
- [ ] 运行单元测试
- [ ] 手动测试修改的功能
- [ ] 检查缓存一致性

### 每个迁移任务
1. [ ] 分析提交改动
2. [ ] 评估冲突风险
3. [ ] 执行迁移（cherry-pick 或手动合并）
4. [ ] 解决冲突（如有）
5. [ ] 验证功能正常
6. [ ] 提交并编写清晰的提交信息

---

## 风险管理

### 高风险操作
- ⚠️ 修改缓存相关代码
- ⚠️ 修改数据库架构
- ⚠️ 大规模重构

### 低风险操作
- ✅ 前端UI调整
- ✅ 文档更新
- ✅ 配置修改
- ✅ 简单bug修复

---

## 回滚策略

如果迁移后发现严重问题：
```bash
# 回滚最近的迁移
git reset --hard HEAD~N

# 或者回到备份分支
git checkout backup-branch
```

---

## 下一步

1. **立即执行**: P0 安全修复 - 先迁移微信内容安全检测 (e9507ac)
2. **然后完成**: 头像上传安全修复 (8426847) 的图片审核部分
3. **逐步完成**: P1 和 P3 的其他修复

---

## 联系信息

如有问题，参考：
- Master 分支提交历史
- 当前分支 (20260225_progressive-migration) - 稳定基线
- 问题分析文档（本次会话生成）
