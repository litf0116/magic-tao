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

            </view>
            <view
                v-if="userStore.user.id"
                class="absolute right-4 top-4 size-6 zoom-in i-solar:settings-linear"
                @click.stop="navTo.navTo('/pages/user/info')"
            ></view>
        </view>

        <view class="my-4 flex items-center">
            <view class="h-3 w-4px mr-2 bg-[#ccc] rounded-full"></view>
            <view>工作台</view>
        </view>

        <view class="myCard py-4 px-4 mb-4 text-[#171717] flex flex-center">
            <view class="text-center">联系老淡开通权限</view>
        </view>



        <view v-if="userStore.user.phoneNumber" class="my-4">
            <uv-button @tap="logout">退出登录</uv-button>
        </view>
        <view class="text-center w-full text-gray-300">{{ version.version }}</view>

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
import version from '@/static/version.json'
import api from '@/utils/api'
import CustomModal from '@/components/customModal.vue'

const userStore = useUserStore()
const navTo = useTo()
const myCount = ref({ auctionSuccess: 0, friend: 0, balance: 0, depositBalance: 0 })

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
    api.client.payDeposit({ openid: userStore.openid, amount: 51 }).then((res: any) => {
        wx.requestPayment({
            provider: 'wxpay',
            timeStamp: `${res.timeStamp}`,
            nonceStr: res.nonceStr,
            package: res.package,
            signType: res.signType,
            paySign: res.paySign,
            success: async (res) => {
                // 更新用户信息和统计数据
                try {
                    await userStore.checkLogin(false, true)
                    getMyCount()
                } catch (error) {
                    getMyCount() // 即使更新失败也要更新统计数据
                }

                Tips.success('支付成功，魔力值已到账')
            },
            fail: (err) => {
                Tips.info('用户取消支付')
            },
        })
    })
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

    api.client.TopUp({ openid: userStore.openid, amount: _value }).then((res: any) => {
        wx.requestPayment({
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
                // Payment failure handling
            },
        })
    })
}

function testPay() {
    api.testpay({ openid: userStore.openid }).then((res) => {
        wx.requestPayment({
            provider: 'wxpay',
            timeStamp: `${res.timeStamp}`,
            nonceStr: res.nonceStr,
            package: res.package,
            signType: res.signType,
            paySign: res.paySign,
            success: (res) => {
                // Test payment success
            },
            fail: (err) => {
                // Test payment failure
            },
        })
    })
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
