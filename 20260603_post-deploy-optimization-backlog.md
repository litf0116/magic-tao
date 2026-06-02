# Post-Deploy Optimization Backlog (2026-06-03)

> 来源: 2026-06-03 backend commit 风险分析会话
> 触发: 镜像 `litengfei0302/molitao-backend:latest` (sha256:cb301849) 自 2026-05-25 01:37 CST 以来 8 个 commit
> 状态: 🔲 待修复
> 关联: PR1 (cfcc05e Apple 登录 + 微信降级) 已部署,后续 PR2 (UGC 1.2) 正在合并

---

## 📊 统计

| 优先级 | 数量 | 类别 |
|--------|------|------|
| P1 高 | 4 | 安全/合规 |
| P2 中 | 6 | 性能/一致性 |
| P3 低 | 4 | 代码质量 |

---

## 🔴 P1 - 高优先级(部署前/下个迭代必修)

### P1-1 帖子标题(title)未做内容审核
- **文件**: `backend/src/TtWork.Project/PostBar/PostService.cs:257, 295`
- **问题**: `CheckSensitiveWordsAsync(input.content)` 只检查 `content` 字段,`title` 字段未校验
- **影响**: 用户可发"合规正文 + 违规标题"绕过审核
- **修复**: 在 Add/Edit 入口同时检查 `input.title` 和 `input.content`
- **关联 commit**: ecd7379

### P1-2 账号注销后 RefreshToken 7 天内仍有效
- **文件**: `backend/src/TtWork.Project/Applications/Core/Authorization/Accounts/AccountAppService.cs:294-330`
- **问题**: `DeleteAccount` 设 `IsActive=false` 但**未撤销/清除 RefreshToken 缓存**
- **影响**: 注销后 7 天内,旧 access token 仍能访问大部分 API(只有登录态会被 IsActive 拦截)
- **修复**: 注销时主动清空用户的 RefreshToken 缓存,缩短攻击窗口
- **关联 commit**: 074f182
- **背景**: 项目约定 PhoneResetPassword 不旋转 SecurityStamp(避免频繁登录),但 DeleteAccount 场景应不同

### P1-3 拉黑只检查"接收方拉黑发送方",语义不完整
- **文件**: `backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs:470-482`
- **代码**:
  ```csharp
  if (b.BlockerId == toUserId.Value && b.BlockedUserId == fromUserId)
  ```
- **未覆盖场景**:
  - 群聊中 B 仍可 @A
  - B 的拍卖消息投递未阻断
  - B 的出价(如果能查到 A 的拍品)未阻断
- **修复**: 业务确认拉黑范围(仅私聊 vs 全局),若全局需在所有消息路径加检查
- **关联 commit**: ecd7379

### P1-4 BlockedUser 软删除列冗余 vs 实体未实现 ISoftDelete
- **文件**:
  - 实体: `backend/src/TtWork.Project/Domains/BlockedUser.cs`
  - 迁移: `backend/src/TtWork.Project.EntityFrameworkCore/Migrations/20250601000000_AddBlockedUser.cs`
- **问题**: 迁移创建了 `IsDeleted`/`DeleterUserId`/`DeletionTime` 列,但实体**未实现 `ISoftDelete` 接口**
- **影响**: ABP 不会自动应用软删除过滤,删除记录**直接物理删除**
- **设计二选一**:
  - 方案 A: 实体加 `ISoftDelete` 接口,使用软删除(保留审计)
  - 方案 B: 迁移去掉软删除列,使用硬删除 + 异步归档
- **关联 commit**: ecd7379

---

## 🟡 P2 - 中优先级(下个迭代改进)

### P2-1 敏感词检查两种实现不一致
- **PostService 实现**: `backend/src/TtWork.Project/PostBar/PostService.cs:44-69`
  - `CheckSensitiveWordsAsync` — 用 `ReadOnlySpan<char>` + 双层 for 循环
  - 返回**所有命中词**的 List
- **MessageSendingService 实现**: `backend/src/TtWork.Project/Services/Messaging/MessageSendingService.cs:543-569`
  - `CheckMsgText` + `IndexOfFirstArray` — 只返回**首个命中词**
- **影响**: 两套算法逻辑等价但行为分叉,后续维护易遗漏
- **修复**: 抽取 `SensitiveWordChecker` 静态类(签名: `Task<string[]> FindSensitiveWordsAsync(string content)`),消除分叉
- **关联 commit**: ecd7379

### P2-2 敏感词错误提示信息不一致
- **帖子**: `帖子内容包含敏感词「xxx」,请修改后提交`
- **消息**: `含有禁用词:xxx`
- **影响**: 前端需要分别处理两套错误文案
- **修复**: 统一为 `内容包含敏感词「xxx」,请修改后提交`(或类似)
- **关联 commit**: ecd7379

