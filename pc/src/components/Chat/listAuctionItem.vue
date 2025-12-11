<template>
    <div
        :class="{ kongjiang: checkIncludes && item.status === '上架' }"
        class="shadow flex flex-row items-center overflow-hidden cursor-pointer"
        @click.stop="showDetail(item.id!)"
    >
        <div class="text-wrap text-sm flex-1 flex flex-col">
            <div v-if="checkIncludes && item.status === '上架'" class="text-[#935F4E] line-clamp-3">
                {{ item.name }}
            </div>
            <div v-else class="text-[#935F4E] line-clamp-3">{{ item.displayIndex }}. {{ item.name }}</div>
            <div v-if="item.finalPrice" class="flex justify-between">
                <div class="text-red-500">
                    成交价:<b>￥{{ item.finalPrice }}</b>
                </div>
                <div class="text-gray-400 text-xs">
                    {{ formatTime(item) }}
                </div>
            </div>
            <div v-else-if="item.currentPrice" class="flex" style="flex-wrap: wrap">
                <div class="text-red-500" style="width: 150px">出价人:{{ item.currentPriceUserName }}</div>
                <div class="text-red-500">当前出价:￥{{ item.currentPrice }}</div>
                <div class="text-red-500" style="margin-left: 5px">倒计时: {{ minutes }}分钟{{ seconds }}秒</div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useCountdown } from '@/utils/countdown'
import dayjs from 'dayjs'
import { AuctionItemDto } from '@/api/appService'
const props = defineProps({
    item: {
        type: Object as PropType<AuctionItemDto>,
        required: true,
    },
    // 移除 index 参数，直接使用 item.displayIndex
})

const checkIncludes = computed(() => {
    return props.item.name.includes('空降') // true
})
// 组件挂载时开始倒计时
onMounted(() => {
    // 开始初始倒计时
    startCountdown()
})
// 组件卸载时清除动画帧请求，防止内存泄漏
onUnmounted(() => {
    stopCountdown()
})
//检查值是否被修改
watch(
    () => props.item,
    (newValue, oldValue) => {
        if (newValue.status == '拍卖中') {
            resetCountdown(newValue.useCountdownTime)
        }
    }
)
// 变量来存储倒计时数据
const { minutes, seconds, isFinished, startCountdown, stopCountdown, resetCountdown } = useCountdown(
    props.item.useCountdownTime
)

const emit = defineEmits(['showDetail'])

function showDetail(id: number) {
    emit('showDetail', id)
}

function formatTime(item: AuctionItemDto) {
    if (item.dealTime) {
        return dayjs(item.dealTime!).format('MM-DD HH:mm')
    }
    return ''
}
</script>
<style>
.shadow {
    padding: 5px;
}

.kongjiang {
    font-weight: 600;
}
</style>
