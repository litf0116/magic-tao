import { defineStore } from 'pinia'
import { useLocalStorage } from '@vueuse/core'
import api from '@/api'
import { BASE_API_URL } from '@/utils/request'
import { ChatGroupDto, ChatMessage, ChatMessageType, UserDtoBase } from '@/api/appService'
import { uniqBy, orderBy } from 'lodash'
import { useEventBus } from '@vueuse/core'
import { useAuctionStore } from '@/stores/auctionStore'
import { convertImageUrl, convertObjectImageUrls, convertObjectImageUrlsArray } from '@/utils/imageUrlConverter'
import { Tips } from '@/composables'

export const onmessageKey = Symbol('onmessageKey')

export enum ChatListItemType {
    group = 0,
    user = 1,
    system = 2,
}

export type ChatListItem = {
    id?: number
    name: string
    type: ChatListItemType
    time?: number
    avatar?: string
    lastMsg?: string
    unread: number
    order: number
    msg?: ChatMessage
}

type ChannelType = { chan: string; online: number }
type InputChannelMsgType = { type: string; content: string }

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

export const useChatStore = defineStore('chat', () => {
    const bus = useEventBus(onmessageKey)

    const route = useRoute()

    //STATE
    const gsocket = ref<WebSocket | null>(null)
    const gsocketTimeId = ref<number | undefined>(undefined)

    const friends = ref<UserDtoBase[]>([])
    const friends0 = ref<UserDtoBase[]>([])
    const groups = ref<ChatGroupDto[]>([])
    const currentChat = ref(AuctionChat)
    const inputChannelMsg = ref<InputChannelMsgType>({ type: 'text', content: '' })

    const chatList: Ref<ChatListItem[]> = useLocalStorage('chatList', [AuctionChat])
    const chatMap = ref<Map<string, ChatMessage[]>>(new Map())

    //state
    const websocketId = useLocalStorage('websocketId', 0)
    const qrUrl = ref('')
    const pubQrUrl = ref('')

    const auctionStore = useAuctionStore()

    //getter

    //action
    const initQr = (str: string) => {
        if (str) {
            const qrLoginUrl = `${BASE_API_URL}/api/tokenAuth/qrLogin?state=${str}`
            console.log('qrLoginUrl', qrLoginUrl)
            qrUrl.value = `${BASE_API_URL}/home/qr?str=${qrLoginUrl}`
            api.tokenAuth.pubQrLogin({ state: str }).then((res) => {
                pubQrUrl.value = res
            })
        } else {
            qrUrl.value = ''
            pubQrUrl.value = ''
        }
    }

    const init = () => {
        return new Promise<string>((resolve) => {
            //初始化一个字符串随机数
            const random: string = Math.random().toString(36)
            return resolve(random)
        })
    }

    const close = () => {
        if (gsocket.value) gsocket.value.close()
        gsocket.value = null
    }

    function getChatList() {
        return new Promise<void>((resolve, reject) => {
            api.client
                .getChatList()
                .then((res: any) => {
                    console.log(res)
                    chatList.value = convertObjectImageUrlsArray(res, ['avatar'])
                    resolve()
                })
                .catch((error) => {
                    console.error('获取聊天列表失败:', error)
                    reject(error)
                })
        })
    }

    const connectServer = (reconnect = false) => {
        return new Promise<string>((resolve) => {
            if (gsocket && !reconnect) {
                return resolve('ok')
            }
            api.ws.preConnect().then(async (res) => {
                if (gsocket.value) {
                    gsocket.value.onclose = () => {
                        //
                    }
                    await gsocket.value.close()
                }
                gsocket.value = null

                clearTimeout(gsocketTimeId.value)

                gsocket.value = new WebSocket(res.server)
                websocketId.value = Number(res.websocketId)

                gsocket.value.onmessage = function (e) {
                    try {
                        let msg = e.data
                        if (typeof msg === 'string') {
                            msg = JSON.parse(msg)
                        }
                        if (msg.type === 'Error') {
                            alert(msg.receipt)
                            return
                        }
                        onmessage(msg)
                    } catch (e) {
                        console.log(e)
                        return
                    }
                }

                gsocket.value.onclose = (e) => {
                    clearTimeout(gsocketTimeId.value)
                    // Tips.info('聊天服务器连接断开', e)
                    console.log('聊天服务器连接断开 ', e)
                    // api.ws.offline({ websocketId: websocketId.value })
                    //检查是不是在聊天室路由,如果是,则重新连接
                    if (route.path.startsWith('/chat')) {
                        console.log('route.path', route.path)
                        gsocket.value = null
                        console.log('websocket onclose 5秒后重新连接', e)
                        gsocketTimeId.value = setTimeout(function () {
                            connectServer(true)
                        }, 5000)
                    } else {
                        gsocket.value = null
                        console.log('非聊天室路由,不重连')
                    }
                }

                gsocket.value.onerror = (e) => {
                    clearTimeout(gsocketTimeId.value)
                    console.log('websocket error 5秒后重新连接', e)
                    gsocket.value = null
                    gsocketTimeId.value = setTimeout(function () {
                        connectServer(true)
                    }, 5000)
                }

                gsocket.value.onopen = async (e: any) => {
                    console.log('websocket connect')
                    Tips.success('聊天服务器连接成功')
                    // await joinChannel('0_lobby')
                    // await getChannels()

                    await getGropus()

                    return resolve('ok')
                }
            })
        })
    }

    const SetCurrentChat = (item: ChatListItem) => {
        console.log('SetCurrentChat', item)

        currentChat.value = item
    }
    const SetCurrentChatId = (id: number, name = '', isGroup = true) => {
        return new Promise<void>((resolve) => {
            const item = chatList.value.find((item) => item.id === id)
            if (item) currentChat.value = item
            else
                currentChat.value = {
                    id: id,
                    name,
                    type: isGroup ? ChatListItemType.group : ChatListItemType.user,
                    time: new Date().getTime(),
                    lastMsg: '',
                    unread: 0,
                    order: 0,
                }
            return resolve()
        })
    }

    const SetUnread = (item: ChatListItem, number: number) => {
        item.unread = number
    }
    const router = useRouter()

    const onmessage = function (msg: ChatMessage) {
        // 转换消息头像URL
        if (msg.avatar) {
            msg.avatar = convertImageUrl(msg.avatar)
        }

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

        bus.emit(msg)
        // console.log('onmessage', msg)
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
            console.log('ChatMessageType.Goodbye', msg)
            if (!msg.msg) {
                console.log('房主解散房间,所有人退出', msg)
                //delete from chatList
                chatList.value = chatList.value.filter((item) => item.id !== _id)
                chatMap.value.delete(`${_id}`)
                if (currentChat.value.id === _id) {
                    Tips.info('房间已解散')
                    router.push({ path: '/chat/index/auction', replace: true })
                }
            } else {
                //踢出某人
                const userId = parseInt(msg.msg)
                console.log('踢出某人', msg.msg, userId)

                if (userId && userId === websocketId.value && currentChat.value.id === _id) {
                    //被踢出
                    chatList.value = chatList.value.filter((item) => item.id !== _id)
                    chatMap.value.delete(`${_id}`)
                    Tips.noCancelConfirm('你已被房主踢出房间')
                    router.push({ path: '/chat/index/auction', replace: true })
                }
            }
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
            // debugger
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
                    order: _id === -1 ? 99 : 0,
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
            // 只处理当前拍卖频道
            if (currentChat.value.id === -1) {
                auctionStore.syncKasecStatus(auctionItemId)
                // 保持原始的KasecStatusChanged类型，设置消息内容
                msg.msg = isKasec ? '拍卖师已开启卡秒，需三倍加价！' : '卡秒已关闭，恢复正常加价'
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

        // 特殊处理：解码卡秒消息、出价消息和成交消息
        if (msg.type === ChatMessageType.AuctionBid && msg.payload) {
            console.log('=== 检测到出价消息 ===')
            console.log('消息类型:', msg.type)
            console.log('消息payload:', msg.payload)
            console.log('payload类型:', typeof msg.payload)

            // 检查是否是编码的卡秒消息
            if (msg.payload.messageType === 'KasecStatusChanged' && msg.payload.encoded) {
                console.log('=== 检测到编码的卡秒消息 ===')
                console.log('卡秒消息payload:', msg.payload)

                // 解码：将消息类型从 AuctionBid 转换为 KasecStatusChanged
                msg.type = ChatMessageType.KasecStatusChanged

                // 处理卡秒状态变更
                const { auctionItemId, isKasec } = msg.payload
                if (auctionItemId && typeof isKasec === 'boolean') {
                    // 只处理当前拍卖频道
                    if (currentChat.value.id === -1) {
                        auctionStore.syncKasecStatus(auctionItemId)
                        // 设置消息内容（移除闪电符号，因为组件中已有图标）
                        msg.msg = isKasec ? '拍卖师已开启卡秒模式，需三倍加价！' : '卡秒已关闭，恢复正常加价规则'
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
            console.log('=== 处理正常出价消息 ===')
            // 使用新的增量更新方法，避免重新请求整个列表
            auctionStore.updateAuctionItemFromBidMessage(msg.payload)
        }

        // 检查是否是编码的成交消息
        if (msg.type === ChatMessageType.AuctionEnd && msg.payload) {
            console.log('=== 检测到AuctionEnd消息 ===')
            console.log('消息类型:', msg.type)
            console.log('消息payload:', msg.payload)
            console.log('payload类型:', typeof msg.payload)

            // 检查是否是编码的成交消息
            if (msg.payload.messageType === 'AuctionDeal' && msg.payload.encoded) {
                console.log('=== 检测到编码的成交消息 ===')
                console.log('成交消息payload:', msg.payload)

                // 解码：将消息类型从 AuctionEnd 转换为 AuctionDeal
                msg.type = ChatMessageType.AuctionDeal

                // 恢复原始payload
                if (msg.payload.originalPayload) {
                    msg.payload = msg.payload.originalPayload
                }

                console.log('=== 成交消息解码完成 ===')
                console.log('解码后消息类型:', msg.type)
                console.log('解码后payload:', msg.payload)
            }
        }

        // 特殊处理：监听拍卖结束消息，为中拍用户创建聊天频道
        // 需要在解码后处理，因为 AuctionDeal 消息被编码为 AuctionEnd 传输，解码后类型变回 AuctionDeal
        if ((msg.type === 'AuctionEnd' || msg.type === ChatMessageType.AuctionDeal) && msg.payload) {
            console.log('检测到拍卖结束消息，为中拍用户创建聊天频道', msg.payload)
            const userStore = useUserStore()

            // 检查 payload 中是否有成交用户信息
            const dealUserId = msg.payload.dealUserId || msg.payload.DealUserId

            if (dealUserId) {
                // 优先使用拍卖成交时间，而不是消息接收时间
                const dealTime = msg.payload.dealTime || msg.payload.DealTime || msg.time || new Date().getTime()

                // 发送者（拍卖师）：为中拍用户创建聊天会话
                if (msg.from === userStore.user.id) {
                    addAuctionDealUser(msg.payload, 'AuctionDeal', dealTime)
                }

                // 接收者（中拍用户）：为拍卖师创建聊天会话
                if (msg.to === userStore.user.id) {
                    // 中拍用户需要看到拍卖师（msg.from），而不是自己
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

    const getGropus = function () {
        groups.value = []
        return new Promise(async (resolve) => {
            await api.chatGroup.getAllPublic({}).then((res) => {
                groups.value = res.items
                return resolve(res.items)
            })
        })
    }

    const getGroupHistory = (groupName = '', lastTime?: number, reload = false) => {
        lastTime = lastTime || new Date().getTime()
        return new Promise<ChatMessage[]>((resolve) => {
            api.message.getChanHistory({ chan: groupName, lastTime: lastTime }).then((res) => {
                if (res.items && res.items.length > 0) {
                    const _t = groupName.split('_')
                    let _id = parseInt(_t[0])
                    if (_id > 0) _id = -_id
                    const _name = _t[1]
                    if (chatMap.value.has(`${_id}`) && !reload) {
                        chatMap.value.set(
                            `${_id}`,
                            uniqBy(
                                orderBy([...res.items, ...chatMap.value.get(`${_id}`)!], [(msg) => msg.time], ['asc']),
                                'id'
                            )
                        )
                    } else {
                        chatMap.value.set(`${_id}`, res.items)
                    }
                }

                return resolve(res.items!)
            })
        })
    }

    const getPrivateHistory = (id: number, lastTime: number | null = null, reload = false) => {
        if (isNaN(id)) {
            console.error('Invalid id for getPrivateHistory:', id)
            return Promise.resolve([])
        }
        lastTime = lastTime || new Date().getTime()
        return new Promise<ChatMessage[]>((resolve) => {
            api.message.getPrivateHistory({ id: id, lastTime: lastTime! }).then((res) => {
                console.log('getPrivateHistory', res.items)
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

                return resolve(res.items!)
            })
        })
    }

    const leaveChannel = async (chan: string) => {
        await api.ws.leaveChannel({ chan: chan }).then(async (res) => {
            // joinedChannel.value = joinedChannel.value.filter((item) => item !== chan)
            chatList.value = chatList.value.filter((item) => item.id !== parseInt(chan.split('_')[0]))
            chatMap.value.delete(`${parseInt(chan.split('_')[0])}`)
            router.push({ path: '/chat/index/auction', replace: true })
        })
    }

    const joinChannel = async (chan: string) => {
        return new Promise((resolve, reject) => {
            api.ws
                .subChannel({ body: { websocketId: websocketId.value, channel: chan } })
                .then(async (res) => {
                    // joinedChannel.value.push(chan)
                    await sendChannelMsg('', chan, ChatMessageType.Welcome)
                    return resolve('ok')
                })
                .catch((e) => {
                    return reject(e)
                })
        })
    }
    const deleteChannel = async (group: ChatGroupDto) => {
        return new Promise(async (resolve) => {
            api.chatGroup.delete({ id: group.id }).then(() => {
                return resolve('ok')
            })
        })
    }

    // const hasJoinedChannel = (chan: string) => {
    //     return joinedChannel.value.includes(chan)
    // }

    const hasGroup = (chan: string) => {
        if (chan === '-1_auction') return true
        return groups.value.find((item) => item.chan === chan)
    }

    const createChannel = (name: string) => {
        return new Promise(async (resolve, reject) => {
            if (!name) {
                return reject('请输入群名称')
            } else {
                const chan = `-${websocketId.value}_${name}`
                joinChannel(chan)
                    .then(async () => {
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
                time: new Date().getTime(),
            }

            await api.ws
                .sendChannelMsg({
                    body: {
                        from: websocketId.value,
                        chan: chan,
                        message: data,
                    },
                })
                .then((res) => {
                    return resolve(res)
                })
                .catch((err) => {
                    Tips.error('消息发送失败，请检查网络连接')
                    console.error('sendChannelMsg error:', err)
                    resolve(null)
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
                time: new Date().getTime(),
            }

            await api.ws
                .sendMsg({
                    body: {
                        from: websocketId.value,
                        to: to,
                        message: data,
                        isReceipt: true,
                    },
                })
                .then((res) => {
                    // 使用服务端返回的消息数据，包含正确的时间戳
                    const serverMessage = res.data.message
                    console.log('server message: {}', serverMessage)
                    chatList.value = [
                        {
                            id: to,
                            name: toName,
                            type: ChatListItemType.user,
                            time: serverMessage.time, // 使用服务端时间戳
                            lastMsg: serverMessage.msg,
                            avatar: avatar,
                            unread: 0,
                            order: 0,
                            msg: serverMessage,
                        },
                        ...chatList.value.filter((item) => item.id !== to),
                    ]
                    if (chatMap.value.has(`${to}`)) {
                        chatMap.value.get(`${to}`)!.push(serverMessage)
                    } else {
                        chatMap.value.set(`${to}`, [serverMessage])
                    }

                    return resolve(res)
                })
                .catch((err) => {
                    Tips.error('私信发送失败，请检查网络连接')
                    console.error('sendMsg error:', err)
                    resolve(null)
                })
        })
    }

    const getUserAvatar = (id: number) => {
        return 'https://image.molitao.top/avater.png'
    }

    const getUserFriends = (status = true) => {
        api.userFriend.getUserFriends({ id: websocketId.value, status: status }).then((res) => {
            if (status) friends.value = res.items!
            else friends0.value = res.items!
        })
    }
    const getCurrentName = () => {
        return currentChat.value.name
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
                order: id === -1 ? 99 : 0,
            },
            ...chatList.value.filter((item) => item.id !== id),
        ]
    }

    const deleteChat = (x: ChatListItem) => {
        console.log('deleteChat', x)
        if (x.type === 1) {
            api.client.deleteChatList({ id: x.id }).then(() => {
                // 删除聊天记录成功
            })
        }
        chatList.value = chatList.value.filter((item) => item.id !== null)
        if (x.id) chatList.value = chatList.value.filter((item) => item.id !== x.id)
    }

    const clear = () => {
        chatList.value = [AuctionChat]
        chatMap.value = new Map()
        // joinedChannel.value = []
    }

    const removeMessage = (id: string) => {
        console.log('removeMessage', id)
        //从chatmap中删除此id的消息
        for (const [key, chatMessages] of chatMap.value.entries()) {
            console.log(key, chatMessages)
            const index = chatMessages.findIndex((message) => message.id === id)
            console.log(index)
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
            console.warn('拍卖结果中缺少中拍用户信息', auctionResult)
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
            avatar: dealUserAvatar ? convertImageUrl(dealUserAvatar) : 'https://image.molitao.top/avater.png',
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

        console.log('已为中拍用户创建聊天频道', dealUserName)
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
                api.message.getPrivateLastId({ id: currentChat.value.id }).then((res) => {
                    return resolve(res)
                })
            } else {
                return resolve('')
            }
        })
    }

    return {
        //LINK - Return State
        websocketId,
        qrUrl,
        pubQrUrl,
        friends,
        friends0,
        groups,
        chatList,
        chatMap,
        inputChannelMsg,
        currentChat,
        // joinedChannel,
        //LINK - Return Action
        close,
        init,
        initQr,
        getChatList,
        connectServer,
        sendChannelMsg,
        sendMsg,
        createChannel,
        leaveChannel,
        joinChannel,
        deleteChannel,
        // hasJoinedChannel,
        hasGroup,
        getGropus,
        getGroupHistory,
        getPrivateHistory,
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
