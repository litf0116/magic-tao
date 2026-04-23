# 测试服务配置指南

## 概述

已为 magic-tao 配置本地 SonarQube 代码质量扫描服务（无限免费）。

## 1. SonarQube 本地部署

### 1.1 快速启动

```bash
cd deploy/docker-compose
docker-compose -f docker-compose-sonarqube.yml up -d
```

访问 http://localhost:9000，初始账号：admin / admin

### 1.2 创建项目和 Token

1. 首次访问 http://localhost:9000，修改默认密码
2. 创建项目：Projects > Create Project > Manual
3. 配置：
   - Project Key: `magic-tao`
   - Display Name: `Magic Tao Backend`
4. 生成 Token：My Account > Security > Tokens

## 2. GitHub Secrets 配置

在 GitHub 仓库 Settings > Secrets and variables > Actions 中添加：

| Secret 名称 | 值 |
|-------------|-----|
| SONARQUBE_URL | http://localhost:9000 或公网地址 |
| SONARQUBE_TOKEN | 从 SonarQube 生成的 Token |

## 3. CI 配置

CI 工作流已配置 SonarScanner：
- 每次 PR 自动运行代码质量扫描
- 覆盖率收集（Coverlet + OpenCover）

## 4. 公网访问方案

SonarQube 需要公网访问才能用于 GitHub Actions：

**方案 1：内网穿透**
```bash
ngrok http 9000
```

**方案 2：部署到公网服务器**
```bash
ssh your-server
cd magic-tao/deploy/docker-compose
docker-compose -f docker-compose-sonarqube.yml up -d
```

## 5. 本地测试覆盖率

```bash
cd backend
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

## 6. 常见问题

| 问题 | 解决方案 |
|------|----------|
| SonarQube 启动失败 | 检查 Docker 是否运行，端口 9000 是否被占用 |
| CI 无法连接 | 确保 SONARQUBE_URL 是公网可访问地址 |
| 覆盖率 0% | 确保使用 `--collect:"XPlat Code Coverage"` |