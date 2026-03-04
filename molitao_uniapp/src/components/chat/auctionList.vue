<template>
    <view>
        <view class="flex sticky top-0 left-0 right-0 p-2 h-10">
            <!-- <el-radio-group v-model="activeName" fill="#f4835a" text="#fff">
                <el-radio-button value="1">待秒杀</el-radio-button>
                <el-radio-button value="2">已成交</el-radio-button>
            </el-radio-group> -->

            <view
                class="flex-1 flex flex-center py-2 rounded text-sm font-500"
                :class="[activeName === '1' ? 'bg-[#f4835a] text-white' : 'bg-white text-gray-600']"
                @click="activeName = '1'"
                ><text>待秒杀</text></view
            >
            <view
                class="flex-1 flex flex-center py-2 rounded text-sm font-500"
                :class="[activeName === '2' ? 'bg-[#f4835a] text-white' : 'bg-white text-gray-600']"
                @click="activeName = '2'"
                ><text>已成交</text></view
            >
        </view>
        <view class="p-2 overflow-y-scroll" style="height: calc(100vh - 64rpx)">
            <template v-if="activeName === '1'">
                <div class="grid grid-cols-1 gap-2">
                    <listAuctionItem
                        v-for="x in waitList"
                        :key="x.id"
                        :item="x"
                        :index="getItemIndex(x)"
                        @showDetail="showDetail"
                    />
                    <div class="h-12"></div>
                </div>
            </template>
            <template v-else-if="activeName === '2'">
                <div class="grid grid-cols-1 gap-2">
                    <listAuctionItem
                        v-for="(x, index) in auctionStore.list4"
                        :key="x.id"
                        :item="x"
                        :index="index + 1"
                        @showDetail="showDetail"
                    />
                    <div class="h-12"></div>
                </div>
            </template>
        </view>
    </view>
</template>

<script setup lang="ts">
import { onLoad } from '@dcloudio/uni-app'
import listAuctionItem from '@/components/chat/listAuctionItem.vue'
import type { AuctionItemDto } from '@/composables/types'
const activeName = ref('1')
const auctionStore = useAuctionStore()

onLoad(() => {
    auctionStore.getList().then(() => {})
})

const emit = defineEmits(['showDetail'])

function showDetail(item: AuctionItemDto) {
    emit('showDetail', item)
}

watch(
    () => activeName.value,
    (val) => {
        if (val === '1') {
            auctionStore.getList().then(() => {})
        } else if (val === '2') {
            auctionStore.getList(4).then(() => {})
        }
    }
)

const waitList = computed(() => {
    return auctionStore.list.filter((item) => item.status === '上架')
})

const onAuctionItem = computed(() => {
    return auctionStore.list.find((item) => item.status === '秒杀中') || null
})

let normalIndex = 0
const getItemIndex = (item: any) => {
    if (item.name.includes('空降')) {
        return ''
    }
    normalIndex++
    return normalIndex
}
// 当数据变化时重置计数器
watch(
    () => waitList,
    () => {
        normalIndex = 0
    },
    { deep: true }
)
</script>
