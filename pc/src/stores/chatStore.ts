import { defineStore } from 'pinia'
import { useLocalStorage } from '@vueuse/core'
import api from '@/api'
import { BASE_API_URL } from '@/utils/request'
import { ChatGroupDto, ChatMessage, ChatMessageType, UserDtoBase } from '@/api/appService'
import { uniqBy, orderBy } from 'lodash'
import { useEventBus } from '@vueuse/core'
import { useAuctionStore } from '@/stores/auctionStore'

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

const LobbyChat: ChatListItem = {
    id: 0,
    name: 'lobby',
    type: ChatListItemType.group,
    time: new Date().getTime(),
    lastMsg: '',
    unread: 0,
    order: 100,
}
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
    const currentChat = ref(LobbyChat)
    const inputChannelMsg = ref<InputChannelMsgType>({ type: 'text', content: '' })

    //聊天对象表
    const chatList: Ref<ChatListItem[]> = useLocalStorage('chatList', [LobbyChat, AuctionChat])
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
            api.client.getChatList().then((res: any) => {
                chatList.value = res
                return resolve()
            })
            return resolve()
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
                    router.push({ path: '/chat/index/lobby', replace: true })
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
                    router.push({ path: '/chat/index/lobby', replace: true })
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
            //处理私聊消息
            const old = chatList.value.find((item) => item.id === msg.from && item.id !== currentChat.value.id)
            if (msg.from === null) {
                return
            }
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
                // 插入系统提示
                const sysMsg: ChatMessage = {
                    type: ChatMessageType.Text,
                    chan: '-1_auction',
                    msg: isKasec ? '拍卖师已开启卡秒，需三倍加价！' : '卡秒已关闭，恢复正常加价',
                    time: Date.now(),
                    payload: { auctionItemId, isKasec },
                }
                if (chatMap.value.has('-1')) {
                    chatMap.value.get('-1')!.push(sysMsg)
                } else {
                    chatMap.value.set('-1', [sysMsg])
                }
            }
            return
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
                            uniqBy(orderBy([...res.items, ...chatMap.value.get(`${_id}`)!], [(msg) => msg.sequenceNumber || msg.time], ['asc']), 'id')
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
                            uniqBy(orderBy([...res.items, ...chatMap.value.get(`${id}`)!], [(msg) => msg.sequenceNumber || msg.time], ['asc']), 'id')
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
            router.push({ path: '/chat/index/lobby', replace: true })
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
        if (chan === '0_lobby') return true
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
                    // 使用服务端返回的消息数据，包含正确的时间戳和序列号
                    const serverMessage = res.data.message
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
        })
    }

    const getUserAvatar = (id: number) => {
        return 'https://cdn.molitao.top/avater.png'
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
        chatList.value = [LobbyChat, AuctionChat]
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
        getServerLastId,
    }
})
