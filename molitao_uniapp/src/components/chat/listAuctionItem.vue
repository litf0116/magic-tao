<template>
    <view
        :class="{ kongjiang: checkIncludes }"
        class="shadow flex flex-row items-center overflow-hidden cursor-pointer"
        @click.stop="showDetail(item)"
    >
        <div
            class="text-[#935F4E] line-clamp-3"
            style="margin-right: 5px"
            v-if="checkIncludes && item.status === '上架'"
        >
            {{ index }}
        </div>
        <div class="text-[#935F4E] line-clamp-3" style="margin-right: 5px" v-else>{{ index }}.</div>
        <!-- <image :src="getImgUrl(item.imageUrl!, true)" class="w-16 h-16 rounded" mode="aspectFill" /> -->
        <view class="text-wrap text-xs px-2 flex-1 flex flex-col">
            <view class="text-[#935F4E] line-clamp-3">{{ item.name }}</view>
            <view v-if="item.finalPrice" class="flex justify-between">
                <view class="text-red-500">
                    成交价:<b>￥{{ item.finalPrice }}</b>
                </view>
                <view class="text-gray-400 text-xs">
                    {{ formatTime(item) }}
                </view>
            </view>
        </view>
    </view>
</template>

<script setup lang="ts">
import type { AuctionItemDto } from '@/composables/types'
import { getImgUrl } from '@/composables/index'
import dayjs from 'dayjs'
import type { PropType } from 'vue'
const props = defineProps({
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

function formatTime(item: AuctionItemDto) {
    if (item.dealTime) {
        return dayjs(item.dealTime!).format('MM-DD HH:mm')
    }
    return ''
}

const checkIncludes = computed(() => {
    return props.item?.name?.includes('空降')
})
</script>
<style>
.kongjiang {
    font-weight: 600;
}
</style>
