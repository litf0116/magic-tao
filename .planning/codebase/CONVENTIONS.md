# Coding Conventions

**Analysis Date:** 2026-05-22

## Language Overview

| Module | Language | Framework |
|--------|----------|-----------|
| backend | C# (.NET 8) | ABP Framework |
| pc | TypeScript (Vue 3) | Vite, Pinia, Element Plus |
| molitao_uniapp | TypeScript (Vue 3) | UniApp, Pinia |
| molitao_h5 | TypeScript (Vue 3) | UniApp, Pinia |

---

## Backend (C# .NET 8 + ABP Framework)

### Naming Conventions

**Files:**
- Solution/Project: `PascalCase` (e.g., `TtWork.Project.sln`)
- Namespace: `PascalCase` (e.g., `TtWork.Project.Domains`)
- Entity files: `PascalCase.cs` (e.g., `AuctionItem.cs`)

**Classes & Methods:**
- Class names: `PascalCase` (e.g., `AuctionItemAppService`)
- Method names: `PascalCase` (e.g., `EndAuction`)
- Interfaces: `IPascalCase` prefix (e.g., `IRepository<T>`)

**Variables & Fields:**
- Private fields: `_camelCase` (e.g., `_auctionRepository`)
- Constants: `PascalCase` (e.g., `MaxCumulativeAmount`)
- Parameters: `camelCase` (e.g., `auctionItemId`)
- Local variables: `camelCase` (e.g., `currentPrice`)

**Examples from codebase:**
```csharp
// Entity: TtWork.SoMall.Tests/AuctionItemTests.cs
public class AuctionItemTests
{
    [Fact]
    public void RollBack_Should_Reset_Price_To_PreviousBid()
    {
        var previousBid = new BidHistory { BidPrice = 100 };
        var auctionItem = new AuctionItem { CurrentPrice = 150 };
        auctionItem.RollBack(previousBid);
        auctionItem.CurrentPrice.ShouldBe(100);
    }
}
```

### Code Style

**Formatting:**
- Follow C# 10+ conventions
- Use `.editorconfig` if present (not found in repo)
- braces on new lines for namespaces and class definitions

**Imports:**
- `using` statements at top of file
- Sorted alphabetically within groups
- Group: System → Third-party → Project

**Type Usage:**
- Explicit types; avoid `var` except for obvious inference
- Avoid `dynamic`, prefer `unknown` in test contexts
- Use nullable reference types (`string?`, `int?`)

### Error Handling

**Patterns:**
```csharp
// Use UserFriendlyException for business errors
throw new UserFriendlyException("Error message");

// Try-catch with meaningful messages
try { }
catch (Exception ex)
{
    Logger.LogError("Context: {Message}", ex.Message);
    throw;
}
```

**Validation:**
- Input validation at API boundary using DTOs with data annotations
- Use `FluentValidation` for complex validation rules (not observed in codebase)

### Logging

**Framework:** ABP's `ILogger<T>`

**Patterns:**
```csharp
// Constructor injection
private readonly IRepository<AuctionItem> _auctionRepository;
private readonly ILogger<AuctionItemAppService> _logger;

// Usage
_logger.LogInformation("Auction {AuctionId} ended at {Time}", auctionId, DateTime.UtcNow);
```

**Rules:**
- Never log sensitive information (passwords, tokens, personal data)
- Log both success and failure scenarios at appropriate levels

---

## Frontend (Vue 3 + TypeScript)

### Naming Conventions

**Files:**
- Vue components: `PascalCase.vue` (e.g., `UserProfile.vue`)
- TypeScript files: `camelCase.ts` (e.g., `chatStore.ts`)
- Composables: `useXxx.ts` (e.g., `usePayment.ts`)
- Stores: `XxxStore.ts` (e.g., `auctionStore.ts`)

**Variables & Functions:**
- Variables: `camelCase` (e.g., `currentPrice`, `userList`)
- Functions: `camelCase` (e.g., `getPaymentStatus`, `mergeHistoryForChannel`)
- Constants: `SCREAMING_SNAKE_CASE` (e.g., `MAX_RETRY_COUNT`)
- Types/Interfaces: `PascalCase` (optionally with `I` prefix) (e.g., `PaymentResult`)

**Components (template):**
- Component names: `PascalCase` (e.g., `<UserProfile />`)
- Events: `kebab-case` (e.g., `@click`, `@close-modal`)

### Code Style

**Formatting:**
- Tool: ESLint + Prettier
- Config: `pc/.eslintrc`, `pc/.prettierrc.json`
- Key Prettier settings: `singleQuote: true`, `semi: false`, `printWidth: 120`
- Line endings: `endOfLine: auto`

