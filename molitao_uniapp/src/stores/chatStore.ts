import api from '@/utils/api'
import { defineStore } from 'pinia'
import { uniqBy, orderBy } from 'lodash'
import { useEventBus } from '@vueuse/core'
import {
    ChatMessageStatus,
    type ChatListItem,
    type ChatMessage,
    type UserDtoBase,
    ChatMessageType,
} from '../composables/types'
import { useStorageRef } from '@/composables/useStorageRef'
import { useAuctionStore } from './auctionStore'
import { convertImageUrl } from '@/utils/imageUrlConverter'

// 测试友好：导出一个纯函数，方便单元测试历史合并逻辑
export function mergeHistoryForChannel(
    groupName: string,
    newItems: ChatMessage[],
    existing: ChatMessage[] = [],
    reloadFlag: boolean = false
): ChatMessage[] {
    const t = groupName.split('_')
    let id = parseInt(t[0])
    if (id > 0) id = -id
    if (newItems.length > 0) {
        if (existing.length > 0 && !reloadFlag) {
            return uniqBy(orderBy([...newItems, ...existing], [(m) => m.time], ['asc']), 'id')
        }
        return newItems
    } else if (reloadFlag) {
        return []
    }
    return existing
}
type ChannelType = { chan: string; online: number }

const AuctionChat: ChatListItem = {
    id: -1,
    name: 'auction',
    type: ChatListItemType.group,
    time: new Date().getTime(),
    lastMsg: '',
    unread: 0,
    order: 99,
}

export const CREATEGROUPEVENT = 'CREATE_GROUP_EVENT'
export const onmessageKey = Symbol('onmessageKey')

