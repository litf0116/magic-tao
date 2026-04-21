<template>
    <view class="min-h-screen bg-true-gray-100 relative dark:bg-true-gray-900 dark:text-white">
        <!-- 背景图片层 - 使用 image 标签替代 CSS background -->
        <image
            v-if="topBg"
            class="absolute top-0 left-0 w-full"
            style="height: 373rpx; z-index: 0"
            src="https://image.molitao.top/20250330/097jzbhb3jq364wcrsyjw61qb0bj9xob.png!w300"
            mode="aspectFill"
        />
        <view
            class="relative"
            style="z-index: 1"
            :class="[cssClass ? cssClass : '']"
        >
            <view v-if="showLoading" class="loading">
                <!-- <tui-loading></tui-loading> -->
            </view>
            <template v-else>
                <slot />
            </template>
        </view>
        <!-- <tui-footer /> -->
    </view>
</template>

<script lang="ts" setup>
import { onLoad, onUnload } from '@dcloudio/uni-app'
import { useEventBus } from '@vueuse/core'
const appStore = useAppStore()

const props = defineProps({
    showLoading: {
        type: Boolean,
        default: false,
    },
    topBg: {
        type: Boolean,
        default: true,
    },
    cssClass: {
        type: String,
        default: '',
    },
})

const isDark = computed(() => appStore.darkMode)

const userStore = useUserStore()
const chatStore = useChatStore()

onLoad(() => {
    // console.log("tui-page onLoad", props)
})
</script>

<style lang="scss">
.topBg {
    background: url('https://image.molitao.top/20250330/097jzbhb3jq364wcrsyjw61qb0bj9xob.png');
    background-size: contain;
    // background-attachment: fixed;
    background-size: 100% 100%;
    // position: fixed;
    // width: 100vw;
    // height: 373rpx;
    // top: 0;
    // z-index: 0;
}
</style>
@/stores/appStore
