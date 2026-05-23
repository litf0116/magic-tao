---
plan_id: 06-FIX-PLAN-1
plan_name: 代码质量修复（Bug + 日志清理 + Timer 清理）
wave: 1
depends_on: []
files_modified:
  - molitao_uniapp/src/utils/signalr.ts
  - molitao_uniapp/src/components/chat/chatMain.vue
  - molitao_uniapp/src/pages/tabbar/index.vue
  - molitao_uniapp/src/utils/pushH5.ts
  - molitao_uniapp/src/utils/appUpdate.ts
  - molitao_uniapp/src/utils/chat.js
  - molitao_uniapp/src/utils/tokenManager.ts
  - molitao_uniapp/src/utils/propertyConverter.ts
  - molitao_uniapp/src/composables/index.ts
  - molitao_uniapp/src/App.vue
  - molitao_uniapp/src/stores/chatStore.ts
  - molitao_uniapp/src/pages/push-demo/index.vue
  - molitao_uniapp/src/pages/chat/auction.vue
  - molitao_uniapp/src/pages/user/bind-phone.vue
  - molitao_uniapp/src/pages/user/balanceLog.vue
  - molitao_uniapp/src/pages/user/depositLog.vue
  - molitao_uniapp/src/pages/user/info.vue
  - molitao_uniapp/src/pages/user/auctionSuccessList.vue
  - molitao_uniapp/src/pages/announce/list.vue
  - molitao_uniapp/src/pages/index/login.vue
  - molitao_uniapp/src/pages/index/my.vue
  - molitao_uniapp/src/pages/index/index.vue
  - molitao_uniapp/src/pages/chat/contacts.vue
  - molitao_uniapp/src/pages/chat/privateChat.vue
  - molitao_uniapp/src/pages/chat/groupChat.vue
  - molitao_uniapp/src/pages/protocol/agreement.vue
  - molitao_uniapp/src/pages/protocol/privacy.vue
  - molitao_uniapp/src/pages/tradingPost/addPost.vue
  - molitao_uniapp/src/pages/tradingPost/postDetail.vue
  - molitao_uniapp/src/pages/tradingPost/index.vue
autonomous: true
requirements:
  - MP-02
  - MP-03
---

<objective>
修复微信小程序现有的代码质量问题：清理生产环境的 console.log 调试日志、修复 setInterval 在页面卸载时未清理的问题、完善异常处理能力。目标是无功能的纯代码质量提升。
</objective>

<tasks>

## Task 1: setInterval 清理检查与修复

**Wave:** 1
**Files:** signalr.ts, chatMain.vue, tabbar/index.vue

<read_first>
- molitao_uniapp/src/utils/signalr.ts (查看 setInterval 和 onUnload 生命周期)
- molitao_uniapp/src/components/chat/chatMain.vue (查看 setInterval 和 onUnload/onHide)
- molitao_uniapp/src/pages/tabbar/index.vue (查看 setInterval 和 onUnload/onHide)
</read_first>

<action>
检查以下 3 个文件中 setInterval 的清理情况，确保在页面卸载（onUnload/onHide/onBeforeUnmount）时调用 clearInterval：

1. **signalr.ts:56** — `this.ping_timerId = setInterval(...)`。确保该类有销毁方法或在 onUnload 时清理。
2. **chatMain.vue:361** — `timeId = setInterval(...)`。检查 onUnload/onHide 生命周期是否有 clearInterval(timeId)。如果缺少则添加。
3. **tabbar/index.vue:132** — `timer.value = setInterval(...)`。检查 onUnload/onHide 生命周期内是否有 `clearInterval(timer.value)`。如果缺少则添加。

**对于每个修复：**
- 确认页面/组件存在 onUnload/onHide/onBeforeUnmount 生命周期
- 如无则添加，并在其中调用 clearInterval
- 如已有则补充 clearInterval 调用
</action>

<acceptance_criteria>
- [ ] signalr.ts: ping_timerId 在类销毁时清除，或组件卸载时通过生命周期清除
- [ ] chatMain.vue: onUnload 或 onHide 中有 clearInterval(timeId)
- [ ] tabbar/index.vue: onUnload 或 onHide 中有 clearInterval(timer.value)
- [ ] 3 处修复后不得引入新功能或行为变更
- [ ] `npm run lint:fix` 通过
</acceptance_criteria>

---

## Task 2: 清理生产代码中的 console.log 调试日志

**Wave:** 1
**Files:** 所有包含 console.log 的 .vue 和 .ts 文件（20+ 文件）

