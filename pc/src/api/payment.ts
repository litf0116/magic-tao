import request from '@/utils/request'
import type { PaymentQuery, PaymentResult } from '@/types/payment'

/**
 * 创建支付订单响应类型
 */
export interface CreatePaymentOrderResponse {
    /** 支付二维码 URL */
    code_url: string
    /** 商户订单号 */
    outTradeNo: string
    /** 支付金额（单位：元） */
    amount: number
}

/**
 * 创建支付订单
 * @description 调用后端 PayDepositNative 接口创建支付订单，返回二维码 URL 和订单号
 * @param amount 支付金额（单位：元），可选，默认 0.01
 * @returns 包含支付二维码 URL、订单号和金额的响应对象
 * @example
 * // 创建 100 元的支付订单
 * const result = await createPaymentOrder(100)
 * console.log(result.code_url) // 二维码 URL
 * console.log(result.outTradeNo) // 订单号
 */
export function createPaymentOrder(amount = 51): Promise<CreatePaymentOrderResponse> {
    const url = '/api/services/app/Client/PayDepositNative'
    return request({
        method: 'get',
        url,
        params: { amount },
    }).then((response) => response.data)
}

/**
 * 查询支付订单状态
 * @description 调用后端 GetPayOrderStatus 接口查询支付订单状态
 * @param query 查询参数，包含商户订单号 outTradeNo
 * @returns 支付结果，包含状态、订单号、金额等信息
 * @example
 * // 查询订单状态
 * const status = await getPaymentStatus({ outTradeNo: '20250110123456' })
 * if (status.status === PaymentStatus.Success) {
 *   console.log('支付成功')
 * }
 */
export function getPaymentStatus(query: PaymentQuery): Promise<PaymentResult> {
    const url = '/api/services/app/Client/GetPayOrderStatus'
    return request({
        method: 'get',
        url,
        params: { outTradeNo: query.outTradeNo },
    }).then((response) => response.data)
}
