<template>
    <div class="auction-start-message border-red-500 border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-red-500 text-white rounded-lb-lg px-2 font-bold text-xs">开始拍卖</div>
        <div class="max-w-350px min-w-200px" @tap="handleAction">
            <div>商品名称: {{ payloadData.name }}</div>
            <rich-text :nodes="payloadData.description"></rich-text>
        </div>
    </div>
</template>

<script setup lang="ts">

import { convertAuctionPayload } from '@/utils/propertyConverter'

const props = defineProps<{
    message: any
    catchImage?: (e: any, payload: any) => void
}>()

const emit = defineEmits(['action'])

const payloadData = computed(() => {
    const convertedPayload = convertAuctionPayload(props.message.payload)
    return convertedPayload
})

// 处理点击事件，emit 统一的 action 事件
function handleAction(e: any) {
    emit('action', { message: props.message, payload: payloadData.value })

    // 如果提供了catchImage函数，也调用它进行图片预览
    if (props.catchImage) {
        props.catchImage(e, props.message.payload)
    }
}
</script>

<style scoped>
.auction-start-message {
    margin: 8px 0;
    background: #fff5f5;
}
</style>
