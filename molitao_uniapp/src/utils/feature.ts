/**
 * 功能开关配置
 */

const isWeixin = typeof uni !== 'undefined' && uni.getSystemInfoSync?.().platform === 'mp-weixin'

export const FeatureConfig = {
    chat: true,
    privateChat: true,
    groupChat: true,
    friendAdd: true,

    goodsList: true,
    goodsDetail: true,
    goodsFavorite: true,

    postList: true,
    postDetail: true,
    postPublish: true,
    postComment: true,

    userCenter: true,
    userInfo: true,
    userSetting: true,

    payment: true,
    orderList: true,

    push: true,

    auction: !isWeixin,
}

export function isFeatureEnabled(feature: keyof typeof FeatureConfig): boolean {
    return FeatureConfig[feature] === true
}
