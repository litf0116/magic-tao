<template>
    <view class="p-4">
        <view class="mb-4">
            <text class="text-lg font-bold">推送服务测试</text>
        </view>

        <view class="mb-4 p-3 bg-gray-100 rounded">
            <text class="text-sm text-gray-600">Registration ID:</text>
            <text class="text-sm font-mono mt-1 block break-all">{{ registrationId || '未获取' }}</text>
        </view>

        <view class="mb-4">
            <text class="text-sm text-gray-600">平台: {{ platform }}</text>
        </view>

        <view class="space-y-3">
            <button class="w-full py-3 bg-blue-500 text-white rounded" @tap="initPush">初始化推送服务</button>

            <button class="w-full py-3 bg-green-500 text-white rounded" @tap="setAlias">设置别名 (用户ID)</button>

            <button class="w-full py-3 bg-orange-500 text-white rounded" @tap="testLocalNotification">
                测试本地通知
            </button>

            <button class="w-full py-3 bg-purple-500 text-white rounded" @tap="getRegistrationId">
                获取 Registration ID
            </button>
        </view>

        <view class="mt-6">
            <text class="text-lg font-bold mb-2 block">拍卖订阅测试</text>
            <view class="flex gap-2">
                <input
                    v-model="auctionItemId"
                    type="number"
                    placeholder="输入拍卖品ID"
                    class="flex-1 border border-gray-300 rounded px-3 py-2"
                />
                <button class="px-4 py-2 bg-red-500 text-white rounded" @tap="subscribeAuction">订阅</button>
            </view>
        </view>

        <view class="mt-4 p-3 bg-gray-50 rounded max-h-60 overflow-auto">
            <text class="text-xs font-mono">{{ logs }}</text>
        </view>
    </view>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { pushService } from '@/utils/push'
import { isApp } from '@/utils/platform'

const registrationId = ref('')
const platform = ref('')
const auctionItemId = ref('')
const logs = ref('')

const userStore = useUserStore()
const auctionStore = useAuctionStore()

onMounted(() => {
    platform.value = isApp() ? 'App' : '小程序/H5'
    log(`平台检测: ${platform.value}`)
    log(`当前用户: ${userStore.user.id || '未登录'}`)
})

function log(msg: string) {
    const time = new Date().toLocaleTimeString()
    logs.value = `[${time}] ${msg}\n${logs.value}`
}

function initPush() {
    // #ifdef APP-PLUS
    log('正在初始化推送服务...')
    pushService
        .init()
        .then(() => {
            log('推送服务初始化成功')
            registrationId.value = pushService.getRegistrationId()
            log(`Registration ID: ${registrationId.value}`)
        })
        .catch((error: any) => {
            log(`初始化失败: ${error?.message || error}`)
        })
    // #endif

    // #ifndef APP-PLUS
    log('推送服务仅在 App 端可用')
    // #endif
}

function getRegistrationId() {
    const id = pushService.getRegistrationId()
    registrationId.value = id
    log(`Registration ID: ${id || '未获取'}`)
}

function setAlias() {
    if (!userStore.user.id) {
        log('请先登录')
        return
    }

    // #ifdef APP-PLUS
    const alias = `user_${userStore.user.id}`
    log(`正在设置别名: ${alias}`)
    pushService
        .setAlias(alias)
        .then(() => {
            log('别名设置成功')
        })
        .catch((error: any) => {
            log(`别名设置失败: ${error?.message || error}`)
        })
    // #endif

    // #ifndef APP-PLUS
    log('别名设置仅在 App 端可用')
    // #endif
}

function testLocalNotification() {
    // #ifdef APP-PLUS
    log('发送本地通知测试...')
    pushService.createLocalNotification({
        messageId: 'test_' + Date.now(),
        title: '测试通知',
        content: '这是一条测试通知消息',
        extras: { type: 'test' },
    })
    log('本地通知已发送')
    // #endif

    // #ifndef APP-PLUS
    log('本地通知仅在 App 端可用')
    // #endif
}

async function subscribeAuction() {
    const id = parseInt(auctionItemId.value)
    if (!id) {
        log('请输入有效的拍卖品ID')
        return
    }

    if (!userStore.user.id) {
        log('请先登录')
        return
    }

    // #ifdef APP-PLUS
    const regId = pushService.getRegistrationId()
    if (!regId) {
        log('推送服务未初始化，请先初始化')
        return
    }

    log(`正在订阅拍卖品 ${id}...`)
    log(`Registration ID: ${regId}`)

    try {
        await auctionStore.startNotify(id, 'app', regId)
        log(`订阅成功! 拍卖开始时将收到推送通知`)
    } catch (error: any) {
        log(`订阅失败: ${error?.message || error}`)
    }
    // #endif

    // #ifdef MP-WEIXIN
    log('小程序端请使用拍卖页面的订阅按钮')
    // #endif
}
</script>

<style scoped>
.break-all {
    word-break: break-all;
}
</style>

<route lang="json">
{
    "style": {
        "navigationBarTitleText": "推送测试"
    }
}
</route>
