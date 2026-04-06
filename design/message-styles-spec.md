# UniApp 消息类型与样式规范

## 一、消息类型枚举 (ChatMessageType)

| 类型 | 说明 | 样式特征 |
|------|------|---------|
| `Text` | 文本消息 | 白色背景，圆角12rpx |
| `Image` | 图片消息 | 最大宽度300rpx，圆角12rpx |
| `File` | 文件消息 | - |
| `Receipt` | 回执消息 | - |
| `Welcome` | 欢迎消息 | 居中显示，系统样式 |
| `Goodbye` | 告别消息 | 居中显示，系统样式 |
| `BanUser` | 禁言消息 | 居中显示，系统样式 |
| `Backout` | 撤销消息 | 居中显示，系统样式 |
| `AuctionStart` | 开始秒杀 | 红色边框，浅红背景 |
| `AuctionBid` | 出价消息 | 橙色边框，渐变背景 |
| `AuctionEnd` | 秒杀结束 | 琥珀色边框，橙色背景 |
| `AuctionDeal` | 成交通知 | 绿色边框，浅绿背景 |
| `KasecStatusChanged` | 卡秒状态 | 渐变背景，闪电图标 |
| `Error` | 错误消息 | - |

---

## 二、消息样式详情

### 1. 文本消息 (TextMessage)

```css
.text-content {
    padding: 16rpx;
    border-radius: 12rpx;
    color: #000000;
    background: #ffffff;
    word-break: break-all;
    text-align: left;
    display: block;
}
```

**特点**：
- 白色背景
- 黑色文字
- 支持 Emoji 表情解码
- 支持换行显示

---

### 2. 图片消息 (ImageMessage)

```css
.image-content {
    border-radius: 12rpx;
    width: 300rpx;
    max-height: 300rpx;
}
```

**特点**：
- 固定宽度 300rpx
- 高度自适应（最大 150rpx 计算后）
- 点击可全屏预览
- 长按可收藏到表情

---

### 3. 开始秒杀消息 (AuctionStartMessage)

```css
.auction-start-message {
    border: 2px solid #ef4444;  /* 红色边框 */
    background: #fff5f5;        /* 浅红背景 */
    padding: 8px 16px;
    border-radius: 8px;
    position: relative;
    margin: 8px 0;
}
```

**布局**：
```
┌──────────────────────────────┐
│                    [开始秒杀] │ ← 右上角红色标签
│ 商品名称: XXX                 │
│ 商品描述内容...               │
└──────────────────────────────┘
```

**颜色**：
- 边框: `#ef4444` (red-500)
- 背景: `#fff5f5`
- 标签背景: `#ef4444`
- 标签文字: 白色

---

### 4. 出价消息 (AuctionBidMessage)

```css
.auction-bid-message {
    border: 2px solid #ff7144;  /* 橙色边框 */
    background: #ffb673;        /* 渐变橙色背景 */
    padding: 8px 16px;
    border-radius: 12px;
    position: relative;
    margin: 8px 0;
}
```

**布局**：
```
┌──────────────────────────────┐
│                        [出价] │ ← 右上角橙色标签
│ 商品名称: XXX                 │
│ 当前出价：￥128               │ ← 白色大字，24px
└──────────────────────────────┘
```

**颜色**：
- 边框: `#ff7144`
- 背景: `#ffb673`
- 标签背景: `#ff7144`
- 价格文字: 白色，24px，加粗

---

### 5. 秒杀结束消息 (AuctionEndMessage)

```css
.auction-end-message {
    border: 2px solid #ff9800;  /* 琥珀色边框 */
    padding: 8px 16px;
    border-radius: 8px;
    position: relative;
}
```

**布局**：
```
┌──────────────────────────────┐
│                   [成功秒杀] │ ← 右上角琥珀色标签
│ 恭喜 XXX 最终以 128魔力值    │ ← 红色文字
│ 秒得商品                      │
│ ┌────────────────────┐       │
│ │   商品名称         │       │ ← 橙色边框卡片
│ └────────────────────┘       │
│ 2026-03-31 10:30:00         │
│ 双方私聊秒杀主持确认交易!    │
│ 认准星标小心冒充             │
└──────────────────────────────┘
```

**颜色**：
- 边框: `#ff9800` (amber)
- 标签背景: `#ff9800`
- 商品卡片边框: `#ff9800`
- 商品卡片背景: `#ffb673`
- 价格文字: 红色

---

### 6. 成交通知消息 (AuctionDealMessage)

```css
.auction-deal-message {
    border: 2px solid #22c55e;  /* 绿色边框 */
    padding: 8px 16px;
    border-radius: 8px;
    position: relative;
}
```

**布局**：
```
┌──────────────────────────────┐
│                  [交易通知]  │ ← 右上角绿色标签
│ 恭喜您成功拍得商品！         │ ← 绿色文字
│ ┌────────────────────┐       │
│ │   商品名称         │       │ ← 绿色边框卡片
│ └────────────────────┘       │
│ 成交价: ￥128                │ ← 红色加粗
│ 2026-03-31 10:30:00         │
│ 请联系秒杀主持确认交易详情   │
│ 认准星标，小心冒充           │
└──────────────────────────────┘
```

**颜色**：
- 边框: `#22c55e` (green-500)
- 标签背景: `#22c55e`
- 商品卡片边框: `#22c55e`
- 商品卡片背景: `#86efac`
- 成功文字: `#16a34a`

---

### 7. 卡秒状态消息 (KasecStatusMessage)

