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
                case PaymentStatus.Success:
                    handleSuccess(result)
                    break
                case PaymentStatus.Failed:
                    handleError(new Error(result.message))
                    break
                case PaymentStatus.Pending:
                    break
                default: {
                    const status = result.status
                    if (status === '已退款' || status === '部分退款' || status === '退款中') {
                        handleError(new Error(result.message || '支付退款'))
                    } else if (status === '失败' || status === '取消') {
                        handleError(new Error(result.message || '支付失败'))
                    }
                    break
                }
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
        console.log('[usePayment] initPayment 开始执行')
        console.log('[usePayment] config.amount:', config.amount)

        try {
            qrCodeUrl.value = ''
            orderNo.value = ''
            errorMessage.value = ''
            countdown.value = config.expireTime
            isPolling.value = false

            console.log('[usePayment] 调用 createPaymentOrder，金额:', config.amount)
            const response = await createPaymentOrder(config.amount)
            console.log('[usePayment] createPaymentOrder 返回:', response)
            console.log('[usePayment] response.code_url:', response.code_url)
            console.log('[usePayment] response.outTradeNo:', response.outTradeNo)

            qrCodeUrl.value = response.code_url
            orderNo.value = response.outTradeNo

            console.log('[usePayment] orderNo.value 已设置:', orderNo.value)
            console.log('[usePayment] qrCodeUrl.value 已设置:', qrCodeUrl.value)

            startPolling()
            startCountdown()

            console.log('[usePayment] initPayment 完成，轮询和倒计时已启动')
        } catch (error) {
            console.error('[usePayment] initPayment 异常:', error)
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
