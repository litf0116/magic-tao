# Design System — 魔力淘

## Product Context

- **What this is:** 在线拍卖交易平台，专注于游戏虚拟物品的实时秒杀拍卖
- **Who it's for:** 游戏虚拟物品买卖双方、寻求快速交易的用户、社交电商爱好者
- **Space/industry:** 游戏交易、社交电商、在线拍卖
- **Project type:** 移动端应用（微信小程序 / H5 / APP）

---

## Aesthetic Direction

- **Direction:** Playful/Approachable — 活泼亲和，但不幼稚
- **Decoration level:** Intentional — 微妙的圆角和阴影营造亲和力，保持干净
- **Mood:** 热情、紧迫、可信赖。橙色传递秒杀的刺激感，同时保持交易的严肃性
- **Design goal:** 让用户在拍卖过程中感到兴奋和紧迫，同时信任平台的安全性

---

## Typography

### Font Family

| 角色 | 字体 | 字重 | 用途 |
|------|------|------|------|
| **Display/Title** | Noto Sans SC | Bold (700) | 页面标题、大标题 |
| **Body** | Noto Sans SC | Regular (400) | 正文、描述文字 |
| **UI/Labels** | Noto Sans SC | Medium (500) | 按钮文字、标签 |
| **Data/Price** | DIN Alternate / Noto Sans SC | Bold (700) | 价格、数字展示 |

### Font Scale

| 级别 | 字号 | 行高 | 用途 |
|------|------|------|------|
| Display | 32px | 1.2 | 启动页标题、大型 Banner |
| Title | 24px | 1.3 | 页面标题、卡片标题 |
| Heading | 20px | 1.4 | 区块标题、消息气泡标题 |
| Body | 16px | 1.5 | 正文内容、消息内容 |
| Caption | 14px | 1.5 | 辅助文字、描述 |
| Small | 12px | 1.4 | 标签、时间戳、次要信息 |

### Font Loading

```html
<!-- Google Fonts -->
<link href="https://fonts.googleapis.com/css2?family=Noto+Sans+SC:wght@400;500;700&display=swap" rel="stylesheet">
```

---

## Color

### Approach
Balanced — 主色强调关键操作，语义色传递状态，中性色构建层次

### Primary Colors

| 名称 | 色值 | 用途 |
|------|------|------|
| Primary | `#f4835a` | 主按钮、选中状态、强调文字、导航激活 |
| Primary Light | `#FFB088` | hover 状态、渐变辅助色 |
| Primary Dark | `#E6734A` | 按下状态、渐变辅助色 |

### Neutral Colors

| 名称 | 色值 | 用途 |
|------|------|------|
| Background | `#FAF1F0` | 页面背景（浅橙灰，聊天页使用） |
| Surface | `#FFFFFF` | 卡片背景、消息气泡、输入框 |
| Border | `#EEEEEE` | 分割线、边框 |
| Text Primary | `#1A1A1A` | 主要文字 |
| Text Secondary | `#666666` | 次要文字、描述 |
| Text Muted | `#999999` | 辅助文字、时间戳、占位符 |

### Semantic Colors

| 名称 | 色值 | 用途 |
|------|------|------|
| Success | `#4CAF50` | 成功提示、在线状态、已成交 |
| Warning | `#FF9800` | 警告提示、拍卖中状态 |
| Error | `#F44336` | 错误提示、删除按钮、价格 |
| Info | `#2196F3` | 信息提示、链接 |

### Dark Mode

```css
[data-theme="dark"] {
  --background: #1A1A1A;
  --surface: #2D2D2D;
  --text-primary: #FFFFFF;
  --text-secondary: #AAAAAA;
  --text-muted: #777777;
  --border: #3D3D3D;
}
```

**Dark Mode 策略**:
- 重新设计深色表面，而非简单反转
- Primary 色保持不变，确保品牌一致性
- 适当降低饱和度 10-20%

---

## Spacing

### Base Unit
**4px** — 所有间距为 4 的倍数

### Spacing Scale

| Token | 值 | 用途 |
|-------|-----|------|
| 2xs | 4px | 图标与文字间距 |
| xs | 8px | 紧凑元素间距 |
| sm | 12px | 消息气泡内边距 |
| md | 16px | 卡片内边距、列表项间距 |
| lg | 24px | 区块间距 |
| xl | 32px | 页面内边距 |
| 2xl | 48px | 大区块间距 |
| 3xl | 64px | 页面底部留白 |

