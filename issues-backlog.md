# Issues Backlog - 待修复问题队列

> 创建时间: 2026-05-07
> 项目: magic-tao
> 状态: 进行中

---

## 📊 问题统计

| 优先级 | 数量 | 影响 |
|--------|------|------|
| P0 必须立即修复 | 4 | 系统崩溃/死锁 |
| P1 高优先级 | 4 | 功能失效/错误隐藏 |
| P2 中优先级 | 4 | 安全/性能问题 |
| P3 低优先级 | 3 | 代码质量问题 |

---

## 🔴 P0 - 必须立即修复（会导致系统崩溃/死锁）

### P0-1: UniApp signalr.ts 同步阻塞 async:false
- **文件**: `molitao_uniapp/src/utils/signalr.ts`
- **行号**: 95-96
- **问题**: `async: false` 阻塞 UI 线程
- **影响**: 界面冻结无响应
- **状态**: 🔲 待修复
- **修复方案**: 改为 `async: true` + await

### P0-2: Backend HttpClientService 死锁风险 .Result
- **文件**: `backend/Modules/TtWork.Lib/HttpClientService.cs`
- **行号**: 111, 114, 132, 137, 141
- **问题**: `.SendAsync(request).Result` 同步阻塞
- **影响**: 高并发时可能死锁
- **状态**: 🔲 待修复
- **修复方案**: 改为 await 异步调用

### P0-3: Backend RedisDistributedCache .GetAwaiter().GetResult()
- **文件**: `backend/src/TtWork.Project.Web.Host/Services/RedisDistributedCache.cs`
- **行号**: 28, 53, 74, 95
- **问题**: `GetAsync(key).GetAwaiter().GetResult()`
- **影响**: 死锁风险
- **状态**: 🔲 待修复
- **修复方案**: 改为异步接口实现

### P0-4: UniApp groupChat.vue 定时器未清理
- **文件**: `molitao_uniapp/src/pages/chat/groupChat.vue`
- **问题**: 缺少 onUnload 清理 gsocketTimeId 定时器
- **影响**: 内存泄漏，WebSocket 连接堆积
- **状态**: 🔲 待修复
- **修复方案**: 添加 onUnload 清理定时器

---

## 🔴 P1 - 高优先级（会导致功能失效/错误隐藏）

### P1-1: UniApp Promise 无 catch 错误处理
- **文件**: 多处
  - `molitao_uniapp/src/pages/chat/groupChat.vue:21`
  - `molitao_uniapp/src/pages/chat/privateChat.vue:77,83`
  - `molitao_uniapp/src/stores/userStore.ts:236,263`
  - `molitao_uniapp/src/stores/chatStore.ts:465,542`
- **问题**: `.then()` 没有 `.catch()` 错误处理
- **影响**: 操作失败用户无感知，数据不一致
- **状态**: 🔲 待修复
- **修复方案**: 为所有 Promise 添加 .catch() 统一错误处理

### P1-2: Backend 空 catch 块静默失败
- **文件**: 多处
  - `backend/src/TtWork.Project.Web.Host/Services/RedisDistributedCache.cs:45-118` (4处)
  - `backend/src/TtWork.Project/EventHandlers/MessageSentEventHandler.cs:71,102`
  - `backend/src/TtWork.Project/Applications/AppFeatureSwitchAppService.cs:85`
- **问题**: 空 catch 块吞噬异常
- **影响**: 错误被隐藏，难以调试
- **状态**: 🔲 待修复
- **修复方案**: 至少记录 ILogger.Warning

### P1-3: PC LineChart.vue 内存泄漏事件监听
- **文件**: `pc/src/views/dashboard/LineChart.vue`
- **行号**: 93
- **问题**: `window.addEventListener('resize', ...)` 未清理
- **影响**: 内存持续增长
- **状态**: 🔲 待修复
- **修复方案**: onUnmounted 移除监听

