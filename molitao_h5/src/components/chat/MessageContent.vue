<template>
    <view class="message-content">
        <TextMessage
            v-if="message.type === ChatMessageType.Text"
            :message="message"
            @tap="$emit('showDetail', message)"
        />
        <ImageMessage
            v-else-if="message.type === ChatMessageType.Image"
            :message="message"
            @show-full-screen="$emit('showImage', message)"
            @show-action="$emit('showAction', message)"
        />
        <AuctionStartMessage
            v-else-if="message.type === ChatMessageType.AuctionStart && message.payload"
            :message="message"
            @action="$emit('auctionAction', $event)"
        />
        <AuctionBidMessage
            v-else-if="message.type === ChatMessageType.AuctionBid && message.payload"
            :message="message"
        />
        <AuctionEndMessage
            v-else-if="message.type === ChatMessageType.AuctionEnd && message.payload"
            :message="message"
        />
        <AuctionDealMessage
            v-else-if="message.type === ChatMessageType.AuctionDeal && message.payload"
            :message="message"
        />
        <AudioMessage v-else-if="message.type === ChatMessageType.Audio" :message="message" />
        <FileMessage v-else-if="message.type === ChatMessageType.File" :message="message" />
        <LocationMessage v-else-if="message.type === ChatMessageType.Location" :message="message" />
        <VideoMessage v-else-if="message.type === ChatMessageType.Video" :message="message" />
        <OrderMessage v-else-if="message.type === ChatMessageType.Order && message.payload" :message="message" />
    </view>
</template>

<script setup lang="ts">
import { ChatMessage, ChatMessageType } from '@/composables/types'
import TextMessage from './TextMessage.vue'
import ImageMessage from './ImageMessage.vue'
import AuctionStartMessage from './AuctionStartMessage.vue'
import AuctionBidMessage from './AuctionBidMessage.vue'
import AuctionEndMessage from './AuctionEndMessage.vue'
import AuctionDealMessage from './AuctionDealMessage.vue'
import AudioMessage from './AudioMessage.vue'
import FileMessage from './FileMessage.vue'
import LocationMessage from './LocationMessage.vue'
import VideoMessage from './VideoMessage.vue'
import OrderMessage from './OrderMessage.vue'

const props = defineProps<{
    message: ChatMessage
}>()

defineEmits<{
    showDetail: [message: ChatMessage]
    showImage: [message: ChatMessage]
    showAction: [message: ChatMessage]
    auctionAction: [data: any]
}>()
</script>

<style scoped>
.message-content {
    position: relative;
}
</style>
