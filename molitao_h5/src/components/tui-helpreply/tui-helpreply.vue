<template>
    <view class="fle p-4 bg-white rounded-2 shadow mb-4">
        <view class="flex-auto">
            <view class="flex items-baseline justify-between gap-x-4">
                <view
                    v-if="item.creatorUser"
                    class="text-sm font-semibold leading-6 text-gray-900 flex items-center"
                    @tap="call(item.creatorUser.phoneNumber)"
                >
                    <text v-if="item.creatorUser">{{ item.creatorUser.name }} </text>
                    <view class="size-5 text-green i-mdi:phone-forward mx-2"></view>
                </view>
                <view class="flex-none text-xs text-gray-600">
                    <time :datetime="item.creationTime">{{ formatTime(item.creationTime) }}</time>
                </view>
            </view>
            <view class="mt-1 line-clamp-2 text-sm leading-6 text-gray-600">
                <text>{{ item.content }}</text>
            </view>
            <view v-if="videos && videos.length" class="mt-4">
                <view class="text text-gray-900">视频：</view>
                <view class="mt-2 grid grid-cols-2 gap-2">
                    <tui-video v-for="url in videos" :key="url" :url="url" width="600rpx" height="400rpx" />
                </view>
            </view>
            <view v-if="pics && pics.length" class="mt-4">
                <view class="text text-gray-900">图片：</view>
                <view class="mt-2">
                    <uv-album :urls="pics" multipleSize="220rpx" space="12rpx"></uv-album>
                </view>
            </view>
        </view>
    </view>
</template>
<script setup lang="ts">
import { onLoad } from '@dcloudio/uni-app'

const props = defineProps({
    item: {
        type: Object,
        default: () => {
            fileUrls: ''
        },
    },
})

function formatTime(time: string) {
    return formatDate(time, 'fromNow')
}

const pics = ref([])
const videos = ref([])
function call(number: string) {
    if (number) uni.makePhoneCall({ phoneNumber: number })
}
watchEffect(() => {
    if (props.item && props.item.fileUrls) {
        pics.value = props.item.fileUrls?.split('|').filter((url: string) => !isVideo(url)) ?? []
        videos.value = props.item.fileUrls?.split('|').filter((url: string) => isVideo(url)) ?? []
    }
})

onLoad(() => {})
</script>
