# Codebase Concerns

**Analysis Date:** 2026-05-22

## Tech Debt

**[Backend] Async/Await Blocking Issues:**
- Issue: Synchronous blocking patterns using `.Result`, `.GetAwaiter().GetResult()` in high-concurrency scenarios
- Files: `backend/Modules/TtWork.Lib/HttpClientService.cs:111,114,132,137,141`, `backend/src/TtWork.Project.Web.Host/Services/RedisDistributedCache.cs:28,53,74,95`
- Impact: Deadlock risk under high load
- Fix approach: Convert to fully async methods using `await`

**[Backend] WeChat Reflection Performance:**
- Issue: `WeChatReflectionHelper.cs:9` uses reflection for API calls - performance bottleneck
- Files: `backend/Modules/Tt.HttpClient.Weixin/Extensions/WeChatReflectionHelper.cs`
- Impact: Slow WeChat API responses
- Fix approach: Use expression trees to compile property accessors once

**[Backend] Type Conversion Schema:**
- Issue: `FormatSchemaExtension.cs:62,106` converts all values to int regardless of type
- Files: `backend/Modules/TtWork.Abp.Core/Extensions/FormatSchemaExtension.cs`
- Impact: Data truncation or incorrect values for non-integer fields
- Fix approach: Implement type-specific conversion logic

**[Backend] WeChat Event Handling:**
- Issue: `AbpWeiXinProvider.cs:185` incomplete event key handling for different return types
- Files: `backend/src/TtWork.Project.Web.Host/AbpWeiXinProvider.cs`
- Impact: Incorrect response for certain WeChat events
- Fix approach: Add switch/case for all event keys

**[Backend] App Setting Inheritance:**
- Issue: `AppProvider.cs:42` - `setting.IsInherited` not implemented
- Files: `backend/Modules/TtWork.Abp.AppManagement/Apps/AppProvider.cs`
- Impact: Setting inheritance not working correctly
- Fix approach: Implement inheritance logic

**[Frontend] Type Safety:**
- Issue: 434+ instances of `any` type in PC and UniApp codebases
- Files: `pc/` and `molitao_uniapp/` multiple files
- Impact: Type errors detected only at runtime
- Fix approach: Replace `any` with proper type definitions

**[Frontend] Console.log Overuse:**
- Issue: 418+ console.log statements in production code
- Files: PC/UniApp multiple files
- Impact: Production log noise, performance impact
- Fix approach: Remove or replace with structured logging

**[Flutter App] Duplicate Emoji Collection:**
- Issue: Users can collect same emoji multiple times without deduplication
- Files: `molitao_app/lib/presentation/providers/chat_emoji_store.dart`, `molitao_app/lib/data/repositories/chat_emoji_repository.dart`
- Impact: Duplicate entries in emoji collection, poor UX
- Fix approach: Add URL deduplication check before API call

**[Flutter App] Missing Features (Delayed):**
- Issue: Recharge, withdrawal, and deposit pages paused waiting for payment review
- Files: `molitao_app/lib/presentation/pages/profile/profile_page.dart:705`
- Impact: Users cannot complete payment flows
- Fix approach: Unblock after payment merchant configuration completes

**[Flutter App] Group Settings Incomplete:**
- Issue: Group announcement, mute, admin permissions not implemented
- Files: `molitao_app/lib/presentation/pages/chat/group_chat_page.dart:254`
- Impact: Limited group management functionality
- Fix approach: Wait for backend feature expansion

## Known Bugs

**[UniApp] Promise Without Error Handling:**
- Symptoms: Silent failures when API calls fail, users unaware of operation failures
- Files: `molitao_uniapp/src/pages/chat/groupChat.vue:21`, `molitao_uniapp/src/pages/chat/privateChat.vue:77,83`, `molitao_uniapp/src/stores/userStore.ts:236,263`, `molitao_uniapp/src/stores/chatStore.ts:465,542`
- Trigger: Network errors or API failures during user actions
- Workaround: None - operations silently fail

