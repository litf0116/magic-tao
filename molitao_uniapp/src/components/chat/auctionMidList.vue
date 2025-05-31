<template>
    <view class="shadow flex flex-row items-center overflow-hidden cursor-pointer" @click.stop="showDetail(item)">
        <div class="text-[#935F4E] line-clamp-3" style="margin-right: 5px">{{ index }}.</div>
        <image :src="getImgUrl(item.imageUrl!, true)" class="w-16 h-16 rounded" mode="aspectFill" />
        <view class="text-wrap text-xs px-2 flex-1 flex flex-col">
            <view class="text-[#935F4E] line-clamp-3">{{ item.name }}</view>
            <div v-if="item.currentPrice" class="flex" style="flex-wrap: wrap">
                <div class="text-red-500" style="width: 150px">出价人:{{ item.currentPriceUserName }}</div>
                <div class="text-red-500">当前出价:￥{{ item.currentPrice }}</div>
                <div class="text-red-500" style="margin-bottom: 5px">倒计时: {{ minutes }}分钟{{ seconds }}秒</div>
            </div>
        </view>
    </view>
</template>

<script setup lang="ts">
import type { AuctionItemDto } from '@/composables/types'
import { getImgUrl } from '@/composables/index'
import { useCountdown } from '@/utils/countdown'

const props: any = defineProps({
    item: {
        type: Object as PropType<AuctionItemDto>,
        required: true,
    },
    index: {
        required: true,
    },
})
const emit = defineEmits(['showDetail'])
function showDetail(item: AuctionItemDto) {
    emit('showDetail', item)
}
onMounted(() => {
    // 开始初始倒计时
    startCountdown()
})

// 在组件卸载时停止计时器（如果使用 Vue 的话）
onBeforeUnmount(() => {
    stopCountdown()
})
//检查值是否被修改
watch(
    () => props.item,
    (newValue: any, oldValue) => {
        resetCountdown(newValue.useCountdownTime)
    }
)
// 变量来存储倒计时数据
const { days, hours, minutes, seconds, isFinished, startCountdown, stopCountdown, resetCountdown } = useCountdown(
    props.item.useCountdownTime
)
// watch(() => seconds.value, (newValue, oldValue) => {

//     debugger
// })

const checkIncludes = computed(() => {
    return props.item.name.includes('空降') // true
})
</script>
<style>
.kongjiang {
    font-weight: 600;
}
</style>

function defineProps(arg0: { item: { type: PropType<AuctionItemDto>; required: boolean }; index: { required: boolean } }): any {
  throw new Error('Function not implemented.')
}

function defineEmits(arg0: string[]) {
  throw new Error('Function not implemented.')
}
