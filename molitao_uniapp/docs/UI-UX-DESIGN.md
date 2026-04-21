# 魔力淘小程序 UI/UX 设计规范

## 1. 设计理念

魔力淘是一个微信小程序电商平台，主打魔力值交易功能。设计风格追求**简洁实用**，以白色和浅灰色为基底，用珊瑚橙(`#f4835a`)作为品牌强调色。

---

## 2. 色彩系统

### 品牌色
| 名称 | 色值 | 用途 |
|------|------|------|
| 品牌橙 | `#f4835a` | 主按钮、强调元素、Tab选中态 |
| 品牌橙深 | `#ff7144` | 拍卖出价、价格相关 |
| 暖棕 | `#935F4E` | 竞拍商品名称 |

### 背景色
| 名称 | 色值 | 用途 |
|------|------|------|
| 页面背景 | `#f6f6f6` | 页面底色 |
| 卡片背景 | `white` | 卡片、列表项背景 |
| 输入框背景 | `#f6f6f6` | 图标背景、输入框底色 |

### 文字色
| 名称 | 色值 | 用途 |
|------|------|------|
| 主文字 | `#171717` | 标题、重要文字 |
| 次级文字 | `#666666` 或 `text-gray-600` | 正文、标签 |
| 辅助文字 | `#999999` 或 `text-gray-400` | 占位符、次要提示 |
| 浅文字 | `#ccc` 或 `text-gray-300` | 版本号、分割线 |

### 功能色
| 名称 | 色值 | 用途 |
|------|------|------|
| 成功绿 | - | 成功提示（使用 uni.showToast） |
| 错误红 | `#ef4444` 或 `bg-red-500` | 错误状态 |
| 边框灰 | `border-gray-100` | 列表项分割线 |

---

## 3. 字体系统

### 字号层级
| 名称 | 类名 | 用途 |
|------|------|------|
| 大标题 | `text-lg` (18px) | 数字统计、卡片标题 |
| 正文 | `text-base` (16px) | 输入框、列表内容 |
| 辅助 | `text-sm` (14px) | 标签、说明文字 |
| 最小 | `text-xs` (12px) | 版本号、底部提示 |

### 字体权重
| 名称 | 类名 | 用途 |
|------|------|------|
| 常规 | `font-400` | 正文 |
| 中等 | `font-500` | 按钮文字、标签 |
| 加粗 | `font-700` | 重要标题、价格 |

---

## 4. 间距系统

基于 UnoCSS/Tailwind CSS 的 4px 网格系统：

| 名称 | 类名 | 数值 | 用途 |
|------|------|------|------|
| xs | `space-y-1` / `m-1` | 4px | 紧凑元素间距 |
| sm | `space-y-2` / `m-2` | 8px | 小间距 |
| md | `space-y-3-4` / `m-3` / `p-3` | 12-16px | 标准间距 |
| lg | `space-y-4` / `m-4` / `p-4` | 16px | 卡片内间距 |
| xl | `my-4` / `mx-4` | 16px | 区块间距 |
| 2xl | `my-6` / `mb-6` | 24px | 大区块间距 |

### 页面边距
- 标准页面水平边距：`px-4` (16px)
- 卡片内边距：`p-4` (16px)
- 列表项内边距：`p-2` (8px) 或 `p-3` (12px)

---

## 5. 圆角系统

| 名称 | 类名 | 值 | 用途 |
|------|------|------|------|
| 小圆角 | `rounded` / `rounded-lg` | 8px | 按钮、输入框 |
| 中圆角 | `rounded-4` | 16px | 卡片 |
| 大圆角 | `rounded-6` | 24px | 大按钮 |
| 全圆 | `rounded-full` | 50% | 头像、图标圆形背景 |

---

## 6. 阴影系统

| 名称 | 类名 | 用途 |
|------|------|------|
| 卡片阴影 | `shadow` 或 `shadow-lg` | 列表卡片、弹窗 |
| 无阴影 | 移除 shadow | 扁平列表项 |

---

## 7. 图标系统

### 图标库
使用 **IconPark** (icones.js.org)，通过 UnoCSS 的 i- 前缀引入：

```
i-solar:*** - Solar 图标库
i-icon-park-outline:*** - IconPark 轮廓图标
```

### 常用图标
| 用途 | 图标名 | 示例 |
|------|--------|------|
| 设置 | `i-solar:settings-linear` | 用户卡片右上角 |
| 退出 | `i-solar:logout-3-bold` | 退出按钮 |
| 箭头右 | `i-solar:alt-arrow-right-bold` | 列表项箭头 |
| 认证 | `i-solar:verified-check-bold` | 认证标签 |

---

## 8. 按钮组件

### 主按钮（品牌橙）
```html
<button class="w-full bg-[#f4835a] text-white rounded-lg mb-4">
    确认绑定
</button>
```
- 背景：`bg-[#f4835a]`
- 文字：白色 `text-white`
- 圆角：`rounded-lg` (8px)
- 高度：默认 44px+
- 间距：`mb-4` 与下方元素分隔

