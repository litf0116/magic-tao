<script setup lang="ts">
import { onLaunch, onShow, onHide, onUnload } from '@dcloudio/uni-app'
import { ref } from 'vue'
import { useEventBus } from '@vueuse/core'
import api from '@/utils/api'
import { pushService } from '@/utils/push'
import { useUserStore } from '@/stores/userStore'

const getSystemInfoSync = uni.getSystemInfoSync()
const userStore = useUserStore()

onLaunch(() => {
    userStore.checkLogin()

    pushService.init()

    // #ifdef MP-WEIXIN
    try {
        const updateManager = uni.getUpdateManager()
        updateManager.onCheckForUpdate(() => {})

        updateManager.onUpdateReady(() => {
            uni.showModal({
                title: '更新提示',
                content: '发现新版本，是否重启应用？',
                success(res) {
                    if (res.confirm) {
                        updateManager.applyUpdate()
                    }
                },
            })
        })
        updateManager.onUpdateFailed(() => {})
    } catch (e) {}
    // #endif
})

onShow(() => {})
onHide(() => {})

onUnload(() => {
    unsubscribe()
})

const bus = useEventBus(onmessageKey)
const unsubscribe = bus.on((msg: any) => {
    if (msg.from === userStore.user.id) return
    if (msg.type === ChatMessageType.Welcome && msg.chan !== '0_lobby' && msg.chan !== '-1_auction') {
        ring17()
    } else if ((msg.type === 'Text' || msg.type === 'Image') && !msg.chan) {
        ring11()
    }
})

function ring11() {
    const innerAudioContext = uni.createInnerAudioContext()
    innerAudioContext.autoplay = true
    innerAudioContext.src = '/static/wav/cgsys11.mp3'
    innerAudioContext.onPlay(() => {})
    innerAudioContext.onError(() => {})
}
function ring17() {
    const innerAudioContext = uni.createInnerAudioContext()
    innerAudioContext.autoplay = true
    innerAudioContext.src = '/static/wav/cgsys17.mp3'
    innerAudioContext.onPlay(() => {})
    innerAudioContext.onError(() => {})
}
</script>

<template>
    <!-- 微信小程序专用 -->
</template>

<style>
/**--- 隐藏scroll-view滚动条*/
::-webkit-scrollbar {
    display: none;
}

uni-scroll-view .uni-scroll-view::-webkit-scrollbar {
    display: none;
}
</style>
