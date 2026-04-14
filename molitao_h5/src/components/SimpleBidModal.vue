<!-- SimpleBidModal.vue - 极简的出价弹窗，直接替换 uni.showModal -->
<template>
    <view v-if="show" class="simple-bid-modal">
        <view class="modal-mask" @click="handleMaskClick"></view>
        <view class="modal-content">
            <!-- 标题 -->
            <view class="modal-title">
                {{ isKasec ? '卡秒出价 - 需三倍加价' : title }}
            </view>

            <!-- 卡秒模式警告 -->
            <view v-if="isKasec" class="kasec-warning">
                您已卡秒出价，需加够三倍加价才有效 <br />（最低出价：{{ minPrice }}）
            </view>

            <!-- 提示文本 - 只在非卡秒模式显示 -->
            <view v-if="!isKasec && placeholder" class="placeholder-text">
                {{ placeholder }}
            </view>

            <!-- 输入区域 -->
            <view class="input-section">
                <input v-model="inputValue" class="modal-input" type="digit" @confirm="handleConfirm" />
            </view>

            <!-- 按钮区域 -->
            <view class="modal-buttons">
                <button class="modal-btn cancel" @click="handleCancel">取消</button>
                <button class="modal-btn confirm" :class="{ 'kasec-confirm': isKasec }" @click="handleConfirm">
                    确定
                </button>
            </view>
        </view>
    </view>
</template>

<script>
export default {
    name: 'SimpleBidModal',
    props: {
        show: {
            type: Boolean,
            default: false,
        },
        title: {
            type: String,
            default: '提示',
        },
        placeholder: {
            type: String,
            default: '',
        },
        isKasec: {
            type: Boolean,
            default: false,
        },
        minPrice: {
            type: Number,
            default: 0,
        },
    },
    data() {
        return {
            inputValue: '',
            resolve: null,
            reject: null,
        }
    },
    watch: {
        show(val) {
            if (val) {
                this.inputValue = ''
                // 返回 Promise 以兼容现有的 async/await 模式
                return new Promise((resolve, reject) => {
                    this.resolve = resolve
                    this.reject = reject
                })
            }
        },
    },
    methods: {
        handleMaskClick() {
            // uni.showModal 点击遮罩不会关闭，保持一致
        },
        handleCancel() {
            const result = {
                errMsg: 'showModal:fail cancel',
                confirm: false,
                cancel: true,
                content: '',
            }
            this.$emit('cancel', result)
            this.$emit('update:show', false)
            if (this.reject) {
                this.reject(result)
            }
        },
        handleConfirm() {
            const result = {
                errMsg: 'showModal:ok',
                confirm: true,
                cancel: false,
                content: this.inputValue,
            }
            this.$emit('confirm', result)
            this.$emit('update:show', false)
            if (this.resolve) {
                this.resolve(result)
            }
        },
        // 公开方法供父组件调用，返回 Promise
        showModal() {
            this.$emit('update:show', true)
            return new Promise((resolve, reject) => {
                this.resolve = resolve
                this.reject = reject
            })
        },
    },
}
</script>

<style scoped>
.simple-bid-modal {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 999;
}

.modal-mask {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
}

.modal-content {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 540rpx;
    background: #fff;
    border-radius: 10rpx;
    padding: 40rpx;
}

.modal-title {
    text-align: center;
    font-size: 36rpx;
    font-weight: bold;
    margin-bottom: 40rpx;
}

/* 卡秒模式警告 - 匹配PC端样式 */
.kasec-warning {
    color: #ff0000;
    border: 1px solid #ff0000;
    padding: 8rpx;
    margin-bottom: 16rpx;
    border-radius: 4rpx;
    font-size: 28rpx;
    background: #fff5f5;
    line-height: 1.5;
}

/* 提示文本样式 */
.placeholder-text {
    color: #666;
    font-size: 28rpx;
    margin-bottom: 16rpx;
    text-align: center;
}

.input-section {
    margin-bottom: 40rpx;
}

.modal-input {
    width: 100%;
    height: 80rpx;
    border: 1px solid #ddd;
    border-radius: 8rpx;
    padding: 0 20rpx;
    font-size: 30rpx;
    box-sizing: border-box;
}

.modal-input:focus {
    border-color: #007aff;
}

.modal-buttons {
    display: flex;
    justify-content: space-between;
    gap: 30rpx;
}

.modal-btn {
    flex: 1;
    height: 80rpx;
    line-height: 80rpx;
    text-align: center;
    border-radius: 8rpx;
    font-size: 32rpx;
    border: none;
}

.modal-btn.cancel {
    background: #f5f5f5;
    color: #333;
}

.modal-btn.cancel:active {
    background: #e8e8e8;
}

.modal-btn.confirm {
    background: #007aff;
    color: #fff;
}

.modal-btn.confirm:active {
    background: #0056cc;
}

.modal-btn.kasec-confirm {
    background: linear-gradient(135deg, #ff7144, #ff9500);
    color: #fff;
    font-weight: bold;
}

.modal-btn.kasec-confirm:active {
    background: linear-gradient(135deg, #e5633c, #e68500);
}
</style>
