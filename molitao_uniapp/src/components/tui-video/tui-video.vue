<template>
    <video
        :id="id"
        :src="url"
        class="rounded-2 flex-none"
        controls
        :style="{ width: width, height: height }"
        @play="playVideo"
        @fullscreenchange="screenChange"
    ></video>
</template>
<script setup lang="ts">
const props = withDefaults(
    defineProps<{
        id?: string
        url: string
        width?: string
        height?: string
    }>(),
    {
        id: Math.random().toString(36).substring(2),
        url: '',
        width: '200rpx',
        height: '200rpx',
    }
)

let videoContext: any = null

onMounted(() => {
    videoContext = uni.createVideoContext(props.id)
    // console.log('videoContext', videoContext)
})

function playVideo() {
    // const videoContext = uni.createVideoContext(props.id)
    videoContext.requestFullScreen()
}

function screenChange(e: any) {
    let fullScreen = e.detail.fullScreen
    if (!fullScreen) {
        //退出全屏
        // const videoContext = uni.createVideoContext(props.id)
        videoContext.stop()
    }
}
</script>
