<template>
    <view 
        class="message-item"
        :class="{
            'system-center': isSystemMessage,
            'self': isSelfMessage
        }"
    >
        <WelcomeMessage 
            v-if="message.type === ChatMessageType.Welcome" 
            :fromName="message.fromName" 
        />
        
        <SystemMessage
            v-else-if="message.type === ChatMessageType.BanUser"
            type="BanUser"
            :msg="message.msg"
        />
        
        <SystemMessage
            v-else-if="message.type === ChatMessageType.Backout"
            type="Backout"
            :msg="message.msg"
        />

        <view v-else class="message-item-content">
            <view class="avatar" @tap="$emit('showAction', message)">
                <image :src="getAvatarUrl(message.avatar)" mode="aspectFill"></image>
            </view>

            <view class="content">
                <UserInfo 
                    :message="message"
                    :chat-type="chatType"
                    :show-rules="showGroupChatRules"
                    @toggle-rules="showGroupChatRules = !showGroupChatRules"
                />
                
                <MessageContent 
                    :message="message"
                    @show-detail="$emit('showDetail', message)"
                    @show-image="$emit('showImage', message)"
                    @auction-action="$emit('auctionAction', $event)"
                />
                
                <MessageStatus :message="message" />
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ChatMessage, ChatMessageType } from '@/composables/types'
import { getImgUrl2 } from '@/composables'
import WelcomeMessage from './WelcomeMessage.vue'
import SystemMessage from './SystemMessage.vue'
import UserInfo from './UserInfo.vue'
import MessageContent from './MessageContent.vue'
import MessageStatus from './MessageStatus.vue'

const props = defineProps<{
    message: ChatMessage
    previousMessage?: ChatMessage
    currentUserId: number
    chatType: 'private' | 'group'
}>()

const emit = defineEmits<{
    showAction: [message: ChatMessage]
    showDetail: [message: ChatMessage]
    showImage: [message: ChatMessage]
    auctionAction: [data: any]
}>()

const isSystemMessage = computed(() => {
    return [
        ChatMessageType.Welcome,
        ChatMessageType.BanUser,
        ChatMessageType.Backout
    ].includes(props.message.type)
})

const isSelfMessage = computed(() => {
    return props.message.from === props.currentUserId
})

const getAvatarUrl = (avatar: string): string => {
    return getImgUrl2(avatar, true)
}

const showGroupChatRules = ref(false)
</script>

<style scoped>
.message-item {
    margin-bottom: 20rpx;
}

.message-item-content {
    display: flex;
    align-items: flex-start;
    gap: 16rpx;
}

.message-item-content.self {
    flex-direction: row-reverse;
}

.avatar {
    width: 64rpx;
    height: 64rpx;
    border-radius: 50%;
    overflow: hidden;
    flex-shrink: 0;
}

.avatar image {
    width: 100%;
    height: 100%;
}

.content {
    flex: 1;
    max-width: 70%;
}

.message-item.system-center {
    display: flex;
    justify-content: center;
}
</style>