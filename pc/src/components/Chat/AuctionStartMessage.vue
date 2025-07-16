<template>
    <div class="auction-start-message border-red-500 border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-red-500 text-white rounded-lb-lg px-2 font-bold text-xs">开始拍卖</div>
        <div class="max-w-350px min-w-200px" @click="handleAction">
            <div>商品名称: {{ payloadData.name }}</div>
            <RichTextDisplay :content="payloadData.description" @imageClick="handleImageClick" />
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '../../api/appService'
import RichTextDisplay from './RichTextDisplay.vue'
import { convertAuctionPayload } from '@/utils/propertyConverter'

const props = defineProps<{
    message: ChatMessage
}>()

const emit = defineEmits<{
    action: [{ message: ChatMessage; payload: unknown; type?: string; imageUrl?: string }]
}>()

const payloadData = computed(() => {
    return convertAuctionPayload(props.message.payload)
})

// 处理点击事件，emit 更通用的 action 事件
const handleAction = () => {
    emit('action', { message: props.message, payload: payloadData.value })
}

// 处理图片点击事件
const handleImageClick = (event: Event) => {
    let imageUrl = ''
    if (event.target instanceof HTMLImageElement) {
        imageUrl = event.target.src
    }
    if (imageUrl) {
        emit('action', { message: props.message, payload: payloadData.value, type: 'image', imageUrl })
    }
}
</script>

<style scoped>
.auction-start-message {
    margin: 8px 0;
    background: #fff5f5;
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.auction-start-message:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(239, 68, 68, 0.25);
}
</style>
