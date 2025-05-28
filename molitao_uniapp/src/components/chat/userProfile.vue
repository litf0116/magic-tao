<template>
    <div v-if="profile" class="profile-card">
        <div class="profile-card-title">
            <div class="flex items-center p-4">
                <image :src="getImgUrl(profile.headImgUrl, true)" class="size-24 rounded" mode="aspectFill" />
                <div class="text-lg">{{ profile.name }}</div>
            </div>
        </div>
        <div class="friend-info">
            <div class="info-name">用户编号</div>
            <div class="info-text" @click="copyText(profile.id)">
                <view> {{ profile.id }}</view>
                <view class="i-mdi:content-copy text-gray-500 text-xl"></view>
            </div>
        </div>
        <div class="friend-info">
            <div class="info-name">QQ：</div>
            <div v-if="profile.qq" class="info-text" @click="copyText(profile.qq)">
                <view> {{ profile.qq }}</view>
                <view class="i-mdi:content-copy text-gray-500 text-xl"></view>
            </div>
        </div>
        <div  v-if="profile.wx" class="friend-info" @click="copyText(profile.wx)">
            <div class="info-name">微信：</div>
            <div class="info-text">
                <view> {{ profile.wx }}</view>
                <view class="i-mdi:content-copy text-gray-500 text-xl"></view>
            </div>
        </div>
        <!-- <div class="friend-info">
                    <div class="info-name">手 机</div>
                    <div class="info-text">{{ profile.phoneNumber }}</div>
                </div> -->
        <!-- <div class="flex flex-center space-x-10 p-4">
                    <el-button type="primary" @click="privateChat">发消息</el-button>
                    <el-button @click="profile.friend = null">关闭</el-button>
                </div> -->
    </div>
</template>

<script setup lang="ts">
import { getImgUrl } from '@/composables'
import api from '@/utils/api'

const profile = ref<any>(null)
const prop = defineProps({
    userId: {
        type: Number,
        required: true,
        default: 0,
    },
})

watchEffect(() => {
    if (prop.userId > 0) {
        profile.value = null
        fetch(prop.userId)
    }
})

function fetch(userId: number) {
    api.user.get({ id: userId }).then((res) => {
        // console.log(res)
        profile.value = res
    })
}

function copyText(text: string) {
    if (!text) return
    uni.setClipboardData({
        data: text,
        success: function () {
            uni.showToast({
                title: '复制成功',
                icon: 'none',
            })
        },
    })
}
</script>
<style scoped>
.profile-card {
    height: 100%;
    display: flex;
    flex-direction: column;
}

.profile-card-title {
    @apply flex-1 flex;
    border-bottom: 1rpx solid #eeeeee;
}

.friend-info {
    @apply flex justify-around p-4 text-sm text-gray-700;
}

.info-name {
    text-align: center;
    width: 25vw;
}

.info-text {
    @apply flex flex-1 items-center space-x-4;
}
</style>
