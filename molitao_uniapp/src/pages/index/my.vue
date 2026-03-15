<template>
    <view class="px-4 bg-[#f6f6f6] min-h-screen">
        <view class="myCard p-4 flex flex-col relative">
            <view class="flex flex-center mb-4">
                <image :src="getImgUrl(userStore.user.headImgUrl, true)" mode="aspectFill" class="size-12 rounded-full">
                </image>
                <view class="flex-1 pl-2 flex flex-col">
                    <view>{{ userStore.user.name }}</view>
                    <!-- <view class="pt-1 text-gray-500 text-sm">123</view> -->
                </view>
                <view class="flex flex-center text-xs">
                    <!-- <view class="text-amber size-4 mr-1 i-solar:verified-check-bold"></view>
                    <view>认证摄影师</view> -->
                </view>
            </view>
            <view class="grid grid-cols-4 gap-4">
                <view class="flex flex-col flex-center">
                    <view class="text-lg">{{ myCount.friend }}</view>
                    <view>好友</view>
                </view>
                <!-- <view class="flex flex-col flex-center" @click.stop="navTo.navTo('/pages/user/balanceLog')">
                    <view class="text-lg">{{ myCount.balance }}</view>
                    <view>余额</view>
                </view> -->
                <view class="flex flex-col flex-center" @click.stop="navTo.navTo('/pages/user/depositLog')">
                    <view class="text-lg">{{ myCount.depositBalance }}</view>
                    <view>魔力值</view>
                </view>
            </view>
            <view
                v-if="userStore.user.id"
                class="absolute right-4 top-4 size-6 zoom-in i-solar:settings-linear"
                @click.stop="navTo.navTo('/pages/user/info')"
            ></view>
        </view>

        <view class="my-4 flex items-center">
            <view class="h-3 w-4px mr-2 bg-[#ccc] rounded-full"> </view>
            <view>工作台</view>
        </view>

        <view class="grid grid-cols-2 gap-2 mb-4 text-[#171717]">
            <view class="myCard py-2 flex flex-center zoom-in" @click.stop="payDeposit">
                <view class="mr-2 bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="i-icon-park-outline:protect size-6"></view>
                </view>
                <view class="font-500">魔力值增加</view>
            </view>
            <view class="myCard py-2 flex flex-center zoom-in" @click.stop="cashOut">
                <view class="mr-2 bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="i-icon-park-outline:protect size-6"></view>
                </view>
                <view class="font-500">魔力值减少</view>
            </view>
            <!--   <view class="myCard py-2 flex flex-center zoom-in" @click.stop="topUp">
                <view class="mr-2 bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-icon-park-outline:wechat"></view>
                </view>
                <view class="font-500">余额充值</view>
            </view> -->

            <!-- <view v-if="userStore.isAdmin" class="myCard py-2 flex flex-center">
                <view class="mr-2 bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-icon-park-outline:audit"></view>
                </view>
                <view class="font-500">摄影师审核</view>
            </view> -->
        </view>

        <!-- #ifndef MP-WEIXIN -->
        <view class="my-4 flex items-center">
            <view class="h-3 w-4px mr-2 bg-[#ccc] rounded-full"> </view>
            <view>买家</view>
        </view>
        <view class="myCard py-2 grid grid-cols-4 mb-4 text-[#171717]">
            <view class="flex flex-col flex-center zoom-in" @click.stop="wait">
                <view class="bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-icon-park-outline:payment-method"></view>
                </view>
                <text class="pt-1 text-sm font-500">出价中拍卖</text>
            </view>
            <view class="flex flex-col flex-center zoom-in" @click.stop="wait">
                <view class="bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-mdi:deal-outline"></view>
                </view>
                <text class="pt-1 text-sm font-500">待收货</text>
            </view>
            <view class="flex flex-col flex-center zoom-in" @click.stop="wait">
                <view class="bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-icon-park-outline:order"></view>
                </view>
                <text class="pt-1 text-sm font-500">已成交</text>
            </view>
        </view>
        <view class="my-4 flex items-center">
            <view class="h-3 w-4px mr-2 bg-[#ccc] rounded-full"> </view>
            <view>卖家</view>
        </view>
        <view class="myCard py-2 grid grid-cols-4 mb-4 text-[#171717]">
            <view class="flex flex-col flex-center zoom-in" @click.stop="wait">
                <view class="bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-icon-park-outline:ad-product"></view>
                </view>
                <text class="pt-1 text-sm font-500">我要卖</text>
            </view>
            <view class="flex flex-col flex-center zoom-in" @click.stop="wait">
                <view class="bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-mdi:deal-outline"></view>
                </view>
                <text class="pt-1 text-sm font-500">待发货</text>
            </view>
            <view class="flex flex-col flex-center zoom-in" @click.stop="wait">
                <view class="bg-[#f6f6f6] size-10 rounded-full flex flex-center">
                    <view class="size-6 i-icon-park-outline:order"></view>
                </view>
                <text class="pt-1 text-sm font-500">订单</text>
            </view>
        </view>
        <!-- #endif -->

        <view v-if="userStore.user.phoneNumber" class="my-4">
            <uv-button @tap="logout">退出登录</uv-button>
        </view>
        <view class="text-center w-full text-gray-300">{{ appVersion }}</view>

        <custom-modal
            v-model:show="modalVisible"
            title="提示"
            :showCancel="false"
            confirmText="确定"
            @confirm="handleConfirm"
        >
            <view
                >平台提现功能尚未完善，魔力值退还，请加管理员老淡QQ：383875411，微信：18845639111，私信扫码退款。</view
            >
        </custom-modal>
    </view>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, onBeforeUnmount } from 'vue'