<read_first>
- molitao_uniapp/src/utils/pushH5.ts (10处 console.log)
- molitao_uniapp/src/App.vue (9处 console.log)
- molitao_uniapp/src/stores/chatStore.ts (38处 console.log)
- molitao_uniapp/src/composables/index.ts (3处)
- molitao_uniapp/src/utils/utils.ts (2处)
- molitao_uniapp/src/utils/push.ts (5处)
- molitao_uniapp/src/stores/appFeatureStore.ts (2处)
- molitao_uniapp/src/stores/appStore.ts (1处)
- molitao_uniapp/src/stores/userStore.ts (1处)
- molitao_uniapp/src/utils/upyun-wxapp-sdk.js (3处，第三方库保留)
- 各 page/component .vue 文件中的 console.log
</read_first>

<action>
对 `src/` 目录下所有非第三方库文件中的 console.log 调用进行评估和清理：

**清理规则：**
1. **纯调试日志** — 开发阶段用来跟踪流程的 console.log，直接移除
2. **error catch 中的 `console.log(error)`** — 保留（有价值的错误输出）
3. **用户行为埋点/统计** — 如果具有功能用途（埋点统计），标记为 `// TODO` 或保留
4. **第三方库文件** — 不修改（如 upyun-wxapp-sdk.js）

**重点关注文件（日志最多）：**
- `stores/chatStore.ts`（38处 — 聊天功能日志最多，评估保留必要的错误日志）
- `components/chat/chatMain.vue`（31处）
- `pages/chat/auction.vue`（31处）
- `utils/pushH5.ts`（10处）
- `App.vue`（9处）
- `stores/appFeatureStore.ts`（2处 — 确认是功能调试还是功能逻辑）

**具体操作：**
1. 对每个 console.log 调用判断用途
2. 纯开发调试 → 删除整行
3. catch 块中的 console.log(error) → 保留
4. 功能性的 console.log（如埋点）→ 保留但加注释说明
</action>

<acceptance_criteria>
- [ ] src/ 下第三方库外的 console.log 减少 80% 以上
- [ ] 所有删除的 console.log 确认为纯调试用途（无功能依赖）
- [ ] `npm run lint:fix` 通过
- [ ] `npm run type-check` 通过
</acceptance_criteria>

---

## Task 3: 完善异常处理（空 catch 块修复）

**Wave:** 1
**Files:** 所有包含 try-catch 的 .vue 和 .ts 文件

<read_first>
- molitao_uniapp/src/utils/chat.js:63 — catch 块检查
- molitao_uniapp/src/utils/appUpdate.ts:35,82
- molitao_uniapp/src/utils/pushH5.ts:62,109,141,189
- molitao_uniapp/src/utils/tokenManager.ts:106
- molitao_uniapp/src/utils/propertyConverter.ts:100
- molitao_uniapp/src/pages/user/bind-phone.vue:92
- molitao_uniapp/src/pages/user/info.vue:136,178,223,249
- molitao_uniapp/src/pages/chat/auction.vue:289,335,488
- molitao_uniapp/src/pages/push-demo/index.vue:83,113,165
- molitao_uniapp/src/pages/user/depositLog.vue:43,58
- molitao_uniapp/src/pages/user/auctionSuccessList.vue:87
- molitao_uniapp/src/pages/protocol/agreement.vue:44
- molitao_uniapp/src/pages/protocol/privacy.vue:44
- molitao_uniapp/src/pages/user/balanceLog.vue:39,53
- molitao_uniapp/src/pages/index/my.vue:136,235
</read_first>

<action>
对以下 30 个 catch 块逐一检查：

**检查规则：**
1. **空的 catch 块**（`catch(e) {}` 或 `catch(e) { }`）→ 添加至少一个 console.error 或 uni.showToast 错误提示
2. **仅 `console.log` 不处理** → 添加错误反馈（如 HTTP 请求失败的 catch 中显示错误提示）
3. **用户操作的 catch**（如提现、提交、绑定操作）→ 添加 `uni.showToast({ title: '操作失败', icon: 'none' })` 或具体错误提示
4. **后台任务的 catch**（定时器、SignalR）→ 添加 `console.error` 确保错误可追溯

**不要对以下情况修改：**
- 第三方 SDK 或库的 catch（如 upyun-wxapp-sdk.js）
- 明确的有意忽略（如 Promise.reload().catch(() => {}) 重置场景）
</action>

<acceptance_criteria>
- [ ] 不存在空的 catch 块（空括号内无语句）
- [ ] 用户交互的 catch 块有 uni.showToast 反馈
- [ ] 后台任务 catch 块有 console.error
- [ ] 构建/类型检查通过
</acceptance_criteria>

</tasks>

<verification>
1. `npm run lint:fix` 通过 0 errors
2. `npm run type-check` 通过
3. 小程序在微信开发者工具中预览正常（无白屏/无 Console Error）
4. git diff 仅包含代码清理（无功能变更）
</verification>

<must_haves>
- [ ] 至少修复 3 个小程序 Bug
- [ ] setInterval 清理完成
- [ ] console.log 清理 80% 以上
- [ ] 小程序提审无 Console Error
</must_haves>
