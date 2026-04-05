<template>
    <div class="payment-wrapper">
        <div class="payment-card">
            <!-- 返回按钮 -->
            <div class="back-button">
                <el-button text @click="goBack">
                    <i class="i-carbon:arrow-left mr-2"></i>
                    返回拍卖行
                </el-button>
            </div>

            <!-- 标题 -->
            <div class="header">
                <h1 class="title">保证金支付</h1>
            </div>

            <!-- 金额 -->
            <div class="amount-section">
                <div class="label">支付金额</div>
                <div class="amount">¥ 51.00</div>
                <div class="hint">含手续费 1 元，实际到账 50 元魔力值</div>
            </div>

            <!-- 二维码区域 -->
            <div v-if="paymentState === 'paying'" class="qrcode-section">
                <QrcodeDisplay :code-url="codeUrl" :size="256" />
                <div class="hint">使用微信扫描二维码支付</div>
            </div>

            <!-- 加载状态 -->
            <div v-else-if="paymentState === 'loading'" class="status-section">
                <p>🔄 生成支付二维码中...</p>
            </div>

            <!-- 支付成功 -->
            <div v-else-if="paymentState === 'success'" class="status-section success">
                <p>✅ 支付成功！保证金已到账</p>
            </div>

            <!-- 支付超时 -->
            <div v-else-if="paymentState === 'timeout'" class="status-section timeout">
                <p>⏰ 支付超时，请重新支付</p>
                <el-button type="primary" class="retry-btn" @click="retryPayment">重新支付</el-button>
            </div>

            <!-- 支付错误 -->
            <div v-else-if="paymentState === 'error'" class="status-section error">
                <p>❌ {{ errorMessage || '支付遇到问题，请重试' }}</p>
                <el-button type="primary" class="retry-btn" @click="retryPayment">重新支付</el-button>
            </div>

            <!-- 提示 -->
            <div class="tips-section">
                <h3>温馨提示</h3>
                <ul>
                    <li>保证金用于参与竞拍，可申请退还</li>
                    <li>支付成功后立即到账，无需等待</li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/userStore'
import { payApi } from '@/api/pay'
import QrcodeDisplay from '@/components/Payment/QrcodeDisplay.vue'

// 状态管理
const paymentState = ref<'loading' | 'paying' | 'success' | 'timeout' | 'error'>('loading')
const codeUrl = ref<string>('')
const outTradeNo = ref<string>('')
const errorMessage = ref<string>('')
const elapsedSeconds = ref<number>(0)

// 定时器引用
let pollingTimer: number | null = null

// 常量定义
const POLL_INTERVAL = 3000 // 3秒轮询一次
const MAX_WAIT_TIME = 300000 // 5分钟最大等待时间 (毫秒)
const DEPOSIT_AMOUNT = 51

// 路由和用户存储
const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

// 返回拍卖行
const goBack = () => {
    router.push('/chat/auction/auction')
}

// 计算实际超时时间（支持调试模式）
const getTimeoutTime = () => {
    const debugTimeout = route.query.debug_timeout
    if (debugTimeout) {
        const seconds = parseInt(debugTimeout as string)
        return isNaN(seconds) ? MAX_WAIT_TIME : seconds * 1000
    }
    return MAX_WAIT_TIME
}

// 停止轮询
const stopPolling = () => {
    if (pollingTimer !== null) {
        clearInterval(pollingTimer)
        pollingTimer = null
    }
}

// 处理支付成功
const handleSuccess = () => {
    stopPolling()
    paymentState.value = 'success'
    ElMessage.success('支付成功！保证金已到账')

    // 3秒后跳转到个人中心
    setTimeout(() => {
        router.push('/my')
    }, 3000)
}

// 处理支付超时
const handleTimeout = () => {
    stopPolling()
    paymentState.value = 'timeout'
    ElMessage.warning('支付超时，请重新支付')
}

// 处理支付错误
const handleError = (msg: string) => {
    stopPolling()
    paymentState.value = 'error'
    errorMessage.value = msg
    ElMessage.error(msg)
}

// 轮询支付状态
const pollPaymentStatus = async () => {
    try {
        // 调用API查询订单状态
        const res = await payApi.getOrderStatus(outTradeNo.value)

        // 检查支付是否成功
        if (res.status === '已支付') {
            // 如果支付成功，更新用户信息
            await userStore.getUserInfo()
            handleSuccess()
            return
        }

        // 检查是否超时
        elapsedSeconds.value += 3
        const timeoutTime = getTimeoutTime()

        if (elapsedSeconds.value * 1000 >= timeoutTime) {
            handleTimeout()
            return
        }
    } catch (error) {
        console.error('轮询支付状态失败:', error)
        // 发生错误时不中断轮询，继续尝试
    }
}

