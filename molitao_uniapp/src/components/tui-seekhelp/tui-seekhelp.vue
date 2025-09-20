<template>
    <view class="flex gap-x-4 py-2">
        <video
            v-if="getFirstVideo(item.fileUrls)"
            :id="`${item.id}`"
            class="size-14 rounded-2 flex-none"
            :src="getFirstVideo(item.fileUrls)"
            controls
            @play.stop="playVideo(`${item.id}`)"
            @fullscreenchange="screenChange($event, `${item.id}`)"
        ></video>
        <image
            v-else
            class="size-14 rounded-2 flex-none"
            :src="getFirstImgUrl(item.fileUrls)"
            alt=""
            @tap="imgClick(item.fileUrls)"
        />
        <view class="flex-auto" @tap.stop="goDetail(item.id)">
            <view class="flex items-baseline justify-between gap-x-4">
                <view class="text-sm font-semibold leading-6 text-gray-900">
                    机车编号：{{ item.locomotiveNumber }}
                </view>
                <view class="flex-none text-xs text-gray-600">
                    <time :datetime="item.creationTime">{{ formatTime(item.creationTime) }}</time>
                </view>
            </view>
            <view class="mt-1 line-clamp-2 text-sm leading-6 text-gray-600">故障简要描述：{{ item.content }}</view>
        </view>
    </view>
</template>
<script setup lang="ts">
import { onLoad } from '@dcloudio/uni-app'

const props = defineProps<{
    item: any
}>()

function getFirstVideo(urls: string) {
    if (urls) {
        const first = urls.split('|').find((url) => isVideo(url))
        return first
    }
    return ''
}

function getFirstImgUrl(urls: string) {
    if (urls) {
        const firstImg = urls.split('|').find((url) => !isVideo(url))
        return firstImg
    }
    return ''
}

function formatTime(time: string) {
    return formatDate(time, 'fromNow')
}

function imgClick(str: string) {
    const urls = str.split('|').filter((url) => !isVideo(url))
    if (urls && urls.length > 0) {
        uni.previewImage({
            urls,
        })
    }
}

function screenChange(e, id: string) {
    let fullScreen = e.detail.fullScreen
    if (!fullScreen) {
        //退出全屏
        const videoContext = uni.createVideoContext(id)
        videoContext.stop()
    }
}

function goDetail(id: number) {
    uni.navigateTo({
        url: `/pages/seekHelp/detail?id=${id}`,
        events: {
            refresh: () => {
            },
        },
    })
}

onLoad(() => {})
</script>
