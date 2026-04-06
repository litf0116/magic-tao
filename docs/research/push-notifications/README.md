# 推送通知技术方案研究

## 📋 研究范围

本文档涵盖以下技术栈的推送通知实现方案：
- **后端**: .NET 8 (ABP Framework)
- **前端**: UniApp (Vue 3 + TypeScript)
- **目标平台**: iOS 和 Android

## 🎯 研究目标

1. iOS APNs 和 Android FCM 的实现原理
2. .NET 后端集成 APNs/FCM 的库和最佳实践
3. UniApp App 端接收推送通知的 API 和配置
4. 推送通知权限申请和管理
5. 推送消息的数据格式和处理
6. 本地推送 vs 远程推送的使用场景
7. 第三方推送服务（极光、个推等）对比

## 📁 文档结构

```
research/push-notifications/
├── README.md                      # 本文件
├── docs/
│   ├── 01-ios-apns.md             # iOS APNs 技术文档
│   ├── 02-android-fcm.md          # Android FCM 技术文档
│   ├── 03-dotnet-backend.md       # .NET 后端集成方案
│   ├── 04-uniapp-frontend.md      # UniApp 前端实现方案
│   ├── 05-permission-management.md # 权限管理
│   ├── 06-message-format.md       # 消息格式与处理
│   └── 07-third-party-services.md # 第三方服务对比
├── examples/
│   ├── apns-example.cs            # APNs C# 示例
│   ├── fcm-example.cs             # FCM C# 示例
│   └── uniapp-push-example.ts     # UniApp 推送示例
└── architecture/
    ├── system-design.md           # 系统架构设计
    └── data-flow.md               # 数据流程设计
```

## 🔍 研究发现

### 后端库推荐

1. **dotAPNS** - APNs 专用库
   - GitHub: https://github.com/alexalok/dotAPNS
   - 语言: C#
   - 许可: Apache License 2.0
   - 最后更新: 2026-02-09
   - 特点: 专注 APNs，使用 HTTP/2 API

2. **net-core-push-notifications** - 轻量级多平台库
   - GitHub: https://github.com/andrei-m-code/net-core-push-notifications
   - 语言: C#
   - 许可: MIT License
   - 最后更新: 2026-02-25
   - 特点: 支持 Android, iOS 和 Web，轻量级设计

3. **PushSharp** - 全平台推送库
   - GitHub: https://github.com/Redth/PushSharp
   - 语言: C#
   - 最后更新: 2026-03-05
   - 特点: 支持多平台（iOS, Android, Windows, Amazon, Blackberry）

## 📊 技术选型建议

基于项目需求，推荐方案：

### 方案 A：原生集成（推荐用于自主可控场景）
- **iOS**: 使用 dotAPNS 直接调用 APNs
- **Android**: 使用 Firebase Admin SDK
- **UniApp**: 使用官方推送插件

### 方案 B：统一封装（推荐用于快速开发）
- 使用 net-core-push-notifications 统一管理 iOS/Android 推送
- 减少维护成本，支持多平台

### 方案 C：第三方服务（推荐用于国内业务）
- **极光推送**: 国内市场份额大，文档完善
- **个推**: 功能全面，支持厂商通道
- **优点**: 无需海外服务器，厂商通道保活
- **缺点**: 有成本，数据掌握在第三方

## 🚀 下一步行动

1. 详细阅读各平台技术文档（docs/ 目录）
2. 查看代码示例（examples/ 目录）
3. 理解系统架构设计（architecture/ 目录）
4. 根据项目需求选择合适方案
5. 实施开发计划

## 📝 更新日志

- **2026-03-07**: 创建研究框架，确定技术选型方向
