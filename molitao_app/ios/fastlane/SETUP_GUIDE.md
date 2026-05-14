# Fastlane Match 配置指南

## 📋 概述

Fastlane Match 是 iOS 证书和 Provisioning Profiles 的自动化管理工具，解决了团队协作中证书管理的痛点。

### 核心优势

- ✅ **自动化**: 一条命令同步所有证书
- ✅ **团队协作**: 证书存储在 Git 仓库，团队成员共享
- ✅ **安全**: 证书加密存储，密码保护
- ✅ **CI/CD 友好**: 完美支持 GitHub Actions、Jenkins 等

---

## 🔧 配置步骤

### Step 1: 准备 Apple Developer 账号信息

需要以下信息：

1. **Apple ID**: 您的 Apple Developer 账号邮箱
2. **Team ID**: 在 [Apple Developer](https://developer.apple.com/account) 中查看
   - 路径: Account → Membership → Team ID
3. **App ID**: `com.molitao.molitaoApp` (已自动识别)

### Step 2: 创建证书存储 Git 仓库

**⚠️ 重要**: 仓库必须设置为 **Private**，因为存储加密的证书。

```bash
# 方式 A: 在 GitHub 创建新的 Private 仓库
# 仓库名建议: molitao-ios-certs

# 方式 B: 使用现有 Private 仓库
# 建议创建独立仓库，避免与代码仓库混淆
```

创建后，记录 Git 仓库地址，例如：
- SSH: `git@github.com:YOUR_USERNAME/molitao-ios-certs.git`
- HTTPS: `https://github.com/YOUR_USERNAME/molitao-ios-certs.git`

### Step 3: 配置 Appfile

编辑 `fastlane/Appfile`，填写您的账号信息：

```ruby
apple_id("your-email@example.com")        # 您的 Apple ID
team_id("XXXXXXXXXX")                      # 您的 Team ID
```

### Step 4: 配置 Matchfile

编辑 `fastlane/Matchfile`：

```ruby
git_url("git@github.com:YOUR_USERNAME/molitao-ios-certs.git")  # Git 仓库地址
team_id("XXXXXXXXXX")                                          # Team ID
username("your-email@example.com")                             # Apple ID
```

### Step 5: 设置 Match 密码

**⚠️ 关键步骤**: Match 使用此密码加密证书，丢失将无法恢复！

```bash
# 方式 A: 环境变量 (推荐)
export MATCH_PASSWORD="your-strong-password"

# 方式 B: 添加到 ~/.zshrc 或 ~/.bash_profile
echo 'export MATCH_PASSWORD="your-strong-password"' >> ~/.zshrc
source ~/.zshrc
```

**密码要求**:
- 至少 12 个字符
- 包含大小写字母、数字、特殊符号
- 妥善保管，建议使用密码管理器

### Step 6: (可选) 配置 App Store Connect API Key

推荐使用 API Key 替代 Apple ID 密码，更安全且支持 CI/CD。

#### 创建 API Key

1. 访问 [App Store Connect](https://appstoreconnect.apple.com/access/integrations)
2. 点击 "Users and Access" → "Keys" → "App Store Connect API"
3. 点击 "+" 创建新 Key
4. 选择 "App Manager" 权限
5. 下载 `.p8` 文件 (⚠️ 只能下载一次，妥善保管)
6. 记录 Key ID (格式: `XXXXXXXXXX`)

#### 配置 API Key

将 `.p8` 文件放到 `fastlane/` 目录：

```bash
mkdir -p ~/workspace/magic-tao/molitao_app/ios/fastlane
mv ~/Downloads/AuthKey_XXXXXXXXXX.p8 ~/workspace/magic-tao/molitao_app/ios/fastlane/
```

在 `Appfile` 中添加：

```ruby
app_store_connect_api_key(
  key_id: "XXXXXXXXXX",           # API Key ID
  issuer_id: "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX",  # Issuer ID
  key_filepath: "fastlane/AuthKey_XXXXXXXXXX.p8"      # .p8 文件路径
)
```

---

## 🚀 使用方法

### 基本命令

```bash
cd ~/workspace/magic-tao/molitao_app/ios

# 同步开发证书
fastlane sync_dev_certs

# 同步 App Store 证书
fastlane sync_appstore_certs

# 同步 Ad Hoc 证书
fastlane sync_adhoc_certs

# 同步所有证书
fastlane sync_all_certs
```

### 创建新证书 (首次配置)

```bash
# 创建开发证书
fastlane create_dev_cert

# 创建 App Store 证书
fastlane create_appstore_cert
```

### 构建和上传

```bash
# 构建 Release 版本
fastlane build_release

# 构建 Ad Hoc 版本 (内部测试)
fastlane build_adhoc

# 上传到 TestFlight
fastlane upload_testflight
```

---

## 🔒 安全最佳实践

### 1. Git 仓库安全

```bash
# ✅ 必须: 设置仓库为 Private
# ✅ 建议: 启用 GitHub Branch Protection
# ❌ 禁止: 将证书仓库设为 Public
```

### 2. 密码管理

```bash
# ✅ 推荐: 使用环境变量
export MATCH_PASSWORD="your-password"

# ✅ 推荐: 使用密码管理器 (1Password, LastPass)
# ❌ 禁止: 在代码中明文存储密码
# ❌ 禁止: 提交 Matchfile 时包含 match_password()
```

### 3. API Key 安全

```bash
# ✅ 推荐: 使用 API Key 替代 Apple ID 密码
# ✅ 推荐: .p8 文件添加到 .gitignore
# ❌ 禁止: 将 .p8 文件提交到代码仓库
```

### 4. .gitignore 配置

创建或更新 `.gitignore`:

```gitignore
# Fastlane
fastlane/report.xml
fastlane/.fastlane.yml
fastlane/AuthKey_*.p8
fastlane/.env
*.ipa
*.dSYM.zip
build/
```

---

## 🐛 常见问题

### Q1: Match 密码忘记怎么办？

**A**: 无法恢复！需要：
1. 删除 Git 证书仓库
2. 重新创建证书
3. 重新设置 MATCH_PASSWORD

### Q2: 证书已存在，如何同步？

**A**: 使用 `readonly: true` (默认):

```bash
fastlane sync_appstore_certs
```

### Q3: Team ID 在哪里查看？

**A**: 
1. 登录 [Apple Developer](https://developer.apple.com/account)
2. 点击 "Account" → "Membership"
3. 查看 "Team ID" (10位字符)

### Q4: 如何在 CI/CD 中使用？

**A**: GitHub Actions 示例:

```yaml
- name: Sync Certificates
  env:
    MATCH_PASSWORD: ${{ secrets.MATCH_PASSWORD }}
  run: |
    cd ios
    fastlane sync_appstore_certs
```

### Q5: 多个 App 如何管理？

**A**: 在 Matchfile 中配置多个 App Identifier:

```ruby
app_identifier([
  "com.molitao.molitaoApp",
  "com.molitao.molitaoApp.watchkitapp",
  "com.molitao.molitaoApp.notification"
])
```

---

## 📚 相关文档

- [Fastlane 官方文档](https://docs.fastlane.tools/)
- [Match 文档](https://docs.fastlane.tools/actions/match/)
- [App Store Connect API](https://developer.apple.com/documentation/appstoreconnectapi)

---

## 🎯 下一步

1. ✅ Fastlane 已安装 (v2.232.2)
2. ✅ 配置文件已创建
3. ⏳ 填写 Apple Developer 账号信息
4. ⏳ 创建证书存储 Git 仓库
5. ⏳ 设置 MATCH_PASSWORD
6. ⏳ (可选) 配置 App Store Connect API Key
7. ⏳ 运行 `fastlane create_appstore_cert` 创建证书

---

## 💡 快速开始清单

```bash
# 1. 编辑配置文件
vim fastlane/Appfile      # 填写 Apple ID 和 Team ID
vim fastlane/Matchfile    # 填写 Git 仓库地址

# 2. 设置密码
export MATCH_PASSWORD="your-strong-password"

# 3. 创建证书 (首次)
fastlane create_appstore_cert

# 4. 后续同步 (团队成员)
fastlane sync_appstore_certs
```

---

**🎉 配置完成后，即可使用 fastlane 管理所有 iOS 证书！**
