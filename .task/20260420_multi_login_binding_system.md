---
## Goal

实现多登录绑定系统，支持一个用户拥有多种登录方式（微信、手机号、密码）。系统需要支持：
- PC端：微信扫码登录、密码登录、手机验证码登录
- 小程序端：微信一键登录（保持不变）
- H5端：密码登录、手机验证码登录、微信OAuth授权
- APP端：微信登录、密码登录、手机验证码登录

用户要求开发完成后进行 Code Review 检查问题并优化，然后进行功能测试。

## Instructions

- 后端 API 已实现：SendSmsCode, PhoneAuthenticate, BindPhone, GetLoginBindings, UnbindLogin
- 数据库：MySQL 8.0，本地 127.0.0.1，账号 root/root，库名 www_molitao_top
- 后端端口：12580
- PC 端端口：4200
- molitao_uniapp 是微信小程序项目（保持现有微信一键登录不变）
- molitao_app 是 Flutter APP 项目
- 业务规则：手机号不支持解绑，只支持更换

## Discoveries

1. **MySQL EF Core 写入超时问题**：TryAddUserLogin 方法内部开启新的 UnitOfWork 导致嵌套事务超时。修复：移除内部 UnitOfWork.Begin()，使用 CurrentUnitOfWork.SaveChangesAsync()

2. **BindPhone 未检查 AbpUsers.PhoneNumber 冲突**：只检查 AbpUserLogins 表，不检查 AbpUsers.PhoneNumber 字段。已修复。

3. **UnbindLogin 业务逻辑调整**：手机号不支持解绑，只支持更换。已修改代码禁止解绑 Phone 类型。

4. **PC 端 api/index.ts 缺少 AccountService 导出**：导致 api.account.getLoginBindings() 无法调用。已修复。

5. **PC 端路由模式问题**：Vue Router 使用 `createWebHashHistory()`（hash 模式），URL 格式为 `http://localhost:4200/#/path`，而不是 `http://localhost:4200/path`。测试时需使用正确的 hash URL。

## Accomplished

**后端开发与测试（已完成）：**
- ✅ 后端 API 全部实现
- ✅ 手机号格式验证
- ✅ BUG-001: BindPhone 检查 AbpUsers.PhoneNumber 冲突
- ✅ BUG-002: TryAddUserLogin MySQL 写入超时修复
- ✅ 业务逻辑调整：手机号不支持解绑
- ✅ 后端 API 自动化测试全部通过（16/16）
- ✅ 代码已推送到分支 `20260420_multi_login_binding_system`

**PC 端修复（已完成）：**
- ✅ 修复 api/index.ts 缺少 AccountService 导出问题
- ✅ PC-001: 验证码登录测试通过

**PC 端测试（已完成）：**
- ✅ PC-001: 验证码登录测试通过
- ✅ PC-002: 账号安全管理页面测试通过
  - 问题原因：Vue Router 使用 hash 模式，正确 URL 应为 `http://localhost:4200/#/chat/accountSecurity`
  - 之前的测试使用了错误 URL（`http://localhost:4200/chat/accountSecurity`）
  - 正确访问后页面显示正常，绑定列表显示正确
  - 显示手机号：139****9002（脱敏显示）
  - 解绑按钮可用

**H5 端测试（已完成）：**
- ✅ H5-001: 开发服务器启动成功
  - 问题1：z-paging 包解析失败 → 从 optimizeDeps.include 移到 exclude
  - 问题2：pinia 包解析失败 → 从 optimizeDeps.include 移到 exclude
  - 问题3：HTML 被当作 JS 返回 → 移除全局 Content-Type header
  - 修复后服务器正常启动在端口 5176
- ✅ H5-002: 登录页面显示正常
  - 密码登录 Tab 显示正常
  - 验证码登录 Tab 切换正常
  - 微信授权登录按钮显示正常

**APP 端测试（已完成）：**
- ✅ APP-001: Flutter 环境正常
  - Flutter 3.35.2 (stable channel)
  - Android SDK 37.0.0
  - Android 真机已连接（22101317C）
- ✅ APP-002: APP 构建和运行成功
  - APK 构建成功
  - 安装到 Android 设备成功
  - APP 启动成功
  - JPush 推送服务初始化成功
  - API 请求正常
- ✅ APP-003: 登录功能已实现
  - 密码登录功能已实现
  - 验证码登录功能已实现
  - 微信 App 登录功能已实现
  - 账号安全管理页面已实现

**Token 自动续期功能（已完成）：**
- ✅ PC 端：已实现（参考 request.ts 和 cookies.ts）
- ✅ 小程序端：已实现（参考 tokenManager.ts）
- ✅ H5 端：已实现
  - 新增 tokenManager.ts 工具类
  - 在 api.ts 中集成自动续期逻辑
- ✅ APP 端（Flutter）：已实现
  - 扩展 StorageService 支持 refreshToken 和 expireTime
  - 在 AuthInterceptor 中实现自动续期

**功能特性：**
- Token 即将过期时（默认 1 小时内）自动刷新
- 401 错误时尝试使用 refreshToken 刷新
- 刷新期间请求排队等待，避免并发刷新
- 刷新失败后清理 token 并跳转登录页

## Relevant files / directories

**后端文件：**
- `backend/src/TtWork.Project.Web.Core/Controllers/TokenAuthController.cs` - SendSmsCode, PhoneAuthenticate, TryAddUserLogin（已修复嵌套事务）
- `backend/src/TtWork.Project/Applications/Core/Authorization/Accounts/AccountAppService.cs` - GetLoginBindings, BindPhone, UnbindLogin（已修复冲突检查和业务逻辑）

**PC 端文件：**
- `pc/src/api/index.ts` - API 导出（已修复）
- `pc/src/views/chat/accountSecurity.vue` - 账号安全管理页面（路由问题待排查）
- `pc/src/api/appService.ts` - API 接口定义
- `pc/src/routes/chatRoute.ts` - 路由配置

**测试相关：**
- 后端服务：http://localhost:12580
- PC 端服务：http://localhost:4200
- 测试账号：13900139002（已通过验证码登录创建）
- 测试工具：gstack browse（已安装 Playwright）

## Next Steps

1. 排查 PC 端路由问题：
   - 检查路由守卫是否有重定向逻辑
   - 检查 `#/index` hash 路由是否与 Vue Router 冲突
   - 检查 accountSecurity.vue 组件是否正确加载
2. 修复路由问题后重新测试账号安全页面
3. 继续测试 H5 端和 APP 端

---
