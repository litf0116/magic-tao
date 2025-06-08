<template>
    <view class="deposit-page">
        <!-- 顶部信息区 -->
        <view class="header-section">
            <view class="balance-info">
                <text class="label">当前保证金余额</text>
                <text class="amount">¥{{ userStore.user.depositBalance }}</text>
            </view>
            <view class="deposit-desc">
                <text class="title">保证金说明</text>
                <text class="content">新用户参与拍卖，需要缴纳51元（50元保证金+1元提现手续费）</text>
            </view>
        </view>

        <!-- 缴费金额区 -->
        <view class="amount-section">
            <view class="section-title">选择缴费金额</view>
            <view class="amount-options">
                <view
                    class="amount-item"
                    :class="{ active: selectedAmount === 51 }"
                    @tap="selectAmount(51)"
                >
                    <text class="price">¥51</text>
                    <text class="desc">新用户标准</text>
                </view>
                <view
                    class="amount-item"
                    :class="{ active: selectedAmount === customAmount }"
                    @tap="showCustomInput"
                >
                    <text class="price">自定义</text>
                    <text class="desc">其他金额</text>
                </view>
            </view>
        </view>

        <!-- 支付方式区 -->
        <view class="payment-section">
            <view class="section-title">支付方式</view>
            <view class="payment-method">
                <view class="method-item">
                    <image src="/images/wechat-pay.png" mode="aspectFit" class="method-icon" />
                    <text class="method-name">微信支付</text>
                    <view class="method-selected">
                        <view class="i-icon-park-outline:check-one size-6 text-[#ff7144]"></view>
                    </view>
                </view>
            </view>
        </view>

        <!-- 底部操作区 -->
        <view class="footer-section">
            <view class="pay-button" @tap="handlePay">
                立即支付 ¥{{ selectedAmount }}
            </view>
            <view class="agreement">
                点击支付即表示同意
                <text class="link" @tap="showAgreement">《保证金协议》</text>
            </view>
        </view>

        <!-- 自定义金额输入弹窗 -->
        <uv-popup ref="customAmountPopup" mode="bottom">
            <view class="custom-amount-popup">
                <view class="popup-header">
                    <text class="title">输入金额</text>
                    <view class="close" @tap="closeCustomInput">
                        <view class="i-icon-park-outline:close size-6"></view>
                    </view>
                </view>
                <view class="input-section">
                    <text class="currency">¥</text>
                    <input
                        type="digit"
                        v-model="customAmountInput"
                        placeholder="请输入金额"
                        @input="handleCustomInput"
                    />
                </view>
                <view class="confirm-button" @tap="confirmCustomAmount">
                    确定
                </view>
            </view>
        </uv-popup>
    </view>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Tips } from '@/composables'

const userStore = useUserStore()
const customAmountPopup = ref(null as any)

// 选中的金额
const selectedAmount = ref(51)
// 自定义金额输入
const customAmountInput = ref('')
const customAmount = ref(0)

// 选择固定金额
const selectAmount = (amount: number) => {
    selectedAmount.value = amount
    customAmount.value = 0
}

// 显示自定义金额输入
const showCustomInput = () => {
    customAmountPopup.value.open()
}

// 关闭自定义金额输入
const closeCustomInput = () => {
    customAmountPopup.value.close()
}

// 处理自定义金额输入
const handleCustomInput = (e: any) => {
    const value = e.detail.value
    if (value) {
        customAmount.value = Number(value)
    }
}

// 确认自定义金额
const confirmCustomAmount = () => {
    if (!customAmount.value || customAmount.value < 51) {
        Tips.error('金额不能小于51元')
        return
    }
    selectedAmount.value = customAmount.value
    closeCustomInput()
}

// 处理支付
const handlePay = async () => {
    try {
        const res = await api.client.payDeposit({
            openid: userStore.openid,
            type: 'jsapi'
        })

        wx.requestPayment({
            provider: 'wxpay',
            timeStamp: `${res.timeStamp}`,
            nonceStr: res.nonceStr,
            package: res.package,
            signType: res.signType,
            paySign: res.paySign,
            success: () => {
                Tips.success('支付成功')
                // 刷新用户信息
                userStore.getUserInfo()
                // 返回上一页
                uni.navigateBack()
            },
            fail: (err) => {
                console.error('支付失败:', err)
                Tips.error('支付失败，请重试')
            }
        })
    } catch (error) {
        console.error('发起支付失败:', error)
        Tips.error('发起支付失败，请重试')
    }
}

