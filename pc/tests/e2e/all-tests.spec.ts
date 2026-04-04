import { test, expect, type Page } from '@playwright/test'

class LoginPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto('/#/auth/login')
  }

  async switchToPasswordLogin() {
    await this.page.getByText('使用密码/验证码登录').click()
  }

  async login(username: string, password: string) {
    await this.switchToPasswordLogin()
    await this.page.getByPlaceholder('请输入用户名').fill(username)
    await this.page.getByPlaceholder('请输入密码').fill(password)
    await this.page.getByRole('button', { name: '登录' }).click()
  }

  async waitForLoginSuccess(timeout = 15000) {
    await this.page.waitForLoadState('load', { timeout })
    await this.page.waitForTimeout(2000)
    await this.closeDialogs()
  }

  async closeDialogs() {
    await this.page.waitForTimeout(500)
    const overlay = this.page.locator('.el-overlay.is-message-box')
    if (await overlay.isVisible().catch(() => false)) {
      const buttons = this.page.locator('.el-overlay.is-message-box .el-button')
      const count = await buttons.count()
      if (count > 0) {
        await buttons.last().click()
        await this.page.waitForTimeout(500)
      }
    }
  }

  async isQrCodeVisible() {
    const locator = this.page.getByText('扫码登录魔力淘')
    return await locator.isVisible()
  }

  async getErrorMessage() {
    const message = this.page.locator('.el-message--error')
    return (await message.isVisible()) ? await message.textContent() : null
  }

  async isLoginButtonDisabled() {
    const button = this.page.getByRole('button', { name: '登录' })
    return await button.isDisabled()
  }
}

class HomePage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto('/#/index')
  }

  async hasNavItem(text: string) {
    const locator = this.page.locator(`.nav-item:has-text("${text}")`)
    return await locator.count() > 0
  }
}

class AdminPage {
  constructor(private page: Page) {}

  async gotoDashboard() {
    await this.page.goto('/#/admin/dashboard')
  }

  async gotoAuctionList() {
    await this.page.goto('/#/admin/auction/auctionItem')
  }

  async gotoUserList() {
    await this.page.goto('/#/admin/system/user')
  }

  async isDashboardVisible() {
    return await this.page.getByText('dashboard').isVisible()
  }

  async isSidebarVisible() {
    const sidebar = this.page.locator('.el-menu-vertical, [class*="el-menu"]').first()
    return await sidebar.count() > 0
  }
}

class ChatPage {
  constructor(private page: Page) {}

  async gotoAuction() {
    await this.page.goto('/#/chat/auction/auction')
  }

  async gotoContacts() {
    await this.page.goto('/#/chat/contacts')
  }

  async isAuctionChatVisible() {
    return await this.page.locator('.font-700:has-text("拍卖行")').isVisible()
      .catch(() => false)
      || await this.page.locator('text=拍卖行').first().isVisible()
      .catch(() => false)
  }

  async getChatSidebarItems() {
    return await this.page.locator('.conversation').count()
  }

  async clickChatItem(name: string) {
    await this.page.getByText(name, { exact: true }).click()
  }

  async sendTextMessage(text: string) {
    await this.page.evaluate((msg) => {
      const textarea = document.querySelector('.el-textarea__inner') as HTMLTextAreaElement
      if (!textarea) return

      const nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLTextAreaElement.prototype, 'value')?.set
      if (nativeInputValueSetter) {
        textarea.focus()
        nativeInputValueSetter.call(textarea, msg)
        textarea.dispatchEvent(new Event('input', { bubbles: true }))
        textarea.dispatchEvent(new Event('change', { bubbles: true }))
        textarea.dispatchEvent(new KeyboardEvent('keyup', { key: 'a', bubbles: true }))
      }
    }, text)
    await this.page.waitForTimeout(500)

    const sendBtn = this.page.locator('.send-box .el-button:not([disabled])')
    if (await sendBtn.count() > 0) {
      await sendBtn.click()
    }
  }

  async getMessageContent() {
    return await this.page.locator('.message-payload').last().textContent()
  }

  async waitForMessageAppear(text: string, timeout = 10000) {
    await this.page.waitForFunction((msg) => {
      const msgs = Array.from(document.querySelectorAll('.message-payload'))
      return msgs.some((m) => m.textContent?.includes(msg))
    }, text, { timeout })
  }
}

const TEST_USER = {
  username: process.env.TEST_USERNAME || 'admin',
  password: process.env.TEST_PASSWORD || '123456',
}

const TEST_REGULAR_USER = {
  username: 'feifei',
  password: '123456',
}

