# 拍卖状态统一处理实现

## 问题背景

在拍卖系统中，消息 payload 中的 `status` 字段存在不一致的问题：

- 有些消息中 `status` 是数字值（如：0, 1, 2, 4, 8, 16, 32, 128）
- 有些消息中 `status` 是字符串值（如：'草稿', '上架', '拍卖中', '已成交' 等）

这种不一致导致前端在处理拍卖消息时需要同时判断数字和字符串状态，增加了代码复杂性和出错风险。

## 解决方案

在 `convertAuctionPayload` 函数中添加了状态值的统一转换逻辑，将所有状态值统一转换为字符串格式。

### 实现细节

#### 1. 状态映射表

```typescript
const AUCTION_STATUS_MAP: { [key: number]: string } = {
  0: "草稿",
  1: "上架",
  2: "拍卖中",
  4: "已成交",
  8: "交易成功",
  16: "卖家失约",
  32: "买家失约",
  128: "交易关闭",
};
```

#### 2. 状态转换函数

```typescript
function normalizeAuctionStatus(status: any): string {
  if (status === null || status === undefined) {
    return "";
  }

  // 如果已经是字符串，直接返回
  if (typeof status === "string") {
    return status;
  }

  // 如果是数字，转换为对应的字符串
  if (typeof status === "number") {
    return AUCTION_STATUS_MAP[status] || status.toString();
  }

  // 其他类型转换为字符串
  return String(status);
}
```

#### 3. 集成到 convertAuctionPayload

在 `convertAuctionPayload` 函数中，在完成 PascalCase 到 camelCase 的转换后，添加状态值的统一处理：

```typescript
export function convertAuctionPayload(payload: any): any {
  // ... 现有逻辑 ...

  const convertedPayload = convertPascalToCamel(payload);

  // 统一处理状态值
  if (convertedPayload.status !== undefined) {
    convertedPayload.status = normalizeAuctionStatus(convertedPayload.status);
  }

  return convertedPayload;
}
```

## 修改的文件

### PC 端

- `pc/src/utils/propertyConverter.ts` - 添加状态转换逻辑
- `pc/src/components/Chat/AuctionEndMessage.vue` - 简化状态判断
- `pc/src/utils/propertyConverter.test.ts` - 添加测试用例

### 移动端

- `molitao_uniapp/src/utils/propertyConverter.ts` - 添加状态转换逻辑
- `molitao_uniapp/src/components/chat/chatMain.vue` - 简化状态判断

## 测试用例

创建了完整的测试用例来验证状态转换逻辑：

```typescript
// 数字状态值测试
{ input: { status: 0 }, expected: '草稿' }
{ input: { status: 4 }, expected: '已成交' }

// 字符串状态值测试
{ input: { status: '已成交' }, expected: '已成交' }
{ input: { status: '拍卖中' }, expected: '拍卖中' }

// 边界情况测试
{ input: { status: null }, expected: '' }
{ input: { status: 999 }, expected: '999' }
```

## 优势

1. **统一性**：所有状态值都转换为字符串格式，确保一致性
2. **向后兼容**：支持现有的数字和字符串状态值
3. **容错性**：对未知状态值有合理的降级处理
4. **简化代码**：前端组件不再需要同时判断数字和字符串状态
5. **可维护性**：状态映射集中管理，便于维护和扩展

## 使用示例

### 修改前

```vue
<div v-if="payloadData.status === '已成交' || payloadData.status === 4">
```

### 修改后

```vue
<div v-if="payloadData.status === '已成交'">
```

## 注意事项

1. 这个修改是向后兼容的，不会影响现有功能
2. 所有使用 `convertAuctionPayload` 的地方都会自动受益于这个改进
3. 如果后端发送了新的状态值，只需要在 `AUCTION_STATUS_MAP` 中添加映射即可
4. 建议在部署前运行测试用例验证功能正常
