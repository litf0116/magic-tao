import { defineConfig, devices } from '@playwright/test'
import path from 'path'

const backendDir = path.resolve(__dirname, '../backend')
const backendHost = path.join(backendDir, 'src/TtWork.Project.Web.Host')

export default defineConfig({
  testDir: './tests/e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [['html', { outputFolder: 'playwright-report' }], ['list']],
  globalSetup: './tests/e2e/global-setup.ts',
  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:4201',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  timeout: 60_000,
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        launchOptions: {
          headless: process.env.CI ? true : false,
        },
      },
    },
  ],
})
