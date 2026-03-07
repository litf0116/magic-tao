<template>
    <scroll-view class="conversations" scroll-y="true">
        <view v-if="chatStore.chatList.length">
            <view
                v-for="(x, key) in orderBy(chatStore.chatList, ['order'], ['desc'])"
                :key="key"
                class="scroll-item"
                @tap.stop="chat(x)"
            >
                <view class="item-head">
                    <image v-if="x.id && x.id > 0" :src="getImgUrl(x.avatar, true)" class="head-icon"></image>
                    <template v-else>
                        <!-- 系统公告：支持 ID 或名称匹配 -->
                        <image
                            v-if="x.id === -10 || x.name === '系统公告'"
                            src="/static/images/system-announcement.png"
                            class="head-icon"
                            mode="aspectFill"
                        ></image>
                        <!-- 新手版主群聊：支持 ID 或名称匹配 -->
                        <image
                            v-else-if="x.id === -11 || x.name === '新手版主群聊'"
                            src="/static/images/newbie-mod-chat.png"
                            class="head-icon"
                            mode="aspectFill"
                        ></image>
                        <div
                            v-else-if="x.id === 0"
                            class="head-icon bg-blue-600 text-white font-bold w-full h-full flex flex-center"
                        >
                            大厅
                        </div>
                        <div
                            v-else-if="x.id === -1"
                            class="head-icon bg-green-600 text-white font-bold w-full h-full flex flex-center"
                        >
                            秒杀
                        </div>
                        <!-- 其他系统群组：如果有本地静态图片头像则显示 -->
                        <image
                            v-else-if="x.avatar && x.avatar.startsWith('/static/')"
                            :src="x.avatar"
                            class="head-icon"
                            mode="aspectFill"
                        ></image>
                        <div
                            v-else
                            class="head-icon bg-black text-white font-bold w-full h-full flex flex-center"
                            :style="{ backgroundColor: getRandomColor(x.name) }"
                        >
                            组队
                        </div>
                    </template>
                    <view v-if="x.unread" class="item-head_unread">{{ x.unread }}</view>
                </view>
                <view class="scroll-item_info">
                    <view class="item-info-top">
                        <text
                            class="item-info-top_name"
                            :class="[
                                x.id === -10
                                    ? 'text-purple-600 !font-bold'
                                    : x.id === -11
                                    ? 'text-blue-500 !font-bold'
                                    : x.name === 'lobby'
                                    ? 'text-blue-600 !font-bold'
                                    : x.name === 'auction'
                                    ? 'text-green-600 !font-bold'
                                    : 'text-gray-700',
                            ]"
                        >{{ getChannelDisplayName(x) }}
                        </text>
                        <view class="item-info-top_time"> {{ dayjs(x.time).format('MM-DD HH:mm') }}</view>
                    </view>
                    <view class="item-info-bottom">
                        <view class="item-info-bottom-item">
                            <view class="item-info-top_content">
                                <text>{{ x.lastMsg }}</text>
                            </view>
                            <view class="item-info-bottom_action" @click.stop="showAction(x)"></view>
                        </view>
                    </view>
                </view>
            </view>
        </view>
        <view v-else class="no-conversation">当前没有会话</view>

        <view v-if="actionPopup.visible" class="action-container">
            <view class="layer" @click="actionPopup.visible = false"></view>
            <view class="action-box">
                <view class="action-item" @click="deleteConversation">删除聊天</view>
            </view>
        </view>
    </scroll-view>
</template>

<script setup lang="ts">
import {onLoad, onShow, onUnload} from '@dcloudio/uni-app'
import {ref, reactive, onMounted, onBeforeUnmount} from 'vue'
import {Goto} from '@/composables/goto'
import {orderBy} from 'lodash'
import {getImgUrl} from '@/composables'
import dayjs from 'dayjs'
import type {ChatListItem, ChatMessage} from '@/composables/types'

const chatStore: any = useChatStore()
const userStore = useUserStore()
//初始化
const init = () => {
    if (userStore.token == '') {
        const url = '/pages/index/login'
        uni.navigateTo({url})
        return
    }
    chatStore.getChatList()
    chatStore.connectServer().then(async () => {
        //todo
    })
}
const actionPopup = reactive({
    visible: false,
    conversation: null as ChatMessage | null,
})

async function chat(chat: ChatListItem) {
    await chatStore.SetCurrentChat(chat)
    await chatStore.SetUnread(chat, 0)

    if (chat.type === ChatListItemType.user) {
        Goto.private({
            id: `${chat.id}`,
            name: chat.name,
            avatar: chat.avatar || 'https://cdn.molitao.top/avater.png',
        })
    } else if (chat.id === -10) {
        Goto.group({id: '-10_announcement', name: '系统公告'})
    } else if (chat.id === -11) {
        Goto.group({id: '-11_newbie', name: '新手版主群聊'})
    } else if (chat.id === 0) {
        Goto.lobby()
    } else if (chat.id === -1) {
        Goto.auction()
    } else {
        Goto.group({id: `${chat.id}_${chat.name}`})
    }
}

