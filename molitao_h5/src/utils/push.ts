interface PushMessage {
    messageId: string
    title: string
    content: string
    extras?: Record<string, any>
}

interface PushNotification {
    messageId: string
    title: string
    content: string
    extras?: Record<string, any>
}

declare const uni: any
declare const plus: any

class PushService {
    private isInitialized = false
    private registrationId = ''

    async init(): Promise<void> {
        if (this.isInitialized) {
            return
        }

        try {
            const platform = uni.getSystemInfoSync().platform

            if (platform === 'android' || platform === 'ios') {
                await this.initJPush()
            }

            this.isInitialized = true
        } catch (error) {
            console.error('[Push] 初始化失败:', error)
        }
    }

    private async initJPush(): Promise<void> {
        return new Promise((resolve, reject) => {
            if (typeof plus === 'undefined' || !plus.push) {
                resolve()
                return
            }

            plus.push.getClientInfo(
                (info: any) => {
                    this.registrationId = info.clientid || ''
                    console.log('[Push] Registration ID:', this.registrationId)
                    this.setupListeners()
                    resolve()
                },
                (error: any) => {
                    console.error('[Push] 获取客户端信息失败:', error)
                    reject(error)
                }
            )
        })
    }

    private setupListeners(): void {
        plus.push.addEventListener(
            'receive',
            (msg: any) => {
                console.log('[Push] 收到推送消息:', msg)
                this.handleReceive(msg)
            },
            false
        )

        plus.push.addEventListener(
            'click',
            (msg: any) => {
                console.log('[Push] 点击推送消息:', msg)
                this.handleClick(msg)
            },
            false
        )
    }

    private handleReceive(msg: any): void {
        const payload = this.parseMessage(msg)

        if (payload) {
            uni.$emit('push-receive', payload)
        }
    }

    private handleClick(msg: any): void {
        const payload = this.parseMessage(msg)

        if (payload) {
            uni.$emit('push-click', payload)

            if (payload.extras?.path) {
                uni.navigateTo({
                    url: payload.extras.path,
                })
            }
        }
    }

    private parseMessage(msg: any): PushMessage | null {
        try {
            if (typeof msg.content === 'string') {
                try {
                    const data = JSON.parse(msg.content)
                    return {
                        messageId: data.messageId || msg.uuid || '',
                        title: data.title || msg.title || '',
                        content: data.content || msg.content,
                        extras: data.extras || {},
                    }
                } catch {
                    return {
                        messageId: msg.uuid || '',
                        title: msg.title || '通知',
                        content: msg.content,
                        extras: {},
                    }
                }
            } else if (msg.content) {
                return {
                    messageId: msg.content.messageId || msg.uuid || '',
                    title: msg.content.title || msg.title || '',
                    content: msg.content.content || '',
                    extras: msg.content.extras || {},
                }
            }

            return null
        } catch (error) {
            console.error('[Push] 解析消息失败:', error)
            return null
        }
    }

    getRegistrationId(): string {
        return this.registrationId
    }

    async setAlias(alias: string): Promise<void> {
        return new Promise((resolve, reject) => {
            if (typeof plus === 'undefined' || !plus.push) {
                resolve()
                return
            }

            plus.push.setAlias(
                alias,
                () => {
                    console.log('[Push] 设置别名成功:', alias)
                    resolve()
                },
                (error: any) => {
                    console.error('[Push] 设置别名失败:', error)
                    reject(error)
                }
            )
        })
    }

    async setTags(tags: string[]): Promise<void> {
        return new Promise((resolve, reject) => {
            if (typeof plus === 'undefined' || !plus.push) {
                resolve()
                return
            }

            plus.push.setTags(
                tags,
                () => {
                    console.log('[Push] 设置标签成功:', tags)
                    resolve()
                },
                (error: any) => {
                    console.error('[Push] 设置标签失败:', error)
                    reject(error)
                }
            )
        })
    }

    async bindAliasAndTags(alias: string, tags: string[]): Promise<void> {
        await this.setAlias(alias)
        await this.setTags(tags)
    }

    createLocalNotification(notification: PushNotification): void {
        if (typeof plus === 'undefined' || !plus.push) {
            return
        }

        plus.push.createMessage(notification.content, JSON.stringify(notification.extras || {}), {
            title: notification.title,
        })
    }

    clearAllNotifications(): void {
        if (typeof plus === 'undefined' || !plus.push) {
            return
        }

        plus.push.clear()
    }

    setBadge(badge: number): void {
        if (typeof plus === 'undefined' || !plus.push) {
            return
        }

        if (uni.getSystemInfoSync().platform === 'ios') {
            plus.runtime.setBadgeNumber(badge)
        }
    }
}

export const pushService = new PushService()

export function usePush() {
    return pushService
}
