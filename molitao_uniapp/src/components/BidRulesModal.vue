<!-- BidRulesModal.vue -->
<template>
    <view v-if="show" class="modal">
        <view class="mask" @click="maskClick"></view>
        <view class="modal-content">
            <view class="modal-title">出价规则说明</view>
            <view class="modal-body">
                <!-- 当前价格信息 -->
                <view v-if="currentPrice || minBidPrice" class="price-info">
                    <view v-if="currentPrice" class="current-price">
                        <view class="price-label">当前价格：</view>
                        <view class="price-value">￥{{ currentPrice }}</view>
                    </view>
                    <view v-if="minBidPrice" class="min-price">
                        <view class="price-label">最低出价：</view>
                        <view class="price-value highlight">￥{{ minBidPrice }}</view>
                    </view>
                </view>

                <view class="rules-title">出价规则说明：</view>
                <view class="rules-list">
                    <view v-for="(rule, index) in priceRules" :key="index" class="rule-item">
                        <view class="rule-dot">•</view>
                        <view class="rule-text">{{ rule }}</view>
                    </view>
                </view>
                <view v-if="showKasecWarning" class="kasec-warning">
                    <view class="warning-icon">⚠️</view>
                    <view class="warning-text">卡秒期间需三倍加价</view>
                </view>
            </view>
            <view class="modal-footer">
                <button class="btn-confirm" @click="confirm">我知道了</button>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'BidRulesModal',
    props: {
        show: {
            type: Boolean,
            default: false,
        },
        message: {
            type: String,
            default: '',
        },
        currentPrice: {
            type: Number,
            default: 0,
        },
        minBidPrice: {
            type: Number,
            default: 0,
        },
        maskClosable: {
            type: Boolean,
            default: true,
        },
    },
    computed: {
        priceRules() {
            return [
                '100以内，5R一加',
                '100~1000，5R一加',
                '1000~2000，10R一加',
                '2000~5000，20R一加',
                '5000~1W，50一加',
                '1W以上，100一加',
            ]
        },
        showKasecWarning() {
            return this.message && this.message.includes('卡秒期间需三倍加价')
        },
    },
    methods: {
        maskClick() {
            if (this.maskClosable) {
                this.$emit('update:show', false)
                this.$emit('close')
            }
        },
        confirm() {
            this.$emit('update:show', false)
            this.$emit('confirm')
        },
    },
}
</script>

<style scoped>
.modal {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    z-index: 9999;
}

.mask {
    position: fixed;
    top: 0;
    right: 0;
    bottom: 0;
    left: 0;
    background: rgba(0, 0, 0, 0.6);
}

.modal-content {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 85%;
    max-width: 500rpx;
    background: #fff;
    border-radius: 16rpx;
    box-shadow: 0 8rpx 32rpx rgba(0, 0, 0, 0.1);
}

.modal-title {
    padding: 32rpx 32rpx 16rpx;
    text-align: center;
    font-size: 36rpx;
    font-weight: bold;
    color: #333;
    border-bottom: 1rpx solid #f0f0f0;
}

.modal-body {
    padding: 32rpx;
}

.price-info {
    background: linear-gradient(135deg, #f8f9fa, #e9ecef);
    border-radius: 12rpx;
    padding: 24rpx;
    margin-bottom: 32rpx;
    border: 2rpx solid #dee2e6;
}

.current-price,
.min-price {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 12rpx;
}

.min-price {
    margin-bottom: 0;
}

.price-label {
    font-size: 30rpx;
    color: #666;
    font-weight: 500;
}

.price-value {
    font-size: 32rpx;
    font-weight: bold;
    color: #333;
}

.price-value.highlight {
    color: #ff7144;
    font-size: 36rpx;
}

.rules-title {
    font-size: 32rpx;
    font-weight: 600;
    color: #333;
    margin-bottom: 24rpx;
}

.rules-list {
    margin-bottom: 24rpx;
}

.rule-item {
    display: flex;
    align-items: flex-start;
    margin-bottom: 16rpx;
    padding: 12rpx 16rpx;
    background: #f8f9fa;
    border-radius: 8rpx;
    border-left: 4rpx solid #ff7144;
}

.rule-dot {
    color: #ff7144;
    font-size: 32rpx;
    font-weight: bold;
    margin-right: 12rpx;
    line-height: 1;
}

.rule-text {
    font-size: 28rpx;
    color: #555;
    line-height: 1.4;
    flex: 1;
}

.kasec-warning {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16rpx;
    background: #fff3cd;
    border: 2rpx solid #ffeaa7;
    border-radius: 8rpx;
    margin-top: 16rpx;
}

.warning-icon {
    font-size: 32rpx;
    margin-right: 8rpx;
}

.warning-text {
    font-size: 28rpx;
    color: #856404;
    font-weight: 600;
}

.modal-footer {
    padding: 24rpx 32rpx 32rpx;
    border-top: 1rpx solid #f0f0f0;
}

.btn-confirm {
    width: 100%;
    height: 88rpx;
    line-height: 88rpx;
    text-align: center;
    font-size: 32rpx;
    color: #fff;
    background: linear-gradient(135deg, #ff7144, #ff9500);
    border-radius: 44rpx;
    border: none;
    box-shadow: 0 4rpx 16rpx rgba(255, 113, 68, 0.3);
}

.btn-confirm:active {
    opacity: 0.8;
    transform: translateY(2rpx);
}
</style>
