/**
 * 拍卖相关工具方法
 */

/**
 * 计算竞拍最低报价
 * @param currentPrice - 当前价格
 * @param isKasec - 是否为卡秒模式
 * @returns 最低报价
 */
export const calculateMinBidPrice = (currentPrice: number = 0, isKasec: boolean = false): number => {
    let minPrice = 0

    if (currentPrice) {
        if (currentPrice < 100) {
            minPrice = currentPrice + 1
        } else if (currentPrice < 1000) {
            minPrice = currentPrice + 5
        } else if (currentPrice < 2000) {
            minPrice = currentPrice + 10
        } else if (currentPrice < 5000) {
            minPrice = currentPrice + 20
        } else if (currentPrice < 10000) {
            minPrice = currentPrice + 50
        } else {
            minPrice = currentPrice + 100
        }
    }

    console.log('计算最低出价:', {
        currentPrice,
        minPrice,
        isKasec,
    })

    // 卡秒模式下，最低价格增幅为普通模式的3倍
    if (isKasec) {
        minPrice = currentPrice + (minPrice - currentPrice) * 3
        console.log('卡秒模式 - 调整后最低出价:', minPrice)
    }

    return minPrice
}

export default {
    calculateMinBidPrice,
}
