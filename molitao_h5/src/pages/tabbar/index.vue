<template>
    <view class="container">
        <!-- ============================================= -->
        <!-- #ifdef MP-WEIXIN -->
        <!-- 小程序: 4个tab (首页、会话、通讯录、个人中心) -->
        <!-- ============================================= -->
        <view class="component-page" style="height: 100%">
            <home v-if="current == 0" @refreshCurrentVal="toIndex"></home>
            <chat v-if="current == 1" ref="chatRef" @refreshCurrentVal="getUserFriendCount"></chat>
            <contacts v-if="current == 2" ref="contactsRef" @refreshCurrentVal="getUserFriendCount"></contacts>
            <my v-if="current == personalCenterIndex" @refreshCurrentVal="toIndex"></my>
        </view>

        <!-- 小程序 tabbar -->
        <view class="tabbar">
            <template v-for="(item, index) in tabbarList" :key="index">
                <view class="tabbar-item" @click="toIndex(index)">
                    <view class="icon-wrapper">
                        <image :src="current === index ? item.selectedIconPath : item.iconPath"></image>
                        <view v-if="index == 2 && badgeCount > 0" class="badge">{{ badgeCount }}</view>
                    </view>
                    <text :class="['font-title', current === index ? 'font-title-active' : '']">{{ item.text }}</text>
                </view>
            </template>
        </view>
        <!-- #endif -->
        <!-- ============================================= -->

        <!-- ============================================= -->
        <!-- #ifndef MP-WEIXIN -->
        <!-- APP/H5: 5个tab (首页、会话、交易站、通讯录、个人中心) -->
        <!-- ============================================= -->
        <view class="component-page" style="height: 100%">
            <home v-if="current == 0" @refreshCurrentVal="toIndex"></home>
            <chat v-if="current == 1" ref="chatRef" @refreshCurrentVal="getUserFriendCount"></chat>
            <tradingPost v-if="current == 2" ref="tradingPostRef" @updateModalConfig="updateModal"></tradingPost>
            <contacts v-if="current == 3" ref="contactsRef" @refreshCurrentVal="getUserFriendCount"></contacts>
            <my v-if="current == personalCenterIndex" @refreshCurrentVal="toIndex"></my>
        </view>

        <!-- APP/H5 tabbar -->
        <view class="tabbar">
            <template v-for="(item, index) in tabbarList" :key="index">
                <view class="tabbar-item" @click="toIndex(index)">
                    <view class="icon-wrapper">
                        <image :src="current === index ? item.selectedIconPath : item.iconPath"></image>
                        <view v-if="index == 3 && badgeCount > 0" class="badge">{{ badgeCount }}</view>
                    </view>
                    <text :class="['font-title', current === index ? 'font-title-active' : '']">{{ item.text }}</text>
                </view>
            </template>
            <!-- 中间发布按钮 -->
            <view class="mid-btn-arc" :style="elementStyle"></view>
            <view class="mid-btn" @click="toIndex(2)">
                <image class="mid-img" src="../../static/images/add.png"></image>
            </view>
        </view>
        <!-- #endif -->
        <!-- ============================================= -->

        <custom-modal
            v-model:show="modalConfig.show"
            style="z-index: 99999"
            :title="modalConfig.title"
            :showCancel="modalConfig.showCancel"
            :cancelText="modalConfig.cancelText"
            :confirmText="modalConfig.confirmText"
            @cancel="onCancel"
            @confirm="onConfirm"
        >
            <rich-text :nodes="modalConfig.content"></rich-text>
        </custom-modal>
    </view>
</template>

<script setup>
// =============================================
// 公共引入
// =============================================
import { onShow, onUnload, onPullDownRefresh, onReachBottom } from '@dcloudio/uni-app'
import { ref, reactive } from 'vue'
import api from '@/utils/api'
import home from '../index/index.vue'
import chat from '../chat/index.vue'
import contacts from '../chat/contacts.vue'
import my from '../index/my.vue'
import CustomModal from '@/components/customModal.vue'

