---
plan_id: 06-UI-PLAN-2
plan_name: UI/UX 体验优化
wave: 1
depends_on: []
files_modified:
  - molitao_uniapp/src/pages/tabbar/index.vue
  - molitao_uniapp/src/pages/index/index.vue
  - molitao_uniapp/src/pages/index/my.vue
  - molitao_uniapp/src/pages/index/login.vue
  - molitao_uniapp/src/pages/tradingPost/index.vue
  - molitao_uniapp/src/pages/tradingPost/addPost.vue
  - molitao_uniapp/src/pages/tradingPost/postDetail.vue
  - molitao_uniapp/src/pages/chat/auction.vue
  - molitao_uniapp/src/pages/chat/contacts.vue
  - molitao_uniapp/src/pages/user/info.vue
  - molitao_uniapp/src/pages/user/bind-phone.vue
  - molitao_uniapp/src/pages/user/depositLog.vue
  - molitao_uniapp/src/components/chat/chatMain.vue
  - molitao_uniapp/src/components/chat/AuctionStartMessage.vue
  - molitao_uniapp/src/components/chat/AuctionEndMessage.vue
  - molitao_uniapp/src/components/chat/AuctionBidMessage.vue
  - molitao_uniapp/src/components/chat/AuctionDealMessage.vue
  - molitao_uniapp/src/components/chat/auctionList.vue
  - molitao_uniapp/src/components/chat/auctionMidList.vue
  - molitao_uniapp/src/components/chat/userProfile.vue
  - molitao_uniapp/src/components/chat/ImageMessage.vue
  - molitao_uniapp/src/components/tui-video/tui-video.vue
  - molitao_uniapp/src/components/tui-bar/tui-weather.vue
  - molitao_uniapp/src/components/tui-page/tui-page.vue
  - molitao_uniapp/src/components/tui-report/tui-report.vue
  - molitao_uniapp/src/components/tui-seekhelp/tui-seekhelp.vue
  - molitao_uniapp/src/utils/imageUrlConverter.ts
  - molitao_uniapp/src/utils/countdown.ts
autonomous: true
requirements:
  - MP-01
  - MP-03
---

<objective>
优化微信小程序的 UI/UX 体验：改善页面加载渲染性能、优化图片处理、修复 UI 一致性问题和细节体验。
</objective>

<tasks>

## Task 1: 页面加载性能优化

**Wave:** 1
**Files:** 主页面 .vue 文件和工具函数

<read_first>
- molitao_uniapp/src/pages/tabbar/index.vue (首页，setInterval + 页面渲染)
- molitao_uniapp/src/pages/tradingPost/index.vue (交易站，最大文件 19处any)
- molitao_uniapp/src/utils/imageUrlConverter.ts (图片URL处理)
- molitao_uniapp/src/utils/countdown.ts (倒计时工具)
</read_first>

<action>
**性能优化：**

1. **图片懒加载增强**：
   - 检查页面中 `v-for` 列表渲染的图片，确保使用了 `lazy-load` 属性或 `mode="widthFix"` （小程序原生选项）
   - 重点：交易站列表（tradingPost/index.vue）、聊天消息列表（chatMain.vue）
   
2. **渲染性能**：
   - 大列表使用 `<scroll-view>` 代替原生滚动（如已有则保持不变）
   - 确保 `v-for` 循环中的 `:key` 绑定正确（使用唯一 ID，不用 index）

3. **图片URL处理优化**：
   - `imageUrlConverter.ts` 检查是否有不必要的网络请求或重复转换
</action>

<acceptance_criteria>
- [ ] 列表渲染图片已添加 lazy-load 属性（哪里适用）
- [ ] `v-for` 循环 key 绑定唯一 ID（非 index）
- [ ] 无新的 Console Error
- [ ] 无功能退化
</acceptance_criteria>

---

## Task 2: UI 体验细节优化

**Wave:** 1
**Files:** 各页面和组件 .vue 文件