import { getImgUrl } from '@/composables'
import { getAppVersion } from '@/utils/version'
import api from '@/utils/api'
import CustomModal from '@/components/customModal.vue'

const userStore = useUserStore()
const navTo = useTo()
const myCount = ref({ auctionSuccess: 0, friend: 0, balance: 0, depositBalance: 0 })
const appVersion = getAppVersion()

const modalVisible = ref(false)
const emit = defineEmits(['refreshCurrentVal'])
onMounted(async () => {
    await userStore.checkLogin(true, false)
    if (userStore.user.id) {
        getMyCount()
    }
    uni.hideHomeButton()
})

function getMyCount() {
    api.client.getMyCount().then((res: any) => {
        myCount.value = res
    })
}
//魔力值充值
function payDeposit() {
    // #ifdef MP-WEIXIN
    api.client.payDeposit({ openid: userStore.openid, amount: 51 }).then((res: any) => {
        uni.requestPayment({
            provider: 'wxpay',
            timeStamp: `${res.timeStamp}`,
            nonceStr: res.nonceStr,
            package: res.package,
            signType: res.signType,
            paySign: res.paySign,
            success: async (res) => {
                try {
                    await userStore.checkLogin(false, true)
                    getMyCount()
                } catch (error) {
                    getMyCount()
                }
                Tips.success('支付成功，魔力值已到账')
            },
            fail: (err) => {
                Tips.info('用户取消支付')
            },
        })
    })
    // #endif

    // #ifdef APP-PLUS
    Tips.info('App 端支付功能开发中，请使用小程序充值')
    // #endif
}
//保证金提交信息弹窗
function cashOut() {
    modalVisible.value = true
}
//隐藏弹窗
const handleConfirm = () => {
    modalVisible.value = false
}

//提款
async function payWithdrawal() {
    //输入要提现的金额
    const amount = await Tips.prompt('', '提现', '请输入提现金额')
    if (!amount) return
    const _value = Number(amount)
    if (!_value) {
        Tips.noCancelModal('请输入数字')
        return
    }
    var userInfo = userStore.user
    api.client.PayWithdrawal({ userId: userInfo.id, amount: _value }).then((res: any) => {
        Tips.noCancelModal('操作成功，等待管理员审批')
    })
}
async function topUp() {
    //输入要充值的金额
    const amount = await Tips.prompt('', '余额充值', '充值请输入充值金额')
    if (!amount) return
    const _value = Number(amount)
    if (!_value) {
        Tips.noCancelModal('请输入数字')
        return
    }

    // #ifdef MP-WEIXIN
    api.client.TopUp({ openid: userStore.openid, amount: _value }).then((res: any) => {
        uni.requestPayment({
            provider: 'wxpay',
            timeStamp: `${res.timeStamp}`,
            nonceStr: res.nonceStr,
            package: res.package,
            signType: res.signType,
            paySign: res.paySign,
            success: (res) => {
                Tips.noCancelModal('充值成功,余额显示数量将会稍后更新', '充值成功').then(() => {
                    getMyCount()
                })
            },
            fail: (err) => {
                Tips.info('用户取消支付')
            },
        })
    })
    // #endif

    // #ifdef APP-PLUS
    Tips.info('App 端支付功能开发中，请使用小程序充值')
    // #endif
}

function testPay() {
    // #ifdef MP-WEIXIN
    api.testpay({ openid: userStore.openid }).then((res: any) => {
        uni.requestPayment({
            provider: 'wxpay',
            timeStamp: `${res.timeStamp}`,
            nonceStr: res.nonceStr,
            package: res.package,
            signType: res.signType,
            paySign: res.paySign,
            success: (res) => {
                Tips.success('支付成功')
            },
            fail: (err) => {
                Tips.info('用户取消支付')
            },
        })
    })
    // #endif

    // #ifdef APP-PLUS
    Tips.info('App 端支付功能开发中')
    // #endif
}

function wait() {
    Tips.info('功能开发中')
}
function logout() {
    uni.showModal({
        content: '确定要退出登录么',
        success: (e) => {
            if (e.confirm) {
                api.tokenAuth
                    .Logout()
                    .then(() => {
                        userStore.clear()
                        toIndex()
                    })
                    .catch((e) => {
                        userStore.clear()
                        toIndex()
                    })
            }
        },
    })
}
function toIndex() {
    emit('refreshCurrentVal', 0)
}
</script>
<style lang="scss">
.myCard {
    @apply bg-white rounded-4;
}
</style>
<route lang="json">
{
    "layout": "main",
    "style": {
        "navigationBarTitleText": "个人中心",
        "navigationBarBackgroundColor": "#f6f6f6",
        "navigationBarTextStyle": "black"
    }
}
</route>
