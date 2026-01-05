<template>
    <view class="container">
        <!-- 组件页面 -->
        <view class="component-page" style="height: 100%">
            <home v-if="current == 0" @refreshCurrentVal="toIndex"></home>
            <chat v-if="current == 1" ref="chatRef" @refreshCurrentVal="getUserFriendCount"></chat>
            <tradingPost v-if="current == 2" ref="tradingPostRef" @updateModalConfig="updateModal"></tradingPost>
            <contacts v-if="current == 3" ref="contactsRef" @refreshCurrentVal="getUserFriendCount"></contacts>
            <my v-if="current == 4" @refreshCurrentVal="toIndex"></my>
        </view>

        <!-- tabbar -->
        <view class="tabbar">
            <template v-for="(item, index) in tabbarList" :key="index">
                <view class="tabbar-item" @click="toIndex(index)">
                    <view class="icon-wrapper">
                        <image :src="current === index ? item.selectedIconPath : item.iconPath"></image>
                        <!-- 添加角标 -->
                        <view v-if="index == 3 && badgeCount > 0" class="badge">{{ badgeCount }}</view>
                    </view>
                    <text :class="['font-title', current === index ? 'font-title-active' : '']">{{ item.text }}</text>
                </view>
            </template>
            <view class="mid-btn-arc" :style="elementStyle"></view>
            <view class="mid-btn" @click="toIndex(2)">
                <image class="mid-img" src="../../static/images/add.png"></image>
            </view>
        </view>

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
import { onLoad, onShow, onUnload, onPullDownRefresh, onReachBottom } from '@dcloudio/uni-app'
import { ref, reactive, onMounted, onBeforeUnmount } from 'vue'
import api from '@/utils/api'
import home from '../index/index.vue'
import chat from '../chat/index.vue'
import tradingPost from '../tradingPost/index.vue'
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

const tradingPostRef = ref(null)
const contactsRef = ref(null)

const timer = ref(null)
const badgeCount = ref(0)
const current = ref(0)
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
const chatRef = ref(null)
const userStore = useUserStore()
const elementStyle = ref({
    left: '50%', // 默认值
})
onShow(() => {
    uni.$on('refreshView', () => {
        if (chatRef.value) {
            chatRef.value.init()
        }
    })
    uni.getSystemInfo({
        success: (res) => {
            const screenWidth = res.windowWidth
            // 手动计算 100rpx 等于多少 px
            // 标准比例：750rpx = 屏幕宽度(px)
            const rpxRatio = screenWidth / 750
            const offsetPx = 100 * rpxRatio

            // 屏幕中点
            const halfScreen = screenWidth / 2
            //获取系统信息
            const systemInfo = uni.getSystemInfoSync()
            // 判断操作系统
            if (systemInfo.platform === 'android') {
                elementStyle.value = {
                    left: `${halfScreen - offsetPx}px`,
                }
            } else if (systemInfo.platform === 'ios') {
                elementStyle.value = {
                    left: `${halfScreen - offsetPx + 6}px`,
                }
            } else {
                elementStyle.value = {
                    left: `${halfScreen - offsetPx + 6}px`,
                }
            }
        },
    })
    if (userStore.token) {
        getUserFriendCount()
    }
    // 启动定时器 - 每5秒执行一次
    timer.value = setInterval(() => {
        getUserFriendCount()
    }, 30000)
})
onUnload(() => {
    // 页面卸载时移除事件监听，避免内存泄漏
    uni.$off('refreshView')
})
onBeforeUnmount(() => {
    if (timer.value) {
        clearInterval(timer.value)
        timer.value = null
    }
})
onReachBottom(() => {
    uni.$emit('onReachBottom')
})

//选择跳转页面
const toIndex = (index) => {
    current.value = index
    switch (current.value) {
        case 0:
            uni.setNavigationBarTitle({
                title: '魔力淘',
            })
            break
        case 1:
            uni.setNavigationBarTitle({
                title: '会话',
            })
            break
        case 2:
            uni.setNavigationBarTitle({
                title: '交易站',
            })
            break
        case 3:
            uni.setNavigationBarTitle({
                title: '联系人',
            })
            break
        case 4:
            uni.setNavigationBarTitle({
                title: '个人中心',
            })
            break
    }

    if (current.value === 4) {
        //动态修改状态栏的文字颜色
        uni.setNavigationBarColor({
            frontColor: '#000000',
            backgroundColor: '#f6f6f6',
        })
    } else {
        uni.setNavigationBarColor({
            frontColor: '#ffffff',
            backgroundColor: '#F4835a',
        })
    }
    if (current.value == 1) {
        setTimeout(() => {
            chatRef.value.init()
        }, 300)
    }
}
//获取用户好友申请记录
const getUserFriendCount = () => {
    api.userFriend.GetUserFriendCount().then((res) => {
        badgeCount.value = res
    })
}
//更新数据
const updateModal = (data) => {
    Object.assign(modalConfig, data)
}
// 取消按钮处理
const onCancel = () => {
    modalConfig.show = false
}
// 确认按钮处理
const onConfirm = () => {
    modalConfig.show = false
}
</script>

<style lang="scss">
/* 内容区域样式 */
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
    /* 提高层级 */
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

.mid-btn-arc {
    position: fixed;
    bottom: 50rpx;
    // left: calc(51% - 100rpx);
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