function getChannelDisplayName(chat: ChatListItem) {
    if (chat.id === -10) return '系统公告'
    if (chat.id === -11) return '新手版主群聊'
    if (chat.name === 'lobby') return '勇者招募所'
    if (chat.name === 'auction') return '秒杀场'
    return chat.name
}

function showAction(conversation: any) {
    actionPopup.visible = true
    actionPopup.conversation = conversation
}

function deleteConversation() {
    actionPopup.visible = false
    uni.showModal({
        title: '提示',
        content: '确定删除聊天记录吗？',
        success: async (res) => {
            if (res.confirm) {
                let conversation: any = actionPopup.conversation!
                chatStore.deleteChat(conversation)
            }
        },
    })
}

//根据传入的string不同,获得不同的随机颜色
const getRandomColor = (str: string) => {
    if (!str) return '#000'
    let hash = 0
    for (let i = 0; i < str.length; i++) {
        hash = str.charCodeAt(i) + ((hash << 5) - hash)
    }
    let color = '#'
    for (let i = 0; i < 3; i++) {
        let value = (hash >> (i * 8)) & 0xff
        color += ('00' + value.toString(16)).slice(-2)
    }
    return color
}

defineExpose({
    init,
})
</script>

<style lang="scss" scoped>
.conversations {
    width: 750rpx;
    overflow-x: hidden;
    display: flex;
    flex-direction: column;
    box-sizing: border-box;
    height: 100%;

    .scroll-item {
        height: 152rpx;
        display: flex;
        align-items: center;
        padding-left: 32rpx;
    }

    .scroll-item .head-icon {
        width: 100rpx;
        height: 100rpx;
        margin-right: 28rpx;
    }

    .scroll-item_info {
        height: 151rpx;
        width: 590rpx;
        padding-right: 32rpx;
        box-sizing: border-box;
        border-bottom: 1px solid #efefef;
    }

    .scroll-item_info .item-info-top {
        padding-top: 20rpx;
        height: 60rpx;
        line-height: 60rpx;
        display: flex;
        align-items: center;
        justify-content: space-between;
    }

    .item-info-top_name {
        font-size: 34rpx;
    }

    .item-info-top_time {
        font-size: 26rpx;
        color: rgba(179, 179, 179, 0.8);
        font-family: Source Han Sans CN;
    }

    .item-info-bottom {
        height: 40rpx;
        line-height: 40rpx;
        overflow: hidden;
    }

    .item-info-bottom-item {
        display: flex;
        justify-content: space-between;
    }
}

.item-info-bottom .item-info-top_content {
    font-size: 30rpx;
    color: #b3b3b3;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.item-info-bottom .item-info-bottom_action {
    width: 50rpx;
    height: 50rpx;
    font-size: 20rpx;
    background: url('@/static/images/action.png') no-repeat center;
    background-size: 28rpx 30rpx;
}

.no-conversation {
    width: 100%;
    text-align: center;
    height: 80rpx;
    line-height: 80rpx;
    font-size: 28rpx;
    color: #9d9d9d;
}

.item-head {
    position: relative;
}

.item-head .item-head_unread {
    padding: 6rpx;
    background-color: #ee593c;
    color: #ffffff;
    font-size: 24rpx;
    line-height: 28rpx;
    border-radius: 24rpx;
    min-width: 24rpx;
    min-height: 24rpx;
    text-align: center;
    position: absolute;
    top: 0;
    right: 15rpx;
}

.action-container {
    width: 100%;
    height: 100%;
    position: fixed;
    top: 0;
    left: 0;
    display: flex;
    justify-content: center;
    align-items: center;
}

.action-container .layer {
    position: absolute;
    top: 0;
    left: 0;
    background: rgba(51, 51, 51, 0.5);
    width: 100%;
    height: 100%;
    z-index: 99;
}

.action-box {
    width: 400rpx;
    background: #ffffff;
    position: relative;
    z-index: 100;
    border-radius: 20rpx;
    overflow: hidden;
}

.action-item {
    text-align: center;
    line-height: 120rpx;
    font-size: 34rpx;
    color: #262628;
    border-bottom: 1px solid #efefef;
}

.unread-text {
    color: #d02129;
}
</style>

<route lang="json">
{
"style": {
"navigationBarTitleText": "会话"
}
}
</route>
