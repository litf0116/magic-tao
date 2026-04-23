# 测试服务配置指南

## 概述

已为 magic-tao 配置测试覆盖率收集服务。

## 1. CI 测试覆盖率

CI 工作流已配置 Coverlet 覆盖率收集：
- 每次 push/PR 自动运行测试
- 生成覆盖率报告（cobertura.xml）
- 报告保存在 GitHub Actions Artifacts 中（保留 7 天）

### 查看覆盖率报告

1. 进入 PR 或 commit 的 GitHub Actions
2. 点击 `backend-build` job
3. 找到 `Upload coverage reports` step
4. 下载 `coverage-reports` artifact

## 2. 本地 SonarQube（可选）

如需代码质量扫描，可在本地部署 SonarQube。

### 2.1 快速启动

```bash
cd deploy/docker-compose
docker-compose -f docker-compose-sonarqube.yml up -d
```

访问 http://localhost:9002，初始账号：admin / admin

### 2.2 本地扫描

```bash
dotnet tool install --global dotnet-sonarscanner

cd backend
dotnet sonarscanner begin \
  /k:"magic-tao" \
  /n:"Magic Tao Backend" \
  /d:sonar.host.url="http://localhost:9002"

dotnet build Molitao.sln --configuration Release

dotnet sonarscanner end
```

## 3. 本地测试覆盖率

### 3.1 运行测试并收集覆盖率

```bash
cd backend
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

### 3.2 查看覆盖率报告

报告生成在 `backend/**/TestResults/` 目录：
- `coverage.cobertura.xml` - Cobertura 格式

### 3.3 生成 HTML 覆盖率报告

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html
# 打开 coverage-report/index.html
```

## 4. 常见问题

| 问题 | 解决方案 |
|------|----------|
| 覆盖率 0% | 确保使用 `--collect:"XPlat Code Coverage"` |
| SonarQube 启动失败 | 检查 Docker 是否运行，端口 9000 是否被占用 |
| SonarScanner 连接失败 | 确保 SonarQube 运行在 localhost:9000 |