# 证书创建与同步指南

## 📋 概述

Fastlane Match 支持三种证书类型：

| 类型 | 用途 | Provisioning Profile |
|------|------|----------------------|
| **development** | 开发调试 | iOS Development |
| **appstore** | App Store 发布 | App Store Distribution |
| **adhoc** | 内部测试分发 | Ad Hoc Distribution |

---

## 🚀 快速开始

### 前置条件检查

```bash
cd ~/workspace/magic-tao/molitao_app/ios/fastlane

# 运行预检查
./check.sh
```

确保所有检查通过后再继续。

---

## 📝 首次创建证书

⚠️ **重要**: 首次创建需要 Apple Developer 账号有足够权限。

### Step 1: 设置 MATCH_PASSWORD

```bash
# 如果尚未设置
export MATCH_PASSWORD="your-strong-password"

# 验证
echo $MATCH_PASSWORD
```

### Step 2: 创建开发证书

```bash
cd ~/workspace/magic-tao/molitao_app/ios

# 创建开发证书
fastlane create_dev_cert
```

这将：
1. 创建 iOS Development Certificate
2. 创建 Development Provisioning Profile
3. 加密并上传到 Git 仓库
4. 安装到本地 Keychain

### Step 3: 创建 App Store 证书

```bash
# 创建 App Store 证书
fastlane create_appstore_cert
```

这将：
1. 创建 iOS Distribution Certificate
2. 创建 App Store Provisioning Profile
3. 加密并上传到 Git 仓库
4. 安装到本地 Keychain

### Step 4: 创建 Ad Hoc 证书（可选）

```bash
# 创建 Ad Hoc 证书（用于内部测试）
fastlane create_adhoc_cert
```

---

## 🔄 同步现有证书

团队成员或 CI/CD 环境使用：

### 同步开发证书

```bash
fastlane sync_dev_certs
```

### 同步 App Store 证书

```bash
fastlane sync_appstore_certs
```

### 同步 Ad Hoc 证书

```bash
fastlane sync_adhoc_certs
```

### 同步所有证书

```bash
fastlane sync_all_certs
```

---

## 🔧 高级用法

### 强制刷新 Provisioning Profile

当添加新设备时，需要刷新 Profile：

```bash
# 修改 Matchfile 中的 readonly 为 false
# 或使用命令行参数
fastlane match appstore --force_for_new_devices true
```

### 指定 Bundle ID

如果项目有多个 App（如 Watch App）：

```bash
fastlane match appstore --app_identifier "com.molitao.molitaoApp.watchkitapp"
```

### 使用不同 Git 分支

```bash
fastlane match appstore --git_branch "enterprise"
```

---

## 🏗️ 构建与上传

### 构建 Release 版本

```bash
cd ~/workspace/magic-tao/molitao_app/ios

# 构建 App Store 版本
fastlane build_release
```

输出位置：`./build/molitao_app.ipa`

### 构建 Ad Hoc 版本

```bash
# 构建内部测试版本
fastlane build_adhoc
```

输出位置：`./build/molitao_app_adhoc.ipa`

### 上传到 TestFlight

```bash
# 构建 + 上传
fastlane upload_testflight
```

---

## 🔍 验证证书

### 查看本地证书

```bash
# 查看所有证书
fastlane match list

# 查看特定类型
fastlane match list --type development
fastlane match list --type appstore
```

### 检查 Provisioning Profile

```bash
# 在 Xcode 中查看
open ~/Library/MobileDevice/Provisioning\ Profiles
```

---

## 🗑️ 撤销证书

⚠️ **危险操作**: 撤销将影响所有使用该证书的 App。

### 撤销特定证书

```bash
# 撤销开发证书
fastlane match nuke development

# 撤销 App Store 证书
fastlane match nuke distribution
```

### 撤销所有证书

```bash
fastlane match nuke all
```

---

## 🐛 常见问题

### Q1: 证书创建失败："No available certificate"