### Component Spacing

| 组件 | Padding | Margin |
|------|---------|--------|
| Button | 12px 24px | - |
| Card | 16px | 16px |
| Message Bubble | 12px 16px | 4px vertical |
| Input | 12px 16px | 12px bottom |
| List Item | 12px 16px | - |

---

## Layout

### Approach
Grid-disciplined — 数据密集型界面需要清晰的信息层级

### Grid System

| 断点 | 列数 | 间距 | 页边距 |
|------|------|------|--------|
| Mobile (< 600px) | 4 | 16px | 16px |
| Tablet (600-1024px) | 8 | 24px | 24px |
| Desktop (> 1024px) | 12 | 24px | 32px |

### Max Content Width
**1200px** — 内容区域最大宽度

### Border Radius

| Token | 值 | 用途 |
|-------|-----|------|
| sm | 4px | 小按钮、标签、消息气泡尖角 |
| md | 8px | 输入框、卡片内部元素 |
| lg | 12px | 卡片、按钮、消息气泡 |
| xl | 16px | 大卡片、弹窗 |
| full | 999px | 圆形头像、药丸按钮 |

---

## Components

### Button

**Primary Button**
```css
.btn-primary {
  background: #f4835a;
  color: #FFFFFF;
  padding: 12px 24px;
  border-radius: 999px;
  font-weight: 500;
  font-size: 14px;
}
.btn-primary:hover { background: #E6734A; }
.btn-primary:active { background: #D6633A; }
```

**Secondary Button**
```css
.btn-secondary {
  background: #FAF1F0;
  color: #1A1A1A;
  border: 1px solid #EEEEEE;
  /* ... */
}
```

**Ghost Button**
```css
.btn-ghost {
  background: transparent;
  color: #f4835a;
  /* ... */
}
```

### Message Bubble

**Received (Left)**
```css
.message-received {
  background: #FFFFFF;
  color: #1A1A1A;
  border-radius: 12px 12px 12px 4px;
  padding: 12px 16px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
```

**Sent (Right)**
```css
.message-sent {
  background: #f4835a;
  color: #FFFFFF;
  border-radius: 12px 12px 4px 12px;
  padding: 12px 16px;
}
```

### Card

```css
.card {
  background: #FFFFFF;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}
```

### Input

```css
.input {
  background: #FAF1F0;
  border: 1px solid #EEEEEE;
  border-radius: 8px;
  padding: 12px 16px;
  font-size: 14px;
  color: #1A1A1A;
}
.input:focus {
  border-color: #f4835a;
  outline: none;
}
.input::placeholder {
  color: #999999;
}
```

### Tag

```css
.tag {
  padding: 4px 12px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 500;
}
.tag-primary { background: #f4835a; color: white; }
.tag-success { background: rgba(76,175,80,0.1); color: #4CAF50; }
.tag-warning { background: rgba(255,152,0,0.1); color: #FF9800; }
.tag-error { background: rgba(244,67,54,0.1); color: #F44336; }
```

---

## Motion

### Approach
Minimal-functional — 状态转换流畅但不打扰，适合交易场景

### Easing

| 类型 | 缓动函数 | 用途 |
|------|----------|------|
| Enter | `ease-out` | 元素出现 |
| Exit | `ease-in` | 元素消失 |
| Move | `ease-in-out` | 位置变化 |

### Duration

| 类型 | 时长 | 用途 |
|------|------|------|
| Micro | 50-100ms | 按钮点击反馈 |
| Short | 150-250ms | 页面切换、展开收起 |
| Medium | 250-400ms | 弹窗出现 |
| Long | 400-700ms | 复杂动画 |

### Animation Examples

**Button Press**
```css
.btn:active {
  transform: scale(0.98);
  transition: transform 50ms ease-out;
}
```

**Message Appear**
```css
@keyframes message-in {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
.message-bubble {
  animation: message-in 150ms ease-out;
}
```

---

## Icons

### Icon Size

