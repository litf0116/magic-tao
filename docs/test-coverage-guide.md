# 单元测试覆盖率统计配置指南

## 概述

本文档配置魔力淘项目的单元测试覆盖率统计功能，覆盖所有技术栈。

---

## 📊 测试覆盖率现状

| 模块 | 当前状态 | 目标覆盖率 | 优先级 |
|------|----------|------------|--------|
| Backend (.NET) | ⚠️ 仅2个测试文件 | 70% | 高 |
| PC (Vue) | ⚠️ 仅E2E测试 | 60% | 中 |
| Flutter App | ❌ 无测试 | 50% | 中 |
| UniApp | ⚠️ 2个测试文件 | 60% | 低 |

---

## 🔧 配置步骤

### 1. Backend (.NET 8 + xUnit + Coverlet)

#### 1.1 安装 Coverlet（已安装）

项目已包含 `coverlet.collector` 包：

```xml
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

#### 1.2 创建覆盖率配置文件

```xml
<!-- backend/test/TtWork.Project.Tests/Directory.build.props -->
<Project>
  <PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>opencover</CoverletOutputFormat>
    <CoverletOutput>../coverage.xml</CoverletOutput>
    <Threshold>70</Threshold>
    <Exclude>[TtWork.*.Tests]*</Exclude>
    <ExcludeByAttribute>Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute</ExcludeByAttribute>
  </PropertyGroup>
</Project>
```

#### 1.3 添加测试脚本

```bash
#!/bin/bash
# backend/scripts/run-tests-with-coverage.sh

echo "🧪 运行单元测试并生成覆盖率报告..."

cd backend

# 运行测试并收集覆盖率
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=../coverage.xml \
  /p:Threshold=70 \
  /p:Exclude="[TtWork.*.Tests]*" \
  /p:ExcludeByAttribute="Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute"

# 生成 HTML 报告（需要安装 reportgenerator）
if command -v reportgenerator &> /dev/null; then
  echo "📊 生成 HTML 覆盖率报告..."
  reportgenerator \
    -reports:./TestResults/**/coverage.opencover.xml \
    -targetdir:./coverage-report \
    -reporttypes:Html

  echo "✅ 报告已生成: backend/coverage-report/index.html"
  open ./coverage-report/index.html
else
  echo "⚠️  reportgenerator 未安装"
  echo "安装方法: dotnet tool install -g dotnet-reportgenerator-globaltool"
fi
```

#### 1.4 CI/CD 集成

```yaml
# .github/workflows/backend-tests.yml
name: Backend Tests

on:
  push:
    paths:
      - 'backend/**'
  pull_request:
    paths:
      - 'backend/**'

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run Tests with Coverage
        run: |
          cd backend
          dotnet test \
            --collect:"XPlat Code Coverage" \
            --results-directory ./TestResults \
            /p:CollectCoverage=true \
            /p:CoverletOutputFormat=opencover \
            /p:Threshold=70

      - name: Upload Coverage Report
        uses: codecov/codecov-action@v3
        with:
          files: ./backend/TestResults/**/coverage.opencover.xml
          flags: backend
          name: backend-coverage
```

---

### 2. PC (Vue 3 + Vitest)

#### 2.1 安装 Vitest

```bash
cd pc
npm install --save-dev vitest @vitest/coverage-c8
```

#### 2.2 配置 Vitest

```typescript
// pc/vitest.config.ts
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'

export default defineConfig({
  plugins: [
    vue(),
    AutoImport({
      imports: ['vue', 'vue-router', 'pinia'],
      dts: 'src/auto-imports.d.ts',
    }),
    Components({
      resolvers: [ElementPlusResolver()],
      dts: 'src/components.d.ts',
    }),
  ],
  test: {
    // 测试环境
    environment: 'jsdom',

    // 全局变量
    globals: true,

    // 覆盖率配置
    coverage: {
      provider: 'c8',
      reporter: ['text', 'json', 'html', 'lcov'],
      reportsDirectory: './coverage',

      // 覆盖率阈值
      thresholds: {
        lines: 60,
        functions: 60,
        branches: 50,
        statements: 60,
      },

      // 包含和排除
      include: [
        'src/**/*.ts',
        'src/**/*.vue',
      ],
      exclude: [
        'src/**/*.d.ts',
        'src/**/*.spec.ts',
        'src/**/*.test.ts',
        'src/main.ts',
        'src/router/**/*.ts',
      ],
    },

    // 设置文件
    setupFiles: ['./tests/setup.ts'],

    // 包含的测试文件
    include: ['**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'],

    // 排除
    exclude: [
      'node_modules',
      'dist',
      '.idea',
      '.git',
      '.cache',
    ],
  },

  resolve: {
    alias: {
      '@': '/src',
    },
  },
})
```

#### 2.3 添加测试设置文件

```typescript
// pc/tests/setup.ts
import { config } from '@vue/test-utils'

