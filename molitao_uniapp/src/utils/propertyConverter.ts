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
 * 拍卖消息payload的专用转换函数
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
            console.warn('Failed to parse payload string:', e)
            return payload
        }
    }

    // 使用通用转换函数
    return convertPascalToCamel(payload)
}
