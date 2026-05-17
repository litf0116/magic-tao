# UniApp 多平台开发架构方案

> **文档版本**: v1.0
> **创建日期**: 2026-03-12
> **适用项目**: 魔力淘 UniApp 项目
> **团队规模**: 单人开发

---

## 一、方案概述

### 1.1 核心决策

**采用方案**: 单代码库 + 条件编译

### 1.2 决策依据

| 考量因素 | 单代码库 | 独立代码库 | 选择理由 |
|---------|----------|------------|----------|
| 代码复用 | 70-80% | 30-50% | 单人团队必须最大化复用 |
| 维护成本 | 低 | 高 | 单人开发时间有限 |
| 开发效率 | 高 | 中 | 快速迭代需求 |
| 团队要求 | 低 | 高 | 单人开发，简单为上 |
| Bug修复 | 1次 | 2次 | 节省50%工作量 |

### 1.3 适用场景

- ✅ 小程序和App核心功能70%以上相同
- ✅ 单人开发团队
- ✅ 快速迭代需求
- ✅ App无审核限制（自建下载页）

---

## 二、架构设计

### 2.1 项目结构

```
molitao_uniapp/
├── src/
│   ├── pages/                    # 页面
│   ├── components/              # 组件
│   ├── api/                     # API接口
│   ├── store/                   # 状态管理
│   ├── utils/                   # 工具函数
│   │   ├── feature.ts           # 功能开关（新增）
│   │   ├── platform.ts          # 平台判断（新增）
│   │   └── request.ts           # 网络请求
│   ├── types/                   # 类型定义
│   ├── App.vue
│   ├── main.ts
│   ├── pages.json               # 页面配置
│   └── manifest.json            # 应用配置
├── docs/                        # 文档
│   ├── UniApp多平台开发架构方案.md
│   ├── 用户协议.md
│   └── 隐私政策.md
└── package.json
```

### 2.2 核心文件说明

| 文件 | 作用 | 状态 |
|------|------|------|
| `utils/feature.ts` | 功能开关配置 | ✅ 必需 |
| `utils/platform.ts` | 平台判断工具 | ✅ 必需 |
| `package.json` | 构建脚本配置 | ⚠️ 需更新 |

---

## 三、核心配置文件

### 3.1 功能开关配置

**文件**: `src/utils/feature.ts`

```typescript
/**
 * 功能开关配置
 * 用于管理小程序和App的功能差异
 * 小程序审核敏感功能可在此控制
 */

export const FeatureConfig = {
  // ========== 通用功能（所有平台都可用） ==========

  // 聊天功能
  chat: true,
  privateChat: true,
  groupChat: true,
  friendAdd: true,

  // 商品功能
  goodsList: true,
  goodsDetail: true,
  goodsFavorite: true,

  // 帖子功能
  postList: true,
  postDetail: true,
  postPublish: true,
  postComment: true,

  // 用户功能
  userCenter: true,
  userInfo: true,
  userSetting: true,

  // 支付功能
  payment: true,
  orderList: true,

  // 消息推送
  push: true,

  // ========== 小程序限制功能（审核敏感） ==========

  #ifdef MP-WEIXIN
  // 以下功能因小程序审核问题暂时隐藏
  auction: false,      // 拍卖/秒杀功能
  trading: false,      // 直接交易功能
  voiceMessage: false, // 语音消息
  #endif

  // ========== App 专属功能（可开放） ==========

  #ifdef APP-PLUS
  auction: true,       // 拍卖/秒杀功能
  trading: true,       // 直接交易功能
  voiceMessage: true,  // 语音消息
  #endif
}

/**
 * 判断功能是否可用
 */
export function isFeatureEnabled(feature: keyof typeof FeatureConfig): boolean {
  return FeatureConfig[feature] === true
}
```

**使用说明**:
- 所有功能开关在此集中管理
- 使用条件编译控制平台差异
- 新增功能时在此添加开关

---

### 3.2 平台判断工具

**文件**: `src/utils/platform.ts`

