# Testing Patterns

**Analysis Date:** 2026-05-22

## Test Frameworks

| Module | Framework | Test Runner | Assertion Library |
|--------|-----------|-------------|-------------------|
| backend | xUnit | dotnet test | Shouldly |
| pc | Playwright | npx playwright test | @playwright/test expect |
| molitao_h5 | Vitest | vitest | Vitest expect |
| molitao_uniapp | Vitest | vitest | Vitest expect |

---

## Backend Testing (xUnit + Shouldly)

### Project Structure

```
backend/test/
├── TtWork.Project.Tests/         # Main test project
│   ├── Applications/
│   │   └── Auctions/
│   │       └── EndAuctionTests.cs
│   └── UnitTest1.cs
└── TtWork.SoMall.Tests/         # Integration test project
    ├── AuctionItemTests.cs
    ├── ChatChannelServiceTests.cs
    ├── Domains/
    │   └── Pays/
    │       └── PayOrderTests.cs
    └── *.cs
```

### Test Naming

Pattern: `Method_State_ExpectedResult`

```csharp
// From TtWork.SoMall.Tests/AuctionItemTests.cs
[Fact]
public void RollBack_Should_Reset_Price_To_PreviousBid()
{
    // Test body
}

[Theory]
[InlineData(1000, 500, 1500)]
public void IncrementAmount_Calculation_ShouldBeCorrect(
    decimal current, 
    decimal increment, 
    decimal expected)
{
    // Test body
}
```

### Test Structure

```csharp
using Shouldly;
using Xunit;

namespace TtWork.Project.Tests.Applications.Auctions;

public class EndAuctionTransactionTests
{
    private const decimal MaxCumulativeAmount = 999999999m;

    #region 金额累加计算测试

    [Fact]
    public void IncrementAmount_ExceedsMax_ShouldCapAtMax()
    {
        // Arrange
        var currentAmount = 999999000m;
        var incrementAmount = 2000m;

        // Act
        var newAmount = currentAmount + incrementAmount;
        if (newAmount > MaxCumulativeAmount)
        {
            newAmount = MaxCumulativeAmount;
        }

        // Assert
        newAmount.ShouldBe(MaxCumulativeAmount);
    }

    #endregion
}
```

### Assertions (Shouldly)

```csharp
// Equality
auctionItem.CurrentPrice.ShouldBe(100);

// Null checks
auctionItem.CurrentPrice.ShouldBeNull();

// Collection
result.Length.ShouldBe(3);
result[0].time.ShouldBe(2);

// Exception messages
await Should.ThrowAsync<Exception>(() => service.DeleteAsync(id));
```

### Mocking

**Framework:** Moq (referenced from CLAUDE.md)

**Pattern:**
```csharp
// Not observed in codebase - pattern from CLAUDE.md guidance
[Fact]
public async Task GetAuction_Should_Return_Correct_Result()
{
    // Arrange
    var mockRepo = new Mock<IRepository<AuctionItem>>();
    mockRepo.Setup(x => x.GetAsync(It.IsAny<long>()))
        .ReturnsAsync(new AuctionItem { Name = "Test" });
    
    var service = new AuctionItemAppService(mockRepo.Object);
    
    // Act
    var result = await service.GetAsync(1);
    
    // Assert
    result.Name.ShouldBe("Test");
}
```

### Integration Tests

**EF Core InMemory Database:**
```csharp
// From CLAUDE.md
// Use InMemory database for integration tests
var options = new DbContextOptionsBuilder<AbpDbContext>()
    .UseInMemoryDatabase(databaseName: "Test_" + Guid.NewGuid().ToString())
    .Options;
```

### Run Commands

```bash
# All tests
cd backend && dotnet test

# Single test class
dotnet test --filter "FullyQualifiedName~AuctionItemTests"

# With coverage (requires Coverlet)
dotnet test /p:CollectCoverage=true
```

---

## Frontend E2E Testing (Playwright)

### Project Structure

```
pc/
├── tests/
│   └── e2e/
│       ├── all-tests.spec.ts
│       ├── auction-chat.spec.ts
│       └── debug-auction.spec.ts
├── playwright.config.ts
└── package.json
```

### Test Configuration (from pc/package.json)

```json
{
    "scripts": {
        "test:e2e": "npx playwright test",
        "test:e2e:ui": "npx playwright test --ui",
        "test:e2e:headed": "npx playwright test --headed",
        "test:e2e:report": "npx playwright show-report"
    }
}
```

### Test Pattern (from pc/tests/e2e/debug-auction.spec.ts)

