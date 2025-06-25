<template>
    <div
        class="auction-bid-message border-[#ff7144] bg-[#ffb673] border-2 border-solid py-2 px-4 rounded-xl relative overflow-hidden"
        @click="handleAction"
    >
        <div class="absolute top-0 right-0 bg-red-500 text-white rounded-lb-lg px-2 font-bold text-xs">出价</div>
        <div class="max-w-350px min-w-200px" style="margin-top: 10px">
            <div>商品名称: {{ payloadData.Name || payloadData.name }}</div>
            <div style="color: #fff; font-size: 24px">当前出价：￥{{ payloadData.CurrentPrice }}</div>
        </div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/api/appService'

export default {
    name: 'AuctionBidMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
    },
    emits: ['action'],
    setup(props, { emit }) {
        // 解析payload数据
        const payloadData = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload!)
            }
            return payload
        })

        // 处理点击事件，emit 更通用的 action 事件
        const handleAction = () => {
            emit('action', { message: props.message, payload: payloadData.value })
        }

        return {
            payloadData,
            handleAction,
        }
    },
}
</script>

<style scoped>
.auction-bid-message {
    cursor: pointer;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

.auction-bid-message:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(255, 113, 68, 0.3);
}
</style>
