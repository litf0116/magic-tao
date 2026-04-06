# 魔力淘 Flutter App

## 项目概述

魔力淘 - 在线拍卖交易平台，使用 Flutter 开发，支持 iOS、Android、Web 三端。

## 快速命令

```bash
# 获取依赖
flutter pub get

# 运行开发服务器
flutter run

# 构建生产版本
flutter build apk --release
flutter build ios --release

# 代码分析
flutter analyze

# 运行测试
flutter test
```

## 项目结构

```
lib/
├── core/           # 核心功能
│   ├── router/     # 路由配置
│   ├── theme/      # 主题配置
│   └── widgets/    # 通用组件
├── data/           # 数据层
│   ├── api/        # API 客户端
│   ├── models/     # 数据模型
│   ├── repositories/ # 仓库层
│   └── services/   # 服务层
├── presentation/   # 表现层
│   ├── pages/      # 页面
│   ├── providers/  # 状态管理
│   └── widgets/    # 业务组件
└── main.dart       # 入口
```

## 技术栈

- Flutter 3.x + Dart 3.x
- flutter_riverpod - 状态管理
- go_router - 路由
- dio - 网络请求
- cached_network_image - 图片缓存

## 相关文档

- **PRD.md** - 产品需求文档，包含完整功能说明
- **DESIGN.md** - 设计规范文档，包含设计系统规范

## Design System

Always read DESIGN.md before making any visual or UI decisions.
All font choices, colors, spacing, and aesthetic direction are defined there.
Do not deviate without explicit user approval.
In QA mode, flag any code that doesn't match DESIGN.md.

### 设计预览

设计预览页面位于 `/tmp/molitao-design-preview.html`，包含：
- 调色板展示
- 字体规范
- 按钮组件
- 表单元素
- 聊天组件
- 拍卖组件
- 间距系统
- 圆角规范

## 主色调

- Primary: `#f4835a` (橙色)
- Background: `#FAF1F0` (浅橙灰)
- Success: `#4CAF50`
- Warning: `#FF9800`
- Error: `#F44336`