<template>
    <el-dialog
        v-model="visible"
        title="充值保证金"
        width="450px"
        :close-on-click-modal="false"
        :before-close="handleClose"
        class="deposit-recharge-dialog"
    >
        <!-- 支付金额 -->
        <div class="amount-section">
            <div class="label">支付金额</div>
            <div class="amount">¥51.00</div>
            <div class="hint">含手续费 ¥1，保证金到账 ¥50</div>
        </div>

        <!-- 二维码区域 -->
        <div v-if="state === 'loading'" class="status-section">
            <el-icon class="is-loading"><i-ep-loading /></el-icon>
            <span>正在生成支付二维码...</span>
        </div>

        <div v-else-if="state === 'paying'" class="qrcode-section">
            <div class="qrcode-wrapper">
                <QrcodeDisplay :code-url="qrCodeUrl" :size="220" />
                <div v-if="countdown > 0" class="countdown">
                    ⏱️ 有效期剩余 {{ formatCountdown(countdown) }}
                </div>
                <div v-else class="countdown expired">
                    二维码已过期
                </div>
            </div>
            <div class="qrcode-hint">
                <p>请使用微信扫一扫完成支付</p>
                <p class="order-info">订单号：{{ orderNo }}</p>
            </div>
        </div>

        <div v-else-if="state === 'success'" class="status-section success">
            <el-icon><i-ep-circle-check /></el-icon>
            <span>支付成功！保证金已到账</span>
        </div>

        <div v-else-if="state === 'timeout'" class="status-section timeout">
            <el-icon><i-ep-clock /></el-icon>
            <span>二维码已过期</span>
            <el-button type="primary" size="small" @click="refreshQrCode">
                刷新二维码
            </el-button>
        </div>

        <div v-else-if="state === 'error'" class="status-section error">
            <el-icon><i-ep-circle-close /></el-icon>
            <span>{{ errorMsg || '生成二维码失败' }}</span>
            <el-button type="primary" size="small" @click="refreshQrCode">
                重试
            </el-button>
        </div>

        <!-- 操作按钮 -->
        <template #footer>
            <div class="dialog-footer">
                <el-button v-if="state === 'paying'" @click="handleClose">
                    取消支付
                </el-button>
                <el-button v-else @click="handleClose">
                    关闭
                </el-button>
                <el-button
                    v-if="state === 'paying'"
                    type="primary"
                    :loading="checking"
                    @click="manualCheck"
                >
                    我已支付
                </el-button>
            </div>
        </template>
    </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/userStore'
import { payApi } from '@/api/pay'
import QrcodeDisplay from '@/components/Payment/QrcodeDisplay.vue'
import api from '@/api'

type PaymentState = 'loading' | 'paying' | 'success' | 'timeout' | 'error'

const props = defineProps<{
    modelValue: boolean
}>()

const emit = defineEmits<{
    'update:modelValue': [value: boolean]
    'success': []
}>()

const userStore = useUserStore()

// 状态
const state = ref<PaymentState>('loading')
const qrCodeUrl = ref('')
const orderNo = ref('')
const initialBalance = ref(0)
const errorMsg = ref('')
const checking = ref(false)

// 倒计时
const countdown = ref(300) // 5分钟 = 300秒
const EXPIRE_TIME = 300 // 二维码有效期5分钟

// 定时器
let pollTimer: number | null = null
let countdownTimer: number | null = null

// 可见性
const visible = computed({
    get: () => props.modelValue,
    set: (val) => emit('update:modelValue', val)
})

// 监听弹窗显示
watch(visible, (val) => {
    if (val) {
        initPayment()
    } else {
        clearAllTimers()
    }
})

