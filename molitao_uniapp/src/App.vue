<script setup lang="ts">
import { onLaunch, onShow, onHide, onUnload } from '@dcloudio/uni-app'
import { ref } from 'vue'
import { useEventBus } from '@vueuse/core'
import api from '@/utils/api'
import { pushService } from '@/utils/push'
import { appUpdateManager } from '@/utils/appUpdate'
import UpdateModal from '@/components/UpdateModal.vue'

const getSystemInfoSync = uni.getSystemInfoSync()

const showUpdateModal = ref(false)
const versionInfo = ref<any>(null)
const downloading = ref(false)
const downloadProgress = ref(0)

onLaunch(() => {
    userStore.checkLogin()

    pushService.init()

    checkForUpdate()

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

async function checkForUpdate() {
    // #ifdef APP-PLUS
    try {
        const update = await appUpdateManager.checkUpdate()
        if (update) {
            versionInfo.value = update
            showUpdateModal.value = true
        }
    } catch (error) {
        console.error('检查更新失败', error)
    }
    // #endif
}

async function handleUpdate() {
    // #ifdef APP-PLUS
    try {
        downloading.value = true
        downloadProgress.value = 0

        await appUpdateManager.downloadAndInstall(
            versionInfo.value.downloadUrl,
            versionInfo.value.fileName,
            versionInfo.value.isForceUpdate,
            (progress: number) => {
                downloadProgress.value = progress
            }
        )

        showUpdateModal.value = false
    } catch (error) {
        console.error('更新失败', error)
        uni.showToast({
            title: '更新失败，请重试',
            icon: 'none',
        })
        downloading.value = false
    }
    // #endif
}

function handleCancelUpdate() {
    if (!versionInfo.value?.isForceUpdate) {
        showUpdateModal.value = false
    }
}

onShow(() => {})
onHide(() => {})

onUnload(() => {
    unsubscribe()
})

const userStore = useUserStore()

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
    <UpdateModal
        v-model:visible="showUpdateModal"
        :version-info="versionInfo"
        :downloading="downloading"
        :progress="downloadProgress"
        @confirm="handleUpdate"
        @cancel="handleCancelUpdate"
    />
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