| 级别 | 尺寸 | 用途 |
|------|------|------|
| xs | 16px | 行内图标 |
| sm | 20px | 按钮图标、列表图标 |
| md | 24px | 导航图标、Tab 图标 |
| lg | 32px | 功能入口图标 |
| xl | 48px | 空状态图标 |

### Icon Style
- 使用 Material Icons 或自定义图标
- 颜色与上下文一致
- 保持视觉重量平衡

---

## Mobile-Specific Patterns

### Tab Bar

- 高度: 56px + SafeArea
- 图标: 24px
- 文字: 12px
- 选中色: `#f4835a`
- 未选中色: `#999999`

### Navigation Bar

- 高度: 44px + StatusBar
- 背景色: `#f4835a`
- 文字色: `#FFFFFF`
- 标题字号: 17px Bold

### Safe Areas

```css
/* iOS */
padding-top: env(safe-area-inset-top);
padding-bottom: env(safe-area-inset-bottom);

/* Android */
padding-top: 24px; /* Status bar height */
padding-bottom: 0; /* Navigation bar handled by system */
```

---

## Accessibility

### Color Contrast
- 正文文字与背景对比度 ≥ 4.5:1
- 大标题与背景对比度 ≥ 3:1
- 交互元素对比度 ≥ 3:1

### Touch Targets
- 最小点击区域: 44px × 44px
- 按钮间距: 8px minimum

### Focus States
- 所有交互元素需有明显的 focus 状态
- 使用 Primary 色作为 focus ring

---

## Design Tokens Summary

```css
:root {
  /* Colors */
  --primary: #f4835a;
  --primary-light: #FFB088;
  --primary-dark: #E6734A;
  --background: #FAF1F0;
  --surface: #FFFFFF;
  --border: #EEEEEE;
  --text-primary: #1A1A1A;
  --text-secondary: #666666;
  --text-muted: #999999;
  --success: #4CAF50;
  --warning: #FF9800;
  --error: #F44336;
  --info: #2196F3;
  
  /* Spacing */
  --space-2xs: 4px;
  --space-xs: 8px;
  --space-sm: 12px;
  --space-md: 16px;
  --space-lg: 24px;
  --space-xl: 32px;
  --space-2xl: 48px;
  --space-3xl: 64px;
  
  /* Border Radius */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-xl: 16px;
  --radius-full: 999px;
  
  /* Typography */
  --font-family: 'Noto Sans SC', sans-serif;
  --font-display: 32px;
  --font-title: 24px;
  --font-heading: 20px;
  --font-body: 16px;
  --font-caption: 14px;
  --font-small: 12px;
  
  /* Shadows */
  --shadow-sm: 0 1px 2px rgba(0,0,0,0.05);
  --shadow-md: 0 4px 6px rgba(0,0,0,0.1);
  --shadow-lg: 0 10px 15px rgba(0,0,0,0.1);
}
```

---

## Decisions Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-03-30 | 初始设计系统创建 | 基于 PRD.md 产品需求，创建完整的设计规范 |
| 2026-03-30 | 主色 #f4835a | 延续现有品牌色，传递热情与紧迫感 |
| 2026-03-30 | Noto Sans SC 字体 | 中文友好、免费商用、Google Fonts 可用 |
| 2026-03-30 | 圆角设计 | 亲和力强，适合游戏交易社区氛围 |
| 2026-03-30 | 4px 基础间距单位 | 与 8px grid 系统兼容，灵活性高 |

---

## Figma/Sketch 使用指南

### 新建设计文件

1. **Frame 尺寸**:
   - 移动端: 375 × 812px (iPhone X)
   - 小程序: 375 × 812px
   - 平板: 768 × 1024px

2. **字体设置**:
   - 主要字体: Noto Sans SC
   - 确保安装字体后使用

3. **颜色设置**:
   - 创建 Color Styles，命名与 Design Tokens 一致
   - 使用 CSS Variables 中的色值

4. **组件库**:
   - Button (Primary/Secondary/Ghost)
   - Input (Default/Focus/Error)
   - Message Bubble (Sent/Received)
   - Card
   - Tag
   - Auction Item

### 导出规范

- 图标: SVG 格式，1x/2x/3x
- 图片: PNG/JPEG, 2x 默认
- 切图命名: `模块_功能_状态@倍率.png`

---

**文档版本**: v1.0  
**更新日期**: 2026-03-30  
**维护者**: 设计团队