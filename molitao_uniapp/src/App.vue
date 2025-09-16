<script setup lang="ts">
import { onLaunch, onShow, onHide, onUnload } from '@dcloudio/uni-app'
import { useEventBus } from '@vueuse/core'
import api from '@/utils/api'
// console.log('pages', pages)

const getSystemInfoSync = uni.getSystemInfoSync()
console.log('getSystemInfoSync', getSystemInfoSync)

onLaunch(() => {
    // console.debug('App Launch')
    // userStore.code2Session().then(() => {
    userStore.checkLogin()
    // })
    // #ifdef MP-WEIXIN
    try {
        const updateManager = uni.getUpdateManager()
        // console.log("updateManager 加载成功", updateManager)
        updateManager.onCheckForUpdate(() => {
            // 请求完新版本信息的回调
            // console.debug(res)
        })

        updateManager.onUpdateReady(() => {
            uni.showModal({
                title: '更新提示',
                content: '发现新版本，是否重启应用？',
                success(res) {
                    if (res.confirm) {
                        // 新的版本已经下载好，调用 applyUpdate 应用新版本并重启
                        updateManager.applyUpdate()
                    }
                },
            })
        })
        updateManager.onUpdateFailed(() => {
            // 新的版本下载失败
        })
    } catch (e) {}
})
onShow(() => {
    // console.debug("App Show")
})
onHide(() => {
    // console.log("App Hide")
})

onUnload(() => {
    unsubscribe()
})

const userStore = useUserStore()

const bus = useEventBus(onmessageKey)
//LINK[epic=处理收到消息] - TtPage处理收到消息
const unsubscribe = bus.on((msg: any) => {
    // console.log('App.vue onmessageKey', msg)
    //LINK - 播放提示音
    if (msg.from === userStore.user.id) return //自己发的消息不提醒
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
    innerAudioContext.onPlay(() => {
        // console.log('开始播放')
    })
    innerAudioContext.onError(() => {
        console.log('播放失败')
    })
}
function ring17() {
    const innerAudioContext = uni.createInnerAudioContext()
    innerAudioContext.autoplay = true
    innerAudioContext.src = '/static/wav/cgsys17.mp3'
    innerAudioContext.onPlay(() => {
        // console.log('开始播放')
    })
    innerAudioContext.onError(() => {
        console.log('播放失败')
    })
}
</script>
<style>
/**--- 隐藏scroll-view滚动条*/
::-webkit-scrollbar {
    display: none;
}

uni-scroll-view .uni-scroll-view::-webkit-scrollbar {
    display: none;
}
</style>