// 格式化倒计时
const formatCountdown = (seconds: number) => {
    const m = Math.floor(seconds / 60)
    const s = seconds % 60
    return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

// 初始化支付
const initPayment = async () => {
    try {
        state.value = 'loading'
        errorMsg.value = ''
        countdown.value = EXPIRE_TIME

        // 获取初始余额
        const userInfo = await userStore.getUserInfo()
        initialBalance.value = userInfo.user?.depositBalance || 0

        // 获取支付二维码
        const response = await payApi.payDepositNative(51)
        qrCodeUrl.value = response.code_url
        orderNo.value = response.outTradeNo || Date.now().toString()

        state.value = 'paying'

        // 启动轮询和倒计时
        startPolling()
        startCountdown()
    } catch (error: any) {
        console.error('初始化支付失败:', error)
        errorMsg.value = error.message || '生成二维码失败'
        state.value = 'error'
    }
}

// 刷新二维码
const refreshQrCode = () => {
    clearAllTimers()
    initPayment()
}

// 启动倒计时
const startCountdown = () => {
    countdown.value = EXPIRE_TIME
    countdownTimer = window.setInterval(() => {
        countdown.value--
        if (countdown.value <= 0) {
            state.value = 'timeout'
            clearAllTimers()
        }
    }, 1000)
}

// 启动轮询 - 双重检查机制
const startPolling = () => {
    // 清除旧定时器
    clearPollTimer()

    // 每3秒轮询一次
    pollTimer = window.setInterval(async () => {
        await checkPaymentStatus()
    }, 3000)
}

// 检查支付状态
const checkPaymentStatus = async () => {
    try {
        // 方式1: 查询订单状态（更可靠）
        if (orderNo.value) {
            const orderStatus = await payApi.getOrderStatus(orderNo.value)
            if (orderStatus.status === '已支付') {
                handleSuccess()
                return
            }
            if (orderStatus.status === '取消' || orderStatus.status === 'NOT_FOUND') {
                state.value = 'timeout'
                clearAllTimers()
                return
            }
        }

        // 方式2: 余额变化检测（兜底）
        const userInfo = await userStore.getUserInfo()
        const currentBalance = userInfo.user?.depositBalance || 0

        if (currentBalance >= initialBalance.value + 50) {
            handleSuccess()
            return
        }
    } catch (error) {
        console.error('检查支付状态失败:', error)
    }
}

// 手动检查（用户点击"我已支付"）
const manualCheck = async () => {
    checking.value = true
    await checkPaymentStatus()
    checking.value = false

    if (state.value !== 'success') {
        ElMessage.info('尚未检测到支付成功，请确认已完成支付')
    }
}

// 支付成功处理
const handleSuccess = () => {
    clearAllTimers()
    state.value = 'success'
    ElMessage.success('支付成功！保证金已到账')

    // 2秒后自动关闭
    setTimeout(() => {
        handleClose()
        emit('success')
    }, 2000)
}

// 清除轮询定时器
const clearPollTimer = () => {
    if (pollTimer) {
        clearInterval(pollTimer)
        pollTimer = null
    }
}

// 清除倒计时定时器
const clearCountdownTimer = () => {
    if (countdownTimer) {
        clearInterval(countdownTimer)
        countdownTimer = null
    }
}

// 清除所有定时器
const clearAllTimers = () => {
    clearPollTimer()
    clearCountdownTimer()
}

// 关闭弹窗
const handleClose = () => {
    clearAllTimers()
    visible.value = false
    // 重置状态
    setTimeout(() => {
        state.value = 'loading'
        qrCodeUrl.value = ''
        orderNo.value = ''
    }, 300)
}

// 组件卸载时清理
onUnmounted(() => {
    clearAllTimers()
})
</script>

<style scoped>
.deposit-recharge-dialog :deep(.el-dialog__body) {
    padding: 20px 30px;
}

.amount-section {
    background: #f5f5f5;
    border-radius: 8px;
    padding: 20px;
    text-align: center;
    margin-bottom: 24px;
}

.amount-section .label {
    font-size: 14px;
    color: #666;
    margin-bottom: 8px;
}

.amount-section .amount {
    font-size: 32px;
    font-weight: bold;
    color: #52c41a;
    margin: 8px 0;
}

.amount-section .hint {
    font-size: 12px;
    color: #999;
}

.qrcode-section {
    text-align: center;
}

.qrcode-wrapper {
    display: inline-block;
    border: 1px solid #e8e8e8;
    border-radius: 8px;
    padding: 16px;
    background: white;
    margin-bottom: 16px;
}

.countdown {
    margin-top: 12px;
    font-size: 14px;
    color: #666;
}

.countdown.expired {
    color: #ff4d4f;
    font-weight: bold;
}

.qrcode-hint {
    text-align: center;
}

.qrcode-hint p {
    margin: 4px 0;
    font-size: 14px;
    color: #666;
}

.qrcode-hint .order-info {
    font-size: 12px;
    color: #999;
    font-family: monospace;
}

.status-section {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 40px 20px;
    gap: 16px;
}

.status-section .el-icon {
    font-size: 48px;
}

.status-section span {
    font-size: 16px;
    color: #666;
}

.status-section.success {
    color: #52c41a;
}

.status-section.success span {
    color: #52c41a;
    font-weight: bold;
}

.status-section.timeout {
    color: #faad14;
}

.status-section.timeout span {
    color: #faad14;
}

.status-section.error {
    color: #ff4d4f;
}

.status-section.error span {
    color: #ff4d4f;
}

.status-section :deep(.el-icon.is-loading) {
    animation: rotating 2s linear infinite;
}

@keyframes rotating {
    from {
        transform: rotate(0deg);
    }
    to {
        transform: rotate(360deg);
    }
}

:deep(.el-icon) {
    font-size: 48px;
}

.dialog-footer {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}
</style>