test.describe('登录页面', () => {
  test('默认显示二维码登录', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    expect(await loginPage.isQrCodeVisible()).toBe(true)
  })

  test('可以切换到密码登录', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.switchToPasswordLogin()
    expect(await loginPage.isQrCodeVisible()).toBe(false)
    expect(await loginPage.isLoginButtonDisabled()).toBe(true)
  })

  test('登录按钮在输入为空时禁用', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.switchToPasswordLogin()
    expect(await loginPage.isLoginButtonDisabled()).toBe(true)

    await page.getByPlaceholder('请输入用户名').fill('testuser')
    expect(await loginPage.isLoginButtonDisabled()).toBe(true)
  })

  test('登录按钮在输入完整时启用', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.switchToPasswordLogin()
    await page.getByPlaceholder('请输入用户名').fill('testuser')
    await page.getByPlaceholder('请输入密码').fill('password')
    expect(await loginPage.isLoginButtonDisabled()).toBe(false)
  })
})

test.describe('首页导航', () => {
  test('首页可以正常加载', async ({ page }) => {
    const homePage = new HomePage(page)
    await homePage.goto()
    await expect(page).toHaveURL(/.*#\/index/)
  })

  test('首页包含主要导航项', async ({ page }) => {
    const homePage = new HomePage(page)
    await homePage.goto()
    await page.waitForLoadState('networkidle')
    await page.waitForSelector('.nav-item', { timeout: 10000 })
    expect(await homePage.hasNavItem('首页')).toBe(true)
    expect(await homePage.hasNavItem('拍卖行')).toBe(true)
  })
})

test.describe('管理后台', () => {
  test('登录后可以访问 Dashboard', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.login(TEST_USER.username, TEST_USER.password)
    await loginPage.waitForLoginSuccess()

    const adminPage = new AdminPage(page)
    await adminPage.gotoDashboard()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)
    await loginPage.closeDialogs()
    await expect(page).toHaveURL(/.*#\/admin\/dashboard/, { timeout: 15000 })
  })

  test('侧边栏可见', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.login(TEST_USER.username, TEST_USER.password)
    await loginPage.waitForLoginSuccess()

    const adminPage = new AdminPage(page)
    await adminPage.gotoDashboard()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(1000)
    await loginPage.closeDialogs()
    await page.waitForTimeout(500)
    await page.waitForSelector('[class*="el-menu"]', { timeout: 10000 })
    expect(await adminPage.isSidebarVisible()).toBe(true)
  })
})

test.describe('聊天功能', () => {
  test('联系人页面需要登录后访问', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.login(TEST_REGULAR_USER.username, TEST_REGULAR_USER.password)
    await loginPage.waitForLoginSuccess()

    const chatPage = new ChatPage(page)
    await chatPage.gotoContacts()
    await page.waitForTimeout(1000)
    await loginPage.closeDialogs()
    await expect(page).toHaveURL(/.*#\/chat\/contacts/)
  })

  test('登录后拍卖行聊天输入框可正常使用', async ({ page }) => {
    const loginPage = new LoginPage(page)
    const chatPage = new ChatPage(page)

    await loginPage.goto()
    await loginPage.login(TEST_REGULAR_USER.username, TEST_REGULAR_USER.password)
    await loginPage.waitForLoginSuccess()

    await chatPage.gotoAuction()
    await page.waitForLoadState('networkidle')
    await page.waitForTimeout(2000)
    await loginPage.closeDialogs()
    await page.waitForTimeout(1000)

    await expect(page).toHaveURL(/.*#\/chat\/auction\/auction/)
    expect(await chatPage.isAuctionChatVisible()).toBe(true)

    await page.waitForSelector('.el-textarea__inner', { timeout: 10000 })

    await page.evaluate(() => {
      const textarea = document.querySelector('.el-textarea__inner') as HTMLTextAreaElement
      if (!textarea) return
      const nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLTextAreaElement.prototype, 'value')?.set
      if (nativeInputValueSetter) {
        textarea.focus()
        nativeInputValueSetter.call(textarea, '测试消息')
        textarea.dispatchEvent(new Event('input', { bubbles: true }))
        textarea.dispatchEvent(new Event('change', { bubbles: true }))
      }
    })
    await page.waitForTimeout(500)

    const sendBtn = page.locator('.send-box .el-button:not([disabled])')
    expect(await sendBtn.count()).toBeGreaterThan(0)
  })

  test('普通用户登录后可以访问聊天页面', async ({ page }) => {
    const loginPage = new LoginPage(page)
    const chatPage = new ChatPage(page)

    await loginPage.goto()
    await loginPage.login(TEST_REGULAR_USER.username, TEST_REGULAR_USER.password)
    await loginPage.waitForLoginSuccess()

    await chatPage.gotoAuction()
    await page.waitForTimeout(2000)
    await loginPage.closeDialogs()
    await page.waitForTimeout(1000)
    await expect(page).toHaveURL(/.*#\/chat\/auction\/auction/)

    expect(await chatPage.isAuctionChatVisible()).toBe(true)
  })
})
