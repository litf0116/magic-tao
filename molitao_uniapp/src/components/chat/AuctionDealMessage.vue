<template>
    <div class="border-green border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-green text-white rounded-lb-lg px-2 font-bold text-xs">交易通知</div>
        <div class="max-w-350px min-w-200px" @tap="handleAction">
            <div class="text-green-600">
                <text>恭喜您成功拍得商品！</text>
            </div>
            <div
                class="auction-item-box"
                style="
                    border: 2.5px solid #22c55e;
                    border-radius: 12px;
                    padding: 10px 16px;
                    margin: 8px 0;
                    display: inline-block;
                    box-shadow: 0 0 8px 2px rgba(34, 197, 94, 0.2);
                    background: #86efac;
                "
            >
                <span style="font-weight: bold; font-size: 20px">{{ message.payload.name }}</span>
            </div>
            <div class="text-lg font-bold text-red-500">
                <text>成交价: ￥{{ message.payload.finalPrice }}</text>
            </div>
            <div class="text-sm" style="margin-left: 12px">{{ message.payload.dealTime }}</div>
            <div class="mt-2 text-sm text-gray-600">
                请联系拍卖师确认交易详情<br />
                认准星标，小心冒充<br />
                感谢您的参与！
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { type ChatMessage } from '@/composables/types'

export default {
    name: 'AuctionDealMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
    },
    emits: ['action'],
    setup(props: any, { emit }: any) {
        // 处理点击事件，emit 更通用的 action 事件
        const handleAction = () => {
            emit('action', { message: props.message, payload: props.message.payload })
        }

        return {
            handleAction,
        }
    },
}
</script>
