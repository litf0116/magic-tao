# 最低报价计算工具方法使用说明

## 概述

项目中已经创建了一个通用的最低报价计算工具方法 `calculateMinBidPrice`，用于统一处理竞拍的最低报价逻辑。

## 方法位置

- **UniApp端**: `/src/utils/auction.ts` 中的 `calculateMinBidPrice` 方法
- **PC端**: `/src/utils/auction.ts` 中的 `calculateMinBidPrice` 方法

## 方法签名

```typescript
// UniApp端
const calculateMinBidPrice = (currentPrice: number = 0, isKasec: boolean = false): number

// PC端
export const calculateMinBidPrice = (currentPrice = 0, isKasec = false): number
```

## 参数说明

- `currentPrice`: 当前价格，默认为 0
- `isKasec`: 是否为卡秒模式，默认为 false

## 计算规则

### 基础增幅规则：
- 当前价格 < 100: +1
- 当前价格 < 1000: +5  
- 当前价格 < 2000: +10
- 当前价格 < 5000: +20
- 当前价格 < 10000: +50
- 当前价格 >= 10000: +100

### 卡秒模式规则：
当 `isKasec = true` 时，最低价格增幅为普通模式的3倍

## 使用示例

### UniApp端使用：

```vue
<script setup lang="ts">
import { calculateMinBidPrice } from '@/utils/auction'

// 计算最低出价
const minPrice = calculateMinBidPrice(onAuctionItem.value.currentPrice, auctionStore.isKasec)
</script>
```

### PC端使用：

```vue
<script setup lang="ts">
import { calculateMinBidPrice } from '@/utils/auction'

// 计算最低出价
const minPrice = calculateMinBidPrice(onAuctionItem.value.currentPrice, auctionStore.isKasec)
</script>
```

## 已更新的文件

### UniApp端：
- `/src/pages/chat/auction.vue` - 已使用新的工具方法替换原有逻辑

### PC端：
- `/src/components/Chat/AuctionList.vue` - 已使用新的工具方法替换原有逻辑
- `/src/components/Chat/auctionItemDetail.vue` - 已使用新的工具方法替换原有逻辑

## 优势

1. **代码复用**: 统一的计算逻辑，避免重复代码
2. **易于维护**: 只需在一个地方修改计算规则
3. **类型安全**: 提供了完整的TypeScript类型支持
4. **调试友好**: 内置了调试日志输出
5. **一致性**: 确保所有地方使用相同的计算逻辑
