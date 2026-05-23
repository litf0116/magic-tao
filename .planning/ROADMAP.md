# Roadmap: 魔力淘 (MoliTao)

**Milestone:** v1.0 项目梳理与移动端发布
**Defined:** 2026-05-22
**Updated:** 2026-05-22 (Phase 2-5 restructured after Phase 1 audit)
**Phases:** 7 | **Requirements mapped:** 23/23 ✓

---

## Phase 1: 项目梳理与分析 ✓ 完成

**Goal:** 深入理解后端业务逻辑与数据模型，完成需求文档化，识别技术债与优化机会。

**Requirements:** ANAL-01, ANAL-02

**Success criteria:**
1. ✅ 核心业务流程（用户、认证、聊天、支付、推送）已梳理
2. ✅ 产出 5 份分析文档（database-schema / data-models / business-flows / api-inventory / tech-debt-log）
3. ✅ 识别 18 项技术债（2 critical / 7 high / 5 medium / 4 low）
4. ✅ Flutter App 代码审计确认：所有核心功能已实现，无需从零开发

---

## Phase 2: Flutter 发布准备与配置 (iOS + Android)

**Goal:** 完成 Flutter App iOS + Android 首次上架所需的全部平台配置工作。

**背景:** Phase 1 代码审计确认 Flutter App 所有核心功能已实现。本阶段不开发新功能，只做平台配置。

**需求:** CONFIG-01, CONFIG-02, CONFIG-03, CONFIG-04

**决策锚点（来自 discuss-phase）：**
- iOS + Android 同时首发
- 微信支付：保留 H5 引导，不接入 IAP
- iOS 端禁用 APK 版本更新弹窗
- 提现入口从 Profile 页面隐藏
- UI 范围：最小改动过审核

**Scope:**
1. iOS 发布证书 + Provisioning Profile 创建配置
2. Android 签名密钥（keystore）检查和配置
3. 极光推送 iOS 生产环境配置（上传 APNS Key 到极光控制台）
4. Universal Link 服务端 `apple-app-site-association` 文件配置验证
5. Info.plist / AndroidManifest.xml 审核合规检查
6. Release 构建验证（iOS Archive → IPA，Android build → APK）

---

## Phase 3: Flutter 审核前修复与代码清理 ✓ 完成

**Goal:** 根据 Phase 2 决策清单，执行最小的代码修改确保审核通过。

**需求:** CLEAN-01, CLEAN-02, CLEAN-03

**Scope:**
1. ✅ 禁用 iOS 端 APK 版本更新弹窗
2. ✅ 隐藏 Profile 页面提现入口（替换为"即将上线"提示）
3. ✅ 清理 `debugPrint` 调试日志残留（移除 6 处纯调试日志）
4. ✅ 修复编译警告（`flutter analyze`: 0 error, 0 warning）
5. ✅ 处理其他影响审核的问题

**Plans:** 2/2 complete

---

## Phase 4: App 商店素材与法律合规

**Goal:** 准备 App Store 和 Android 应用市场上架所需所有材料。

**需求:** STORE-01, STORE-02, STORE-03

**Scope:**
1. App Store / Google Play 截图（4 种尺寸）
2. 应用描述、关键词、宣传文本
3. 隐私政策（App 版本）
4. 用户协议（App 版本）
5. 审核备注说明

---

## Phase 5: Flutter 提审与上架

**Goal:** Flutter App 成功提交至 App Store 审核，并发布到 Android 应用市场。

**需求:** RELEASE-01, RELEASE-02, RELEASE-03

**Scope:**
1. iOS 构建 IPA + Transporter/App Store Connect 上传
2. App Store 审核提交与跟踪
3. Android 构建 APK/AAB + 应用市场上传
4. 审核被拒时快速响应修改
5. 发布后验证（生产环境下载安装测试）

---

## Phase 6: 微信小程序体验优化

**Goal:** 优化微信小程序 UI/UX 体验，修复已知 Bug，提升性能。

**需求:** MP-01, MP-02, MP-03

**Success criteria:**
1. 至少修复 3 个小程序已知 Bug（如空 catch、定时器未清理）
2. 页面加载速度明显提升
3. UI 一致性改善（与设计规范对齐）
4. 无新的 Console Error
5. 小程序提交微信审核通过

**Key context:** 小程序已上线运营（UniApp），与 Flutter App 共享后端 API，修改需谨慎。

---

## Phase 7: 里程碑收尾

**Goal:** 全功能回归测试，CHANGELOG 归档，文档完善，项目复盘。

**需求:** QA-01, QA-02, QA-03

**Scope:**
1. Flutter App (iOS+Android) 全功能回归测试
2. 微信小程序回归测试
3. CHANGELOG 更新
4. 项目文档归档
5. 技术债跟踪表更新
6. 里程碑复盘

---

## Phase Dependencies

```
Phase 1 (项目梳理) ──→ Phase 2 (发布配置) ──→ Phase 3 (审核修复)
                                                      │
                                                      ├──→ Phase 4 (商店素材)
                                                      │         │
                                                      │         └──→ Phase 5 (提审上架)
                                                      │
                                                      └──→ Phase 6 (小程序优化) ──→ Phase 7 (收尾)
```

**关键约束：**
- Phase 1 必须先完成（已 ✓ 完成）
- Phase 2 → Phase 3 → Phase 4 → Phase 5 顺序执行（Flutter 发布流水线）
- Phase 6 可独立于 Phase 3-5 并行开始（小程序与 Flutter 无代码依赖）
- Phase 7 必须在 Phase 5 + Phase 6 都完成后才能开始

---

*Last updated: 2026-05-22 (restructured after Phase 1 audit — Flutter App feature-complete, roadmapped as release pipeline)*