// 显示保证金协议
const showAgreement = () => {
    uni.navigateTo({
        url: '/pages/agreement/deposit'
    })
}
</script>

<style lang="scss" scoped>
.deposit-page {
    min-height: 100vh;
    background-color: #f5f5f5;
    padding: 20rpx;
}

.header-section {
    background-color: #fff;
    border-radius: 16rpx;
    padding: 30rpx;
    margin-bottom: 20rpx;

    .balance-info {
        text-align: center;
        margin-bottom: 20rpx;

        .label {
            font-size: 28rpx;
            color: #666;
        }

        .amount {
            display: block;
            font-size: 48rpx;
            font-weight: bold;
            color: #333;
            margin-top: 10rpx;
        }
    }

    .deposit-desc {
        .title {
            font-size: 28rpx;
            color: #333;
            font-weight: bold;
        }

        .content {
            display: block;
            font-size: 26rpx;
            color: #666;
            margin-top: 10rpx;
        }
    }
}

.amount-section {
    background-color: #fff;
    border-radius: 16rpx;
    padding: 30rpx;
    margin-bottom: 20rpx;

    .section-title {
        font-size: 28rpx;
        color: #333;
        font-weight: bold;
        margin-bottom: 20rpx;
    }

    .amount-options {
        display: flex;
        gap: 20rpx;

        .amount-item {
            flex: 1;
            background-color: #f8f8f8;
            border-radius: 12rpx;
            padding: 20rpx;
            text-align: center;
            border: 2rpx solid transparent;

            &.active {
                background-color: #fff5f2;
                border-color: #ff7144;
            }

            .price {
                display: block;
                font-size: 32rpx;
                font-weight: bold;
                color: #333;
            }

            .desc {
                display: block;
                font-size: 24rpx;
                color: #999;
                margin-top: 6rpx;
            }
        }
    }
}

.payment-section {
    background-color: #fff;
    border-radius: 16rpx;
    padding: 30rpx;
    margin-bottom: 20rpx;

    .section-title {
        font-size: 28rpx;
        color: #333;
        font-weight: bold;
        margin-bottom: 20rpx;
    }

    .method-item {
        display: flex;
        align-items: center;
        padding: 20rpx 0;

        .method-icon {
            width: 48rpx;
            height: 48rpx;
            margin-right: 20rpx;
        }

        .method-name {
            flex: 1;
            font-size: 28rpx;
            color: #333;
        }
    }
}

.footer-section {
    position: fixed;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: #fff;
    padding: 20rpx 30rpx;
    padding-bottom: calc(20rpx + env(safe-area-inset-bottom));

    .pay-button {
        background-color: #ff7144;
        color: #fff;
        text-align: center;
        padding: 24rpx 0;
        border-radius: 12rpx;
        font-size: 32rpx;
        font-weight: bold;
    }

    .agreement {
        text-align: center;
        font-size: 24rpx;
        color: #999;
        margin-top: 16rpx;

        .link {
            color: #ff7144;
        }
    }
}

.custom-amount-popup {
    background-color: #fff;
    border-radius: 24rpx 24rpx 0 0;
    padding: 30rpx;

    .popup-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 30rpx;

        .title {
            font-size: 32rpx;
            font-weight: bold;
            color: #333;
        }
    }

    .input-section {
        display: flex;
        align-items: center;
        border-bottom: 2rpx solid #eee;
        padding: 20rpx 0;
        margin-bottom: 30rpx;

        .currency {
            font-size: 40rpx;
            color: #333;
            margin-right: 10rpx;
        }

        input {
            flex: 1;
            font-size: 40rpx;
            color: #333;
        }
    }

    .confirm-button {
        background-color: #ff7144;
        color: #fff;
        text-align: center;
        padding: 24rpx 0;
        border-radius: 12rpx;
        font-size: 32rpx;
        font-weight: bold;
    }
}
</style>

<route lang="json">
{
    "style": {
        "navigationBarTitleText": "保证金缴费"
    }
}
</route>
