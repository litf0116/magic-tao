<template>
    <view>
        <view class="flex sticky top-0 left-0 right-0 p-2 h-10" style="border-bottom: 1px solid #908e8e">
            <view class="flex-1 flex flex-center py-2 rounded text-sm font-500" @click="activeName = '1'"
                ><text>秒杀中</text></view
            >
        </view>
        <view class="p-2 overflow-y-scroll" style="height: calc(100vh - 64rpx)">
            <template v-if="activeName === '1'">
                <div class="grid grid-cols-1 gap-2">
                    <auctionMidList
                        v-for="(x, index) in auctionStore.auctionMidList"
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
import auctionMidList from '@/components/chat/auctionMidList.vue'
import type { AuctionItemDto } from '@/composables/types'

const activeName = ref('1')

const auctionStore = useAuctionStore()

const emit = defineEmits(['showDetail'])

function showDetail(item: AuctionItemDto) {
    emit('showDetail', item)
}
</script>