// 全局组件 stub
config.global.stubs = {}

// 全局 mock
config.global.mocks = {
  $router: {
    push: vi.fn(),
    replace: vi.fn(),
  },
  $route: {
    params: {},
    query: {},
  },
}

// 自动导入测试工具
import { vi } from 'vitest'

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
}
global.localStorage = localStorageMock as any

// Mock sessionStorage
global.sessionStorage = localStorageMock as any
```

#### 2.4 更新 package.json

```json
{
  "scripts": {
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:run": "vitest run",
    "test:coverage": "vitest run --coverage",
    "test:watch": "vitest watch"
  },
  "devDependencies": {
    "vitest": "^1.0.0",
    "@vitest/coverage-c8": "^1.0.0",
    "@vue/test-utils": "^2.4.0",
    "jsdom": "^24.0.0",
    "happy-dom": "^14.0.0"
  }
}
```

#### 2.5 创建示例测试

```typescript
// pc/src/utils/__tests__/request.test.ts
import { describe, it, expect, vi, beforeEach } from 'vitest'
import axios from 'axios'
import { request } from '../request'

vi.mock('axios')

describe('request utils', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('should make GET request', async () => {
    const mockData = { id: 1, name: 'test' }
    vi.mocked(axios.get).mockResolvedValue({ data: mockData })

    const result = await request.get('/api/test')

    expect(axios.get).toHaveBeenCalledWith('/api/test')
    expect(result).toEqual(mockData)
  })

  it('should handle error', async () => {
    const error = new Error('Network error')
    vi.mocked(axios.get).mockRejectedValue(error)

    await expect(request.get('/api/test')).rejects.toThrow('Network error')
  })
})
```

---

### 3. Flutter App

#### 3.1 启用覆盖率

Flutter 内置覆盖率支持，无需额外安装。

#### 3.2 创建测试脚本

```bash
#!/bin/bash
# molitao_app/scripts/run-tests-with-coverage.sh

echo "🧪 运行 Flutter 测试并生成覆盖率报告..."

cd molitao_app

# 运行测试并生成覆盖率数据
flutter test --coverage

# 检查是否生成覆盖率文件
if [ -f "coverage/lcov.info" ]; then
  echo "✅ 覆盖率数据已生成: coverage/lcov.info"

  # 使用 lcov 生成 HTML 报告
  if command -v genhtml &> /dev/null; then
    echo "📊 生成 HTML 报告..."
    genhtml coverage/lcov.info -o coverage/html

    echo "✅ 报告已生成: molitao_app/coverage/html/index.html"
    open coverage/html/index.html
  else
    echo "⚠️  genhtml 未安装"
    echo "安装方法 (macOS): brew install lcov"
    echo "安装方法 (Linux): sudo apt-get install lcov"
  fi

  # 移除测试文件的覆盖率
  lcov --remove coverage/lcov.info '**/*.g.dart' '**/*.freezed.dart' '**/test/**' -o coverage/lcov_cleaned.info

  # 生成摘要
  echo ""
  echo "📈 覆盖率摘要:"
  lcov --summary coverage/lcov_cleaned.info
else
  echo "❌ 覆盖率数据生成失败"
  exit 1
fi
```

#### 3.3 创建示例测试

```dart
// molitao_app/test/utils/image_url_converter_test.dart
import 'package:flutter_test/flutter_test.dart';
import 'package:molitao_app/core/utils/image_url_converter.dart';

void main() {
  group('ImageUrlConverter', () {
    test('should convert relative path to full URL', () {
      const relativePath = '/uploads/avatar.png';
      const baseUrl = 'https://molitao.top';

      final result = ImageUrlConverter.convert(relativePath, baseUrl);

      expect(result, equals('https://molitao.top/uploads/avatar.png'));
    });

    test('should return full URL as-is', () {
      const fullUrl = 'https://example.com/image.png';

      final result = ImageUrlConverter.convert(fullUrl, 'https://molitao.top');

      expect(result, equals(fullUrl));
    });

    test('should handle empty path', () {
      const emptyPath = '';

      final result = ImageUrlConverter.convert(emptyPath, 'https://molitao.top');

      expect(result, isEmpty);
    });
  });
}
```

#### 3.4 CI/CD 集成

```yaml
# .github/workflows/flutter-tests.yml
name: Flutter Tests

