import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { useStorageRef } from '@/composables/useStorageRef'
import { uniqBy, orderBy } from 'lodash-es'
import {
    ChatMessageStatus,
    ChatMessageType,
    type ChatListItem,
    type ChatMessage,
    ChatListItemType,
} from '@/composables/types'
import api from '@/utils/api'
import { Tips } from '@/composables'
import { Goto } from '@/composables/goto'

export const CREATEGROUPEVENT = 'CREATE_GROUP_EVENT'

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

export const useMessageStore = defineStore('messageStore', () => {
    const chatList = useStorageRef<ChatListItem[]>('chatList', [])
    const chatMap = ref<Map<string, ChatMessage[]>>(new Map())
    const currentChat = ref<ChatListItem | null>(null)
    const historyLoading = ref(false)
    const historyAllLoaded = ref(false)
    const unreadCount = ref(0)

    const currentHistoryMsgs = computed(() => {
        if (!currentChat.value) return []
        const key = getChatKey(currentChat.value)
        return chatMap.value.get(key) || []
    })

    const totalUnreadCount = computed(() => {
        return chatList.value.reduce((sum, item) => sum + (item.unread || 0), 0)
    })

    const getChatKey = (chatItem: ChatListItem): string => {
        if (chatItem.type === ChatListItemType.private) {
            return `private_${chatItem.id}`
        }
        return `group_${chatItem.id}`
    }

    const formatMessageTime = (timestamp: number): string => {
        const now = Date.now()
        const diff = now - timestamp
        
        if (diff < 60 * 1000) return '刚刚'
        if (diff < 60 * 60 * 1000) return `${Math.floor(diff / (60 * 1000))}分钟前`
        if (diff < 24 * 60 * 60 * 1000) return `${Math.floor(diff / (60 * 60 * 1000))}小时前`
        
        const date = new Date(timestamp)
        return date.toLocaleString('zh-CN', {
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        })
    }

    const shouldShowTimeDivider = (currentMsg: ChatMessage, previousMsg?: ChatMessage): boolean => {
        if (!previousMsg) return true
        const timeDiff = currentMsg.time - previousMsg.time
        return timeDiff > 5 * 60 * 1000
    }

    const setCurrentChat = (chat: ChatListItem | null) => {
        currentChat.value = chat
        if (chat) {
            const chatItem = chatList.value.find(item => item.id === chat.id && item.type === chat.type)
            if (chatItem) {
                chatItem.unread = 0
            }
        }
    }

    const addMessage = (chatItem: ChatListItem, message: ChatMessage) => {
        const key = getChatKey(chatItem)
        const existingMessages = chatMap.value.get(key) || []
        
        if (existingMessages.some(msg => msg.id === message.id)) {
            return
        }

        const newMessages = [...existingMessages, message]
        chatMap.value.set(key, newMessages)

        const chatListItem = chatList.value.find(item => item.id === chatItem.id && item.type === chatItem.type)
        if (chatListItem) {
            chatListItem.lastMsg = message.msg
            chatListItem.time = message.time
            
            if (currentChat.value?.id !== chatItem.id || currentChat.value?.type !== chatItem.type) {
                chatListItem.unread = (chatListItem.unread || 0) + 1
            }
        }
    }

    const addMessages = (chatItem: ChatListItem, messages: ChatMessage[], reload = false) => {
        const key = getChatKey(chatItem)
        const existingMessages = chatMap.value.get(key) || []
        
        const mergedMessages = mergeHistoryForChannel(key, messages, existingMessages, reload)
        chatMap.value.set(key, mergedMessages)
    }

    const updateMessageStatus = (chatItem: ChatListItem, messageId: string, status: ChatMessageStatus) => {
        const key = getChatKey(chatItem)
        const messages = chatMap.value.get(key) || []
        
        const message = messages.find(msg => msg.id === messageId)
        if (message) {
            message.status = status
        }
    }

    const clearChatMessages = (chatItem: ChatListItem) => {
        const key = getChatKey(chatItem)
        chatMap.value.delete(key)
    }

    const loadHistoryMessages = async (chatItem: ChatListItem, lastTime?: number, size = 20) => {
        historyLoading.value = true
        
        try {
            let messages: ChatMessage[] = []
            
            if (chatItem.type === ChatListItemType.private) {
                const res = await api.message.getPrivateHistory({
                    id: chatItem.id,
                    lastTime,
                    size
                })
                messages = res.items || []
            } else {
                const res = await api.message.getChanHistory({
                    chan: `group_${chatItem.id}_${chatItem.name}`,
                    lastTime,
                    size
                })
                messages = res.items || []
            }
            
            addMessages(chatItem, messages, !lastTime)
            
            if (messages.length < size) {
                const chatListItem = chatList.value.find(item => 
                    item.id === chatItem.id && item.type === chatItem.type
                )
                if (chatListItem) {
                    chatListItem.allLoaded = true
                }
            }
            
            return messages
        } catch (error) {
            console.error('Failed to load history messages:', error)
            throw error
        } finally {
            historyLoading.value = false
        }
    }

    const markAsRead = (chatItem: ChatListItem) => {
        const chatListItem = chatList.value.find(item => 
            item.id === chatItem.id && item.type === chatItem.type
        )
        if (chatListItem) {
            chatListItem.unread = 0
        }
    }

    const deleteChat = (chatItem: ChatListItem) => {
        chatList.value = chatList.value.filter(item => 
            !(item.id === chatItem.id && item.type === chatItem.type)
        )
        clearChatMessages(chatItem)
        
        if (currentChat.value?.id === chatItem.id && currentChat.value?.type === chatItem.type) {
            setCurrentChat(null)
        }
    }

    const initializeChatList = async () => {
        try {
            const res = await api.client.getChatList()
            chatList.value = res.map((item: any) => ({
                ...item,
                unread: item.unread || 0,
                allLoaded: false
            }))
        } catch (error) {
            console.error('Failed to initialize chat list:', error)
            throw error
        }
    }

    return {
        chatList: computed(() => chatList.value),
        chatMap: computed(() => chatMap.value),
        currentChat: computed(() => currentChat.value),
        currentHistoryMsgs,
        historyLoading: computed(() => historyLoading.value),
        historyAllLoaded: computed(() => historyAllLoaded.value),
        unreadCount: computed(() => totalUnreadCount.value),
        
        setCurrentChat,
        addMessage,
        addMessages,
        updateMessageStatus,
        clearChatMessages,
        loadHistoryMessages,
        markAsRead,
        deleteChat,
        initializeChatList,
        formatMessageTime,
        shouldShowTimeDivider
    }
})