```css
.kasec-status-message {
    display: flex;
    align-items: center;
    padding: 24rpx;
    border-radius: 20rpx;
    border: 4rpx solid;
    font-weight: 600;
    box-shadow: 0 4rpx 16rpx rgba(0, 0, 0, 0.1);
}

/* 卡秒开启状态 */
.kasec-enabled {
    background: linear-gradient(135deg, #fff5f5 0%, #fed7d7 100%);
    border-color: #e53e3e;
    color: #c53030;
}

/* 卡秒关闭状态 */
.kasec-disabled {
    background: linear-gradient(135deg, #f0fff4 0%, #c6f6d5 100%);
    border-color: #38a169;
    color: #2f855a;
}
```

**布局**：
```
┌────────────────────────────────┐
│ ⚡ 秒杀主持已开启卡秒模式...   │ ← 红色背景（开启）
└────────────────────────────────┘

┌────────────────────────────────┐
│ ⚡ 卡秒已关闭，恢复正常加价... │ ← 绿色背景（关闭）
└────────────────────────────────┘
```

**特点**：
- 闪电图标 ⚡ 有脉冲动画
- 渐变背景
- 开启状态：红色系
- 关闭状态：绿色系

---

### 8. 系统消息 (SystemMessage)

```css
.system-message {
    display: flex;
    justify-content: center;
    align-items: center;
    color: #888;
    font-size: 14px;
    margin: 10px 0;
    text-align: center;
    width: 100%;
}
```

**布局**：
```
────────────────────────────────
       欢迎来到秒杀场
────────────────────────────────
```

**特点**：
- 居中显示
- 灰色文字
- 无背景框

---

### 9. 欢迎消息 (WelcomeMessage)

**布局**：
```
────────────────────────────────
    欢迎 张三 进入秒杀场
────────────────────────────────
```

**特点**：
- 居中显示
- 显示用户名

---

## 三、消息项布局结构

### 普通消息布局

```html
<view class="message-item-content">
    <!-- 头像 -->
    <view class="avatar">
        <image src="用户头像"></image>
    </view>
    
    <!-- 消息内容 -->
    <view class="content">
        <!-- 用户名/标签 -->
        <view class="message-fromName">
            <span class="tag">管理员</span>
            用户名
        </view>
        
        <!-- 消息体 -->
        <view class="message-payload">
            <!-- 具体消息组件 -->
        </view>
    </view>
</view>
```

### 自己发送的消息（右对齐）

```css
.message-item-content.self {
    flex-direction: row-reverse;
}
```

### 系统消息布局（居中）

```css
.message-item.system-center {
    justify-content: center;
}
```

---

## 四、整体聊天界面结构

```
┌─────────────────────────────────────┐
│         点击获取历史消息             │
├─────────────────────────────────────┤
│ 时间: 2026-03-31 10:30              │
├─────────────────────────────────────┤
│ ┌───┐                               │
│ │头│  管理员 张三                    │
│ │像│  ┌──────────────────┐          │
│ └───┘  │ 消息内容...      │          │
│        └──────────────────┘          │
├─────────────────────────────────────┤
│           欢迎李四进入秒杀场          │ ← 系统消息居中
├─────────────────────────────────────┤
│                   ┌───┐              │
│  ┌──────────┐    │头│ 李四           │
│  │我的消息   │    │像│               │
│  └──────────┘    └───┘              │
├─────────────────────────────────────┤
│ 时间: 2026-03-31 10:35              │
├─────────────────────────────────────┤
│ ┌───┐                               │
│ │头│  ┌────────────────────────┐    │
│ │像│  │ [出价]                 │    │
│ └───┘│ 商品: XXX              │    │
│      │ 当前出价: ￥128        │    │
│      └────────────────────────┘    │
└─────────────────────────────────────┘
```

---

## 五、CSS 变量定义

```css
:root {
    /* 主题色 */
    --primary-color: #F4835a;
    --primary-light: #ff7144;
    
    /* 消息颜色 */
    --msg-text-bg: #ffffff;
    --msg-text-color: #000000;
    
    /* 出价消息 */
    --msg-bid-border: #ff7144;
    --msg-bid-bg: #ffb673;
    
    /* 秒杀开始 */
    --msg-start-border: #ef4444;
    --msg-start-bg: #fff5f5;
    
    /* 秒杀结束 */
    --msg-end-border: #ff9800;
    --msg-end-bg: #ffb673;
    
    /* 成交消息 */
    --msg-deal-border: #22c55e;
    --msg-deal-bg: #86efac;
    
    /* 卡秒开启 */
    --msg-kasec-enabled-border: #e53e3e;
    --msg-kasec-enabled-bg: linear-gradient(135deg, #fff5f5 0%, #fed7d7 100%);
    
    /* 卡秒关闭 */
    --msg-kasec-disabled-border: #38a169;
    --msg-kasec-disabled-bg: linear-gradient(135deg, #f0fff4 0%, #c6f6d5 100%);
    
    /* 系统消息 */
    --msg-system-color: #888888;
}
```

---

## 六、设计稿中的应用

在 HTML 设计稿中，需要展示以下消息类型的示例：

1. **普通文本消息** - 白色气泡
2. **图片消息** - 带圆角的图片
3. **出价消息** - 橙色边框+渐变背景
4. **开始秒杀** - 红色边框+浅红背景
5. **秒杀结束** - 琥珀色边框+橙色背景
6. **成交通知** - 绿色边框+浅绿背景
7. **卡秒状态** - 渐变背景+闪电图标
8. **系统消息** - 居中灰色文字

---

*文档创建时间: 2026-03-31*
*基于 UniApp molitao_uniapp 消息组件分析*