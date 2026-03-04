/**
 * 将PascalCase属性转换为camelCase的通用函数
 * 用于兼容老旧消息中的属性命名
 */
export function convertPascalToCamel(obj: any): any {
    if (obj === null || obj === undefined) {
        return obj
    }

    // 如果是数组，递归处理每个元素
    if (Array.isArray(obj)) {
        return obj.map((item) => convertPascalToCamel(item))
    }

    // 如果是对象，处理每个属性
    if (typeof obj === 'object') {
        const result: any = {}

        for (const key in obj) {
            if (obj.hasOwnProperty(key)) {
                // 将PascalCase转换为camelCase
                const camelKey = key.charAt(0).toLowerCase() + key.slice(1)

                // 递归处理嵌套对象
                result[camelKey] = convertPascalToCamel(obj[key])
            }
        }

        return result
    }

    // 如果是基本类型，直接返回
    return obj
}

/**
 * 秒杀状态枚举映射
 * 将数字状态值转换为字符串状态值
 *
 * 测试用例：
 * - normalizeAuctionStatus(0) => '草稿'
 * - normalizeAuctionStatus(1) => '上架'
 * - normalizeAuctionStatus(2) => '秒杀中'
 * - normalizeAuctionStatus(4) => '已成交'
 * - normalizeAuctionStatus(8) => '交易成功'
 * - normalizeAuctionStatus(16) => '卖家失约'
 * - normalizeAuctionStatus(32) => '买家失约'
 * - normalizeAuctionStatus(128) => '交易关闭'
 * - normalizeAuctionStatus('已成交') => '已成交'
 * - normalizeAuctionStatus('秒杀中') => '秒杀中'
 */
const AUCTION_STATUS_MAP: { [key: number]: string } = {
    0: '草稿',
    1: '上架',
    2: '秒杀中',
    4: '已成交',
    8: '交易成功',
    16: '卖家失约',
    32: '买家失约',
    128: '交易关闭',
}

/**
 * 统一处理秒杀状态值
 * @param status 状态值（可能是数字或字符串）
 * @returns 统一后的字符串状态值
 */
function normalizeAuctionStatus(status: any): string {
    if (status === null || status === undefined) {
        return ''
    }

    // 如果已经是字符串，直接返回
    if (typeof status === 'string') {
        return status
    }

    // 如果是数字，转换为对应的字符串
    if (typeof status === 'number') {
        return AUCTION_STATUS_MAP[status] || status.toString()
    }

    // 其他类型转换为字符串
    return String(status)
}

/**
 * 秒杀消息payload的专用转换函数
 * 处理AuctionItemDto相关的属性转换
 */
export function convertAuctionPayload(payload: any): any {
    if (!payload) {
        return payload
    }

    // 如果payload是字符串，先解析为对象
    if (typeof payload === 'string') {
        try {
            payload = JSON.parse(payload)
        } catch (e) {
            // Failed to parse payload string
            return payload
        }
    }

    // 使用通用转换函数
    const convertedPayload = convertPascalToCamel(payload)

    // 统一处理状态值
    if (convertedPayload.status !== undefined) {
        convertedPayload.status = normalizeAuctionStatus(convertedPayload.status)
    }

    return convertedPayload
}
