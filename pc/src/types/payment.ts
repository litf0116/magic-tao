/**
 * 支付结果状态枚举
 * 对应后端 GetPayOrderStatus 接口返回的 status 字段
 */
export enum PaymentStatus {
    /** 支付中 - 用户已扫码但尚未完成支付 */
    Pending = 'pending',
    /** 支付成功 - 用户已完成支付 */
    Success = 'success',
    /** 支付失败 - 支付超时或其他原因导致失败 */
    Failed = 'failed',
}

/**
 * 支付选项配置
 * 用于初始化支付流程时的配置参数
 */
export interface PaymentOptions {
    /** 支付金额（单位：元） */
    amount: number
    /** 支付轮询间隔（单位：毫秒），默认 3000ms */
    pollingInterval?: number
    /** 支付订单过期时间（单位：秒），默认 300 秒 */
    expireTime?: number
}

/**
 * 支付查询参数
 * 用于查询支付订单状态
 */
export interface PaymentQuery {
    /** 商户订单号（后端返回的 outTradeNo） */
    outTradeNo: string
}

/**
 * 支付结果
 * 支付流程完成后的结果数据
 */
export interface PaymentResult {
    status: string
    /** 订单号 */
    orderId: string
    /** 商户订单号 */
    outTradeNo: string
    /** 支付金额（单位：元） */
    amount: number
    /** 支付成功时间（仅当 status 为 Success 时存在） */
    paidAt?: string
    /** 交易流水号（仅当 status 为 Success 时存在） */
    tradeNo?: string
    /** 结果消息 */
    message: string
}
