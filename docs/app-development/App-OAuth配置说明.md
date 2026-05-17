# UniApp App 配置 - OAuth 配置说明

## 微信开放平台登录配置

### 1. manifest.json 配置

在 `app-plus` 部分添加 OAuth 配置：

```json
{
  "app-plus": {
    "oauth": {
      "weixin": {
        "appid": "__待填入微信开放平台AppID__",
        "universalLinks": ""
      }
    },
    "distribute": {
      "sdkConfigs": {
        "oauth": {
          "weixin": {
            "appid": "__待填入微信开放平台AppID__"
          }
        }
      }
    }
  }
}
```

### 配置说明

| 配置项 | 说明 | 必填 | 示例 |
|--------|------|------|------|
| `oauth.weixin.appid` | 微信开放平台移动应用 AppID | ✅ 是 | `wxa开头的AppID` |
| `oauth.weixin.universalLinks` | Universal Links（iOS 可选） | ❌ 否 | `https://example.com/ulink/` |
| `sdkConfigs.oauth.weixin.appid` | 同上，用于打包配置 | ✅ 是 | 同上 |

### 获取 AppID 步骤

1. 访问微信开放平台：https://open.weixin.qq.com/
2. 登录后进入"管理中心"
3. 创建"移动应用"
4. 填写应用信息：
   - 应用名称
   - 应用描述
   - 应用图标（多尺寸）
   - 应用截图
   - 应用官网
5. 等待审核（1-3 天）
6. 审核通过后获取 AppID 和 AppSecret

### Android 配置

**AndroidManifest.xml** (已有，无需修改)

```xml
<application>
  <!-- 微信登录和分享相关 -->
  <activity
    android:name=".wxapi.WXEntryActivity"
    android:exported="true"
    android:launchMode="singleTask"
  />
  <activity
    android:name=".wxapi.WXPayEntryActivity"
    android:exported="true"
  />
</application>
```

### iOS 配置

**info.plist** (需要添加 URL Scheme)

```xml
<key>LSApplicationQueriesSchemes</key>
<array>
  <string>weixin</string>
  <string>wechat</string>
</array>

<key>CFBundleURLTypes</key>
<array>
  <dict>
    <key>CFBundleURLName</key>
    <string>weixin</string>
    <key>CFBundleURLSchemes</key>
    <array>
      <string>wx开头的AppID</string>
    </array>
  </dict>
</array>
```

### 后端配置

在 `appsettings.json` 或 `appsettings.Production.json` 中添加：

```json
{
  "Weixin": {
    "AppId": "__微信开放平台AppID__",
    "AppSecret": "__微信开放平台AppSecret__",
    "OpenPlatform": true
  }
}
```

### 配置检查清单

- [ ] 获取微信开放平台 AppID 和 AppSecret
- [ ] 更新 manifest.json OAuth 配置
- [ ] 更新后端 appsettings.json
- [ ] AndroidManifest.xml 确认已有微信相关配置
- [ ] iOS info.plist 添加 URL Scheme（如需 iOS 支持）
- [ ] 重新构建 App 测试

### 注意事项

1. **AppID 区分**
   - 小程序 AppID：`wx开头的字符串`
   - 开放平台 AppID：`wxa开头的字符串`
   - 两者不能混用

2. **微信分享和登录**
   - App 端微信登录依赖微信开放平台
   - 微信分享也需要开放平台配置

3. **审核周期**
   - 微信开放平台审核通常需要 1-3 天
   - 建议提前申请

4. **测试限制**
   - 未通过审核的应用无法使用微信登录
   - 可以使用测试账号进行测试

### 常见问题

**Q: App 端微信登录点击没反应？**
A: 检查微信 App 是否安装，检查 AppID 是否正确配置

**Q: 授权失败？**
A: 检查后端 AppSecret 是否配置正确，检查网络连接

**Q: 登录成功后获取不到用户信息？**
A: 需要在微信授权时请求 `snsapi_userinfo` 权限

### 参考文档

- [微信开放平台文档](https://open.weixin.qq.com/)
- [UniApp OAuth 登录文档](https://uniapp.dcloud.net.cn/api/plugins/oauth.html)
- [微信登录开发指南](https://developers.weixin.qq.com/doc/oplatform/Mobile_App/WeChat_Login/Development_Guide.html)