```typescript
/**
 * 平台判断工具
 */

export const Platform = {
  name: 'unknown',
  isMiniprogram: false,
  isApp: false,
  isH5: false
}

// #ifdef MP-WEIXIN
Platform.name = 'miniprogram'
Platform.isMiniprogram = true
// #endif

// #ifdef APP-PLUS
Platform.name = 'app'
Platform.isApp = true
// #endif

// #ifdef H5
Platform.name = 'h5'
Platform.isH5 = true
// #endif

/**
 * 是否为小程序
 */
export function isMiniprogram(): boolean {
  return Platform.isMiniprogram
}

/**
 * 是否为App
 */
export function isApp(): boolean {
  return Platform.isApp
}

/**
 * 是否为H5
 */
export function isH5(): boolean {
  return Platform.isH5
}

/**
 * 获取当前平台名称
 */
export function getPlatform(): 'miniprogram' | 'app' | 'h5' | 'unknown' {
  return Platform.name as any
}
```

**使用说明**:
- 统一的平台判断接口
- 避免散落各处重复判断
- 新增平台时只需修改此文件

---

### 3.3 构建脚本配置

**文件**: `package.json`

```json
{
  "scripts": {
    "dev:mp": "uni -p mp-weixin",
    "dev:app-android": "uni -p app-android",
    "dev:app-ios": "uni -p app-ios",
    "build:mp": "uni build -p mp-weixin",
    "build:app-android": "uni build -p app-android",
    "build:app-ios": "uni build -p app-ios"
  }
}
```

---

## 四、代码规范

### 4.1 条件编译使用规范

#### 页面级条件编译

```vue
<template>
  <view class="container">
    <!-- 通用内容 -->
    <text>通用内容</text>

    <!-- App专属功能 -->
    #ifdef APP-PLUS
    <!-- App专属：拍卖功能（小程序审核敏感） -->
    <button @click="goToAuction">参与拍卖</button>
    #endif
  </view>
</template>
```

**规范**:
- ✅ 条件编译处添加注释说明
- ✅ 注释中说明平台差异原因
- ✅ 保持代码缩进清晰

---

#### 组件级条件编译

```vue
<template>
  <view class="chat-container">
    <!-- 语音消息 -->
    #ifdef APP-PLUS
    <button v-if="FeatureConfig.voiceMessage" @click="startRecord">
      🎤 语音
    </button>
    #endif

    #ifdef MP-WEIXIN
    <!-- 小程序：语音功能暂未开放 -->
    <button disabled>🚫 语音功能暂未开放</button>
    #endif
  </view>
</template>
```

---

#### API级条件编译

```typescript
// api/trading.ts
import { request } from './request'

/**
 * 获取拍卖列表
 */
export function getAuctionList() {
  #ifdef MP-WEIXIN
  // 小程序：返回空列表
  return Promise.resolve({ data: { list: [], total: 0 } })
  #endif

  #ifdef APP-PLUS
  // App：返回拍卖列表
  return request({
    url: '/api/auction/list'
  })
  #endif
}
```

---

#### 条件编译最佳实践

| 场景 | ✅ 推荐 | ❌ 不推荐 |
|------|---------|----------|
| 功能开关 | `FeatureConfig.auction` | 散落各处的if判断 |
| 平台判断 | `isApp()` | `#ifdef APP-PLUS` 在逻辑中 |
| 隐藏功能 | 条件编译整个UI块 | 用CSS隐藏 |
| API差异 | 条件编译返回不同结果 | 返回后前端判断 |

---

### 4.2 功能开关使用规范

```typescript
import { FeatureConfig, isFeatureEnabled } from '@/utils/feature'

// 方式1：直接访问配置
if (FeatureConfig.auction) {
  // 拍卖功能
}

// 方式2：使用工具函数
if (isFeatureEnabled('auction')) {
  // 拍卖功能
}
```

**规范**:
- ✅ 所有平台差异在 `FeatureConfig` 中定义
- ✅ 新增功能时同步更新配置
- ✅ 审核敏感功能及时标记

