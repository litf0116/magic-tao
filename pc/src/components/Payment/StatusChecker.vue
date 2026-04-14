<template>
    <div class="status-checker">
        <div class="status-text">
            <span v-if="status === 'checking'">🔄 支付中... ({{ formattedTime }})</span>
            <span v-else-if="status === 'success'">✅ 支付成功</span>
            <span v-else-if="status === 'timeout'">⏱️ 支付超时</span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

// Props 类型定义
interface Props {
    orderId: string // 订单 ID
    maxWaitTime?: number // 最大等待时间（毫秒），默认 300000 (5 分钟)
}

// Emits 类型定义
interface Emits {
    (e: 'success'): void // 支付成功回调
    (e: 'timeout'): void // 支付超时回调
    (e: 'error', error: Error): void // 错误回调
}

const props = withDefaults(defineProps<Props>(), {
    maxWaitTime: 300000, // 默认 5 分钟
})

const emit = defineEmits<Emits>()

// 轮询状态
const status = ref<'checking' | 'success' | 'timeout'>('checking')
const elapsedTime = ref(0) // 已等待时间（秒）

// 轮询间隔：3 秒
const POLL_INTERVAL = 3000

// 格式化已等待时间
const formattedTime = computed(() => {
    const seconds = elapsedTime.value
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`
})

// 定时器引用
let pollTimer: ReturnType<typeof setInterval> | null = null

// 轮询支付状态
const pollPaymentStatus = async () => {
    try {
        // TODO: Task 8 集成时添加实际 API 调用
        // const response = await checkPaymentStatus(props.orderId)
        // if (response.paid) {
        //   handleSuccess()
        // }

        // 临时测试：模拟支付成功（实际使用时删除）
        // if (elapsedTime.value > 10) {
        //   handleSuccess()
        //   return
        // }

        elapsedTime.value += Math.floor(POLL_INTERVAL / 1000)

        // 检查是否超时
        if (elapsedTime.value * 1000 >= props.maxWaitTime) {
            handleTimeout()
        }
    } catch (error) {
        console.error('轮询支付状态失败:', error)
        emit('error', error instanceof Error ? error : new Error('轮询失败'))
    }
}

// 支付成功处理（预留，Task 8 集成时使用）
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const handleSuccess = () => {
    status.value = 'success'
    stopPolling()
    emit('success')
}

// 支付超时处理
const handleTimeout = () => {
    status.value = 'timeout'
    stopPolling()
    emit('timeout')
}

// 停止轮询
const stopPolling = () => {
    if (pollTimer) {
        clearInterval(pollTimer)
        pollTimer = null
    }
}

// 开始轮询
const startPolling = () => {
    // 立即执行一次
    pollPaymentStatus()

    // 设置定时器
    pollTimer = setInterval(() => {
        pollPaymentStatus()
    }, POLL_INTERVAL)
}

// 组件挂载时开始轮询
onMounted(() => {
    if (props.orderId) {
        startPolling()
    } else {
        emit('error', new Error('订单 ID 不能为空'))
    }
})

// 组件卸载时清除定时器
onUnmounted(() => {
    stopPolling()
})

// 暴露方法供外部调用
defineExpose({
    stopPolling,
    startPolling,
})
</script>

<style scoped>
.status-checker {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 24px;
}

.status-text {
    font-size: 16px;
    color: #333;
    text-align: center;
}

.status-text span {
    display: inline-flex;
    align-items: center;
    gap: 8px;
}
</style>
