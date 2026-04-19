<template>
    <view class="contacts">
        <scroll-view
            class="scroll-container"
            scroll-y="true"
            refresher-enabled="true"
            :refresher-triggered="refresh"
            :enhanced="true"
            :bounce="true"
            :show-scrollbar="true"
            fast-deceleration
            @refresherrefresh="onRefresh"
        >
            <!-- 已登录时显示通讯录 -->
            <template v-if="isLoggedIn">
                <view class="flex items-center bg-white">
                    <view class="flex-1 m-2">
                        <uv-input
                            ref="searchInput"
                            v-model="filterText"
                            shape="circle"
                            placeholder="搜索联系人"
                            prefixIcon="search"
                            prefixIconStyle="font-size: 22px;color: #909399"
                        ></uv-input>
                    </view>
                </view>
                <view class="contacts-container">
                    <!-- <view class="user-list">
                <view class="user-list-item" v-for="(group, id) in groups" :key="id">
                    <view class="user-item-avatar">
                        <image :src="group.avatar" />
                    </view>
                    <view class="user-item-info">
                        <span class="user-item-info__name">{{ group.name }}</span>
                    </view>
                </view>
            </view> -->
                    <template v-if="chatStore.friends0 && chatStore.friends0.length">
                        <view class="contacts-title">好友申请</view>
                        <view class="user-list">
                            <view v-for="friend in chatStore.friends0" :key="friend.id" class="user-list-item">
                                <view class="user-item-avatar">
                                    <image :src="getImgUrl(friend.headImgUrl!, true)" mode="aspectFill"></image>
                                </view>
                                <view class="user-item-info">
                                    <span class="user-item-info__name">{{ friend.name }}</span>
                                </view>
                                <view class="flex mr-2 space-x-1">
                                    <uv-button
                                        type="success"
                                        size="small"
                                        text="同意"
                                        @click="agree(friend.id!, true)"
                                    ></uv-button>
                                    <uv-button
                                        type="default"
                                        :plain="true"
                                        size="small"
                                        text="拒绝"
                                        @click="agree(friend.id!, false)"
                                    ></uv-button>
                                </view>
                            </view>
                        </view>
                    </template>
                    <view class="contacts-title">好友</view>
                    <view class="user-list">
                        <view
                            v-for="friend in (chatStore.friends || []).filter((x) => x.name.indexOf(filterText) > -1)"
                            :key="friend.id"
                            class="user-list-item"
                            @click="privateChat(friend)"
                        >
                            <view class="user-item-avatar">
                                <image :src="getImgUrl(friend.headImgUrl!, true)" mode="aspectFill"></image>
                            </view>
                            <view class="user-item-info">
                                <span class="user-item-info__name">{{ friend.name }}</span>
                            </view>
                        </view>
                    </view>
                </view>
            </template>
        </scroll-view>
    </view>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onBeforeUnmount, computed } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import type { UserDtoBase } from '@/composables/types'
import api from '@/utils/api'
import { getImgUrl } from '@/composables'
// 创建子组件的引用
const chatStore = useChatStore()
const userStore = useUserStore()
const filterText = ref('')
const refresh = ref(false)
const profile = ref({
    friend: null as UserDtoBase | null,
    group: null,
})

// 登录状态
const isLoggedIn = computed(() => !!userStore.token)

var emit = defineEmits(['refreshCurrentVal'])

//初始化
const init = () => {
    // 未登录时直接跳转登录页
    if (!userStore.token) {
        uni.navigateTo({
            url: '/pages/index/login',
        })
        return
    }
    fetchFriends()
}

onMounted(() => {
    init()
})

// 页面显示时检查登录状态
onShow(() => {
    init()
})
//下拉刷新
const onRefresh = async () => {
    refresh.value = true
    fetchFriends()
    setTimeout(() => {
        refresh.value = false
    }, 300)
    emit('refreshCurrentVal')
}
function fetchFriends() {
    chatStore.getUserFriends(true)
    chatStore.getUserFriends(false)
}

function showFriendProfile(friend: UserDtoBase) {
    profile.value.group = null
    profile.value.friend = friend
}

function privateChat(friend: UserDtoBase) {
    Goto.private({
        id: friend.id! + '',
        name: friend.name!,
        avatar: friend.headImgUrl!,
    })
}

function agree(id: number, s: boolean) {
    api.userFriend.agree({ id: id, status: s }).then(() => {
        fetchFriends()
        emit('refreshCurrentVal')
    })
}

defineExpose({
    init,
})
</script>

<style lang="scss" scoped>
.scroll-container {
    height: 100vh;
    width: 100%;
}

.scroll-container {
    -webkit-overflow-scrolling: touch;
    overscroll-behavior: contain;
}

.scroll-content {
    min-height: 101%;
    padding: 20rpx;
    box-sizing: border-box;
    transform: translateZ(0);
    -webkit-transform: translateZ(0);
    will-change: transform;
}

/* 自定义滚动条样式 */
::-webkit-scrollbar {
    width: 0;
    height: 0;
    background: transparent;
}

.contacts {
    width: 100%;
    height: 100%;
    display: flex;
    flex-direction: column;
}

.contacts .contacts-container {
    width: 100%;
    height: 100%;
    overflow: auto;
}

.contacts .user-list-item {
    height: 132rpx;
    padding-left: 32rpx;
    display: flex;
    align-items: center;
}

.contacts .contacts-title {
    height: 80rpx;
    line-height: 100rpx;
    font-size: 30rpx;
    color: #666666;
    background: #f3f4f7;
    text-indent: 44rpx;
}

.contacts .user-list {
    flex-grow: 1;
    background: #ffffff;
    display: flex;
    flex-direction: column;
}

.contacts .user-item-avatar {
    width: 96rpx;
    height: 96rpx;
    margin-right: 32rpx;
    overflow: hidden;
    position: relative;
}

.contacts .user-item-avatar image {
    width: 100%;
    height: 100%;
    display: block;
}

.contacts .user-item-info {
    height: 130rpx;
    padding-right: 32rpx;
    line-height: 88rpx;
    flex-grow: 1;
    border-bottom: 1px solid #efefef;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.contacts .user-item-info__name {
    font-size: 30rpx;
    font-family: Source Han Sans CN;
    font-style: normal;
    font-weight: bold;
    color: #262628;
}
</style>
<route lang="json">
{
    "style": {
        "navigationBarTitleText": "联系人",
        "enablePullDownRefresh": true
    }
}
</route>
