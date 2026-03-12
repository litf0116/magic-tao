# App 微信登录集成指南

> **状态**: 代码框架已准备完成，等待微信开放平台审核通过
> **更新时间**: 2026-03-12
> **预计完成时间**: 微信开放平台审核通过后 1-2 天

---

## 📋 前置条件

### 1. 微信开放平台账号
- [ ] 已注册微信开放平台账号
- [ ] 已提交移动应用申请
- [ ] 已获得 AppID 和 AppSecret

### 2. 开发环境
- [ ] HBuilderX 最新版本
- [ ] Android SDK 已配置
- [ ] 测试设备（Android 手机）

### 3. 服务器配置
- [ ] 后端 `appsettings.json` 已配置微信 AppID 和 AppSecret
- [ ] 前端 `manifest.json` 已配置 OAuth

---

## 🎯 集成步骤总览

### 阶段一：配置准备（已完成）
- ✅ 后端认证接口代码框架
- ✅ 前端 `manifest.json` OAuth 配置模板
- ✅ userStore.ts `appWxLogin` 方法
- ✅ API 接口定义

### 阶段二：配置填入（待微信开放平台审核通过）
1. 填入微信开放平台 AppID
2. 配置后端 appsettings.json
3. 重新构建 App

### 阶段三：测试验证
1. 本地测试登录流程
2. 真机测试
3. 完整功能测试

---

## 📝 详细步骤

### 步骤 1: 获取微信开放平台 AppID

1. 访问微信开放平台：https://open.weixin.qq.com/
2. 登录后进入"管理中心"
3. 找到"移动应用" → 查看应用详情
4. 记录下：
   - **AppID**: `wxa开头的字符串`
   - **AppSecret**: 需要单独申请查看

### 步骤 2: 配置 manifest.json

**文件位置**: `molitao_uniapp/src/manifest.json`

**找到并修改以下配置**:

```json
{
  "app-plus": {
    "oauth": {
      "weixin": {
        "appid": "wxa你的AppID",  // 填入微信开放平台 AppID
        "universalLinks": ""       // 可选，iOS Universal Links
      }
    },
    "distribute": {
      "sdkConfigs": {
        "oauth": {
          "weixin": {
            "appid": "wxa你的AppID"  // 填入微信开放平台 AppID
          }
        }
      }
    }
  }
}
```

**注意事项**:
- AppID 格式：`wxa` 开头，后面跟着字母和数字
- 如果支持 iOS，需要配置 Universal Links
- 配置后需要重新构建 App

### 步骤 3: 配置后端 appsettings.json

**文件位置**: `backend/src/TtWork.Project.Web.Host/appsettings.json`

**添加或修改以下配置**:

```json
{
  "Weixin": {
    "AppId": "wxa你的AppID",
    "AppSecret": "你的AppSecret",
    "OpenPlatform": true
  }
}
```

**注意**: `AppSecret` 是敏感信息，不要提交到代码仓库！

### 步骤 4: 启用 App 端微信登录按钮

**文件位置**: `molitao_uniapp/src/pages/index/login.vue`

**当前状态**: 微信登录按钮已显示，但 `appWxLogin` 方法会在微信开放平台审核通过前抛出错误

**微信开放平台通过后，取消注释以下代码**:

```vue
<!-- #ifdef APP-PLUS -->
<button
    class="w-full bg-green-500 text-white rounded-lg mb-3 py-3 font-bold"
    :disabled="isLoading"
    @tap="handleAppWxLogin"
>
    微信登录
</button>
<!-- #endif -->
```

```typescript
// App 端微信登录
const handleAppWxLogin = async () => {
    try {
        await userStore.appWxLogin()
        uni.showToast({
            title: '登录成功',
            icon: 'success'
        })
        uni.$emit('refreshView')
        uni.navigateBack()
    } catch (error: any) {
        uni.showToast({
            title: error?.message || '微信登录失败',
            icon: 'none'
        })
    }
}
```

### 步骤 5: 重新构建 App

```bash
# 构建 Android APK
cd molitao_uniapp
npm run build:app-android

# 或者使用 HBuilderX 构建
# HBuilderX → 发行 → 原生 App-云打包
```

### 步骤 6: 测试登录流程

#### 测试清单

1. **环境检查**
   - [ ] 微信 App 已安装（必须）
   - [ ] 测试设备网络正常
   - [ ] App 已成功安装

