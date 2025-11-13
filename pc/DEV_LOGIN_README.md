# 开发调试登录功能 - 使用指南

## 🎉 功能已完成！

### 📋 功能概述
为PC端添加了开发调试登录功能，允许开发者直接通过用户ID快速登录，无需输入用户名密码。

### 🔧 实现的功能

1. **后台接口增强**
   - 修改了 `GenerateTokenForUser` 接口的IP检查逻辑
   - 现在支持本地IP范围访问：
     - `127.0.0.1`
     - `192.168.x.x`
     - `10.x.x.x`
     - `172.x.x.x`
     - `::1` (IPv6本地地址)

2. **前端API层**
   - 新增 `devAuthAPI.ts` 文件
   - 提供 `generateTokenForUser(userId)` 函数

3. **用户Store扩展**
   - 添加 `devLogin(userId)` 方法
   - 仅在开发环境中可用
   - 自动设置token和用户信息

4. **登录页面UI**
   - 在密码登录区域添加开发调试按钮
   - 提供用户1、用户2、用户3快速登录
   - 仅在开发环境中显示

5. **开发工具**
   - 创建了独立的调试助手脚本 `dev-login-helper.js`
   - 提供测试页面 `test-dev-login.html`

## 🚀 使用方法

### 方法1：通过登录页面
1. 访问前端应用：http://localhost:4200
2. 进入登录页面：http://localhost:4200/#/auth/login
3. 切换到"密码/验证码登录"标签
4. 在"开发调试登录"区域点击对应的用户按钮
5. 系统自动完成登录并跳转

**可用用户：**
- 用户1：需要验证数据库中是否存在
- 用户2：admin - 管理员用户
- 用户3：需要验证数据库中是否存在
- 用户14：oFzSV6st7nn8ZeoTEQqbveyjfMAU - 多角色用户 (Admin, AuctionManager, AuctionUser, Manager)

### 方法2：通过控制台
1. 在浏览器中打开任意页面
2. 打开开发者工具控制台
3. 加载调试助手脚本：
   ```javascript
   // 在控制台中粘贴以下代码
   const script = document.createElement('script');
   script.src = 'http://localhost:4200/dev-login-helper.js';
   document.head.appendChild(script);
   ```
4. 使用命令登录：
   ```javascript
   devLogin(1)   // 登录用户1 (如果存在)
   devLogin(2)   // 登录用户2 (admin)
   devLogin(3)   // 登录用户3 (如果存在)
   devLogin(14)  // 登录用户14 (多角色用户)
   ```
5. 验证token存储：
   ```javascript
   verifyTokenStorage()  // 检查token是否正确设置到localStorage和Cookie
   ```

### 方法3：直接API调用
```javascript
fetch('http://localhost:12580/api/TokenAuth/GenerateTokenForUser', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ userId: 2 })
})
.then(res => res.json())
.then(data => console.log(data));
```

## 🔐 Token存储机制

### 存储位置
- **localStorage**: 存储token和用户信息
  - `token`: JWT访问令牌
  - `user`: 用户基本信息（ID、用户名、邮箱等）
- **Cookie**: 存储token用于HTTP请求
  - `token`: 与localStorage相同的JWT令牌
  - `access_token`: 备用token存储

### 存储验证
开发调试登录会自动验证token是否正确存储：
- ✅ localStorage token已设置
- ✅ localStorage用户信息已设置
- ✅ Cookie token已设置

### 验证工具
1. **控制台验证**：
   ```javascript
   verifyTokenStorage()  // 检查存储状态
   ```

2. **测试页面验证**：
   - 打开 `pc/token-storage-test.html`
   - 点击"检查存储状态"按钮

## 🔒 安全特性

1. **环境限制**：仅在开发环境中可用
2. **IP限制**：仅允许本地IP访问后台接口
3. **权限检查**：需要有效的用户ID
4. **日志记录**：后台记录所有token生成请求
5. **存储验证**：自动验证token是否正确设置

## 📁 相关文件

### 后端文件
- `backend/src/TtWork.Project.Web.Core/Controllers/TokenAuthController.cs:770`
  - 修改了IP检查逻辑，支持本地IP范围

### 前端文件
- `pc/src/api/devAuthAPI.ts`
  - 新增的开发调试API文件
- `pc/src/stores/userStore.ts:78`
  - 添加了devLogin方法
- `pc/src/views/auth/login.vue:57`
  - 添加了开发调试登录按钮
- `pc/vite.config.mts:17`
  - 添加了API代理配置

### 工具文件
- `pc/dev-login-helper.js`
  - 独立的调试助手脚本，支持token存储验证
- `pc/test-dev-login.html`
  - API测试页面
- `pc/token-storage-test.html`
  - Token存储功能测试页面

## 🛠 启动服务

### 后台服务
```bash
cd backend
dotnet run --project src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj
```
服务地址：http://localhost:12580

### 前端服务
```bash
cd pc
npm run dev
```
服务地址：http://localhost:4200

## ✅ 测试验证

1. 确保后台服务运行在12580端口
2. 确保前端服务运行在4200端口
3. 访问登录页面验证开发调试按钮显示
4. 点击用户按钮验证登录功能
5. 检查用户信息是否正确加载

## 🐛 故障排除

### 问题：接口返回"此接口仅允许本地访问"
**解决方案**：确保请求来自本地IP范围，检查后台服务是否为最新版本

### 问题：开发调试按钮不显示
**解决方案**：确保当前为开发环境，检查 `import.meta.env.MODE` 是否为 'development'

### 问题：登录后用户信息不正确
**解决方案**：检查后台数据库中的用户数据，确保用户ID存在且状态正常

---

🎮 现在您可以享受便捷的开发调试登录体验了！