const modalConfig = reactive({
    show: false,
    title: '【魔力淘】交易行使用规范',
    content: '<div style="color: red;">这是一段<strong>HTML</strong>内容</div>',
    showCancel: false,
    cancelText: '取消',
    confirmText: '确定',
})

// =============================================
// #ifdef MP-WEIXIN
// 小程序: 4个tab
// =============================================
const chatRef = ref(null)
const contactsRef = ref(null)
const tradingPostRef = ref(null) // 声明但不使用，避名交叉引用问题
const current = ref(0)
const badgeCount = ref(0)
const timer = ref(null)
const personalCenterIndex = 3

const tabbarList = reactive([
    {
        pagePath: 'pages/index/index',
        iconPath: '../../static/images/tab1_b.png',
        selectedIconPath: '../../static/images/tab1.png',
        text: '首页',
    },
    {
        pagePath: 'pages/chat/index',
        iconPath: '../../static/images/tab2_b.png',
        selectedIconPath: '../../static/images/tab2.png',
        text: '会话列表',
    },
    {
        pagePath: 'pages/chat/contacts',
        iconPath: '../../static/images/tab3_b.png',
        selectedIconPath: '../../static/images/tab3.png',
        text: '通讯录',
    },
    {
        pagePath: 'pages/index/my',
        iconPath: '../../static/images/tab4_b.png',
        selectedIconPath: '../../static/images/tab4.png',
        text: '个人中心',
    },
])

const titleMap = {
    0: '魔力淘',
    1: '会话',
    2: '通讯录',
    3: '个人中心',
}
// #endif
// =============================================

// =============================================
// #ifndef MP-WEIXIN
// APP/H5: 5个tab
// =============================================
import tradingPost from '../tradingPost/index.vue'

const chatRef = ref(null)
const contactsRef = ref(null)
const tradingPostRef = ref(null)
const elementStyle = ref({ left: '50%' })

const current = ref(0)
const badgeCount = ref(0)
const timer = ref(null)
const personalCenterIndex = 4

const tabbarList = reactive([
    {
        pagePath: 'pages/index/index',
        iconPath: '../../static/images/tab1_b.png',
        selectedIconPath: '../../static/images/tab1.png',
        text: '首页',
    },
    {
        pagePath: 'pages/chat/index',
        iconPath: '../../static/images/tab2_b.png',
        selectedIconPath: '../../static/images/tab2.png',
        text: '会话列表',
    },
    {
        pagePath: 'pages/tradingPost/index',
        iconPath: '../../static/images/tab3_b.png',
        selectedIconPath: '../../static/images/tab3.png',
        text: '交易站',
    },
    {
        pagePath: 'pages/chat/contacts',
        iconPath: '../../static/images/tab3_b.png',
        selectedIconPath: '../../static/images/tab3.png',
        text: '通讯录',
    },
    {
        pagePath: 'pages/index/my',
        iconPath: '../../static/images/tab4_b.png',
        selectedIconPath: '../../static/images/tab4.png',
        text: '个人中心',
    },
])

const titleMap = {
    0: '魔力淘',
    1: '会话',
    2: '交易站',
    3: '通讯录',
    4: '个人中心',
}
// #endif
// =============================================

const userStore = useUserStore()

// =============================================
// 公共逻辑
// =============================================
onShow(() => {
    uni.$on('refreshView', () => {
        if (chatRef.value) {
            chatRef.value.init()
        }
    })

    // #ifndef MP-WEIXIN
    // APP/H5 需要计算中间按钮位置
    uni.getSystemInfo({
        success: (res) => {
            const screenWidth = res.windowWidth
            const rpxRatio = screenWidth / 750
            const offsetPx = 100 * rpxRatio
            const halfScreen = screenWidth / 2
            const systemInfo = uni.getSystemInfoSync()
            if (systemInfo.platform === 'android') {
                elementStyle.value = { left: `${halfScreen - offsetPx}px` }
            } else {
                elementStyle.value = { left: `${halfScreen - offsetPx + 6}px` }
            }
        },
    })
    // #endif

    if (userStore.token) {
        getUserFriendCount()
        timer.value = setInterval(() => {
            if (userStore.token) {
                getUserFriendCount()
            }
        }, 30000)
    }
})