2. **登录流程测试**
   - [ ] 点击"微信登录"按钮
   - [ ] 弹出微信授权页面
   - [ ] 用户同意授权
   - [ ] 成功返回 App
   - [ ] 登录成功提示
   - [ ] 能正常访问功能

3. **异常情况测试**
   - [ ] 用户取消授权
   - [ ] 网络异常
   - [ ] 微信未安装
   - [ ] AppID 配置错误

---

## 🔍 常见问题排查

### 问题 1: 点击微信登录没反应

**可能原因**:
- 微信 App 未安装
- manifest.json OAuth 配置错误
- AppID 填写错误

**解决方案**:
1. 确认微信 App 已安装
2. 检查 manifest.json 中 AppID 是否正确
3. 重新构建 App

### 问题 2: 授权失败

**可能原因**:
- 后端 AppSecret 配置错误
- 网络连接问题
- 微信开放平台应用未通过审核

**解决方案**:
1. 检查后端 appsettings.json 配置
2. 检查网络连接
3. 确认微信开放平台应用状态

### 问题 3: 登录成功后获取不到用户信息

**可能原因**:
- unionid 未获取到
- 用户表未正确关联

**解决方案**:
1. 检查后端日志，确认是否获取到 unionid
2. 确认微信开放平台应用已绑定公众号

### 问题 4: LSP 类型错误

**已知问题**: `weixinAppAuthenticate` 方法在 API 中未定义

**解决方案**: 已在 `api.ts` 中添加 `weixinAppAuthenticate` 方法定义

---

## 📊 代码架构说明

### 后端架构

```
TokenAuthController.cs
├── Authenticate()                          // 账号密码登录
├── WeixinMiniAuthenticate()               // 小程序微信登录
├── WeixinMiniPhoneAuthenticate()          // 小程序手机号登录
└── AuthenticateWeixinApp()               // App 微信登录（新增）
    └── WeixinManger.GetOpenPlatformAccessTokenAsync()  // 微信开放平台 API
```

### 前端架构

```
userStore.ts
├── login()                                // 账号密码登录
├── wxLogin()                              // 小程序微信登录
├── phoneLogin()                           // 小程序手机号登录
└── appWxLogin()                           // App 微信登录（新增）
```

```
api.ts
├── authenticate()                         // 账号密码登录 API
├── weixinMiniAuthenticate()              // 小程序微信登录 API
├── phoneAuth()                           // 小程序手机号登录 API
└── weixinAppAuthenticate()               // App 微信登录 API（新增）
```

---

## 📚 参考文档

### 官方文档
- [微信开放平台移动应用开发指南](https://developers.weixin.qq.com/doc/oplatform/Mobile_App/WeChat_Login/Development_Guide.html)
- [UniApp OAuth 登录文档](https://uniapp.dcloud.net.cn/api/plugins/oauth.html)
- [微信登录授权流程](https://developers.weixin.qq.com/doc/oplatform/Mobile_App/WeChat_Login/Authorization.html)

### 项目文档
- `docs/App-OAuth配置说明.md` - OAuth 配置详细说明
- `docs/App-功能迁移计划.md` - App 功能迁移计划
- `docs/UniApp多端开发架构方案.md` - 多端开发架构

---

## ✅ 检查清单

在微信开放平台审核通过后，按以下顺序操作：

### 配置阶段
- [ ] 获取微信开放平台 AppID 和 AppSecret
- [ ] 更新 `manifest.json` OAuth 配置
- [ ] 更新后端 `appsettings.json` 配置
- [ ] 提交配置文件到版本控制

### 构建阶段
- [ ] 重新构建 Android APK
- [ ] 在测试设备上安装 APK
- [ ] 确认微信 App 已安装

### 测试阶段
- [ ] 测试微信登录流程
- [ ] 测试授权成功场景
- [ ] 测试授权取消场景
- [ ] 测试异常情况

### 上线阶段
- [ ] 构建 Release APK
- [ ] 签名 APK
- [ ] 上传到服务器
- [ ] 发布更新通知

---

## 🎉 完成标志

当以下条件都满足时，App 微信登录功能才算完成：

- [ ] 用户可以通过微信登录成功登录
- [ ] 已注册用户能正常登录
- [ ] 未注册用户能自动注册
- [ ] 所有异常情况都有友好提示
- [ ] 通过真机测试验证
- [ ] 代码已提交到版本控制

---

**文档维护**: 开发团队
**最后更新**: 2026-03-12