**ESLint key rules (pc/.eslintrc):**
```json
{
    "rules": {
        "@typescript-eslint/ban-ts-comment": "off",
        "@typescript-eslint/no-non-null-assertion": "off",
        "@typescript-eslint/no-empty-function": "off",
        "@typescript-eslint/no-explicit-any": "warn",
        "vue/no-unused-vars": "warn"
    }
}
```

**Imports:**
- Use absolute imports from `@/` root alias
- Organize alphabetically within groups
- Avoid deep relative paths (e.g., `../../`)

**TypeScript:**
- Strict mode enabled (implied by `vue-tsc --noEmit`)
- Avoid `any`, prefer `unknown`
- Use `as const` for readonly literal types

**Example from `pc/src/utils/request.ts`:**
```typescript
export function normalizeResponse<T = any>(response: any): T {
    if (isAbpResponse(response)) {
        return extractAbpResult<T>(response)
    }
    if (isSimpleResponse(response)) {
        return extractSimpleResult<T>(response)
    }
    if (response?.data !== undefined) {
        return response.data as T
    }
    return response as T
}
```

### Component Structure

**Order:**
```vue
<template>
  <!-- template content -->
</template>

<script setup lang="ts">
// Composition API with TypeScript
</script>

<style scoped>
/* scoped styles with UnoCSS classes */
</style>
```

**Props/Emits:**
```typescript
// Props with defineProps<T>()
const props = defineProps<{
    title: string
    count?: number
}>()

// Emits with defineEmits<T>()
const emit = defineEmits<{
    (e: 'update', value: number): void
    (e: 'close'): void
}>()
```

### Error Handling

**Frontend (Axios):**
```typescript
// From pc/src/utils/request.ts
service.interceptors.response.use(
    (response) => {
        if (!isSuccessResponse(response)) {
            const errorMsg = getErrorMessage(response) || '操作失败'
            ElMessage.error(errorMsg)
            return Promise.reject(response.data?.error || response.data)
        }
        return normalizeResponse(response)
    },
    (err: any) => {
        // Centralized error handling with user feedback
        ElMessage.error(err.message || '网络请求失败')
        return Promise.reject(err)
    }
)
```

**Rules:**
- All API calls use try-catch with meaningful error messages
- User-friendly messages via `ElMessage.error()` and `ElMessage.success()`
- Token refresh handled automatically on 401 responses

---

## Shared Patterns

### API Response Handling (PC/UniApp)

**ABP Standard Format:**
```typescript
interface AbpResponse<T> {
    __abp: boolean
    success: boolean
    result?: T
    error?: {
        code?: number
        message: string
        details?: any
        validationErrors?: { message: string; members?: string[] }[]
    }
}
```

**Normalization flow:**
1. axios response → interceptor → `normalizeResponse()` extracts `result`
2. API functions return `response.data.result` directly (NOT `response.data`)
3. Types defined in `src/types/` for all DTOs

### State Management (Pinia)

**Store Pattern:**
```typescript
// pc/src/stores/auctionStore.ts
export const useAuctionStore = defineStore('auction', () => {
    // State
    const items = ref<AuctionItem[]>([])
    
    // Getters
    const activeItems = computed(() => items.value.filter(x => x.status === 'active'))
    
    // Actions
    async function fetchItems() {
        const res = await auctionItemAPI.getList()
        items.value = res
    }
    
    return { items, activeItems, fetchItems }
})
```

### iOS Compatibility (UniApp)

**Required patterns:**
```vue
<!-- Text must be wrapped in <text> tag -->
<view class="text-sm"><text>待秒杀</text></view>

<!-- Font size and weight must be explicit -->
<view class="text-sm font-500">已成交</view>

<!-- Tab height minimum 40px (h-10) -->
<view class="h-10">...</view>
```

---

## Anti-Patterns

### Backend

| Pattern | Problem | Correct |
|---------|---------|---------|
| Hardcoded SQL | SQL injection risk | Use LINQ or parameterized queries |
| Static service methods | Couples to DI container | Inject via constructor |
| Catching all exceptions silently | Hides failures | Log and rethrow or handle explicitly |

### Frontend

| Pattern | Problem | Correct |
|---------|---------|---------|
| `response.data.data` | Double unwrapping in API layer | Return from interceptor directly |
| `any` type | No type safety | Use `unknown` or specific types |
| `console.log` in production | Leaks to console | Remove or use logger |
| Inline styles | Not maintainable | Use UnoCSS atomic classes |

---

## Performance Considerations

**Backend:**
- Use `AsNoTracking()` for read-only queries
- Avoid N+1 queries with eager loading or explicit selects
- Use caching (`IDistributedCache`) for frequently accessed data

**Frontend:**
- Lazy load routes: `() => import('@/views/Home.vue')`
- Component lazy loading: `defineAsyncComponent`
- Virtual scrolling for long lists
- Image lazy loading with `loading="lazy"`