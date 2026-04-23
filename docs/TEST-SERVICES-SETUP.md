# 测试服务配置指南

## 概述

已为 magic-tao 配置完整的测试服务：
- **Codecov** - 覆盖率追踪与质量门禁
- **SonarCloud** - 代码质量与安全扫描

## 1. Codecov 配置（覆盖率追踪）

### 1.1 获取 Codecov Token

1. 访问 [codecov.io](https://codecov.io)
2. 使用 GitHub 登录
3. 关联你的仓库 `magic-tao`
4. 复制页面显示的 token

### 1.2 添加 GitHub Secrets

```bash
# 在 GitHub 仓库 Settings > Secrets and variables > Actions 中添加：
CODECOV_TOKEN: <your-token>
```

### 1.3 配置覆盖率阈值

当前 CI 配置 `/p:Threshold=50` 表示覆盖率低于 50% 会警告。

**建议调整**：
- 新项目：从 30% 开始，逐步提高
- 成熟项目：设置 60-70%
- 关键模块（支付、订单）：设置 80%+

### 1.4 Codecov PR 注释设置

在 Codecov 仪表盘中可配置：
- PR 覆盖率变化注释
- 覆盖率下降阻止合并（Coverage Diff）
- 覆盖率历史趋势图

---

## 2. SonarCloud 配置（代码质量）

### 2.1 获取 SonarCloud Token

1. 访问 [sonarcloud.io](https://sonarcloud.io)
2. 使用 GitHub 登录
3. 创建 Organization（如果需要）
4. 添加仓库 `magic-tao`
5. 在 Account > Security 生成 token

### 2.2 添加 GitHub Secrets

```bash
SONAR_TOKEN: <your-token>
SONAR_ORGANIZATION: <your-org-key>
```

### 2.3 修改 SonarCloud Project Key

在 `.github/workflows/ci.yml` 中修改：

```yaml
/d:sonar.projectKey=your-actual-project-key
```

在 SonarCloud 仪表盘的项目设置中可以找到。

### 2.4 SonarCloud 质量门禁

建议在 SonarCloud 中配置：
- **Reliability**: A 或 B（无阻塞性错误）
- **Security**: A 或 B（无高危漏洞）
- **Coverage**: > 50%（可调整）
- **Duplications**: < 3%
- **Issues**: 阻止新增 Critical/Blocker 级别问题

---

## 3. AI 测试生成工具（可选）

### 3.1 Diffblue (商业)

最成熟的 AI 测试生成工具，支持 .NET：
- [diffblue.com](https://www.diffblue.com)
- 自动分析代码生成测试用例
- 支持回归测试保护

### 3.2 EvoGit (开源)

Git 驱动的智能测试建议：
- 分析 git history 识别高风险变更
- 建议需要补充测试的模块

### 3.3 手动策略（免费）

基于行业经验的测试用例补充：

**高风险模块优先测试**：
1. 支付/订单处理
2. 库存/库存扣减
3. 认证/授权
4. 文件上传/下载
5. 第三方 API 集成

**测试用例设计原则**：
- 边界条件测试（0, -1, 最大值, null）
- 异常路径测试（超时、网络错误、服务不可用）
- 并发测试（多线程同时操作）
- 权限测试（越权操作）

---

## 4. 验证配置

### 4.1 本地测试覆盖率

```bash
cd backend
dotnet test --configuration Release --collect:"XPlat Code Coverage"
# 报告生成在 TestResults 目录
```

### 4.2 GitHub Actions 验证

推送代码后检查：
1. Actions 运行是否成功
2. Codecov 注释是否出现在 PR
3. SonarCloud 扫描是否完成

### 4.3 常见问题

**Q: Codecov 上传失败？**
A: 检查 `CODECOV_TOKEN` 是否正确配置

**Q: SonarCloud 扫描失败？**
A: 检查 `SONAR_TOKEN` 和 `SONAR_ORGANIZATION`

**Q: 覆盖率 0%？**
A: 确保使用 `--collect:"XPlat Code Coverage"` 参数

---

## 5. 持续改进

### 5.1 提高覆盖率建议

1. **关键业务模块**：优先达到 80%+
2. **工具类/Helper**：达到 70%+
3. **Controller 层**：至少 50%（API 路径覆盖）

### 5.2 测试类型建议

| 类型 | 工具 | 目的 |
|------|------|------|
| 单元测试 | xUnit + Moq | 验证独立逻辑 |
| 集成测试 | Testcontainers | 验证数据库/API 集成 |
| 端到端测试 | Playwright | 验证用户流程 |
| 性能测试 | BenchmarkDotNet | 验证性能基准 |

---

## 6. 快速开始清单

- [ ] 注册 Codecov 并关联仓库
- [ ] 添加 `CODECOV_TOKEN` 到 GitHub Secrets
- [ ] 注册 SonarCloud 并关联仓库
- [ ] 添加 `SONAR_TOKEN` 和 `SONAR_ORGANIZATION` 到 GitHub Secrets
- [ ] 修改 ci.yml 中的 `sonar.projectKey`
- [ ] 推送代码验证 CI 运行
- [ ] 检查 PR 中的 Codecov 注释
- [ ] 检查 SonarCloud 扫描结果