---

### 4.3 平台判断使用规范

```typescript
import { isApp, isMiniprogram, getPlatform } from '@/utils/platform'

// 方式1：布尔判断
if (isApp()) {
  // App逻辑
}

// 方式2：平台名称
const platform = getPlatform()
if (platform === 'app') {
  // App逻辑
}
```

**规范**:
- ✅ 统一使用工具函数
- ✅ 避免直接访问 `Platform` 对象
- ✅ 逻辑代码中使用工具函数，条件编译在编译时处理

---

## 五、开发流程

### 5.1 日常开发工作流

```bash
# 1. 开发小程序功能
npm run dev:mp

# 2. 测试小程序
# 在微信开发者工具中测试

# 3. 切换到App开发（Ctrl+C 停止后）
npm run dev:app-android

# 4. 测试App
# 在真机或模拟器中测试
```

### 5.2 新功能开发流程

```
1. 确定功能适用平台
   ↓
2. 在 FeatureConfig 中添加开关
   ↓
3. 开发通用功能
   ↓
4. 添加平台差异处理（条件编译）
   ↓
5. 测试小程序
   ↓
6. 测试App
   ↓
7. 提交代码
```

### 5.3 Bug修复流程

```
1. 定位Bug所在文件
   ↓
2. 修复代码
   ↓
3. 检查是否有平台差异
   ↓
4. 测试小程序
   ↓
5. 测试App
   ↓
6. 提交修复
```

---

## 六、平台差异记录

### 6.1 小程序限制的功能

| 功能 | 原因 | 代码位置 |
|------|------|----------|
| 拍卖/秒杀 | 审核敏感 | `feature.ts` line 42 |
| 直接交易 | 审核敏感 | `feature.ts` line 43 |
| 语音消息 | 审核敏感 | `feature.ts` line 44 |

### 6.2 App专属功能

| 功能 | 说明 | 代码位置 |
|------|------|----------|
| 拍卖/秒杀 | 完整拍卖功能 | `feature.ts` line 50 |
| 直接交易 | 完整交易功能 | `feature.ts` line 51 |
| 语音消息 | 完整语音功能 | `feature.ts` line 52 |

---

## 七、构建和发布

### 7.1 构建命令

| 命令 | 用途 | 输出目录 |
|------|------|----------|
| `npm run dev:mp` | 小程序开发 | `dist/dev/mp-weixin` |
| `npm run build:mp` | 小程序构建 | `dist/build/mp-weixin` |
| `npm run dev:app-android` | Android开发 | `dist/dev/app-android` |
| `npm run build:app-android` | Android构建 | `dist/build/app-android` |
| `npm run dev:app-ios` | iOS开发 | `dist/dev/app-ios` |
| `npm run build:app-ios` | iOS构建 | `dist/build/app-ios` |

### 7.2 发布流程

#### 小程序发布

```bash
# 1. 构建小程序
npm run build:mp

# 2. 打开微信开发者工具
# 3. 上传代码
# 4. 提交审核
```

#### App发布

```bash
# 1. 构建 Android APK
npm run build:app-android

# 2. 打包 APK
# 3. 上传到自建下载页

# 或 iOS
npm run build:app-ios
# 打包 IPA
# 上传到 App Store（如需）
```

---

## 八、注意事项

### 8.1 避免的坑

| 场景 | ✅ 正确做法 | ❌ 错误做法 |
|------|-----------|----------|
| 功能差异 | 使用条件编译 | 写两套组件 |
| 功能开关 | 集中在 `feature.ts` | 散落在各处 |
| 平台判断 | 使用工具函数 | 重复写判断逻辑 |
| 代码注释 | 说明平台差异原因 | 无注释 |
| 测试 | 每次改动测试两平台 | 只测单一平台 |

### 8.2 性能优化

- ✅ 使用条件编译减少无用代码
- ✅ 图片按平台加载不同尺寸
- ✅ API按平台返回不同数据量
- ✅ 懒加载非关键组件

### 8.3 安全规范

