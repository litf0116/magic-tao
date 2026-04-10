/**
 * 支付场景配置
 */
export const PAYMENT_SCENARIOS = {
    /** 保证金充值 */
    DEPOSIT: 'deposit',
    /** 会员认证 */
    MEMBER_VERIFICATION: 'member_verification',
} as const

/**
 * 支付金额配置（单位：元）
 */
export const PAYMENT_AMOUNTS = {
    /** 保证金充值金额：51 元（50 保证金 + 1 平台费） */
    DEPOSIT_TOTAL: 51,
    /** 保证金金额 */
    DEPOSIT_BASE: 50,
    /** 平台服务费 */
    PLATFORM_FEE: 1,
} as const

/**
 * 轮询配置
 */
export const POLL_CONFIG = {
    /** 订单状态轮询间隔（毫秒） */
    INTERVAL: 3000,
} as const

/**
 * 订单过期配置
 */
export const EXPIRE_CONFIG = {
    /** 订单过期时间（秒） */
    TIME: 300,
} as const

/**
 * 支付常量汇总
 */
export const PAYMENT_CONSTANTS = {
    /** 支付金额 */
    AMOUNT: PAYMENT_AMOUNTS.DEPOSIT_TOTAL,
    /** 轮询间隔（毫秒） */
    POLL_INTERVAL: POLL_CONFIG.INTERVAL,
    /** 过期时间（秒） */
    EXPIRE_TIME: EXPIRE_CONFIG.TIME,
    /** 平台费（元） */
    PLATFORM_FEE: PAYMENT_AMOUNTS.PLATFORM_FEE,
} as const