### P1-4: PC auctionItemDetail.vue 内存泄漏
- **文件**: `pc/src/components/Chat/auctionItemDetail.vue`
- **行号**: 247, 257
- **问题**: `img.addEventListener('click', ...)` 未清理
- **影响**: 内存泄漏
- **状态**: 🔲 待修复
- **修复方案**: 添加 onUnmounted cleanup

---

## 🟡 P2 - 中优先级（可能导致安全问题/性能下降）

### P2-1: PC editAuctionItem.vue XSS 风险
- **文件**: `pc/src/components/Chat/editAuctionItem.vue`
- **行号**: 154, 166, 180, 183, 195
- **问题**: 使用 `innerHTML` 直接设置内容
- **影响**: 恶意脚本注入风险
- **状态**: 🔲 待修复
- **修复方案**: 使用 DOMPurify 消毒

### P2-2: PC announceDiv.vue XSS 风险
- **文件**: `pc/src/components/Chat/announceDiv.vue`
- **行号**: 52-57
- **问题**: `dangerouslyUseHTMLString: true` 传入未消毒 HTML
- **影响**: XSS 攻击风险
- **状态**: 🔲 待修复
- **修复方案**: 使用 DOMPurify 消毒

### P2-3: UniApp API Key 硬编码
- **文件**: `molitao_uniapp/src/stores/appStore.ts`
- **行号**: 45, 67
- **问题**: 和风天气 API Key 明文暴露
- **影响**: Key 泄露风险
- **状态**: 🔲 待修复
- **修复方案**: 移至环境变量

### P2-4: Backend 非线程安全 Random
- **文件**: 多处
  - `backend/src/TtWork.Project.Web.Core/Controllers/TokenAuthController.cs:481`
  - `backend/src/TtWork.Project.Web.Core/Authentication/External/WechatMiniOpenidProviderApi.cs:44`
  - `backend/src/TtWork.Project/Services/MessageSequenceService.cs:206,249`
- **问题**: `new Random()` 高并发下不安全
- **影响**: 验证码可能失效
- **状态**: 🔲 待修复
- **修复方案**: 使用 Random.Shared (.NET 6+)

---

## 🟢 P3 - 低优先级（代码质量/可维护性）

### P3-1: console.log 过度使用
- **文件**: PC/UniApp 多文件
- **数量**: 418+ 处
- **影响**: 生产日志噪音
- **状态**: 🔲 待修复
- **修复方案**: 移除或替换为 logger

### P3-2: TODO 标记遗留
- **文件**: Backend 4 个文件
- **数量**: 5 处
- **影响**: 未完成代码
- **状态**: 🔲 待修复

### P3-3: any 类型滥用
- **文件**: PC 77 文件 / UniApp 大量
- **数量**: 434+ 处
- **影响**: 类型不安全
- **状态**: 🔲 待修复

---

## ✅ 修复记录

| 日期 | Issue | 状态 | 验证方式 |
|------|-------|------|----------|
| 2026-05-07 | P0-1: UniApp signalr.ts async:false | ✅ 已修复 | Backend build 0 error |
| 2026-05-07 | P0-2: Backend HttpClientService .Result | ✅ 已修复 | Backend build 0 error |
| 2026-05-07 | P0-3: Backend RedisDistributedCache GetAwaiter | ✅ 已修复 | Backend build 0 error |
| 2026-05-07 | P0-4: UniApp groupChat.vue 定时器清理 | ✅ 已修复 | 代码审查 |
| 2026-05-07 | P1-1: UniApp Promise 无 catch | ✅ 已修复 | 代码审查 |
| 2026-05-07 | P1-2: Backend 空 catch 块添加日志 | ✅ 已修复 | Backend build 0 error |
| 2026-05-07 | P1-3: PC LineChart.vue 内存泄漏 | ✅ 已修复 | 代码审查 |
| 2026-05-07 | P1-4: PC auctionItemDetail.vue 内存泄漏 | ✅ 已修复 | 代码审查 |

---

## 🔄 执行流程

1. 按优先级顺序修复问题
2. 修复后进行相关测试
3. 验证通过后提交 git
4. 记录到修复记录表
5. 无法解决则回退重试或跳过

