import { ref, onUnmounted } from 'vue'
import { createPaymentOrder, getPaymentStatus } from '@/api/payment'
import { PaymentOptions, PaymentResult, PaymentStatus } from '@/types/payment'
import { PAYMENT_CONSTANTS } from '@/config/paymentConfig'

/**
 * 支付组合式函数
 * 提供支付流程的核心逻辑，包括创建订单、轮询状态、倒计时等功能
 */
export function usePayment(options?: Partial<PaymentOptions>) {
    const config = {
        amount: PAYMENT_CONSTANTS.AMOUNT,
        pollingInterval: PAYMENT_CONSTANTS.POLL_INTERVAL,
        expireTime: PAYMENT_CONSTANTS.EXPIRE_TIME,
        ...options,
    }

    const qrCodeUrl = ref<string>('')
    const orderNo = ref<string>('')
    const countdown = ref<number>(config.expireTime)
    const errorMessage = ref<string>('')
    const isPolling = ref<boolean>(false)

    let pollTimer: number | null = null
    let countdownTimer: number | null = null

    const formatCountdown = (seconds: number): string => {
        const m = Math.floor(seconds / 60)
        const s = seconds % 60
        return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
    }

    const handleSuccess = (result: PaymentResult) => {
        cleanup()
        console.log('Payment successful:', result)
    }

    const handleError = (error: Error | unknown) => {
        cleanup()
        const message = error instanceof Error ? error.message : '支付失败'
        errorMessage.value = message
        console.error('Payment failed:', error)
    }

    const checkPaymentStatus = async (): Promise<void> => {
        if (!orderNo.value) {
            console.warn('No order number to check payment status')
            return
        }

        try {
            const query = { outTradeNo: orderNo.value }
            const result: PaymentResult = await getPaymentStatus(query)

            switch (result.status) {
                case '已支付':
                    handleSuccess(result)
                    break
                case '已退款':
                case '取消':
                    handleError(new Error(result.message))
                    break
                case '未支付':
                case '退款中':
                case '部分退款':
                    break
                default:
                    console.warn('Unknown payment status:', result.status)
                    break
            }
        } catch (error) {
            console.error('Failed to check payment status:', error)
        }
    }

    const startPolling = () => {
        if (pollTimer) {
            clearInterval(pollTimer)
        }

        isPolling.value = true
        pollTimer = window.setInterval(async () => {
            await checkPaymentStatus()
        }, config.pollingInterval) as unknown as number
    }

    const startCountdown = () => {
        if (countdownTimer) {
            clearInterval(countdownTimer)
        }

        countdown.value = config.expireTime
        countdownTimer = window.setInterval(() => {
            countdown.value--
            if (countdown.value <= 0) {
                cleanup()
                errorMessage.value = '支付二维码已过期'
            }
        }, 1000) as unknown as number
    }

    const initPayment = async () => {
        try {
            qrCodeUrl.value = ''
            orderNo.value = ''
            errorMessage.value = ''
            countdown.value = config.expireTime
            isPolling.value = false

            const response = await createPaymentOrder(config.amount)

            qrCodeUrl.value = response.code_url
            orderNo.value = response.outTradeNo

            startPolling()
            startCountdown()
        } catch (error) {
            handleError(error)
        }
    }

    const retry = async () => {
        await initPayment()
    }

    const cleanup = () => {
        if (pollTimer) {
            clearInterval(pollTimer)
            pollTimer = null
        }

        if (countdownTimer) {
            clearInterval(countdownTimer)
            countdownTimer = null
        }

        isPolling.value = false
    }

    onUnmounted(() => {
        cleanup()
    })

    return {
        qrCodeUrl,
        orderNo,
        countdown,
        errorMessage,
        isPolling,
        initPayment,
        retry,
        cleanup,
        formatCountdown,
    }
}