### 次级按钮
```html
<button class="w-full mb-32 rounded-6">
    返回
</button>
```
- 背景：透明或白色
- 边框：无或 `border`
- 文字：`text-gray-600`
- 圆角：`rounded-6` (24px)

### 列表项按钮（卡片式）
```html
<view class="py-3 px-4 bg-white rounded-4 flex items-center justify-between">
    <view class="flex items-center">
        <view class="size-5 i-solar:logout-3-bold text-gray-400 mr-3"></view>
        <text class="text-sm text-gray-600">退出登录</text>
    </view>
    <view class="size-4 i-solar:alt-arrow-right-bold text-gray-300"></view>
</view>
```

---

## 9. 卡片组件

### 标准卡片
```html
<view class="myCard p-4 bg-white rounded-4">
    <!-- 卡片内容 -->
</view>
```
```css
.myCard {
    @apply bg-white rounded-4;
}
```

### 列表卡片（带阴影）
```html
<view class="bg-white p-4 rounded-2 shadow-lg">
    <!-- 内容 -->
</view>
```

---

## 10. 输入框组件

### 标准输入框
```html
<view class="flex items-center border-b border-gray-100 pb-3">
    <text class="text-gray-600 w-20">手机号</text>
    <input
        v-model="form.phoneNumber"
        type="number"
        placeholder="请输入手机号"
        class="flex-1 text-base"
    />
</view>
```

---

## 11. 页面布局模式

### 标准页面结构
```
<view class="px-4 bg-[#f6f6f6] min-h-screen">
    <!-- 顶部卡片（如用户信息卡片）-->
    <view class="myCard p-4">...</view>

    <!-- 区块标题 -->
    <view class="my-4 flex items-center">
        <view class="h-3 w-4px mr-2 bg-[#ccc] rounded-full"></view>
        <view>区块标题</view>
    </view>

    <!-- 内容网格或列表 -->
    <view class="grid grid-cols-2 gap-2">...</view>

    <!-- 底部操作按钮 -->
    <view class="my-4">
        <button>操作</button>
    </view>
</view>
```

### 登录/绑定页面结构
```
<view class="h-[100vh] px-4 relative flex flex-col">
    <!-- 顶部 Logo 区域 -->
    <view class="flex-1 flex flex-col items-center flex-center">
        <image class="h-[10vh]" />
        <text class="font-bold text-lg">标题</text>
    </view>

    <!-- 底部表单区域 -->
    <view class="w-full">
        <view class="bg-white rounded-lg p-4 mb-4">
            <!-- 表单项 -->
        </view>
        <button class="w-full bg-[#f4835a] text-white rounded-lg mb-4">
            确认
        </button>
    </view>
</view>
```

---

## 12. iOS 兼容性规范

### 文本必须用 `<text>` 包裹
```html
<!-- ✅ 正确 -->
<text class="text-sm">待秒杀</text>

<!-- ❌ 错误 -->
<view class="text-sm">待秒杀</view>
```

### 字体样式必须显式定义
```html
<!-- ✅ 正确 -->
<text class="text-sm font-500">已成交</text>

<!-- ❌ 错误 -->
<text class="">已成交</text>
```

---

## 13. 退出按钮设计规范

### 推荐样式（卡片列表式）
```html
<view
    class="py-3 px-4 bg-white rounded-4 flex items-center justify-between active:bg-gray-50"
    @tap="logout"
>
    <view class="flex items-center">
        <view class="size-5 i-solar:logout-3-bold text-gray-400 mr-3"></view>
        <text class="text-sm text-gray-600">退出登录</text>
    </view>
    <view class="size-4 i-solar:alt-arrow-right-bold text-gray-300"></view>
</view>
```

### 设计要点
- 位置：页面底部，版本号上方
- 样式：白色卡片背景，与列表项风格一致
- 图标：左侧退出图标（灰色）
- 箭头：右侧箭头指示（可选）
- 点击反馈：`active:bg-gray-50`
- 间距：`mx-4` 水平边距，与页面边距一致

---

## 14. 状态设计

### 加载状态
- 按钮禁用：`disabled` + 文字变为"加载中..."
- 使用 `uni.showLoading` / `uni.hideLoading`

### 错误状态
- 使用 `uni.showToast` 图标为 `none`
- 文字颜色默认

### 空状态
- 使用灰色文字提示
- 可配合空状态图标

---

## 15. 组件命名约定

| 组件类型 | 命名方式 | 示例 |
|----------|----------|------|
| 页面 | PascalCase | `UserInfo.vue` |
| 组件 | kebab-case | `custom-modal.vue` |
| 样式类 | UnoCSS | `px-4`, `text-lg` |
| 图标 | i-{library}:{name} | `i-solar:settings-linear` |

---

## 附录：色值速查表

```
品牌橙:   #f4835a  (主按钮、强调)
品牌橙深: #ff7144  (出价、价格)
暖棕:     #935F4E  (商品名称)
页面背景: #f6f6f6  (页面底色)
主文字:   #171717  (标题)
次级文字: #666666  (正文)
辅助文字: #999999  (提示)
浅文字:   #cccccc  (分割线)
成功:     #22c55e  (可用 uni.showToast)
错误:     #ef4444  (错误状态)
```