**[Backend] Empty Catch Blocks Swallowing Exceptions:**
- Symptoms: Errors hidden from debugging, silent failures in production
- Files: `backend/src/TtWork.Project.Web.Host/Services/RedisDistributedCache.cs:45-118` (4 places), `backend/src/TtWork.Project/EventHandlers/MessageSentEventHandler.cs:71,102`, `backend/src/TtWork.Project/Applications/AppFeatureSwitchAppService.cs:85`
- Trigger: Any exception in cached code paths
- Workaround: Check logs for "silent failure" indicators

**[PC] Memory Leaks from Event Listeners:**
- Symptoms: Memory grows over time, browser tab becomes unresponsive
- Files: `pc/src/views/dashboard/LineChart.vue:93`, `pc/src/components/Chat/auctionItemDetail.vue:247,257`
- Trigger: Resize events and image click listeners not cleaned up
- Workaround: Periodic page refresh

**[UniApp] Timer Not Cleared on Page Exit:**
- Symptoms: WebSocket connections accumulate, memory leak
- Files: `molitao_uniapp/src/pages/chat/groupChat.vue` (gsocketTimeId timer)
- Trigger: Navigating away from group chat page
- Workaround: Restart app to clear timers

## Security Considerations

**[PC] XSS Vulnerability in Chat Components:**
- Risk: Malicious scripts can be injected via innerHTML usage
- Files: `pc/src/components/Chat/editAuctionItem.vue:154,166,180,183,195`, `pc/src/components/Chat/announceDiv.vue:52-57`
- Current mitigation: DOMPurify added to some components
- Recommendations: Audit all `dangerouslyUseHTMLString` usages, ensure all user content is sanitized

**[UniApp] API Key Exposure:**
- Risk: Weather API key hardcoded in source code
- Files: `molitao_uniapp/src/stores/appStore.ts:45,67`
- Current mitigation: Keys moved to environment variables (per issues-backlog.md - marked fixed)
- Recommendations: Verify all API keys are in env vars, rotate any exposed keys

**[Backend] Non-Thread-Safe Random:**
- Risk: Verification codes may fail under high concurrency
- Files: `backend/src/TtWork.Project.Web.Core/Controllers/TokenAuthController.cs:481`, `backend/src/TtWork.Project.Web.Core/Authentication/External/WechatMiniOpenidProviderApi.cs:44`, `backend/src/TtWork.Project/Services/MessageSequenceService.cs:206,249`
- Current mitigation: Using `Random.Shared` (.NET 6+)
- Recommendations: Audit all random number generation for thread safety

## Performance Bottlenecks

**[Backend] Redis Cache Serialization:**
- Problem: Heavy serialization/deserialization for cached objects
- Files: `backend/src/TtWork.Project/Services/Cache/AuctionItemCacheManager.cs`
- Cause: Large object graphs cached without compression
- Improvement path: Implement object pooling for cache entries

**[Backend] SQL Query Complexity:**
- Problem: Complex LINQ queries with N+1 potential in ChatList
- Files: `backend/src/TtWork.Project/Caches/ChatListCacheService.cs`
- Cause: Multiple joins and in-memory filtering
- Improvement path: Pre-compute user status fields, single query optimization

**[Frontend] Image URL Conversion Overhead:**
- Problem: Repeated image URL transformation on every render
- Files: `molitao_uniapp/src/utils/imageUrlConverter.ts`, `molitao_h5/src/utils/imageUrlConverter.ts`
- Cause: No caching of converted URLs
- Improvement path: Memoize URL conversions

## Fragile Areas

**[Backend] BidEligibilityService Concurrent Access:**
- Files: `backend/src/TtWork.Project/Services/BidEligibilityService.cs`
- Why fragile: In-memory locks combined with distributed Redis cache can desync
- Safe modification: Always test concurrent bid scenarios before changing locking logic
- Test coverage: `BidEligibilityServiceKasecTests.cs` covers basic scenarios but not race conditions

