<template>
    <main class="min-h-screen">
        <div class="wrap">
            <div class="header">
                <img
                    class="logo2"
                    src="https://cdn.molitao.top/molitao/2025-03-30/upload_4kascmjk9aaz06s7j0frqdjfq8m3zmxc.png"
                />
            </div>
            <public-nav />
            <div class="content px-4">
                <div class="home-container">
                    <div class="home-menu">
                        <div class="my-5 mx-auto">
                            <img class="user-avatar" :src="userStore.user.headImgUrl" />
                            <div class="user-profile">
                                <div class="user-profile-main">
                                    <div class="user-profile-header">
                                        <img :src="userStore.user.headImgUrl" />
                                        <div>{{ userStore.user.name }}</div>
                                    </div>
                                    <div class="user-profile-info">
                                        <div class="user-profile-info-title">手机</div>
                                        <div>{{ userStore.user.phoneNumber }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="menu-box">
                            <div class="menu-list">
                                <router-link class="menu-item" to="/chat/auction/auction" replace>
                                    <div>
                                        <i class="iconfont icon-zaixiankefu"></i>
                                        <span v-if="unreadAmount" class="menu-unread">{{ unreadAmount }}</span>
                                    </div>
                                </router-link>

                                <router-link class="menu-item" to="/chat/contacts" replace>
                                    <div><i class="iconfont icon-haoyou"></i></div>
                                    <el-badge v-if="userFriendCount > 0" :value="userFriendCount" class="badgeItem">
                                    </el-badge>
                                </router-link>

                                <!-- <router-link class="menu-item" to="/chat/account" replace>
                                    <div>
                                        <i class="size-7 i-icon-park-outline:payment-method"></i>
                                    </div>
                                </router-link> -->
                            </div>

                            <div>
                                <div class="mb-4 text-center cursor-pointer">
                                    <div
                                        class="i-carbon:settings size-7 text-gray-500 hover:text-gray-700"
                                        @click="openSetting"
                                    />
                                </div>
                                <!-- <div class="text-center cursor-pointer">
                                    <div
                                        class="i-carbon:logout size-7 text-gray-500 hover:text-gray-700"
                                        @click="logout"
                                    />
                                </div> -->

                                <div class="exit">
                                    <i class="iconfont icon-h hover:text-gray-700" @click="logout"></i>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="home-main">
                        <router-view v-slot="{ Component, route }">
                            <component :is="Component" :key="route.fullPath" />
                        </router-view>
                    </div>
                </div>
            </div>

            <Footer />
        </div>
        <user-setting ref="settingRef" />
        <!-- 图片预览弹窗 -->
        <div v-if="imagePreview.visible" class="image-preview">
            <img v-if="imagePreview.url" :src="imagePreview.url" alt="图片" />
            <span class="close" @click="imagePreview.visible = false">×</span>
        </div>
    </main>
</template>

<script setup lang="ts">
import { onmessageKey } from '@/stores/chatStore'
import { useEventBus } from '@vueuse/core'
import { ChatMessageType } from '@/api/appService'
import { useSound } from '@vueuse/sound'
import { ElMessageBox } from 'element-plus'
import UserSetting from '@/components/UserSetting.vue'
import publicNav from './publicNav.vue'
import Footer from '@/components/Footer.vue'
import { GetUserFriendCount } from '@/api/userFriendAPI'

const router = useRouter()

import s1 from '@/assets/wav/cgsys11.mp3'
import s2 from '@/assets/wav/cgsys17.mp3'

const chatStore = useChatStore()
const userStore = useUserStore()
const settingRef = ref<InstanceType<typeof UserSetting> | null>(null)

// 图片预览弹出框
const imagePreview = ref({
    visible: false,
    url: '',
})

const showImagePreview = (url: string) => {
    imagePreview.value.url = url
    imagePreview.value.visible = true
}
const hideImagePreview = () => {
    imagePreview.value.visible = false
}
const userFriendCount = ref(0)
const timer = ref(null)

provide('showImagePreview', showImagePreview)
provide('hideImagePreview', hideImagePreview)

function openSetting() {
    settingRef.value?.show(true)
}

// const { play1 } = useSound(s1)
// const { play2 } = useSound('https://cdn.wujiangapp.com/PicGo/202403301644905.mp3')
const ring11 = useSound(s1)
const ring17 = useSound(s2)

const bus = useEventBus(onmessageKey)

//LINK[epic=处理收到消息] - Layout处理收到消息
const unsubscribe = bus.on((msg: any) => {
    console.log('Recive ChatMessage', msg)
    //LINK - 播放提示音
    if (msg.from === userStore.user.id) return //自己发的消息不提醒

    if (msg.type === ChatMessageType.Welcome && msg.chan !== '0_lobby' && msg.chan !== '-1_auction') {
        ring17.play()
    } else if ((msg.type === 'Text' || msg.type === 'Image' || msg.type === 'AuctionDeal') && !msg.chan) {
        ring11.play()
    }
})

const unreadAmount = ref(0)

onMounted(() => {
    console.log('Chat mounted')
    chatStore.getChatList()
    chatStore.connectServer(true).then(async () => {
        //todo
    })
    getUserFriendCount()

    timer.value = setInterval(() => {
        getUserFriendCount()
    }, 1000 * 30)
})

onUnmounted(() => {
    chatStore.close()
    // unregister the listener
    unsubscribe()
    // or
    // bus.off(listener)
    // 清除定时器
    if (timer.value) {
        clearInterval(timer.value)
        timer.value = null
    }
})

function logout() {
    //confirm
    ElMessageBox.confirm('确定要退出登录吗?', '提示', {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning',
    }).then(() => {
        chatStore.clear()
        userStore.logout()
        router.push('/')
    })
}
//获取用户好友申请列表
const getUserFriendCount = async () => {
    var res = await GetUserFriendCount()
    if (res.data > 0) {
        userFriendCount.value = res.data
    } else {
        userFriendCount.value = -1
    }
}
//提供方法
provide('layoutMethods', { getUserFriendCount })
</script>

<style lang="scss" scoped>
.badgeItem {
    position: absolute;
    top: 0px;
    left: 40px;
}

main {
    display: flex;
    justify-content: center;
    // align-items: center;
    background: #eefaff url(https://cdn.molitao.top/molitao/2025-03-30/upload_3wj6aaa2dnfyy9zq63x6iuyv75bfnudn.png)
        no-repeat center top;
}

.wrap {
    @apply w-[95vw] lg:w-[1232px] flex flex-col items-center relative;

    .header {
        @apply w-full h-[110px] flex flex-col justify-end items-start lg:w-1100px lg:mx-auto;
        background: url(https://cdn.molitao.top/20250330/04j40l4ynlbh3v3h4bgfe7j2pxiqjg8d.png) no-repeat;
        background-position: top -40px right -20px;
        background-size: 434px 363px;

        .logo2 {
            @apply w-344px;
        }
    }

    .content {
        @apply w-full min-h-400px relative mt-24px lg:mt-53px w-[90vw] xl:w-[1232px];
        background: url(https://cdn.molitao.top/molitao/2025-03-30/upload_qxgt8fo3iymdi0heth3rnqipc83rzawn.png) repeat-y
            center center / 100% 100%;
    }

    .content::before {
        content: '';
        @apply block absolute w-full h-18px -top-18px lg:-top-53px lg:h-53px left-0 right-0;
        background: url(https://cdn.molitao.top/molitao/2025-03-30/upload_iw2aq9rsovog4lr3v036irwm90nyos20.png)
            no-repeat center center / 100% 100%;
    }

    .content::after {
        content: '';
        @apply block absolute w-full h-18px -bottom-18px lg:h-45px lg:-bottom-45px left-0 right-0;
        background: url(https://cdn.molitao.top/molitao/2025-03-30/upload_to45oxex09l2uu1ltntj09n6z1x4y0df.png)
            no-repeat center center / 100% 100%;
    }
}
</style>

<style scoped>
.home-container {
    @apply min-h-700px;
    /* background: #ffffff; */
    width: 100%;
    height: 100%;
    display: flex;
    position: relative;
}

.home-menu {
    width: 60px;
    background-color: #f7f7f7;
    border-right: 1px solid #eeeeee;
    display: flex;
    flex-direction: column;
    align-items: center;
}

.user-avatar {
    @apply w-40px h-40px rounded-lg cursor-pointer;
}

.user-avatar:hover + .user-profile {
    @apply block;
}

.user-profile {
    @apply hidden text-white absolute top-0 left-70px w-250px h-200px bg-white z-999;
}

.user-profile-main {
    @apply border-1 border-solid border-gray-200 bg-white text-gray-800 rounded;
}

.user-profile-header {
    padding: 18px 20px;
    border-bottom: 1px solid #ebeef5;
    display: flex;
    flex-direction: column;
    align-items: center;
    font-size: 15px;
    font-weight: bold;
}

.user-profile-header img {
    width: 45px;
    height: 45px;
}

.user-profile-info {
    display: flex;
    padding: 10px 20px;
    font-size: 14px;
    color: #666666;
    line-height: 28px;
}

.user-profile-info-title {
    width: 70px;
}

.menu-box {
    padding: 40px 0;
    flex: 1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    align-items: center;
}

.menu-list {
    display: flex;
    flex-direction: column;
    align-items: center;
}

.menu-item {
    color: #303133;
    cursor: pointer;
    height: 56px;
    position: relative;
}

.menu-unread {
    @apply absolute flex flex-center -top-5px right-5px w-18px h-18px text-12px text-center rounded-full bg-[#d02129] text-white;
}

.router-link-active i {
    color: #d02129 !important;
}

.iconfont {
    padding: 15px;
    font-size: 28px;
    color: #606266;
    cursor: pointer;
}

.iconfont:active {
    color: #d02129;
}

.home-main {
    padding: 0;
    flex: 1;
}

.image-preview {
    max-width: 750px;
    max-height: 500px;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    position: fixed;
    margin: auto;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 9998;
}

.image-preview img {
    max-width: 750px;
    max-height: 500px;
}

.image-preview .close {
    font-size: 50px;
    line-height: 24px;
    cursor: pointer;
    color: #ffffff;
    position: absolute;
    top: 10px;
    right: 5px;
    z-index: 1002;
}
</style>
