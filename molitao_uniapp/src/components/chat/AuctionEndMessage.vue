<template>
    <div class="border-amber border-2 border-solid py-2 px-4 rounded-lg relative">
        <div class="absolute top-0 right-0 bg-amber text-white rounded-lb-lg px-2 font-bold text-xs">成功竞拍</div>
        <div class="max-w-350px min-w-200px" @tap="handleShowDetails">
            <!-- 已成交状态 -->
            <div v-if="payloadData.status === '已成交'">
                <div class="text-red-500">
                    <text>恭喜 {{ payloadData.dealUserName }} 最终以 </text>
                    <text class="text-lg">￥{{ payloadData.finalPrice }}</text>
                    <text> 拍得商品</text>
                </div>
                <div
                    class="auction-item-box"
                    style="
                        border: 1px solid #ffb673;
                        border-radius: 10px;
                        padding: 8px 12px;
                        margin: 8px 0;
                        display: inline-block;
                    "
                >
                    <span style="font-weight: bold; font-size: 20px">{{ payloadData.name }}</span>
                </div>
                <div class="text-sm" style="margin-left: 12px">{{ formattedDealTime }}</div>
                <div class="mt-2 text-sm text-gray-600">
                    买卖双方私聊拍卖师确认交易!<br />
                    认准星标小心冒充<br />
                    有请下一件拍品
                </div>
            </div>
            <!-- 流拍状态 -->
            <div v-else-if="payloadData.status === '上架'">
                <div>商品流拍</div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { computed } from 'vue'
import { type ChatMessage } from '@/composables/types'
import dayjs from 'dayjs'

export default {
    name: 'AuctionEndMessage',
    props: {
        message: {
            type: Object as () => ChatMessage,
            required: true,
        },
    },
    emits: ['showDetails'],
    setup(props: any, { emit }: any) {
        // 解析payload数据
        const payloadData = computed(() => {
            let payload = props.message.payload
            if (typeof payload === 'string') {
                payload = JSON.parse(payload!)
            }
            console.log('payloadData', payload)
            return payload
        })

        // 格式化交易时间
        const formattedDealTime = computed(() => {
            if (payloadData.value.dealTime) {
                return dayjs(payloadData.value.dealTime).format('YYYY-MM-DD HH:mm:ss')
            }
            return ''
        })

        // 处理点击显示详情事件
        const handleShowDetails = () => {
            emit('showDetails', props.message)
        }

        return {
            payloadData,
            formattedDealTime,
            handleShowDetails,
        }
    },
}
</script>