export const useChatStore = defineStore('chatStore', () => {
    const bus = useEventBus(onmessageKey)
    // LINK - ChatStore State

    let gsocket: WechatMiniprogram.SocketTask | null = null
    const gsocketTimeId = ref<number | undefined>(undefined)

    const friends = ref<UserDtoBase[]>([])
    const friends0 = ref<UserDtoBase[]>([])
    const groups = ref<ChannelType[]>([])
    const currentChat = ref(AuctionChat)

    //聊天对象表
    const chatList: Ref<ChatListItem[]> = useStorageRef<ChatListItem[]>('chatList', [AuctionChat])
    const chatMap = ref<Map<string, ChatMessage[]>>(new Map())

    //LINK - ChatStore Getter

    //LINK - ChatStore Actions
    const close = () => {
        wx.closeSocket()
    }
    const websocketId = useStorageRef<number>('websocketId', 0)

    const isConnect = () => {
        return gsocket !== null
    }

    function sleep(ms: number) {
        return new Promise((resolve) => setTimeout(resolve, ms))
    }

    function getChatList() {
        return new Promise<void>((resolve, reject) => {
            api.client.getChatList().then((res: any) => {
                chatList.value = res
                return resolve()
            })
            return resolve()
        })
    }

    const connectServer = (reconnect = false) => {
        return new Promise<string>(async (resolve) => {
            if (gsocket !== null && !reconnect) {
                return resolve('ok')
            }
            await api.ws.preConnect().then(async (res: any) => {
                websocketId.value = Number(res.websocketId)
                if (gsocket) {
                    gsocket!.close({})
                }
                gsocket = null
                await sleep(1000)
                clearTimeout(gsocketTimeId.value)

                gsocket = wx.connectSocket({
                    url: res.server,
                    success: (res) => {
                        //接口调用成功的回调函数
                        // console.log('connectSocket 接口调用成功', res)
                    },
                })

                gsocket.onMessage((e: any) => {
                    // console.log('websocket message', e)
                    try {
                        let msg = e.data
                        if (typeof msg === 'string') {
                            msg = JSON.parse(msg)
                        }
                        if (msg.type === 'Error') {
                            // alert(msg.receipt)
                            // console.log('websocket message error', msg)
                            return
                        }
                        onmessage(msg)
                    } catch (e) {
                        // console.log('onMessage Error', e)
                        return
                    }
                })

                gsocket.onClose(() => {
                    clearTimeout(gsocketTimeId.value)
                    // Tips.info('聊天服务器连接断开')
                    // console.log('websocket disconnect')
                    // api.ws.offline({ websocketId: websocketId.value })
                    //检查是不是在聊天室路由,如果是,则重新连接
                    const curPage = getCurrentPages()
                    const route = curPage[curPage.length - 1].route! //获取当前页面的路由
                    // console.log('route', route)
                    if (route.startsWith('pages/chat/')) {
                        // console.log('聊天室路由,重连')
                        gsocket = null
                        gsocketTimeId.value = setTimeout(function () {
                            connectServer(true)
                        }, 5000)
                    } else {
                        gsocket = null
                        // console.log('非聊天室路由,不重连')
                    }
                })

                gsocket.onError((e) => {
                    // console.log('websocket error 5秒后重新连接', e)
                    gsocket = null
                    clearTimeout(gsocketTimeId.value)
                    gsocketTimeId.value = setTimeout(function () {
                        connectServer(true)
                    }, 5000)
                })

                gsocket.onOpen(() => {
                    // console.log('onOpen')
                    // await getGropus()
                    // console.log('聊天服务器连接成功')
                    Tips.success('聊天服务器连接成功')
                    return resolve('ok')
                })
            })
        })
    }

    const auctionStore = useAuctionStore()

    const onmessage = function (msg: ChatMessage) {
        // 转换消息类型：将数值类型转换为字符串类型
        if (typeof msg.type === 'number') {
            const typeMap: { [key: number]: ChatMessageType } = {
                1: ChatMessageType.Text,
                2: ChatMessageType.Image,
                3: ChatMessageType.File,
                10: ChatMessageType.Receipt,
                100: ChatMessageType.Welcome,
                101: ChatMessageType.Goodbye,
                102: ChatMessageType.BanUser,
                110: ChatMessageType.Backout,
                1000: ChatMessageType.AuctionStart,
                1002: ChatMessageType.AuctionBid,
                1010: ChatMessageType.AuctionEnd,
                1011: ChatMessageType.AuctionDeal,
                2000: ChatMessageType.KasecStatusChanged,
                '-1': ChatMessageType.Error,
            }
            if (typeMap[msg.type as number]) {
                msg.type = typeMap[msg.type as number]
            }
        }

        // console.log('onmessage', msg)
        bus.emit(msg)
        //未读消息数量
        const unreadCount = uni.getStorageSync('unreadCount')
        if (msg.chan == null) {
            const count = unreadCount === '' ? 1 : Number(unreadCount) + 1
            uni.setStorageSync('unreadCount', count.toString())
            uni.$emit('eventUnread')
        }

        if (msg.receipt) {
            if (msg.receipt === '用户不在线') {
                // alert('用户不在线')
                return
            } else if (msg.receipt === '发送成功') {
                return
            }
            return
        }

        if (msg.chan && msg.type === ChatMessageType.Goodbye) {
            //房主解散房间,所有人退出
            const _id = parseInt(msg.chan.split('_')[0])
            api.ws.leaveChannel({ chan: msg.chan }).then(() => {
                //delete from chatList
                chatList.value = chatList.value.filter((item) => item.id !== _id)
                chatMap.value.delete(`${_id}`)
                groups.value = groups.value.filter((item) => item.chan !== msg.chan)
                if (currentChat.value.id === _id) {
                    Tips.info('房间已解散')
                    Goto.lobby()
                }
            })
        } else if (msg.chan) {
            if (msg.msg === CREATEGROUPEVENT) {
                getGropus()
                return
            }
            //处理群聊天消息
            const _t = msg.chan.split('_')
            let _id = parseInt(_t[0])
            if (_id > 0) _id = -_id
            if (_id === null) return
            //
            const _name = _t[1]
            const old = chatList.value.find((item) => item.id === _id && item.id !== currentChat.value.id)
            chatList.value = [
                {
                    id: _id,
                    name: _name,
                    type: ChatListItemType.group,
                    time: msg.time,
                    lastMsg: msg.msg,
                    avatar: msg.avatar,
                    unread: old ? old.unread + 1 : 0,
                    order: _id === 0 ? 100 : _id === -1 ? 99 : 0,
                    msg: msg,
                },
                ...chatList.value.filter((item) => item.id !== _id),
            ]
            if (chatMap.value.has(`${_id}`)) {
                chatMap.value.get(`${_id}`)!.push(msg)
                if (chatMap.value.get(`${_id}`)!.length > 800) {
                    chatMap.value.set(`${_id}`, chatMap.value.get(`${_id}`)!.slice(-750))
                }
            } else {
                chatMap.value.set(`${_id}`, [msg])
            }
        } else if (msg.type === 'Text' || msg.type === 'Image' || msg.type === 'File') {
            //处理私聊消息（不包括AuctionDeal，由下面的特殊处理逻辑处理）
            const old = chatList.value.find((item) => item.id === msg.from && item.id !== currentChat.value.id)
            if (msg.from === null) return
            chatList.value = [
                {
                    id: msg.from,
                    name: msg.fromName!,
                    type: ChatListItemType.user,
                    time: msg.time,
                    lastMsg: msg.msg,
                    avatar: msg.avatar,
                    unread: old ? old.unread + 1 : 0,
                    order: 0,
                    msg: msg,
                },
                ...chatList.value.filter((item) => item.id !== msg.from),
            ]
            if (chatMap.value.has(`${msg.from}`)) {
                chatMap.value.get(`${msg.from}`)!.push(msg)
            } else {
                chatMap.value.set(`${msg.from}`, [msg])
            }
        }

        // 处理卡秒状态变更系统消息
        if (msg.type === ChatMessageType.KasecStatusChanged && msg.payload) {
            const { auctionItemId, isKasec } = msg.payload
            // 只处理当前秒杀频道
            if (currentChat.value.id === -1) {
                if (typeof auctionStore.syncKasecStatus === 'function') {
                    auctionStore.syncKasecStatus(auctionItemId)
                }
                // 保持原始的KasecStatusChanged类型，设置消息内容
                msg.msg = isKasec ? '秒杀主持已开启卡秒，需三倍加价！' : '卡秒已关闭，恢复正常加价'
                msg.time = Date.now()
                // 直接添加到聊天记录中，不转换类型
                if (chatMap.value.has('-1')) {
                    chatMap.value.get('-1')!.push(msg)
                } else {
                    chatMap.value.set('-1', [msg])
                }
            }
            return
        }

        // 特殊处理：解码卡秒消息和出价消息
        if (msg.type === ChatMessageType.AuctionBid && msg.payload) {
            // console.log('=== 检测到出价消息 ===')
            // console.log('消息类型:', msg.type)
            // console.log('消息payload:', msg.payload)
            // console.log('payload类型:', typeof msg.payload)

            // 检查是否是编码的卡秒消息
            if (msg.payload.messageType === 'KasecStatusChanged' && msg.payload.encoded) {
                // console.log('=== 检测到编码的卡秒消息 ===')
                // console.log('卡秒消息payload:', msg.payload)

                // 解码：将消息类型从 AuctionBid 转换为 KasecStatusChanged
                msg.type = ChatMessageType.KasecStatusChanged

                // 处理卡秒状态变更
                const { auctionItemId, isKasec } = msg.payload
                if (auctionItemId && typeof isKasec === 'boolean') {
                    // 只处理当前秒杀频道
                    if (currentChat.value.id === -1) {
                        if (typeof auctionStore.syncKasecStatus === 'function') {
                            auctionStore.syncKasecStatus(auctionItemId)
                        }
                        // 设置消息内容（移除闪电符号，因为组件中已有图标）
                        msg.msg = isKasec ? '秒杀主持已开启卡秒模式，需三倍加价！' : '卡秒已关闭，恢复正常加价规则'
                        msg.time = Date.now()
                        // 直接添加到聊天记录中
                        if (chatMap.value.has('-1')) {
                            chatMap.value.get('-1')!.push(msg)
                        } else {
                            chatMap.value.set('-1', [msg])
                        }
                    }
                }
                return
            }

            // 处理正常的出价消息
            // console.log('=== 处理正常出价消息 ===')
            // 使用新的增量更新方法，避免重新请求整个列表
            if (typeof (auctionStore as any).updateAuctionItemFromBidMessage === 'function') {
                ;(auctionStore as any).updateAuctionItemFromBidMessage(msg.payload)
            }
        }

        // 检查是否是编码的成交消息
        if (msg.type === ChatMessageType.AuctionEnd && msg.payload) {
            // console.log('=== 检测到AuctionEnd消息 ===')
            // console.log('消息类型:', msg.type)
            // console.log('消息payload:', msg.payload)
            // console.log('payload类型:', typeof msg.payload)

            // 检查是否是编码的成交消息
            if (msg.payload.messageType === 'AuctionDeal' && msg.payload.encoded) {
                // console.log('=== 检测到编码的成交消息 ===')
                // console.log('成交消息payload:', msg.payload)

                // 解码：将消息类型从 AuctionEnd 转换为 AuctionDeal
                msg.type = ChatMessageType.AuctionDeal

                // 恢复原始payload
                if (msg.payload.originalPayload) {
                    msg.payload = msg.payload.originalPayload
                }

                // console.log('=== 成交消息解码完成 ===')
                // console.log('解码后消息类型:', msg.type)
                // console.log('解码后payload:', msg.payload)
            }
        }

        // 特殊处理：监听秒杀结束消息，为秒中用户创建聊天频道
        // 需要在解码后处理，因为 AuctionDeal 消息被编码为 AuctionEnd 传输，解码后类型变回 AuctionDeal
        if ((msg.type === 'AuctionEnd' || msg.type === ChatMessageType.AuctionDeal) && msg.payload) {
            // console.log('检测到秒杀结束消息，为秒中用户创建聊天频道', msg.payload)
            const userStore = useUserStore()

            // 检查 payload 中是否有成交用户信息
            const dealUserId = msg.payload.dealUserId || msg.payload.DealUserId

            if (dealUserId) {
                // 优先使用秒杀成交时间，而不是消息接收时间
                const dealTime = msg.payload.dealTime || msg.payload.DealTime || msg.time || new Date().getTime()

                // 发送者（秒杀主持）：为秒中用户创建聊天会话
                if (msg.from === userStore.user.id) {
                    addAuctionDealUser(msg.payload, 'AuctionDeal', dealTime)
                }

                // 接收者（中标用户）：为秒杀师创建聊天会话
                if (msg.to === userStore.user.id) {
                    // 中标用户需要看到秒杀师（msg.from），而不是自己
                    const existingChat = chatList.value.find((item) => item.id === msg.from)
                    if (!existingChat) {
                        chatList.value = [
                            {
                                id: msg.from,
                                name: msg.fromName!,
                                type: ChatListItemType.user,
                                time: dealTime,
                                lastMsg: msg.msg,
                                avatar: msg.avatar,
                                unread: 0,
                                order: 0,
                                msg: msg,
                            },
                            ...chatList.value.filter((item) => item.id !== msg.from),
                        ]
                        if (!chatMap.value.has(`${msg.from}`)) {
                            chatMap.value.set(`${msg.from}`, [msg])
                        }
                    }
                }
            }
        }
    }

    const SetCurrentChat = (item: ChatListItem) => {
        // console.log('SetCurrentChat', item)
        uni.removeStorageSync('unreadCount')
        currentChat.value = item
    }
    const SetCurrentChatId = (id: number, name = '', isGroup = true) => {
        return new Promise<void>((resolve) => {
            // console.log('SetCurrentChatId', id, name, isGroup)
            const item = chatList.value.find((item) => item.id === id)
            // console.log('find', item)
            if (item) currentChat.value = item
            else
                currentChat.value = {
                    id: id,
                    name: name,
                    type: isGroup ? ChatListItemType.group : ChatListItemType.user,
                    time: new Date().getTime(),
                    lastMsg: '',
                    unread: 0,
                    order: 0,
                }
            // console.log('currentChat.value', currentChat.value)
            return resolve()
        })
    }

    const SetUnread = (item: ChatListItem, number: number) => {
        item.unread = number
    }

    const getGropus = function () {
        groups.value = []
        return new Promise(async (resolve) => {
            await api.ws.getChannels().then((res: any) => {
                groups.value = res.channels
                return resolve(res.channels)
            })
        })
    }

    // 辅助：判断是否为系统频道（- 开头的编号，如 -10_announcement、-11_newbie 等）
    const isSystemChannelGroup = (groupName: string) => {
        const t = groupName.split('_')
        const id = parseInt(t[0])
        return id < 0
    }

    // 辅助：系统频道历史合并的明确实现（便于单元测试）
    const applySystemChannelRules = (
        groupName: string,
        newItems: ChatMessage[],
        existing: ChatMessage[] = [],
        reloadFlag: boolean
    ) => {
        const t = groupName.split('_')
        let id = parseInt(t[0])
        if (id > 0) id = -id
        const key = `${id}`
        if (newItems.length > 0) {
            if (chatMap.value.has(key) && !reloadFlag) {
                chatMap.value.set(key, uniqBy(orderBy([...newItems, ...existing], [(msg) => msg.time], ['asc']), 'id'))

                // Note: 移除了内联导出的测试辅助函数，统一在文件顶部定义可测试导出的实现
            } else {
                chatMap.value.set(key, newItems)
            }
        } else if (reloadFlag) {
            if (!chatMap.value.has(key)) chatMap.value.set(key, [])
        }
    }

    const getGroupHistory = (groupName = '', lastTime?: number, reload = false) => {
        lastTime = lastTime || new Date().getTime()
        return new Promise<ChatMessage[]>((resolve, reject) => {
            api.message
                .getChanHistory({ chan: groupName, lastTime: lastTime })
                .then((res) => {
                    const _t = groupName.split('_')
                    let _id = parseInt(_t[0])
                    if (_id > 0) _id = -_id
                    const key = `${_id}`
                    const existing = chatMap.value.get(key) || []
                    if (res.items && res.items.length > 0) {
                        if (chatMap.value.has(key) && !reload) {
                            chatMap.value.set(
                                key,
                                uniqBy(orderBy([...res.items, ...existing], [(msg) => msg.time], ['asc']), 'id')
                            )
                        } else {
                            chatMap.value.set(key, res.items)
                        }
                    } else if (reload) {
                        if (!chatMap.value.has(key)) chatMap.value.set(key, [])
                    }
                    return resolve(res.items || [])
                })
                .catch((error) => {
                    console.error('[getGroupHistory] API 调用失败:', error)
                    return reject(error)
                })
        })
    }

    const getPrivateHistory = (id: number, lastTime: number | null = null, reload = false) => {
        if (isNaN(id)) {
            // console.error('Invalid id for getPrivateHistory:', id)
            return Promise.resolve([])
        }
        lastTime = lastTime || new Date().getTime()
        return new Promise<ChatMessage[]>((resolve) => {
            api.message.getPrivateHistory({ id: id, lastTime: lastTime! }).then((res) => {
                if (res.items) {
                    if (chatMap.value.has(`${id}`) && !reload) {
                        chatMap.value.set(
                            `${id}`,
                            uniqBy(
                                orderBy([...res.items, ...chatMap.value.get(`${id}`)!], [(msg) => msg.time], ['asc']),
                                'id'
                            )
                        )
                    } else {
                        chatMap.value.set(`${id}`, res.items)
                    }
                }
                return resolve(res.items || [])
            })
        })
    }

    const leaveChannel = async (chan: string) => {
        await api.ws.leaveChannel({ chan: chan }).then(async (res) => {
            chatList.value = chatList.value.filter((item) => item.id !== parseInt(chan.split('_')[0]))
            chatMap.value.delete(`${parseInt(chan.split('_')[0])}`)
            Goto.lobby()
        })
    }

    const joinChannel = async (chan: string) => {
        return new Promise(async (resolve, reject) => {
            api.ws
                .subChannel({ websocketId: websocketId.value, channel: chan })
                .then(async (res) => {
                    await sendChannelMsg('', chan, ChatMessageType.Welcome)
                    return resolve('ok')
                })
                .catch((e) => {
                    return reject(e)
                })
        })
    }
    const deleteChannel = async (chan: string) => {
        return new Promise(async (resolve) => {
            sendChannelMsg('', chan, ChatMessageType.Goodbye).then(() => {
                api.ws.delChannel({ chan: chan }).then(() => {
                    return resolve('ok')
                })
            })
        })
    }

    /**
     * 系统频道常量定义
     * -10_announcement: 系统公告频道
     * -11_newbie: 新手群聊频道
     */
    const SYSTEM_CHANNELS = {
        ANNOUNCEMENT: '-10_announcement',
        NEWBIE: '-11_newbie',
    } as const

    const hasGroup = (chan: string) => {
        const systemChannelList = [SYSTEM_CHANNELS.ANNOUNCEMENT, SYSTEM_CHANNELS.NEWBIE] as string[]
        if (systemChannelList.includes(chan)) {
            return { chan, online: 0 }
        }

        return groups.value.find((item) => item.chan === chan)
    }

    const hasMyGroup = () => {
        return groups.value.find((item) => item.chan.startsWith(`-${websocketId.value}`))
    }

    const createChannel = (name: string) => {
        return new Promise(async (resolve, reject) => {
            if (!name) {
                return reject('请输入群名称')
            } else {
                const reg =
                    /^((?:[\u3400-\u4DB5\u4E00-\u9FEA\uFA0E\uFA0F\uFA11\uFA13\uFA14\uFA1F\uFA21\uFA23\uFA24\uFA27-\uFA29]|[\uD840-\uD868\uD86A-\uD86C\uD86F-\uD872\uD874-\uD879][\uDC00-\uDFFF]|\uD869[\uDC00-\uDED6\uDF00-\uDFFF]|\uD86D[\uDC00-\uDF34\uDF40-\uDFFF]|\uD86E[\uDC00-\uDC1D\uDC20-\uDFFF]|\uD873[\uDC00-\uDEA1\uDEB0-\uDFFF]|\uD87A[\uDC00-\uDFE0])|([0-9a-zA-Z])){4,12}$/
                if (!reg.test(name)) {
                    return reject('群名称只能是4-12位中文或字母数字组合')
                }

                const chan = `-${websocketId.value}_${name}`
                joinChannel(chan)
                    .then(async () => {
                        await sendChannelMsg(CREATEGROUPEVENT, '0_lobby', ChatMessageType.Welcome)
                        return resolve('ok')
                    })
                    .catch((e) => {
                        return reject(e)
                    })
            }
        })
    }
    const sendChannelMsg = async (msg = '', chan: string, type = ChatMessageType.Text, payload: any = {}) => {
        chan = chan || `${currentChat.value.id}_${currentChat.value.name}`
        return new Promise(async (resolve) => {
            if (!chan) {
                alert('请先加入群聊')
                return
            }
            const userStore = useUserStore()
            const data: ChatMessage = {
                type: type,
                chan: chan,
                from: websocketId.value,
                fromName: userStore.user.name,
                avatar: userStore.user.headImgUrl,
                msg: msg,
                payload: payload,
            }

            return api.ws
                .sendChannelMsg({
                    from: websocketId.value,
                    chan: chan,
                    message: data,
                })
                .then((res) => {
                    return resolve(res)
                })
                .catch((err) => {
                    return resolve(null)
                })
        })
    }

    const sendMsg = async (
        to: number,
        toName: string,
        avatar: string,
        msg: string,
        type = ChatMessageType.Text,
        payload: any = {}
    ) => {
        return new Promise(async (resolve) => {
            const userStore = useUserStore()
            const data: ChatMessage = {
                type: type,
                from: websocketId.value,
                fromName: userStore.user.name,
                avatar: userStore.user.headImgUrl,
                msg: msg,
                to: to,
                payload: payload,
            }
            // console.log('sendMsg', data)
            await api.ws
                .sendMsg({
                    from: websocketId.value,
                    to: to,
                    message: data,
                    isReceipt: true,
                })
                .then((res) => {
                    chatList.value = [
                        {
                            id: to,
                            name: toName,
                            type: ChatListItemType.user,
                            time: data.time,
                            lastMsg: data.msg,
                            avatar: avatar,
                            unread: 0,
                            order: 0,
                            msg: res.data.message,
                        },
                        ...chatList.value.filter((item) => item.id !== to),
                    ]
                    if (chatMap.value.has(`${to}`)) {
                        chatMap.value.get(`${to}`)!.push(res.data.message)
                    } else {
                        chatMap.value.set(`${to}`, [res.data.message])
                    }

                    return resolve(res)
                })
        })
    }

    const getUserAvatar = (id: number) => {
        return convertImageUrl('https://image.molitao.top/avater.png')
    }

    const getUserFriends = (status = true) => {
        api.userFriend.getUserFriends({ id: websocketId.value, status: status }).then((res) => {
            if (status) friends.value = res.items!
            else friends0.value = res.items!
        })
    }
    const getCurrentName = () => {
        return currentChat.value.name === 'lobby' ? '勇者招募所' : currentChat.value.name
    }

    const addChatList = (id: number, name: string, avatar: string) => {
        if (chatList.value.find((item) => item.id === id)) return
        chatList.value = [
            {
                id: id,
                name: name,
                type: ChatListItemType.user,
                time: new Date().getTime(),
                lastMsg: '',
                avatar: avatar,
                unread: 0,
                order: id === 0 ? 100 : id === -1 ? 99 : 0,
            },
            ...chatList.value.filter((item) => item.id !== id),
        ]
    }

    const deleteChat = (x: ChatListItem) => {
        // console.log('deleteChat', x)
        if (x.type === 1) {
            api.client.DeleteChatList({ id: x.id }).then(() => {})
        }
        chatList.value = chatList.value.filter((item) => item.id !== null)
        if (x.id) chatList.value = chatList.value.filter((item) => item.id !== x.id)
    }

    const clear = () => {
        const lobbyChat =
            typeof (globalThis as any).LobbyChat !== 'undefined' ? (globalThis as any).LobbyChat : AuctionChat
        chatList.value = [lobbyChat, AuctionChat]
        chatMap.value = new Map()
    }

    const removeMessage = (id: string) => {
        // console.log('removeMessage', id)
        //从chatmap中删除此id的消息
        for (const [key, chatMessages] of chatMap.value.entries()) {
            // console.log(key, chatMessages)
            const index = chatMessages.findIndex((message) => message.id === id)
            // console.log('撤销消息index:', index)
            if (index !== -1) {
                chatMessages.splice(index, 1)
                break
            }
        }
    }

    // 新增：为中拍用户创建聊天频道
    const addAuctionDealUser = (
        auctionResult: any,
        messageType: 'AuctionEnd' | 'AuctionDeal' = 'AuctionEnd',
        msgTime?: number
    ) => {
        // 兼容不同的属性名格式
        const dealUserId = auctionResult.dealUserId || auctionResult.DealUserId
        const dealUserName = auctionResult.dealUserName || auctionResult.DealUserName
        const dealUserAvatar = auctionResult.dealUserAvatar || auctionResult.DealUserAvatar
        const itemName = auctionResult.name || auctionResult.Name

        if (!dealUserId || !dealUserName) {
            // console.warn('秒杀结果中缺少秒中用户信息', auctionResult)
            return
        }

        // 检查是否已经存在该用户的聊天
        const existingChat = chatList.value.find((item) => item.id === dealUserId)
        if (existingChat) {
            // 如果已存在，更新最后消息
            existingChat.lastMsg = `恭喜您拍得了${itemName}`
            existingChat.time = msgTime || auctionResult.time || new Date().getTime()
            // 将聊天项移到顶部
            chatList.value = [existingChat, ...chatList.value.filter((item) => item.id !== dealUserId)]
            return
        }

        // 构建最后消息内容
        let lastMsg = ''
        if (messageType === 'AuctionEnd') {
            lastMsg = `恭喜您拍得了${itemName}`
        } else if (messageType === 'AuctionDeal') {
            lastMsg = auctionResult.toUserMsg || auctionResult.ToUserMsg || `恭喜您拍得了${itemName}`
        }

        // 构建聊天列表项
        const chatItem: ChatListItem = {
            id: dealUserId,
            name: dealUserName,
            type: ChatListItemType.user,
            time: msgTime || auctionResult.time || new Date().getTime(),
            lastMsg: lastMsg,
            avatar: convertImageUrl(dealUserAvatar || 'https://image.molitao.top/avater.png'),
            unread: 0,
            order: 0,
            msg: {
                type: ChatMessageType.AuctionDeal,
                from: dealUserId,
                fromName: dealUserName,
                avatar: dealUserAvatar,
                msg: lastMsg,
                time: msgTime || auctionResult.time || new Date().getTime(),
                payload: auctionResult,
            },
        }

        // 添加到聊天列表顶部
        chatList.value = [chatItem, ...chatList.value.filter((item) => item.id !== dealUserId)]

        // 初始化聊天记录
        if (!chatMap.value.has(`${dealUserId}`)) {
            chatMap.value.set(`${dealUserId}`, [chatItem.msg!])
        }

        // console.log('已为中拍用户创建聊天频道', dealUserName)
    }

    const getServerLastId = () => {
        return new Promise<string>((resolve) => {
            if (currentChat.value.type === 0) {
                //群聊
                const chan = `${currentChat.value.id}_${currentChat.value.name}`
                api.message.getChanLastId({ chan }).then((res) => {
                    return resolve(res)
                })
            } else if (currentChat.value.type === 1) {
                //私聊
                api.message.getPrivateLastId({ id: currentChat.value.id! }).then((res) => {
                    return resolve(res)
                })
            } else {
                return resolve('')
            }
        })
    }

    //LINK - ChatStore return
    return {
        isConnect,
        websocketId,
        friends,
        friends0,
        groups,
        chatList,
        chatMap,
        currentChat,
        close,
        getChatList,
        connectServer,
        sendChannelMsg,
        sendMsg,
        createChannel,
        leaveChannel,
        joinChannel,
        deleteChannel,
        hasGroup,
        hasMyGroup,
        getGroupHistory,
        getPrivateHistory,
        getGropus,
        getUserAvatar,
        SetCurrentChat,
        SetCurrentChatId,
        SetUnread,
        getUserFriends,
        getCurrentName,
        addChatList,
        deleteChat,
        clear,
        removeMessage,
        addAuctionDealUser,
        getServerLastId,
    }
})
