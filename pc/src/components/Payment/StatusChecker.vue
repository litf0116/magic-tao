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
import { getPaymentStatus } from '@/api/payment'

interface Props {
    orderId: string
    maxWaitTime?: number
}

interface Emits {
    (e: 'success'): void
    (e: 'timeout'): void
    (e: 'error', error: Error): void
}

const props = withDefaults(defineProps<Props>(), {
    maxWaitTime: 300000,
})

const emit = defineEmits<Emits>()

const status = ref<'checking' | 'success' | 'timeout'>('checking')
const elapsedTime = ref(0)

const POLL_INTERVAL = 3000

const formattedTime = computed(() => {
    const seconds = elapsedTime.value
    const minutes = Math.floor(seconds / 60)
    const remainingSeconds = seconds % 60
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`
})

let pollTimer: ReturnType<typeof setInterval> | null = null

const pollPaymentStatus = async () => {
    try {
        const response = await getPaymentStatus({ outTradeNo: props.orderId })

        if (response.status === '已支付' || response.status === '1') {
            handleSuccess()
            return
        }

        elapsedTime.value += Math.floor(POLL_INTERVAL / 1000)

        if (elapsedTime.value * 1000 >= props.maxWaitTime) {
            handleTimeout()
        }
    } catch (error) {
        console.error('轮询支付状态失败:', error)
        emit('error', error instanceof Error ? error : new Error('轮询失败'))
    }
}

const handleSuccess = () => {
    status.value = 'success'
    stopPolling()
    emit('success')
}

const handleTimeout = () => {
    status.value = 'timeout'
    stopPolling()
    emit('timeout')
}

const stopPolling = () => {
    if (pollTimer) {
        clearInterval(pollTimer)
        pollTimer = null
    }
}

const startPolling = () => {
    pollPaymentStatus()

    pollTimer = setInterval(() => {
        pollPaymentStatus()
    }, POLL_INTERVAL)
}

onMounted(() => {
    if (props.orderId) {
        startPolling()
    } else {
        emit('error', new Error('订单 ID 不能为空'))
    }
})

onUnmounted(() => {
    stopPolling()
})

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
