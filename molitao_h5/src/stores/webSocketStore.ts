import { ref } from 'vue'
import { defineStore } from 'pinia'
import api from '@/utils/api'
import { Tips } from '@/composables'
import { Goto } from '@/composables/goto'

interface WebSocketState {
    socket: WechatMiniprogram.SocketTask | null
    isConnected: boolean
    websocketId: number
    reconnectAttempts: number
    reconnectTimer: number | undefined
}

export const useWebSocketStore = defineStore('webSocketStore', () => {
    const state = ref<WebSocketState>({
        socket: null,
        isConnected: false,
        websocketId: 0,
        reconnectAttempts: 0,
        reconnectTimer: undefined,
    })

    const messageHandlers = ref<Array<(message: any) => void>>([])

    const addMessageHandler = (handler: (message: any) => void) => {
        messageHandlers.value.push(handler)
    }

    const removeMessageHandler = (handler: (message: any) => void) => {
        const index = messageHandlers.value.indexOf(handler)
        if (index > -1) {
            messageHandlers.value.splice(index, 1)
        }
    }

    const notifyHandlers = (message: any) => {
        messageHandlers.value.forEach((handler) => {
            try {
                handler(message)
            } catch (error) {
                console.error('WebSocket message handler error:', error)
            }
        })
    }

    const sleep = (ms: number): Promise<void> => {
        return new Promise((resolve) => setTimeout(resolve, ms))
    }

    const shouldReconnect = (): boolean => {
        const curPage = getCurrentPages()
        const route = curPage[curPage.length - 1]?.route
        return route?.startsWith('pages/chat/') || false
    }

    const connect = async (reconnect = false): Promise<string> => {
        if (state.value.socket !== null && !reconnect) {
            return 'ok'
        }

        try {
            const res = await api.ws.preConnect()
            state.value.websocketId = Number(res.websocketId)

            if (state.value.socket) {
                state.value.socket.close({})
            }
            state.value.socket = null
            await sleep(1000)
            clearTimeout(state.value.reconnectTimer)

            state.value.socket = wx.connectSocket({
                url: res.server,
                success: (result) => {
                    console.log('WebSocket connection initiated:', result)
                },
            })

            state.value.socket.onMessage((e: any) => {
                try {
                    let msg = e.data
                    if (typeof msg === 'string') {
                        msg = JSON.parse(msg)
                    }
                    if (msg.type === 'Error') {
                        return
                    }
                    notifyHandlers(msg)
                } catch (error) {
                    console.error('WebSocket message processing error:', error)
                }
            })

            state.value.socket.onClose(() => {
                clearTimeout(state.value.reconnectTimer)
                state.value.isConnected = false
                state.value.socket = null

                if (shouldReconnect()) {
                    state.value.reconnectTimer = setTimeout(() => {
                        connect(true)
                    }, 5000)
                }
            })

            state.value.socket.onError((error) => {
                console.error('WebSocket error:', error)
                state.value.socket = null
                state.value.isConnected = false
                clearTimeout(state.value.reconnectTimer)

                state.value.reconnectTimer = setTimeout(() => {
                    connect(true)
                }, 5000)
            })

            state.value.socket.onOpen(() => {
                state.value.isConnected = true
                state.value.reconnectAttempts = 0
                Tips.success('聊天服务器连接成功')
                return 'ok'
            })

            return 'ok'
        } catch (error) {
            console.error('WebSocket connection failed:', error)
            throw error
        }
    }

    const disconnect = () => {
        clearTimeout(state.value.reconnectTimer)
        if (state.value.socket) {
            state.value.socket.close({})
            state.value.socket = null
        }
        state.value.isConnected = false
        state.value.reconnectAttempts = 0
    }

    const sendMessage = (message: any): boolean => {
        if (!state.value.socket || !state.value.isConnected) {
            console.warn('WebSocket not connected, cannot send message')
            return false
        }

        try {
            const messageStr = typeof message === 'string' ? message : JSON.stringify(message)
            state.value.socket.send({
                data: messageStr,
                success: () => {
                    console.log('Message sent successfully')
                },
                fail: (error) => {
                    console.error('Failed to send message:', error)
                },
            })
            return true
        } catch (error) {
            console.error('Error sending message:', error)
            return false
        }
    }

    return {
        socket: computed(() => state.value.socket),
        isConnected: computed(() => state.value.isConnected),
        websocketId: computed(() => state.value.websocketId),

        connect,
        disconnect,
        sendMessage,
        addMessageHandler,
        removeMessageHandler,
    }
})
