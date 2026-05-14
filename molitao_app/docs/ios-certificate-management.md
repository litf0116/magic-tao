# iOS 证书管理配置记录

> 本文档记录 Fastlane Match 证书管理系统的所有配置信息，请妥善保管。

## 📅 配置日期

2026-05-13

---

## 🔐 Apple Developer 账号

| 项目 | 值 |
|------|-----|
| Apple ID | `799849240@qq.com` |
| Team ID | `WX4RK78D62` |
| Team Name | TengFei Li |
| Bundle ID | `com.molitao.molitaoApp` |
| App Name | 魔力淘 |

---

## 🔑 App Store Connect API Key

| 项目 | 值 |
|------|-----|
| Key ID | `6VUQS8645G` |
| Key 文件 | `AuthKey_6VUQS8645G.p8` |
| 文件位置 | `ios/fastlane/AuthKey_6VUQS8645G.p8` |
| 权限 | App Manager |

### 获取 Issuer ID

1. 访问 [App Store Connect](https://appstoreconnect.apple.com/access/integrations/api)
2. 点击 Keys 标签页
3. 在页面顶部找到 **Issuer ID**（类似 `69a6deXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX`）

---

## 📦 Git 证书仓库

| 项目 | 值 |
|------|-----|
| 仓库名称 | `molitao-ios-certs` |
| 仓库地址 | `git@github.com:litf0116/molitao-ios-certs.git` |
| HTTPS 地址 | `https://github.com/litf0116/molitao-ios-certs` |
| 可见性 | Private |
| 分支 | master |

---

## 🔒 Match 加密密码

| 项目 | 值 |
|------|-----|
| 密码 | `w0MgRKODKV9xxghMPPlDmfTk` |
| 密码文件 | `ios/fastlane/.match_password` |

⚠️ **重要**：此密码用于加密存储在 Git 仓库中的所有证书，请务必保管好！

---

## 📁 文件位置

```
molitao_app/ios/fastlane/
├── Appfile                           # Apple 账号配置
├── Fastfile                          # Fastlane 任务定义
├── Matchfile                         # Match 证书配置
├── AuthKey_6VUQS8645G.p8            # API Key 文件
├── .match_password                   # Match 密码（已保存）
├── setup.sh                          # 交互式配置脚本
├── check.sh                          # 配置检查脚本
├── SETUP_GUIDE.md                    # 配置指南
├── API_KEY_GUIDE.md                  # API Key 指南
├── GIT_REPO_GUIDE.md                 # Git 仓库指南
└── CERTIFICATE_GUIDE.md              # 证书管理指南
```

---

## 🚀 常用命令

### 设置环境变量

```bash
export MATCH_PASSWORD="w0MgRKODKV9xxghMPPlDmfTk"
```

### 创建证书

```bash
cd ~/workspace/magic-tao/molitao_app/ios
fastlane create_appstore_cert
```

### 构建 & 上传 TestFlight

```bash
fastlane upload_testflight
```

### 构建 & 上传 App Store

```bash
fastlane upload_appstore
```

---

## 🔄 CI/CD 集成

### GitHub Actions 环境变量

在 GitHub 仓库 Settings → Secrets and variables → Actions 中添加：

| Secret Name | Value |
|-------------|-------|
| `MATCH_PASSWORD` | `w0MgRKODKV9xxghMPPlDmfTk` |
| `FASTLANE_USER` | `799849240@qq.com` |
| `APP_STORE_CONNECT_API_KEY_KEY_ID` | `6VUQS8645G` |
| `APP_STORE_CONNECT_API_KEY_ISSUER_ID` | (从 App Store Connect 获取) |
| `APP_STORE_CONNECT_API_KEY_KEY` | (p8 文件内容，Base64 编码) |

### Base64 编码 p8 文件

```bash
base64 -i ios/fastlane/AuthKey_6VUQS8645G.p8 | pbcopy
```

---

## 📋 检查清单

- [x] Fastlane 安装
- [x] Apple Developer 账号配置
- [x] Git 证书仓库创建
- [x] Match 密码生成
- [x] API Key 配置
- [ ] Issuer ID 获取（需要从 App Store Connect 获取）
- [ ] 创建 Development 证书
- [ ] 创建 AppStore 证书
- [ ] 构建 TestFlight 版本
- [ ] 上传 TestFlight

---

## 🆘 故障排除

### 证书已存在

```bash
# 强制重新创建
fastlane match appstore --force
```

### Git 仓库访问失败

```bash
# 检查 SSH 密钥
ssh -T git@github.com

# 或使用 HTTPS
git remote set-url origin https://github.com/litf0116/molitao-ios-certs.git
```

### Match 密码错误

```bash
# 重新设置环境变量
export MATCH_PASSWORD="w0MgRKODKV9xxghMPPlDmfTk"
```

---

## 📚 相关文档

- [Fastlane 官方文档](https://docs.fastlane.tools/)
- [Match 指南](https://docs.fastlane.tools/actions/match/)
- [App Store Connect API](https://developer.apple.com/documentation/appstoreconnectapi)
