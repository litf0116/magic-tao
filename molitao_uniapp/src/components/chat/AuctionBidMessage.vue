<template>
    <div
        class="auction-bid-message border-[#ff7144] bg-[#ffb673] border-2 border-solid py-2 px-4 rounded-xl relative overflow-hidden"
        @tap="handleAction"
    >
        <div class="absolute top-0 right-0 bg-[#ff7144] text-white rounded-lb-lg px-2 font-bold text-xs">出价</div>
        <div class="max-w-350px min-w-200px">
            <div>商品名称: {{ payloadData.name }}</div>
            <div style="color: #fff; font-size: 24px">当前出价：￥{{ payloadData.currentPrice }}</div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { defineProps, computed } from 'vue'
import { convertAuctionPayload } from '@/utils/propertyConverter'

const props = defineProps<{
    message: any
}>()

const emit = defineEmits(['action'])

const payloadData = computed(() => {
    return convertAuctionPayload(props.message.payload)
})

// 处理点击事件，emit 统一的 action 事件
function handleAction() {
    console.log('AuctionBid handleAction', props.message.payload)
    emit('action', { message: props.message, payload: payloadData.value })
}
</script>

<style scoped>
.auction-bid-message {
    margin: 8px 0;
}
</style>