<read_first>
- molitao_uniapp/src/pages/tabbar/index.vue
- molitao_uniapp/src/pages/index/index.vue
- molitao_uniapp/src/pages/index/my.vue
- molitao_uniapp/src/pages/index/login.vue
- molitao_uniapp/src/pages/tradingPost/index.vue
- molitao_uniapp/src/pages/tradingPost/postDetail.vue
- molitao_uniapp/src/pages/user/info.vue
- molitao_uniapp/src/pages/chat/auction.vue
- molitao_uniapp/src/components/chat/chatMain.vue
</read_first>

<action>
**UI 细节修复（小程序偏好）：**

1. **加载状态**：
   - 检查列表页面是否有加载中/加载完成/空数据的状态提示
   - tradingPost/index.vue（交易站列表）：确保下拉刷新有加载动画，数据为空时有"暂无内容"提示
   - chatMain.vue（聊天列表）：确保消息加载中/无消息的状态展示

2. **操作反馈**：
   - 关键操作（提交发布/出价/登录）在 loading 期间按钮显示 loading 状态
   - 检查 `:loading` 或 `disabled` 属性使用

3. **页面标题**：
   - 各页面的导航栏标题（navigationBarTitleText）与页面内容一致
   - 设置页面时显示合适标题

4. **文字截断与布局**：
   - 长文本显示省略号（text-overflow: ellipsis）
   - 按钮文字不溢出

5. **触摸友好**：
   - 可点击区域是否有合适的 padding 和大小（建议最小 44rpx）
   - 按钮 hover 效果有 `hover-class="uni-hover"` 类

6. **加载动画一致性**：
   - 各页面使用一致的 loading 样式
</action>

<acceptance_criteria>
- [ ] 列表空数据时有友好提示
- [ ] 加载中有 loading 状态显示
- [ ] loading 状态按钮有 loading 动画
- [ ] 页面标题正确
- [ ] 长文本显示省略号
- [ ] 无新的 Console Error
</acceptance_criteria>

---

## Task 3: 用户体验流畅度提升

**Wave:** 1
**Files:** 各个页面和组件

<read_first>
- molitao_uniapp/src/pages/tradingPost/index.vue
- molitao_uniapp/src/pages/tradingPost/addPost.vue
- molitao_uniapp/src/pages/chat/auction.vue
- molitao_uniapp/src/pages/chat/contacts.vue
- molitao_uniapp/src/components/chat/AuctionStartMessage.vue
- molitao_uniapp/src/components/chat/AuctionEndMessage.vue
- molitao_uniapp/src/components/chat/userProfile.vue
- molitao_uniapp/src/components/chat/ImageMessage.vue
- molitao_uniapp/src/components/tui-video/tui-video.vue
- molitao_uniapp/src/components/tui-bar/tui-weather.vue
- molitao_uniapp/src/components/tui-report/tui-report.vue
- molitao_uniapp/src/components/tui-seekhelp/tui-seekhelp.vue
</read_first>

<action>
**交互优化：**

1. **下拉刷新**：
   - 确认所有列表页都支持 `enablePullDownRefresh`（pages.json 中设置）
   - 下拉时有视觉反馈

2. **消息交互**：
   - 聊天页面新消息自动滚动到底部
   - 拍卖消息的时间显示格式统一

3. **快捷操作**：
   - 交易站列表支持点击进入详情（已实现则跳过）
   - 图片/视频查看器点击关闭

4. **页面间跳转体验**：
   - 返回按钮回到正确位置
   - 在微信小程序内跳转使用 `uni.navigateTo` 而不是 `uni.redirectTo`（保留返回栈）
</action>

<acceptance_criteria>
- [ ] 下拉刷新在所有列表页正常工作
- [ ] 聊天新消息自动滚动到底部
- [ ] 页面跳转返回位置正确
- [ ] 图片查看器可以正常关闭
- [ ] 无新的 Console Error
</acceptance_criteria>

</tasks>

<verification>
1. `npm run lint:fix` 通过
2. 微信开发者工具预览无明显视觉异常
3. 所有页面可正常导航
4. 无 Console Error
</verification>

<must_haves>
- [ ] 加载状态和空数据提示
- [ ] UI 一致性改善
- [ ] 页面加载速度无明显退化
- [ ] 提审时无 Console Error
</must_haves>
