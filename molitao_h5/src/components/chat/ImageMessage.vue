<template>
    <image
        class="image-content"
        mode="heightFix"
        :src="imgUrl"
        :style="{ height: imageHeight + 'rpx' }"
        :data-url="originUrl"
        @tap.stop="showImageFullScreen"
        @longpress.stop="showActionPopup(message, true)"
    ></image>
</template>

<script setup lang="ts">
import type { ChatMessage } from '@/composables/types'
import { computed } from 'vue'
const props = defineProps<{
    message: ChatMessage
    showImageFullScreen: (e: any) => void
    showActionPopup: (message: ChatMessage, isLongPress: boolean) => void
}>()

const IMAGE_MAX_WIDTH = 200
const IMAGE_MAX_HEIGHT = 150

function getImageHeight(width: number, height: number) {
    if (width < IMAGE_MAX_WIDTH && height < IMAGE_MAX_HEIGHT) {
        return height
    } else if (width > height) {
        return (IMAGE_MAX_WIDTH / width) * height
    } else if (width === height || width < height) {
        return IMAGE_MAX_HEIGHT
    }
}

function getImgUrl(message: any, thub = true) {
    let payload = message.payload
    if (!payload) return ''
    if (typeof payload === 'string') {
        try {
            payload = JSON.parse(payload)
        } catch {
            return ''
        }
    }
    if (!payload.url) return ''
    if (payload.url.startsWith('http')) {
        return payload.url + (thub ? '!w300' : '')
    }
    return `${import.meta.env.VITE_APP_UPYUN_IMG_URL}${payload.url}${thub ? '!w300' : ''}`
}

const imgUrl = computed(() => getImgUrl(props.message, true))
const originUrl = computed(() => getImgUrl(props.message, false))
const imageHeight = computed(() => {
    let payload = props.message.payload
    if (!payload) return 300
    if (typeof payload === 'string') {
        try {
            payload = JSON.parse(payload)
        } catch {
            return 300
        }
    }
    const h = getImageHeight(payload.width, payload.height)
    return h || 300
})
</script>

<style scoped>
.image-content {
    border-radius: 12rpx;
    width: 300rpx;
    height: 300rpx;
}
</style>