**A**: Apple Developer 账号证书数量达到上限（3个）。

解决：
1. 登录 [Apple Developer](https://developer.apple.com/account)
2. Certificates → 删除不需要的证书
3. 重新运行 `fastlane create_appstore_cert`

### Q2: Provisioning Profile 失败

**A**: 可能原因：
- App ID 未创建
- 设备未注册（Development）
- 权限不足

解决：
```bash
# 强制刷新
fastlane match appstore --force_for_new_devices true
```

### Q3: Git 推送失败

**A**: 检查 Git 访问权限：

```bash
# 测试 SSH
ssh -T git@github.com

# 测试仓库访问
git ls-remote git@github.com:YOUR_USERNAME/molitao-ios-certs.git
```

### Q4: MATCH_PASSWORD 错误

**A**: 确保密码正确：

```bash
# 验证环境变量
echo $MATCH_PASSWORD

# 重新设置
export MATCH_PASSWORD="correct-password"
```

### Q5: 证书已过期

**A**: 证书有效期：
- Development: 1 年
- Distribution: 1 年
- Provisioning Profile: 1 年

解决：
```bash
# 重新创建
fastlane create_appstore_cert
```

---

## 📊 证书有效期监控

创建监控脚本 `fastlane/check_expiry.sh`：

```bash
#!/bin/bash
# 检查证书有效期

echo "Checking certificate expiry..."

# 获取证书信息
security find-identity -v -p codesigning

# 检查 Provisioning Profiles
for profile in ~/Library/MobileDevice/Provisioning\ Profiles/*.mobileprovision; do
    echo "Profile: $profile"
    security cms -D -i "$profile" | grep -A2 "ExpirationDate"
done
```

---

## 🔄 CI/CD 集成

### GitHub Actions 示例

```yaml
name: iOS Build

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: macos-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Ruby
        uses: ruby/setup-ruby@v1
        with:
          ruby-version: '3.2'
          
      - name: Install Fastlane
        run: gem install fastlane
        
      - name: Sync Certificates
        env:
          MATCH_PASSWORD: ${{ secrets.MATCH_PASSWORD }}
          FASTLANE_USER: ${{ secrets.FASTLANE_USER }}
          FASTLANE_PASSWORD: ${{ secrets.FASTLANE_PASSWORD }}
        run: |
          cd ios
          fastlane sync_appstore_certs
          
      - name: Build App
        run: |
          cd ios
          fastlane build_release
          
      - name: Upload to TestFlight
        env:
          APP_STORE_CONNECT_API_KEY_KEY_ID: ${{ secrets.API_KEY_ID }}
          APP_STORE_CONNECT_API_KEY_ISSUER_ID: ${{ secrets.ISSUER_ID }}
          APP_STORE_CONNECT_API_KEY_KEY: ${{ secrets.API_KEY }}
        run: |
          cd ios
          fastlane upload_testflight
```

---

## 📚 相关文档

- [Fastlane Match 文档](https://docs.fastlane.tools/actions/match/)
- [Apple Developer Certificates](https://developer.apple.com/support/certificates/)
- [Provisioning Profiles](https://help.apple.com/xcode/mac/current/#/dev5a80999d6)

---

## 🎯 快速检查清单

### 首次创建

- [ ] 已设置 MATCH_PASSWORD
- [ ] 已运行 `./check.sh` 验证
- [ ] 已创建开发证书 (`fastlane create_dev_cert`)
- [ ] 已创建 App Store 证书 (`fastlane create_appstore_cert`)
- [ ] 已验证证书 (`fastlane match list`)
- [ ] 已测试构建 (`fastlane build_release`)

### 团队成员同步

- [ ] 已获取 MATCH_PASSWORD
- [ ] 已配置 Git 访问权限
- [ ] 已运行 `fastlane sync_all_certs`
- [ ] 已验证证书 (`fastlane match list`)

---

**🎉 配置完成后，即可使用 Fastlane 自动管理所有 iOS 证书！**