// 开始轮询
const startPolling = () => {
    // 清除可能存在的旧定时器
    stopPolling()

    // 重置计时器
    elapsedSeconds.value = 0

    // 开始轮询
    pollingTimer = setInterval(() => {
        pollPaymentStatus()
    }, POLL_INTERVAL) as unknown as number
}

// 重新支付
const retryPayment = async () => {
    try {
        // 重置状态
        paymentState.value = 'loading'
        errorMessage.value = ''

        // 重新初始化支付
        await initPayment()
    } catch (error) {
        console.error('重新支付失败:', error)
        handleError('重新支付失败，请稍后再试')
    }
}

// 初始化支付
const initPayment = async () => {
    try {
        // 调用支付API获取二维码
        const response = await payApi.payDepositNative(DEPOSIT_AMOUNT)

        // 更新状态
        codeUrl.value = response.code_url
        outTradeNo.value = response.outTradeNo
        paymentState.value = 'paying'

        // 开始轮询
        startPolling()
    } catch (error: any) {
        console.error('初始化支付失败:', error)
        const errorMsg = error.message || '初始化支付失败，请重试'
        handleError(errorMsg)
    }
}

// 组件挂载时初始化
onMounted(() => {
    initPayment()
})

// 组件卸载时清理定时器
onUnmounted(() => {
    stopPolling()
})
</script>

<style scoped>
.payment-wrapper {
    width: 100%;
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: flex-start;
    padding: 20px;
}

.payment-card {
    background: white;
    width: 100%;
    max-width: 600px;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    padding: 32px;
    position: relative;
}

.back-button {
    position: absolute;
    top: 16px;
    left: 16px;
    z-index: 10;
}

.header {
    margin-bottom: 24px;
    border-bottom: 1px solid #e8e8e8;
    padding-bottom: 16px;
}

.title {
    font-size: 20px;
    font-weight: bold;
    color: #333;
    text-align: center;
    margin: 0;
}

.amount-section {
    background: #f5f5f5;
    border-radius: 8px;
    padding: 24px;
    text-align: center;
    margin-bottom: 32px;
}

.amount-section .label {
    font-size: 14px;
    color: #666;
}

.amount-section .amount {
    font-size: 36px;
    font-weight: bold;
    color: #52c41a;
    margin: 16px 0;
}

.amount-section .hint {
    font-size: 12px;
    color: #999;
}

.qrcode-section {
    text-align: center;
    margin-bottom: 32px;
}

.qrcode-placeholder {
    width: 256px;
    height: 256px;
    border: 1px solid #e8e8e8;
    border-radius: 4px;
    padding: 16px;
    margin: 0 auto 16px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: white;
}

.qrcode-placeholder p {
    color: #999;
    font-size: 14px;
}

.qrcode-section .hint {
    font-size: 14px;
    color: #666;
}

.status-section {
    text-align: center;
    padding: 16px;
    background: #e6f7ff;
    border-radius: 4px;
    margin-bottom: 24px;
}

.status-section p {
    margin: 0;
    font-size: 14px;
    color: #1890ff;
}

.status-section.success {
    background: #f6ffed;
    border: 1px solid #b7eb8f;
    color: #52c41a;
}

.status-section.timeout {
    background: #fff2f0;
    border: 1px solid #ffccc7;
    color: #ff4d4f;
}

.status-section.error {
    background: #fff2f0;
    border: 1px solid #ffccc7;
    color: #ff4d4f;
}

.retry-btn {
    margin-top: 16px;
}

.tips-section {
    background: #fffbe6;
    border: 1px solid #ffe58f;
    border-radius: 4px;
    padding: 16px;
}

.tips-section h3 {
    font-size: 14px;
    font-weight: bold;
    color: #333;
    margin: 0 0 12px 0;
}

.tips-section ul {
    margin: 0;
    padding-left: 20px;
}

.tips-section li {
    font-size: 12px;
    color: #666;
    line-height: 2;
}

/* 响应式设计 */
@media (max-width: 768px) {
    .payment-card {
        padding: 24px;
    }

    .qrcode-placeholder {
        width: 220px;
        height: 220px;
    }

    .amount-section .amount {
        font-size: 28px;
    }
}
</style>
