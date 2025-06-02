<template>
    <div class="border-red-500 border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-red-500 text-white rounded-lb-lg px-2 font-bold text-xs">
            开始拍卖
        </div>
        <div class="max-w-350px min-w-200px" @click="handleAction">
            <div>商品名称: {{ payloadData.name }}</div>
            <div>{{ payloadData.description }}</div>
        </div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/composables/types'

export default {
    name: 'AuctionStartMessage',
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