### P2-3 头像白名单中 `cdn.molitao.top` 为死代码
- **文件**: `backend/src/TtWork.Project/AppConsts.cs:51`
- **问题**: `cdn.molitao.top` 在白名单中但 Flutter 端零引用(`image.molitao.top` 才是实际使用)
- **相关清理**:
  - `WebsocketController.cs:609-610` 的 `Replace("cdn.molitao.top", "image.molitao.top")` 兼容代码
- **修复**: 从 `AllowedHeadImgUrlPrefixes` 移除 `cdn.molitao.top`,清理 WebsocketController 替换逻辑
- **关联 commit**: 7444094

### P2-4 头像白名单 HTTP 协议残留
- **文件**: `backend/src/TtWork.Project/AppConsts.cs:51-56`
- **当前白名单**:
  - `http://image.molitao.top` ← HTTP
  - `https://image.molitao.top` ← HTTPS
  - `https://thirdwx.qlogo.cn` ← HTTPS
  - `https://wx.qlogo.cn` ← HTTPS
  - `http://wx.qlogo.cn` ← HTTP
- **影响**: 保留 HTTP 前缀是为了兼容老用户/老接口返回值,但又拍云和微信接口已升级 HTTPS
- **修复**: 灰度期观察是否仍有 HTTP 头像流入;稳定后收紧为只允许 HTTPS
- **关联 commit**: 7444094

### P2-5 敏感词检查 O(n×m) 性能
- **文件**: `backend/src/TtWork.Project/PostBar/PostService.cs:44-69`, `MessageSendingService.cs:543`
- **算法**: 对每条敏感词做一次内容全扫
- **估算**: 1000 条敏感词 + 500 字内容 = 50 万次字符比较
- **影响**: 单条插入不会爆,但**批量发布/管理后台导入**场景可能有性能压力
- **修复**: 词库规模 > 5000 时改用 Aho-Corasick 自动机

### P2-6 2a5c429 → 837ccac 6 分钟来回修改未合并
- **commit 1**: `2a5c429` (21:09) - 添加 `User.AppleUserId` 字段
- **commit 2**: `837ccac` (21:16) - 移除该字段,改用 LoginBinding 表
- **问题**: 应在合并前用 `git rebase -i --autosquash` 合并
- **影响**: 历史可读性差,git blame 会显示中间状态
- **修复**: 下次涉及 6 分钟内来回的设计调整,在合并 PR 前用 `git commit --fixup` 合并

---

## 🟢 P3 - 低优先级(代码质量改进)

### P3-1 `JwtSecurityTokenHandler` 已废弃
- **文件**: `backend/src/TtWork.Project.Web.Core/Authentication/External/AppleAuthProviderApi.cs:46`
- **问题**: `new JwtSecurityTokenHandler()` 是 System.IdentityModel.Tokens.Jwt 的旧 API
- **影响**: 编译时会有 obsoletion warning
- **修复**: 改用 `Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler` (新 API)
- **关联 commit**: 2a5c429

### P3-2 `AppleAuthProviderApi.BundleId` 硬编码
- **文件**: `backend/src/TtWork.Project.Web.Core/Authentication/External/AppleAuthProviderApi.cs:22`
- **当前值**: `private const string BundleId = "com.molitao.molitaoApp";`
- **现状**: iOS 项目 bundle ID 验证一致 ✅
- **建议**: 改为 `IOptions<AppleSettings>` 注入(与 WechatSettings 一致),方便配置切换
- **关联 commit**: 2a5c429

### P3-3 d0ece71 的 F1 (Plan Compliance) 验证超时
- **commit 描述自报**: `F1 (Plan Compliance) blocked due to timeout`
- **影响**: 提交未经过完整的 plan compliance 验证
- **修复**: 重新跑 F1 验证,确保 UGC 1.2 收尾 PR 设计合规
- **关联 commit**: d0ece71

### P3-4 UGC 1.2 提交者变更不彻底
- **commit 074f182**: "ICP备案展示、账号注销/忘记密码、CMS法律协议管理" — 9 个变更混在一个 commit
- **commit d0ece71**: 同样大量变更(backend + Flutter + 新页面 + 路由)在单 commit
- **影响**: 单个 commit 涵盖 6+ 个目录,违反原子提交原则
- **建议**: 下次拆分为按目录/按功能的多个 commit

---

## 🛠 推荐修复顺序

1. **本周内(P1)**: P1-1 (title 审核), P1-2 (RefreshToken 清理)
2. **下个迭代(P1+P2)**: P1-3 (拉黑语义确认), P1-4 (BlockedUser 软删除), P2-1 (敏感词实现统一)
3. **后续(P2+P3)**: 剩余项

---

## 📎 关联文件

- 主分析会话: backend 自 2026-05-25 镜像构建以来 8 个 commit
- 详见: `issues-backlog.md` (索引条目待添加)
