# 测试服务配置指南

## 概述

已为 magic-tao 配置完整的本地测试服务：
- **SonarQube** - 本地代码质量与安全扫描（无限免费）
- **Coverage** - 使用 Coverlet 收集测试覆盖率

## 1. SonarQube 本地部署

### 1.1 快速启动

```bash
# 启动 SonarQube + PostgreSQL
docker-compose -f docker-compose.sonarqube.yml up -d

# 等待服务启动（约 30 秒）
# 访问 http://localhost:9000
```

### 1.2 初始化配置

1. 首次访问 http://localhost:9000
2. 登录：admin / admin
3. 修改默认密码
4. 创建项目：Projects > Create Project > Manual
5. 生成 Token：My Account > Security > Tokens

### 1.3 获取 SonarQube URL 和 Token

启动后需要配置 GitHub Secrets：

```bash
SONARQUBE_URL: http://localhost:9000
SONARQUBE_TOKEN: <your-token>
```

### 1.4 SonarQube 质量门禁

建议配置：
- **Reliability**: A 或 B（无阻塞性错误）
- **Security**: A 或 B（无高危漏洞）
- **Coverage**: > 50%（可调整）
- **Duplications**: < 3%
- **Issues**: 阻止新增 Critical/Blocker 级别问题

---

## 2. CI 配置

CI 工作流已配置 SonarScanner：
- 每次 push/PR 自动运行
- 覆盖率收集（Coverlet + OpenCover）
- 代码质量扫描上传到本地 SonarQube

### 2.1 GitHub Secrets 配置

在 GitHub 仓库 Settings > Secrets and variables > Actions 中添加：

| Secret 名称 | 值 | 说明 |
|-------------|-----|------|
| SONARQUBE_URL | http://localhost:9000 | SonarQube 服务器地址 |
| SONARQUBE_TOKEN | xxx | 从 SonarQube 生成的 Token |

### 2.2 本地 SonarQube 访问

如果 SonarQube 运行在本地，需要确保 GitHub Actions 能访问：

**方案 1：使用内网穿透**
```bash
# 使用 ngrok
ngrok http 9000
# 将生成的 URL 配置为 SONARQUBE_URL
```

**方案 2：部署到公网服务器**
```bash
# 在服务器上运行 SonarQube
docker-compose -f docker-compose.sonarqube.yml up -d
# 配置服务器公网 IP 或域名
```

**方案 3：仅本地开发使用**
- 开发时本地运行 SonarQube
- CI 扫描结果不上传（注释掉 sonarscanner step）

---

## 3. 本地测试覆盖率

### 3.1 运行测试并收集覆盖率

```bash
cd backend
dotnet test --configuration Release --collect:"XPlat Code Coverage"
```

### 3.2 查看覆盖率报告

报告生成在 `backend/**/TestResults/` 目录：
- `coverage.cobertura.xml` - Cobertura 格式
- `coverage.opencover.xml` - OpenCover 格式

### 3.3 生成 HTML 覆盖率报告

```bash
# 安装 ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# 生成 HTML 报告
reportgenerator \
  -reports:**/coverage.cobertura.xml \
  -targetdir:coverage-report \
  -reporttypes:Html

# 打开 coverage-report/index.html 查看
```

---

## 4. SonarQube 项目配置

### 4.1 创建项目

1. 访问 http://localhost:9000
2. Projects > Create Project > Manual
3. 配置：
   - Project Key: `magic-tao`
   - Display Name: `Magic Tao Backend`

### 4.2 配置质量门禁

Administration > Quality Gates > Create：
- Reliability: 0 bugs 或 等级 A/B
- Security: 0 漏洞 或 等级 A/B
- Coverage: > 50%
- Duplications: < 3%

### 4.3 关联项目到质量门禁

Project > Quality Gate > 选择配置好的质量门禁

---

## 5. 常见问题

### Q: SonarQube 启动失败？

A：检查 Docker 是否运行，端口 9000 是否被占用：
```bash
docker-compose -f docker-compose.sonarqube.yml ps
docker logs magic-tao-sonarqube-1
```

### Q: CI 无法连接 SonarQube？

A：确保 `SONARQUBE_URL` 是公网可访问的地址，本地 localhost 无法被 GitHub Actions 访问。

### Q: 覆盖率 0%？

A：确保使用 `--collect:"XPlat Code Coverage"` 参数。

### Q: SonarScanner 安装失败？

A：在 CI 中添加重试逻辑或检查网络连接。

---

## 6. docker-compose.sonarqube.yml

```yaml
version: '3.8'

services:
  sonarqube:
    image: sonarqube:community
    container_name: magic-tao-sonarqube
    restart: unless-stopped
    ports:
      - "9000:9000"
    environment:
      - SONAR_ES_BOOTSTRAP_CHECKS_DISABLE=true
      - SONAR_JDBC_URL=jdbc:postgresql://sonarqube-db:5432/sonarqube
      - SONAR_JDBC_USERNAME=sonarqube
      - SONAR_JDBC_PASSWORD=sonarqube
    volumes:
      - sonarqube_data:/opt/sonarqube/data
      - sonarqube_extensions:/opt/sonarqube/extensions
      - sonarqube_logs:/opt/sonarqube/logs
    depends_on:
      - sonarqube-db

  sonarqube-db:
    image: postgres:16
    container_name: magic-tao-sonarqube-db
    restart: unless-stopped
    environment:
      - POSTGRES_USER=sonarqube
      - POSTGRES_PASSWORD=sonarqube
      - POSTGRES_DB=sonarqube
    volumes:
      - sonarqube_db_data:/var/lib/postgresql/data

volumes:
  sonarqube_data:
  sonarqube_extensions:
  sonarqube_logs:
  sonarqube_db_data:
```

---

## 7. 快速开始清单

- [ ] 配置 docker-compose.sonarqube.yml（如需要）
- [ ] 启动 SonarQube 服务
- [ ] 访问 http://localhost:9000 完成初始化
- [ ] 创建项目并生成 Token
- [ ] 添加 `SONARQUBE_URL` 和 `SONARQUBE_TOKEN` 到 GitHub Secrets
- [ ] 推送代码验证 CI 运行
- [ ] 检查 SonarQube 扫描结果

---

## 8. 测试类型建议

| 类型 | 工具 | 目的 |
|------|------|------|
| 单元测试 | xUnit + Shouldly + Moq | 验证独立逻辑 |
| 集成测试 | Testcontainers | 验证数据库/API 集成 |
| 端到端测试 | Playwright | 验证用户流程 |
| 性能测试 | BenchmarkDotNet | 验证性能基准 |
| 覆盖率 | Coverlet + ReportGenerator | 追踪测试覆盖 |

---

## 9. 提高覆盖率建议

1. **关键业务模块**：优先达到 80%+
2. **工具类/Helper**：达到 70%+
3. **Controller 层**：至少 50%（API 路径覆盖）