on:
  push:
    paths:
      - 'molitao_app/**'
  pull_request:
    paths:
      - 'molitao_app/**'

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup Flutter
        uses: subosito/flutter-action@v2
        with:
          flutter-version: '3.19.0'

      - name: Install dependencies
        working-directory: molitao_app
        run: flutter pub get

      - name: Run tests with coverage
        working-directory: molitao_app
        run: flutter test --coverage

      - name: Upload coverage to Codecov
        uses: codecov/codecov-action@v3
        with:
          files: ./molitao_app/coverage/lcov.info
          flags: flutter
          name: flutter-coverage
```

---

### 4. UniApp

#### 4.1 安装 Vitest

```bash
cd molitao_uniapp
npm install --save-dev vitest @vitest/coverage-c8 happy-dom
```

#### 4.2 配置文件

```typescript
// molitao_uniapp/vitest.config.ts
import { defineConfig } from 'vitest/config'

export default defineConfig({
  test: {
    environment: 'happy-dom',
    globals: true,
    coverage: {
      provider: 'c8',
      reporter: ['text', 'json', 'html'],
      reportsDirectory: './coverage',
      thresholds: {
        lines: 60,
        functions: 60,
        branches: 50,
        statements: 60,
      },
      include: [
        'src/**/*.ts',
        'src/**/*.vue',
      ],
      exclude: [
        'src/**/*.d.ts',
        'src/**/*.spec.ts',
        'src/**/*.test.ts',
      ],
    },
  },
})
```

---

## 📈 覆盖率报告查看

### 本地查看

1. **Backend**
   ```bash
   cd backend
   ./scripts/run-tests-with-coverage.sh
   open coverage-report/index.html
   ```

2. **PC**
   ```bash
   cd pc
   npm run test:coverage
   open coverage/index.html
   ```

3. **Flutter**
   ```bash
   cd molitao_app
   ./scripts/run-tests-with-coverage.sh
   open coverage/html/index.html
   ```

4. **UniApp**
   ```bash
   cd molitao_uniapp
   npm run test:coverage
   open coverage/index.html
   ```

### Codecov 集成

1. 创建 `codecov.yml`:

```yaml
# codecov.yml
codecov:
  require_ci_to_pass: yes

coverage:
  precision: 2
  round: down
  range: "70...100"

  status:
    project:
      default:
        target: 70%
        threshold: 5%
    patch:
      default:
        target: 80%

comment:
  layout: "reach,diff,flags,files,footer"
  behavior: default
  require_changes: no

flags:
  backend:
    paths:
      - backend/
  pc:
    paths:
      - pc/
  flutter:
    paths:
      - molitao_app/
  uniapp:
    paths:
      - molitao_uniapp/
```

2. 在 GitHub 仓库设置中添加 Codecov token

---

## 🎯 覆盖率目标与优先级

### 短期目标（1个月内）

- [ ] Backend: 核心业务逻辑达到 50%
- [ ] PC: 工具函数和 API 层达到 40%
- [ ] Flutter: 基础工具类达到 40%

### 中期目标（3个月内）

- [ ] Backend: 整体达到 70%
- [ ] PC: 整体达到 60%
- [ ] Flutter: 整体达到 50%
- [ ] UniApp: 整体达到 60%

### 长期目标（6个月内）

- [ ] 所有模块达到 70%+
- [ ] 核心业务逻辑达到 80%+
- [ ] 建立 CI 覆盖率门禁

---

## 📝 测试编写规范

### 测试命名规范

```
测试方法_状态_期望结果

示例:
- getUserById_WhenUserExists_ReturnsUser
- calculateDiscount_WhenAmountIsZero_ThrowsException
- formatPrice_WhenPriceIsNegative_ReturnsZero
```

### 测试结构 (AAA模式)

```typescript
describe('UserService', () => {
  it('should create user with valid data', async () => {
    // Arrange (准备)
    const userData = { name: 'John', email: 'john@example.com' }
    const mockUser = { id: 1, ...userData }

    vi.mocked(userRepository.create).mockResolvedValue(mockUser)

    // Act (执行)
    const result = await userService.create(userData)

    // Assert (断言)
    expect(result).toEqual(mockUser)
    expect(userRepository.create).toHaveBeenCalledWith(userData)
  })
})
```

### 测试覆盖范围

✅ **必须测试**
- 业务逻辑复杂的方法
- 错误处理逻辑
- 边界条件
- 安全相关功能

⚠️ **可选测试**
- 简单的 CRUD 操作
- 纯展示组件
- 配置文件

❌ **不建议测试**
- 第三方库
- 框架代码
- 自动生成的代码

---

## 🔗 相关资源

- [Coverlet 文档](https://github.com/coverlet-coverage/coverlet)
- [Vitest 文档](https://vitest.dev/)
- [Flutter 测试文档](https://docs.flutter.dev/testing)
- [Codecov 文档](https://docs.codecov.com/)

---

**最后更新**: 2026-04-06