- ✅ 敏感数据不在前端存储
- ✅ 使用HTTPS传输
- ✅ 支付逻辑后端验证
- ✅ 用户信息加密传输

---

## 九、单人团队优化建议

### 9.1 最大化代码复用

| 复用率 | 模块 | 说明 |
|--------|------|------|
| 100% | 核心API | 用户、商品、聊天API完全复用 |
| 100% | 状态管理 | Pinia store完全复用 |
| 100% | 工具函数 | 请求、验证等工具复用 |
| 90% | 通用组件 | 按钮、卡片等组件复用 |
| 70% | 页面逻辑 | 首页、个人中心等复用 |

### 9.2 月度工作量对比

| 维护项 | 单代码库 | 独立代码库 | 节省时间 |
|--------|----------|------------|----------|
| Bug修复 | 5小时/月 | 10小时/月 | 5小时 |
| 功能更新 | 10小时/月 | 20小时/月 | 10小时 |
| 接口调整 | 3小时/月 | 6小时/月 | 3小时 |
| 代码同步 | 0小时/月 | 5小时/月 | 5小时 |
| **总计** | **18小时/月** | **41小时/月** | **23小时/月** |

### 9.3 快速开发技巧

1. **使用快捷命令**
   - 配置npm scripts快捷命令
   - 使用shell脚本自动化

2. **模板化代码**
   - 创建条件编译模板
   - 统一代码风格

3. **文档先行**
   - 新功能先写文档
   - 记录平台差异

---

## 十、实施清单

### 10.1 立即执行（15分钟）

| 任务 | 文件 | 时间 | 状态 |
|------|------|------|------|
| 创建功能配置 | `src/utils/feature.ts` | 5分钟 | ⏳ |
| 创建平台工具 | `src/utils/platform.ts` | 5分钟 | ⏳ |
| 更新构建脚本 | `package.json` | 5分钟 | ⏳ |

### 10.2 渐进式改造（按需）

| 任务 | 时间 | 优先级 |
|------|------|--------|
| 页面级条件编译改造 | 2-3小时 | P0 |
| API级条件编译改造 | 1-2小时 | P1 |
| 组件级条件编译改造 | 按需 | P2 |
| 性能优化 | 按需 | P3 |

### 10.3 不需要做的事情

- ❌ 不改动现有界面代码
- ❌ 不创建独立代码库
- ❌ 不进行大规模重构
- ❌ 不一次性改完所有代码

---

## 十一、常见问题

### Q1: 为什么不创建独立代码库？

**A**:
- 单人团队维护两套代码成本高
- Bug修复、功能更新需要两倍工作量
- 代码同步容易出错
- 小程序和App核心功能70%以上相同

### Q2: 条件编译会让代码变复杂吗？

**A**:
- 条件编译是UniApp原生支持的功能
- 只在平台差异处使用，不影响通用代码
- 通过注释和工具函数保持代码清晰
- 实际上比维护两套代码简单得多

### Q3: 什么时候考虑分家？

**A**:
- 功能差异度 >60%
- 团队规模 >10人
- 各版本独立发布周期 >2周
- 条件编译代码过于复杂

### Q4: 如何确保两平台功能一致？

**A**:
- 所有平台差异在 `FeatureConfig` 中定义
- 使用条件编译明确标注
- 每次改动测试两平台
- 定期审核平台差异

---

## 十二、参考资料

### 官方文档
- [UniApp 条件编译](https://uniapp.dcloud.net.cn/tutorial/platform.html)
- [UniApp 平台差异说明](https://uniapp.dcloud.net.cn/tutorial/platform.html#platform-conditions)

### 项目文档
- `docs/用户协议.md`
- `docs/隐私政策.md`
- `docs/微信开放平台应用申请资料.md`

---

## 十三、更新记录

| 日期 | 版本 | 更新内容 |
|------|------|----------|
| 2026-03-12 | v1.0 | 初始版本 |

---

**文档维护**: 单人开发团队
**审核**: 项目负责人
**生效日期**: 2026-03-12