# App 开发 - 需补充内容清单

> **生成时间**: 2026-03-14
> **更新时间**: 2026-03-14
> **状态**: 前端代码已完成，等待后端和配置补充

---

## 一、已完成的前端工作 ✅

### 1. 拍卖订阅通知改造
- [x] `auction.vue` - 添加 App 端条件编译，使用极光推送
- [x] `auctionStore.ts` - `startNotify` 方法支持多平台参数
- [x] `api.ts` - 更新 `subStartNotify` 接口参数类型
- [x] `push-demo/index.vue` - 新增推送测试页面

### 2. 推送服务
- [x] 极光推送 SDK 集成 (manifest.json 配置)
- [x] `push.ts` 推送服务封装
- [x] 推送消息接收/点击处理
- [x] 别名/标签设置

### 3. App 更新
- [x] 版本检查 API
- [x] APK/WGT 热更新支持
- [x] 更新弹窗组件

### 4. App 微信登录改造
- [x] `login.vue` - 添加 App 端条件编译，调用 `appWxLogin`
- [x] `userStore.ts` - 修复 `appWxLogin` 方法，使用正确的 `plus.oauth.getServices` API

### 5. 支付功能条件编译
- [x] `my.vue` - 支付功能添加条件编译，App 端提示"开发中"
- [x] `auction.vue` - 支付功能添加条件编译，App 端提示"开发中"
- [x] `auctionSuccessList.vue` - 修复 `wx.previewImage` 为 `uni.previewImage`

### 6. 跨平台 API 适配
- [x] `chatMain.vue` - 修复 `wx.previewImage` 为 `uni.previewImage`
- [x] 所有 `wx.requestPayment` 已改为 `uni.requestPayment` 并添加条件编译

---

## 二、需要后端配合 🔴

### 1. 拍卖订阅通知 API 改造

**接口**: `POST /api/services/app/AuctionItem/SubStartNotify`

**当前参数**:
```json
{
    "auctionItemId": number,
    "openid": string
}
```

**需要支持的参数**:
```json
{
    "auctionItemId": number,
    "platform": "miniprogram" | "app",
    "openid": string,           // 小程序端必填
    "registrationId": string    // App端必填
}
```

**后端需要修改**:
1. 接口参数扩展 - 增加 `platform`, `registrationId` 字段
2. 数据库存储 - 增加存储 `registrationId` 的字段
3. 推送发送逻辑 - 拍卖开始时，根据平台类型调用不同的通知方式:
   - 小程序: 调用微信订阅消息 API
   - App: 调用极光推送 API

### 2. App 端推送发送接口

**新增接口**: `POST /api/services/app/Push/SendNotification`

**参数**:
```json
{
    "registrationId": string,   // 极光推送 registrationId
    "title": string,
    "content": string,
    "extras": object            // 额外参数，如跳转路径
}
```

**后端需要实现**:
1. 集成极光推送服务端 SDK
2. 实现推送发送逻辑
3. 支持 iOS (APNs) 和 Android (FCM/极光) 推送

### 3. App 微信登录 API

**接口**: `POST /api/TokenAuth/AuthenticateWeixinApp`

**参数**:
```json
{
    "authCode": string,     // 微信授权码
    "platform": string      // "android" | "ios"
}
```

**后端需要实现**:
1. 调用微信开放平台 API 获取用户信息
2. 完成用户认证流程

### 4. App 端支付功能

**需要配置**:
1. 微信支付 App 端 SDK (需要微信支付商户号)
2. 支付宝支付 SDK (可选)

**后端需要实现**:
1. App 端微信支付参数生成
2. 支付宝支付参数生成 (可选)

**前端已准备**:
- 条件编译框架已添加，App 端暂时提示"支付功能开发中"
- 等后端 SDK 配置完成后，移除提示，启用支付功能

---

## 三、需要配置的项 🔴

### 1. 微信开放平台 AppID

**文件**: `molitao_uniapp/src/manifest.json`

**需要填写**:
```json
{
    "app-plus": {
        "oauth": {
            "weixin": {
                "appid": "",           // ⬅️ 待填写微信开放平台 AppID
                "universalLinks": ""   // ⬅️ iOS UniversalLinks
            }
        }
    }
}
```

**申请流程**:
1. 登录 [微信开放平台](https://open.weixin.qq.com/)
2. 创建移动应用
3. 获取 AppID
4. iOS 需配置 UniversalLinks

### 2. 极光推送 AppKey

**当前配置** (已填写):
```json
{
    "app-plus": {
        "distribute": {
            "sdkConfigs": {
                "push": {
                    "jpush": {
                        "appkey": "4e91398522bb1286f6452efb",
                        "channel": "developer-default"
                    }
                }
            }
        }
    }
}
```

**后端需要配置**:
- 极光推送服务端 AppKey 和 Master Secret

### 3. iOS 推送证书

**需要准备**:
- APNs 开发证书 (.p12)
- APNs 生产证书 (.p12)
- 上传到极光推送控制台

---

## 四、测试检查清单

### 前端测试 (可在模拟器/真机进行)
- [ ] 小程序端订阅消息功能正常
- [ ] App 端推送服务初始化正常
- [ ] App 端获取 Registration ID 正常
- [ ] App 端本地通知正常
- [ ] App 端微信登录按钮可点击 (需要配置 AppID 才能真正登录)
- [ ] App 端账号密码登录正常
- [ ] App 端点击充值显示"支付功能开发中"提示
- [ ] App 端图片预览功能正常

### 后端联调测试 (需要后端配合)
- [ ] App 端订阅拍卖成功存储到数据库
- [ ] 拍卖开始时 App 端收到推送通知
- [ ] 推送通知点击跳转正常
- [ ] App 端微信登录成功获取用户信息
- [ ] App 端支付功能正常

---

## 五、相关文件清单

| 文件 | 改动类型 | 说明 |
|------|----------|------|
| `src/pages/chat/auction.vue` | 修改 | App 端订阅通知条件编译 + 支付条件编译 |
| `src/stores/auctionStore.ts` | 修改 | startNotify 支持多平台 |
| `src/utils/api.ts` | 修改 | subStartNotify 参数扩展 |
| `src/pages/push-demo/index.vue` | 新增 | 推送测试页面 |
| `src/pages/index/login.vue` | 修改 | App 端微信登录条件编译 |
| `src/stores/userStore.ts` | 修改 | 修复 appWxLogin 方法 |
| `src/pages/index/my.vue` | 修改 | 支付功能条件编译 |
| `src/pages/user/auctionSuccessList.vue` | 修改 | wx.previewImage 改为 uni.previewImage |
| `src/components/chat/chatMain.vue` | 修改 | wx.previewImage 改为 uni.previewImage |
| `src/manifest.json` | 待配置 | 微信 AppID 待填写 |

---

## 六、联系方式

如有问题，请联系前端开发人员。

**文档更新时间**: 2026-03-14