```typescript
import { test, expect } from '@playwright/test'

const BASE_URL = 'http://localhost:4200'

test('调试拍卖行发送按钮', async ({ page }) => {
    // Navigate
    await page.goto(`${BASE_URL}/#/auth/login`)
    await page.waitForTimeout(3000)
    
    // Interact
    await page.getByPlaceholder('请输入用户名').fill('feifei')
    await page.getByPlaceholder('请输入密码').fill('123456')
    await page.getByRole('button', { name: '登录' }).click()
    
    // Assert
    await page.waitForTimeout(8000)
    expect(page.url()).toContain('/chat/auction/auction')
})
```

### Helper Patterns

```typescript
// Modal handling pattern from debug-auction.spec.ts
async function closeAnnouncementModal(page) {
    const overlay = page.locator('.el-overlay.is-message-box')
    const isVisible = await overlay.isVisible().catch(() => false)
    
    if (isVisible) {
        await page.keyboard.press('Escape')
        await page.waitForTimeout(500)
        
        const buttons = page.locator('.el-overlay.is-message-box .el-message-box__btns .el-button')
        if (await buttons.count() > 0) {
            await buttons.last().click()
        }
    }
}
```

---

## Unit Testing (Vitest)

### Project Structure

```
molitao_h5/
├── tests/
│   ├── chatStore_history.test.ts
│   └── chatStore_history.boundary.test.ts
├── src/
│   └── stores/
│       └── chatStore.ts
└── package.json
```

### Test Pattern (from molitao_h5/tests/chatStore_history.test.ts)

```typescript
import { describe, it, expect } from 'vitest'
import { mergeHistoryForChannel } from '../src/stores/chatStore'

describe('mergeHistoryForChannel (system channel history merge)', () => {
  it('merges new items with existing keeping chronological order', () => {
    const newItems: any[] = [
      { id: 101, time: 1000 },
      { id: 102, time: 2000 }
    ]
    const existing: any[] = [
      { id: 1, time: 1500 }
    ]
    const res = mergeHistoryForChannel('-10_announcement', newItems as any, existing as any, false)
    
    // Assert
    expect(res.length).toBe(3)
    expect(res[0].time).toBe(1000)
  })
})
```

### Run Commands

```bash
# molitao_h5 / molitao_uniapp
cd molitao_h5 && npm run test     # if configured
cd molitao_h5 && npx vitest       # direct run

# pc
cd pc && npm run test:e2e
```

---

## Test Coverage

**Backend:**
- Coverage target: Not enforced (no `coverlet.collectformat` in csproj observed)
- Key coverage areas: Domain logic (AuctionItem.RollBack, SetBid), AppService methods

**Frontend:**
- E2E tests for critical user flows (login, auction, chat)
- Unit tests for utility functions (mergeHistoryForChannel)
- No enforced coverage threshold

---

## Test Data Factories

**Backend pattern:**
```csharp
// Using object initializers (observed in AuctionItemTests.cs)
var auctionItem = new AuctionItem
{
    Name = "Test Item",
    CurrentPrice = 150,
    CurrentPriceUserId = 2,
    StartingPrice = 50
};
```

**Frontend pattern:**
```typescript
// Inline test data
const newItems = [
    { id: 101, time: 1000 },
    { id: 102, time: 2000 }
]
```

---

## What to Test

### Backend

| Priority | What to Test |
|----------|-------------|
| High | Domain entity methods (RollBack, SetBid, StartAuction) |
| High | Application service business logic |
| High | Validation logic |
| Medium | Repository queries (via integration tests) |
| Low | Infrastructure (logging, caching) |

### Frontend

| Priority | What to Test |
|----------|-------------|
| High | Critical user flows (login, payment, chat send) |
| High | Store/composable logic (mergeHistoryForChannel) |
| Medium | Utility functions (date formatting, data transformation) |
| Low | UI component rendering (visual testing preferred) |

---

## Test Organization

**Backend tests** use `#region` blocks to group related tests:
```csharp
#region 金额累加计算测试
[Fact] public void Test1() { }
[Theory] public void Test2() { }
#endregion

#region 等级变化计算测试
[Fact] public void Test3() { }
#endregion
```

**Frontend tests** use `describe` blocks:
```typescript
describe('mergeHistoryForChannel boundary tests', () => {
  it('merges and sorts by time...', () => { })
})
```

---

## Known Testing Gaps

1. **No Vitest config observed** in molitao_h5/molitao_uniapp - tests may not run automatically
2. **No E2E tests in molitao_h5** - only unit tests present
3. **No mocking framework** observed in frontend tests (uses `any` casts)
4. **Backend integration tests** use InMemory database, not full SQL Server

---

## Verification Commands

```bash
# Backend
cd backend && dotnet build                    # Build first
dotnet test                                   # Run all tests
dotnet test --filter "FullyQualifiedName~TestClassName"  # Single test class

# PC E2E
cd pc && npm run test:e2e                     # Run E2E tests
npm run test:e2e:headed                        # Run with browser visible

# molitao_h5 / molitao_uniapp
npx vitest run                                # Run vitest (if configured)
```