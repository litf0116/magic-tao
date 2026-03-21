/**
 * H5 PWA 推送服务
 * 使用 Web Push API 实现浏览器推送通知
 */

interface PushMessage {
    title: string
    content: string
    url?: string
    messageId?: string
    extras?: Record<string, any>
}

/**
 * 扩展的 PushSubscription 接口，包含 getKey 方法
 */
interface ExtendedPushSubscription extends PushSubscription {
    getKey(keyName: 'p256dh' | 'auth'): ArrayBuffer | null
}

/**
 * VAPID 公钥
 * 用于 Web Push API 订阅
 * 生成时间: 2026-03-16
 * 对应私钥应安全存储在后端服务器
 */
const VAPID_PUBLIC_KEY =
    'MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEwFIhGozygJ_yRTL6h3HQFXyCtD4xsJaZ-H9W2vu8ejKt3iWz4dvGdKnR1mnWHaT4msQmT4vblTr0_5H4Xmrp6g'

export class H5PushService {
    private registration: ServiceWorkerRegistration | null = null
    private subscription: PushSubscription | null = null

    /**
     * 初始化 H5 推送服务
     */
    async init(): Promise<boolean> {
        // #ifdef H5
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            console.warn('[H5Push] 当前浏览器不支持推送功能')
            return false
        }

        try {
            // 等待 Service Worker 准备就绪
            const registration = await navigator.serviceWorker.ready
            this.registration = registration

            console.log('[H5Push] Service Worker 已就绪')

            // 检查是否已订阅
            const subscription = await registration.pushManager.getSubscription()

            if (subscription) {
                console.log('[H5Push] 已存在推送订阅')
                this.subscription = subscription
                return true
            }

            console.log('[H5Push] 未找到订阅，等待用户授权')
            return false
        } catch (error) {
            console.error('[H5Push] 初始化失败:', error)
            return false
        }
        // #endif

        // #ifndef H5
        console.warn('[H5Push] H5 推送仅在 H5 环境可用')
        return false
        // #endif
    }

    /**
     * 请求推送权限并订阅
     */
    async requestPermission(): Promise<boolean> {
        // #ifdef H5
        if (!this.registration) {
            throw new Error('Service Worker 未注册')
        }

        try {
            // 1. 请求通知权限
            console.log('[H5Push] 请求通知权限...')
            const permission = await Notification.requestPermission()

            if (permission !== 'granted') {
                console.warn('[H5Push] 推送权限被拒绝:', permission)
                return false
            }

            console.log('[H5Push] 权限已授予')

            // 2. 订阅推送
            console.log('[H5Push] 订阅推送服务...')
            const subscription = await this.registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: this.urlBase64ToUint8Array(VAPID_PUBLIC_KEY) as BufferSource,
            })

            console.log('[H5Push] 订阅成功:', subscription.endpoint)

            // 3. 保存订阅信息到后端
            await this.sendSubscriptionToServer(subscription)

            this.subscription = subscription
            return true
        } catch (error: any) {
            console.error('[H5Push] 订阅失败:', error)

            // 用户友好的错误提示
            if (error.name === 'NotAllowedError') {
                console.warn('[H5Push] 用户取消了推送授权')
            } else if (error.name === 'AbortError') {
                console.error('[H5Push] 订阅被中止')
            }

            return false
        }
        // #endif

        return false
    }

    /**
     * 取消订阅
     */
    async unsubscribe(): Promise<void> {
        // #ifdef H5
        if (!this.subscription) {
            console.warn('[H5Push] 没有活跃的订阅')
            return
        }

        try {
            await this.subscription.unsubscribe()
            console.log('[H5Push] 已取消订阅')

            this.subscription = null
        } catch (error) {
            console.error('[H5Push] 取消订阅失败:', error)
        }
        // #endif
    }

    /**
     * 检查推送权限状态
     */
    async getPermissionStatus(): Promise<'default' | 'granted' | 'denied'> {
        // #ifdef H5
        if ('Notification' in window) {
            return await Notification.permission
        }
        // #endif

        return 'denied'
    }

    /**
     * 发送订阅信息到后端服务器
     */
    private async sendSubscriptionToServer(subscription: PushSubscription): Promise<void> {
        // #ifdef H5
        console.log('[H5Push] 发送订阅信息到后端...')

        try {
            const response = await fetch('/api/push/subscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    endpoint: subscription.endpoint,
                    keys: {
                        p256dh: this.arrayBufferToBase64(subscription.getKey('p256dh')!),
                        auth: this.arrayBufferToBase64(subscription.getKey('auth')!),
                    },
                    platform: 'h5',
                }),
            })

            if (!response.ok) {
                throw new Error(`订阅失败: ${response.status}`)
            }

            const result = await response.json()
            console.log('[H5Push] 订阅信息保存成功:', result)
        } catch (error) {
            console.error('[H5Push] 发送订阅信息失败:', error)
            // 不抛出错误，允许应用继续运行
        }
        // #endif
    }

    /**
     * 获取当前订阅状态
     */
    getSubscription(): PushSubscription | null {
        return this.subscription
    }

    /**
     * 工具方法：Base64 转 Uint8Array
     */
    private urlBase64ToUint8Array(base64String: string): Uint8Array {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4)
        const base64 = (base64String + padding).replace(/\-/g, '+').replace(/_/g, '/')

        const rawData = window.atob(base64)
        const outputArray = new Uint8Array(rawData.length)

        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i)
        }

        return outputArray
    }

    /**
     * 工具方法：ArrayBuffer 转 Base64
     */
    private arrayBufferToBase64(buffer: ArrayBuffer): string {
        const bytes = new Uint8Array(buffer)
        let binary = ''
        for (let i = 0; i < bytes.length; i++) {
            binary += String.fromCharCode(bytes[i])
        }
        return window.btoa(binary)
    }
}

// 导出单例
export const h5PushService = new H5PushService()

// 便捷的组合式函数
export function useH5Push() {
    return h5PushService
}
