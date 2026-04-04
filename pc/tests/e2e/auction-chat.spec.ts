import { test, expect } from '@playwright/test'

const BASE_URL = 'http://localhost:4200'

const TEST_USER = {
    username: 'feifei',
    password: '123456',
}

async function closeAnnouncementModal(page) {
    const overlay = page.locator('.el-overlay.is-message-box')
    const isVisible = await overlay.isVisible().catch(() => false)
    
    if (isVisible) {
        await page.keyboard.press('Escape')
        await page.waitForTimeout(500)
        
        const stillVisible = await overlay.isVisible().catch(() => false)
        if (stillVisible) {
            const buttons = page.locator('.el-overlay.is-message-box .el-message-box__btns .el-button')
            const count = await buttons.count()
            if (count > 0) {
                await buttons.last().click()
                await page.waitForTimeout(500)
            }
        }
    }
}

async function login(page: any) {
    await page.goto(`${BASE_URL}/#/auth/login`)
    await page.waitForTimeout(3000)
    
    const switchBtn = page.getByText('使用密码/验证码登录')
    const switchVisible = await switchBtn.isVisible().catch(() => false)
    
    if (switchVisible) {
        await switchBtn.click()
        await page.waitForTimeout(1000)
    }
    
    await page.getByPlaceholder('请输入用户名').fill(TEST_USER.username)
    await page.getByPlaceholder('请输入密码').fill(TEST_USER.password)
    await page.getByRole('button', { name: '登录' }).click()
    
    await page.waitForTimeout(8000)
}

test.describe('拍卖场消息发送测试', () => {
    test.beforeEach(async ({ page }) => {
        await login(page)
    })

    test('拍卖场-发送文本消息', async ({ page }) => {
        await page.goto(`${BASE_URL}/#/chat/auction/auction`)
        await page.waitForTimeout(3000)
        
        await closeAnnouncementModal(page)
        
        const textarea = page.locator('textarea')
        await expect(textarea).toBeVisible()
        
        const sendButton = page.locator('button:has-text("发送")')
        
        const message = `自动化测试消息 ${Date.now()}`
        
        await textarea.click()
        await textarea.fill(message)
        await page.waitForTimeout(300)
        
        const isDisabled = await sendButton.isDisabled()
        expect(isDisabled).toBe(false)
        
        await sendButton.click()
        await page.waitForTimeout(2000)
        
        const inputValue = await textarea.inputValue()
        expect(inputValue).toBe('')
        
        const messageSent = page.locator(`.message-content:has-text("${message}")`)
        await expect(messageSent.first()).toBeVisible({ timeout: 5000 })
    })

    test('拍卖场-发送空消息时被禁用', async ({ page }) => {
        await page.goto(`${BASE_URL}/#/chat/auction/auction`)
        await page.waitForTimeout(3000)
        
        await closeAnnouncementModal(page)
        
        const textarea = page.locator('textarea')
        await expect(textarea).toBeVisible()
        
        const sendButton = page.locator('button:has-text("发送")')
        
        const initialDisabled = await sendButton.isDisabled()
        expect(initialDisabled).toBe(true)
        
        await textarea.click()
        await textarea.fill('   ')
        await page.waitForTimeout(300)
        
        const afterSpaceDisabled = await sendButton.isDisabled()
        expect(afterSpaceDisabled).toBe(true)
    })

    test('拍卖场-发送表情消息', async ({ page }) => {
        await page.goto(`${BASE_URL}/#/chat/auction/auction`)
        await page.waitForTimeout(3000)
        
        await closeAnnouncementModal(page)
        
        const textarea = page.locator('textarea')
        await expect(textarea).toBeVisible()
        
        const sendButton = page.locator('button:has-text("发送")')
        
        await textarea.click()
        await textarea.fill('😀')
        await page.waitForTimeout(300)
        
        const isDisabled = await sendButton.isDisabled()
        expect(isDisabled).toBe(false)
        
        await sendButton.click()
        await page.waitForTimeout(2000)
        
        const inputValue = await textarea.inputValue()
        expect(inputValue).toBe('')
    })
})