**[Backend] MessageSequenceService Token Generation:**
- Files: `backend/src/TtWork.Project/Services/MessageSequenceService.cs`
- Why fragile: Sequence generation relies on database + cache coordination
- Safe modification: Test sequence generation under 100+ concurrent requests
- Test coverage: Integration tests exist but high-load scenarios not validated

**[PC] Auction Item Detail Component:**
- Files: `pc/src/components/Chat/auctionItemDetail.vue`
- Why fragile: Large component with many event listeners, image handling
- Safe modification: Test all image loading scenarios, verify cleanup on route change
- Test coverage: Visual testing only

**[UniApp] SignalR Connection Management:**
- Files: `molitao_uniapp/src/utils/signalr.ts`
- Why fragile: Connection state management complex, reconnection logic brittle
- Safe modification: Test connection drops and reconnections
- Test coverage: Basic happy path only

## Scaling Limits

**[Redis] Memory Usage:**
- Current capacity: Cache sizes unbounded, no eviction policies configured
- Limit: Redis OOM when缓存数据 exceeds available memory
- Scaling path: Implement LRU eviction, configure maxmemory policies

**[Database] Connection Pool:**
- Current capacity: Default SqlSugar connection pool settings
- Limit: Connection exhaustion under high concurrent writes
- Scaling path: Tune MaxConnectionPoolSize, implement connection health checks

**[Backend] Thread Pool:**
- Current capacity: Default .NET thread pool for async operations
- Limit: Thread starvation when many long-running async operations
- Scaling path: Configure custom thread pool sizing for background jobs

## Dependencies at Risk

**[JsonProperty] Newtonsoft.Json annotations:**
- Risk: Inconsistent serialization between API responses and local caching
- Impact: `CreateTime` vs `createTime` naming discrepancies cause data binding failures
- Migration plan: Standardize on one naming convention across all DTOs

**[SignalR] Version Compatibility:**
- Risk: Client and server SignalR versions must match exactly
- Impact: Connection failures after backend updates
- Migration plan: Implement version negotiation and backward compatibility

## Missing Critical Features

**[Payment] Merchant Configuration Pending:**
- Problem: App payment features (recharge, withdrawal, deposit) blocked waiting for payment platform review
- Blocks: User payment flows, revenue generation
- Priority: High (business blocking)

**[Backend] Group Feature Expansion:**
- Problem: Group announcements, mute, admin permissions not available
- Blocks: Full group management functionality
- Priority: Medium (partial feature)

## Test Coverage Gaps

**[Backend] Bidding Logic:**
- What's not tested: High-concurrency bid scenarios, bid cancellation, bid eligibility edge cases
- Files: `backend/src/TtWork.Project/Applications/Auctions/AuctionItemAppService.cs`
- Risk: Race conditions in bid processing could cause incorrect final prices
- Priority: High

**[Backend] Chat List Performance:**
- What's not tested: Large user (>1000 chat channels) performance
- Files: `backend/src/TtWork.Project/Caches/ChatListCacheService.cs`
- Risk: Slow query times for power users with many chat channels
- Priority: Medium

**[PC] Real-time Auction Updates:**
- What's not tested: WebSocket message handling under high auction activity
- Files: `pc/src/stores/auctionStore.ts`
- Risk: Missed auction updates during high-traffic periods
- Priority: Medium

**[UniApp] Offline Mode:**
- What's not tested: Message queuing and sync when connection restored
- Files: `molitao_uniapp/src/stores/chatStore.ts`
- Risk: Message loss during network interruptions
- Priority: Low

**[Flutter] Push Notification Delivery:**
- What's not tested: Notification delivery under various network conditions
- Files: `molitao_app/lib/` push notification handling
- Risk: Users missing auction alerts
- Priority: Medium

---

*Concerns audit: 2026-05-22*