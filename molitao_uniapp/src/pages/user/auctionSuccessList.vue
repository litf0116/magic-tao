<template>
    <view>
        <view
            v-for="item in list"
            :key="item.id"
            class="shadow flex flex-row items-center overflow-hidden cursor-pointer"
            @click.stop="showDetail(item!)"
        >
            <image :src="getImgUrl(item.imageUrl, true)" class="w-16 h-16 rounded object-cover" />
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

        <uv-popup ref="popup" @change="popChange">
            <view v-if="showItem" class="p-4">
                <div class="min-w-200px" v-html="getStartContent(showItem!)" @tap="catchImage"></div>
            </view>
        </uv-popup>
    </view>
</template>

<script setup lang="ts">
import type { AuctionItemDto } from '@/composables/types'
import { getImgUrl } from '@/composables'
import api from '@/utils/api'
import { onLoad } from '@dcloudio/uni-app'
import dayjs from 'dayjs'

const list = ref<AuctionItemDto[]>([])
const showItem = ref<AuctionItemDto | null>(null)
const popup = ref(null as any)

onLoad(() => {
    api.auctionItem.getMySuccessList({ skipCount: 0, MaxResultCount: 50 }).then((res) => {
        list.value = res.items
    })
})

function showDetail(e: AuctionItemDto) {
    showItem.value = e
    popup.value.open('bottom')
}

function formatTime(item: AuctionItemDto) {
    if (item.dealTime) {
        return dayjs(item.dealTime!).format('MM-DD HH:mm')
    }
    return ''
}

function popChange(e: { show: boolean; type: string }) {
    console.log(e)
    if (e.show === false) {
        showItem.value = null
    }
}

function getStartContent(item: AuctionItemDto) {
    return `<div>商品名称: ${item.name}</div><div>${item.description}</div>`
}

function catchImage(e: any) {
    console.log('catchImage', e)
    try {
        const description = showItem.value?.description
        if (!description) return
        const list = []
        //从 string中img标签中获取data-url的属性放入数组中
        const reg = /<img.*?data-url=['"](.*?)['"].*?>/g
        let result
        while ((result = reg.exec(description)) !== null) {
            list.push(result[1])
        }

        if (list.length === 0) return
        wx.previewImage({
            current: list[0], // 当前显示图片的http链接
            urls: list, // 需要预览的图片http链接列表
        })

        console.log('catchImage', list)
    } catch (e) {
        console.log('catchImage', e)
    }
}
</script>
