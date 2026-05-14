# App Store Connect API Key 配置指南

## 📋 为什么使用 API Key？

传统方式使用 Apple ID + 密码 + 双因素认证，存在以下问题：
- ❌ 每次操作需要输入验证码
- ❌ 无法在 CI/CD 中自动化
- ❌ 密码泄露风险高
- ❌ 双因素认证令牌过期需要重新生成

**API Key 优势**：
- ✅ 无需双因素认证
- ✅ 完美支持 CI/CD 自动化
- ✅ 可随时撤销，安全性更高
- ✅ 不会过期（除非手动撤销）

---

## 🔑 创建 API Key

### Step 1: 访问 App Store Connect

1. 登录 [App Store Connect](https://appstoreconnect.apple.com)
2. 点击顶部导航栏的 **Users and Access**
3. 选择 **Keys** 标签页
4. 点击 **App Store Connect API** 部分

### Step 2: 创建新 Key

1. 点击 **+** 按钮创建新 Key
2. 输入 Name（建议：`molitao-fastlane`）
3. 选择 Access 权限：
   - **App Manager** (推荐): 可管理 App、上传构建
   - **Admin**: 完全权限（谨慎使用）
   - **Developer**: 仅开发权限

4. 点击 **Generate**

### Step 3: 下载并保存

⚠️ **重要**: .p8 文件只能下载一次！

1. 点击 **Download API Key** 下载 `.p8` 文件
2. 文件名格式：`AuthKey_XXXXXXXXXX.p8`
3. 记录以下信息：
   - **Key ID**: 10位字符（如 `AB12CD34EF`）
   - **Issuer ID**: 在 Keys 页面顶部显示（UUID 格式）

### Step 4: 存放 API Key 文件

```bash
# 创建 fastlane 目录（如果不存在）
mkdir -p ~/workspace/magic-tao/molitao_app/ios/fastlane

# 移动 .p8 文件
mv ~/Downloads/AuthKey_XXXXXXXXXX.p8 ~/workspace/magic-tao/molitao_app/ios/fastlane/

# 设置权限（仅当前用户可读）
chmod 400 ~/workspace/magic-tao/molitao_app/ios/fastlane/AuthKey_*.p8
```

---

## ⚙️ 配置 Fastlane

### 方式 A: 修改 Appfile（推荐）

编辑 `fastlane/Appfile`，添加 API Key 配置：

```ruby
# App configuration for molitao_app

# 使用 App Store Connect API Key（推荐）
app_store_connect_api_key(
  key_id: "AB12CD34EF",                                    # Key ID
  issuer_id: "12345678-1234-1234-1234-123456789012",      # Issuer ID
  key_filepath: "fastlane/AuthKey_AB12CD34EF.p8"          # .p8 文件路径
)

# 以下信息仍需保留（Match 需要）
team_id("XXXXXXXXXX")
bundle_identifier("com.molitao.molitaoApp")
app_name("魔力淘")
```

### 方式 B: 环境变量（CI/CD 推荐）

```bash
# 设置环境变量
export APP_STORE_CONNECT_API_KEY_KEY_ID="AB12CD34EF"
export APP_STORE_CONNECT_API_KEY_ISSUER_ID="12345678-1234-1234-1234-123456789012"
export APP_STORE_CONNECT_API_KEY_KEY_FILEPATH="/path/to/AuthKey_AB12CD34EF.p8"

# 或添加到 ~/.zshrc
echo 'export APP_STORE_CONNECT_API_KEY_KEY_ID="AB12CD34EF"' >> ~/.zshrc
echo 'export APP_STORE_CONNECT_API_KEY_ISSUER_ID="12345678-1234-1234-1234-123456789012"' >> ~/.zshrc
echo 'export APP_STORE_CONNECT_API_KEY_KEY_FILEPATH="$HOME/workspace/magic-tao/molitao_app/ios/fastlane/AuthKey_AB12CD34EF.p8"' >> ~/.zshrc
```

### 方式 C: GitHub Actions（CI/CD）

在 GitHub 仓库设置 Secrets：

1. 进入仓库 Settings → Secrets and variables → Actions
2. 添加以下 Secrets：
   - `APP_STORE_CONNECT_API_KEY_KEY_ID`
   - `APP_STORE_CONNECT_API_KEY_ISSUER_ID`
   - `APP_STORE_CONNECT_API_KEY_KEY`（.p8 文件内容，Base64 编码）

在 workflow 中使用：

```yaml
- name: Setup Fastlane
  env:
    APP_STORE_CONNECT_API_KEY_KEY_ID: ${{ secrets.APP_STORE_CONNECT_API_KEY_KEY_ID }}
    APP_STORE_CONNECT_API_KEY_ISSUER_ID: ${{ secrets.APP_STORE_CONNECT_API_KEY_ISSUER_ID }}
    APP_STORE_CONNECT_API_KEY_KEY: ${{ secrets.APP_STORE_CONNECT_API_KEY_KEY }}
  run: |
    echo "$APP_STORE_CONNECT_API_KEY_KEY" | base64 -d > fastlane/AuthKey.p8
    fastlane sync_appstore_certs
```

---

## 🔒 安全最佳实践

### 1. 文件权限

```bash
# .p8 文件仅当前用户可读
chmod 400 fastlane/AuthKey_*.p8

# 验证权限
ls -l fastlane/AuthKey_*.p8
# 应显示: -r-------- 1 user staff ... AuthKey_*.p8
```

### 2. .gitignore

确保 `.gitignore` 包含：

```gitignore
# App Store Connect API Key
fastlane/AuthKey_*.p8
fastlane/.env
```

### 3. 定期轮换

建议每 6-12 个月轮换一次 API Key：

1. 创建新 Key
2. 更新配置
3. 测试新 Key
4. 撤销旧 Key

### 4. 最小权限原则

- ✅ 使用 **App Manager** 权限（足够上传构建）
- ❌ 避免使用 **Admin** 权限（除非必要）

---

## ✅ 验证配置

运行检查脚本：

```bash
cd ~/workspace/magic-tao/molitao_app/ios/fastlane
./check.sh
```

或手动验证：

```bash
# 测试 API Key 是否有效
fastlane spaceship -u your-email@example.com
```

---

## 🐛 常见问题

### Q1: API Key 无法访问 App？

**A**: 检查权限设置：
- 确保 Key 权限为 **App Manager** 或更高
- 确保您的 Apple ID 有权访问该 App

### Q2: .p8 文件丢失怎么办？

**A**: 无法恢复，需要：
1. 撤销旧 Key
2. 创建新 Key
3. 重新下载 .p8 文件
4. 更新配置

### Q3: CI/CD 中如何使用？

**A**: 使用 GitHub Secrets（见上文"方式 C"）

### Q4: Key ID 和 Issuer ID 在哪里查看？

**A**: 
- **Key ID**: 在 Keys 列表中显示（10位字符）
- **Issuer ID**: 在 Keys 页面顶部显示（UUID 格式）

---

## 📚 相关文档

- [App Store Connect API 官方文档](https://developer.apple.com/documentation/appstoreconnectapi)
- [Fastlane API Key 文档](https://docs.fastlane.tools/app-store-connect-api/)
- [Creating API Keys](https://help.apple.com/app-store-connect/#/devcdfb569cf)

---

## 🎯 快速检查清单

- [ ] 已创建 App Store Connect API Key
- [ ] 已下载 .p8 文件并妥善保管
- [ ] 已记录 Key ID 和 Issuer ID
- [ ] 已将 .p8 文件放到 `fastlane/` 目录
- [ ] 已设置文件权限 `chmod 400`
- [ ] 已添加到 `.gitignore`
- [ ] 已更新 `Appfile` 配置
- [ ] 已运行 `./check.sh` 验证

---

**🎉 配置完成后，Fastlane 将使用 API Key 进行认证，无需双因素验证！**
