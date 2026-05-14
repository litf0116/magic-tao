# Git 证书仓库配置指南

## 📋 为什么需要 Git 仓库？

Fastlane Match 使用 Git 仓库存储加密的 iOS 证书和 Provisioning Profiles，实现：

- ✅ **团队协作**: 所有成员共享同一套证书
- ✅ **版本控制**: 证书变更可追溯
- ✅ **安全存储**: 证书加密，密码保护
- ✅ **CI/CD 支持**: 自动化流水线可访问

---

## 🔐 安全要求

⚠️ **关键安全规则**：

1. **必须使用 Private 仓库**
   - ❌ 绝对禁止使用 Public 仓库
   - ✅ 证书加密存储，但仍需 Private 保护

2. **建议使用独立仓库**
   - ✅ 与代码仓库分离，权限管理更清晰
   - ✅ 避免意外提交到代码仓库

3. **访问控制**
   - ✅ 仅授权团队成员访问
   - ✅ 使用 SSH 密钥或 Deploy Key

---

## 🚀 创建 Git 仓库

### 方式 A: GitHub（推荐）

#### Step 1: 创建仓库

1. 访问 [GitHub New Repository](https://github.com/new)
2. 填写信息：
   - **Repository name**: `molitao-ios-certs` (或自定义)
   - **Description**: iOS certificates for molitao_app
   - **Visibility**: ✅ **Private** (必须)
   - **Initialize**: ❌ 不要勾选（空仓库）

3. 点击 **Create repository**

#### Step 2: 记录仓库地址

```
SSH:    git@github.com:YOUR_USERNAME/molitao-ios-certs.git
HTTPS:  https://github.com/YOUR_USERNAME/molitao-ios-certs.git
```

推荐使用 SSH 地址（需要配置 SSH 密钥）。

#### Step 3: 配置 SSH 密钥（如未配置）

```bash
# 检查是否已有 SSH 密钥
ls -la ~/.ssh

# 如果没有，生成新密钥
ssh-keygen -t ed25519 -C "your-email@example.com"

# 添加到 ssh-agent
eval "$(ssh-agent -s)"
ssh-add ~/.ssh/id_ed25519

# 复制公钥
cat ~/.ssh/id_ed25519.pub
# 添加到 GitHub: Settings → SSH and GPG keys → New SSH key
```

### 方式 B: GitLab

1. 访问 GitLab → New Project
2. 设置：
   - Project name: `molitao-ios-certs`
   - Visibility Level: **Private**
   - Initialize with README: ❌ 不勾选

### 方式 C: Bitbucket

1. 访问 Bitbucket → Create repository
2. 设置：
   - Repository name: `molitao-ios-certs`
   - Access level: ✅ Private repository

---

## ⚙️ 配置 Matchfile

编辑 `fastlane/Matchfile`：

```ruby
# Git 仓库地址
git_url("git@github.com:YOUR_USERNAME/molitao-ios-certs.git")

# Git 分支（可选，默认 master）
git_branch("master")

# 存储模式
storage_mode("git")
```

### 使用 HTTPS（不推荐）

如果必须使用 HTTPS：

```ruby
git_url("https://github.com/YOUR_USERNAME/molitao-ios-certs.git")
```

需要配置 Git 凭据缓存：

```bash
# macOS Keychain
git config --global credential.helper osxkeychain

# 或使用 Personal Access Token
git config --global credential.helper store
```

---

## 🔒 安全配置

### 1. 设置仓库权限

#### GitHub

1. 进入仓库 Settings → Collaborators and teams
2. 添加团队成员：
   - **Write** 权限：开发者（需要同步证书）
   - **Admin** 权限：管理员（需要管理证书）

#### 使用 Deploy Key（CI/CD）

1. 生成 Deploy Key：
   ```bash
   ssh-keygen -t ed25519 -C "molitao-certs-deploy" -f ~/.ssh/molitao_certs_deploy
   ```

2. 添加到 GitHub：
   - 仓库 Settings → Deploy keys → Add deploy key
   - Title: `CI/CD Deploy Key`
   - Key: `cat ~/.ssh/molitao_certs_deploy.pub`
   - ✅ Allow write access

3. 在 CI/CD 中使用：
   ```yaml
   - name: Setup SSH
     run: |
       mkdir -p ~/.ssh
       echo "${{ secrets.DEPLOY_KEY }}" > ~/.ssh/id_ed25519
       chmod 600 ~/.ssh/id_ed25519
   ```

### 2. Branch Protection（可选）

1. 仓库 Settings → Branches → Add rule
2. 配置：
   - Branch name pattern: `master`
   - ✅ Require pull request reviews
   - ✅ Require status checks

---

## ✅ 验证配置

### 测试 Git 访问

```bash
# 测试 SSH 连接
ssh -T git@github.com

# 测试仓库访问
git ls-remote git@github.com:YOUR_USERNAME/molitao-ios-certs.git
```

### 运行检查脚本

```bash
cd ~/workspace/magic-tao/molitao_app/ios/fastlane
./check.sh
```

---

## 🔄 证书仓库结构

Match 创建的仓库结构：

```
molitao-ios-certs/
├── certs/
│   ├── development/        # 开发证书
│   │   └── com.molitao.molitaoApp/
│   │       ├── certificate.pem
│   │       └── certificate.p12
│   ├── distribution/       # App Store 证书
│   │   └── com.molitao.molitaoApp/
│   │       ├── certificate.pem
│   │       └── certificate.p12
│   └── adhoc/             # Ad Hoc 证书
│       └── com.molitao.molitaoApp/
│           ├── certificate.pem
│           └── certificate.p12
└── profiles/
    ├── development/        # 开发 Provisioning Profile
    ├── appstore/          # App Store Provisioning Profile
    └── adhoc/             # Ad Hoc Provisioning Profile
```

所有文件均使用 MATCH_PASSWORD 加密。

---

## 🐛 常见问题

### Q1: Git 访问被拒绝？

**A**: 检查 SSH 密钥配置：

```bash
# 测试 SSH 连接
ssh -T git@github.com

# 如果失败，检查密钥
ls -la ~/.ssh
ssh-add -l
```

### Q2: 仓库已存在，如何迁移？

**A**: Match 会自动处理，无需手动迁移。

### Q3: 团队成员如何访问？

**A**: 
1. 添加为仓库 Collaborator
2. 共享 MATCH_PASSWORD（安全方式）
3. 运行 `fastlane sync_appstore_certs`

### Q4: 如何撤销访问？

**A**: 
1. 从仓库移除 Collaborator
2. 更改 MATCH_PASSWORD（推荐）
3. 重新创建证书（可选）

---

## 📚 相关文档

- [Fastlane Match Git Storage](https://docs.fastlane.tools/actions/match/#git-storage)
- [GitHub SSH Key Setup](https://docs.github.com/en/authentication/connecting-to-github-with-ssh)
- [GitHub Deploy Keys](https://docs.github.com/en/developers/overview/managing-deploy-keys)

---

## 🎯 快速检查清单

- [ ] 已创建 Private Git 仓库
- [ ] 已记录仓库地址
- [ ] 已配置 SSH 密钥（如使用 SSH）
- [ ] 已更新 Matchfile 中的 git_url
- [ ] 已测试 Git 访问 (`git ls-remote`)
- [ ] 已配置团队成员权限
- [ ] 已运行 `./check.sh` 验证

---

**🎉 配置完成后，Match 将使用 Git 仓库存储加密证书！**
