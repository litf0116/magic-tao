import { type FullConfig } from '@playwright/test'
import { spawn, ChildProcess } from 'child_process'
import * as http from 'http'

async function waitForBackend(url: string, timeout = 30000): Promise<boolean> {
  const start = Date.now()
  while (Date.now() - start < timeout) {
    try {
      await new Promise<void>((resolve, reject) => {
        http.get(url, (res) => {
          res.resume()
          res.on('end', () => resolve())
        }).on('error', reject)
      })
      return true
    } catch {
      await new Promise((r) => setTimeout(r, 1000))
    }
  }
  return false
}

export default async function globalSetup(config: FullConfig) {
  const backendUrl = 'http://127.0.0.1:12580'

  console.log('\n🔍 检查后端服务是否已在运行...')
  const isRunning = await waitForBackend(backendUrl, 5000)

  if (isRunning) {
    console.log('✅ 后端服务已在运行，跳过启动')
    return
  }

  console.log('🚀 启动后端服务...')
  const backendProcess = spawn('dotnet', ['run'], {
    cwd: '/Users/mac/workspace/magic-tao/backend/src/TtWork.Project.Web.Host',
    stdio: 'inherit',
    env: { ...process.env },
  })

  console.log('⏳ 等待后端服务就绪...')
  const ready = await waitForBackend(backendUrl, 60000)

  if (!ready) {
    console.error('❌ 后端服务启动超时')
    backendProcess?.kill()
    throw new Error('Backend service failed to start within 60 seconds')
  }

  console.log('✅ 后端服务已就绪')
  ;(globalThis as any).__backendProcess = backendProcess
}