onUnload(() => {
    uni.$off('refreshView')
    if (timer.value) {
        clearInterval(timer.value)
        timer.value = null
    }
})

onReachBottom(() => {
    uni.$emit('onReachBottom')
})

// 选择跳转页面
const toIndex = (index) => {
    current.value = index

    if (titleMap[current.value]) {
        uni.setNavigationBarTitle({ title: titleMap[current.value] })
    }

    if (current.value === personalCenterIndex) {
        uni.setNavigationBarColor({ frontColor: '#000000', backgroundColor: '#f6f6f6' })
    } else {
        uni.setNavigationBarColor({ frontColor: '#ffffff', backgroundColor: '#F4835a' })
    }

    // 会话列表 - index 1
    if (current.value == 1) {
        setTimeout(() => {
            chatRef.value?.init()
        }, 300)
    }

    // 通讯录 - 小程序 index 2，APP/H5 index 3
    // #ifdef MP-WEIXIN
    if (current.value == 2) {
        setTimeout(() => {
            contactsRef.value?.init()
        }, 300)
    }
    // #endif

    // #ifndef MP-WEIXIN
    if (current.value == 3) {
        setTimeout(() => {
            contactsRef.value?.init()
        }, 300)
    }
    // #endif
}

const getUserFriendCount = () => {
    api.userFriend.GetUserFriendCount().then((res) => {
        badgeCount.value = res
    })
}

const updateModal = (data) => {
    Object.assign(modalConfig, data)
}

const onCancel = () => {
    modalConfig.show = false
}

const onConfirm = () => {
    modalConfig.show = false
}
</script>

<style lang="scss">
.content-area {
    flex: 1;
    padding-bottom: 120rpx;
}

.tabbar {
    position: fixed;
    left: 0;
    bottom: 0;
    height: 120rpx;
    width: 100vw;
    background-color: #f9f9f9;
    z-index: 999;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.tabbar-item {
    flex: 1;
    height: 120rpx;
    background-color: #fff;
    z-index: 100;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    transition: transform 0.3s;
}

.font-title {
    font-size: 22rpx;
    margin: 5rpx 0;
    color: #dfdfdf;
    z-index: 100;
}

.font-title-active {
    font-size: 22rpx;
    margin: 5rpx 0;
    color: #000000;
    z-index: 100;
}

// APP/H5 中间发布按钮
// #ifndef MP-WEIXIN
.mid-btn-arc {
    position: fixed;
    bottom: 50rpx;
    background-color: #fff;
    z-index: 98;
    height: 100rpx;
    width: 200rpx;
    clip-path: path('M 50,0 Q 35,0 25,7.5 Q 20.5, 11.5 0, 16 V 50 H 100 V16 Q 79.5,11.5 75,7.5 Q 65,0 50,0 z');
}

.mid-btn {
    position: fixed;
    height: 100rpx;
    width: 100rpx;
    left: 50%;
    bottom: 45rpx;
    transform: translateX(-50rpx);
    background-color: #fff;
    border-radius: 50rpx;
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 100;

    .mid-img {
        width: 80rpx;
        height: 80rpx;
    }
}
// #endif

image {
    width: 50rpx;
    height: 50rpx;
    transition: transform 0.3s, width 0.3s, height 0.3s;
}

.icon-wrapper {
    position: relative;
    display: flex;
    justify-content: center;
    align-items: center;
}

.badge {
    position: absolute;
    top: -10rpx;
    right: -8rpx;
    min-width: 20rpx;
    height: 30rpx;
    padding: 1px 4px;
    background-color: #ff4d4f;
    border-radius: 16rpx;
    color: #fff;
    font-size: 20rpx;
    line-height: 32rpx;
    text-align: center;
}
</style>
