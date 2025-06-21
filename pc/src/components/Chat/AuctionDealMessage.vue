<template>
    <div class="border-green-500 border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-green-500 text-white rounded-lb-lg px-2 font-bold text-xs">交易通知</div>
        <div class="max-w-350px min-w-200px" @click="handleAction">
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
                <span style="font-weight: bold; font-size: 20px">{{ payloadData.name }}</span>
            </div>
            <div class="text-lg font-bold text-red-500">
                <text>成交价: ￥{{ payloadData.finalPrice }}</text>
            </div>
            <div class="text-sm" style="margin-left: 12px">{{ formattedDealTime }}</div>
            <div class="mt-2 text-sm text-gray-600">
                请联系拍卖师确认交易详情<br />
                认准星标，小心冒充<br />
                感谢您的参与！
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import dayjs from 'dayjs'
import { type ChatMessage } from '@/api/appService'

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
        // 解析payload数据
        const payloadData = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload!)
            }
            console.log('deal message payload', payload)
            return payload
        })

        // 格式化交易时间
        const formattedDealTime = computed(() => {
            if (payloadData.value.dealTime) {
                return dayjs(payloadData.value.dealTime).format('YYYY-MM-DD HH:mm:ss')
            }
            return ''
        })

        // 处理点击事件，emit 更通用的 action 事件
        const handleAction = () => {
            emit('action', { message: props.message, payload: payloadData.value })
        }

        return {
            payloadData,
            formattedDealTime,
            handleAction,
        }
    },
}
</script>
