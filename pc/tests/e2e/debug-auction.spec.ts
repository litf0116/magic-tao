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
        console.log('发现公告弹窗，尝试关闭')
        
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
        
        console.log('公告弹窗已关闭')
    }
}

test('调试拍卖行发送按钮', async ({ page }) => {
    await page.goto(`${BASE_URL}/#/auth/login`)
    await page.waitForTimeout(3000)
    
    const switchBtn = page.getByText('使用密码/验证码登录')
    const switchVisible = await switchBtn.isVisible().catch(() => false)
    console.log('切换按钮可见:', switchVisible)
    
    if (switchVisible) {
        await switchBtn.click()
        await page.waitForTimeout(1000)
    }
    
    await page.getByPlaceholder('请输入用户名').fill(TEST_USER.username)
    await page.getByPlaceholder('请输入密码').fill(TEST_USER.password)
    await page.getByRole('button', { name: '登录' }).click()
    
    await page.waitForTimeout(8000)
    console.log('登录成功')
    
    await page.goto(`${BASE_URL}/#/chat/auction/auction`)
    await page.waitForTimeout(5000)
    
    await closeAnnouncementModal(page)
    
    await page.screenshot({ path: '/tmp/auction-page.png', fullPage: true })
    console.log('当前 URL:', page.url())
    
    const textarea = page.locator('textarea')
    const textareaCount = await textarea.count()
    console.log('找到 textarea 数量:', textareaCount)
    
    const sendButton = page.locator('button:has-text("发送")')
    const isDisabled = await sendButton.isDisabled()
    console.log('发送按钮 disabled 状态:', isDisabled)
    
    await textarea.click()
    await textarea.fill('测试消息')
    await page.waitForTimeout(500)
    
    const isDisabledAfterInput = await sendButton.isDisabled()
    console.log('输入后发送按钮 disabled 状态:', isDisabledAfterInput)
    
    await page.screenshot({ path: '/tmp/auction-after-input.png', fullPage: true })
    
    const inputValue = await textarea.inputValue()
    console.log('输入框的值:', JSON.stringify(inputValue))
    
    const buttonAttrs = await sendButton.evaluate((el) => {
        const attrs: Record<string, string> = {}
        Array.from(el.attributes).forEach((attr) => {
            attrs[attr.name] = attr.value
        })
        return attrs
    })
    console.log('按钮属性:', JSON.stringify(buttonAttrs, null, 2))
    
    if (!isDisabledAfterInput) {
        console.log('尝试点击发送按钮...')
        await sendButton.click()
        await page.waitForTimeout(1000)
        await page.screenshot({ path: '/tmp/auction-after-send.png', fullPage: true })
    } else {
        console.log('发送按钮仍然是 disabled 状态，无法点击')
    }
})