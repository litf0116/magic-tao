/**
 * 功能开关配置
 * 用于管理小程序和App的功能差异
 * 小程序审核敏感功能可在此控制
 */

export const FeatureConfig = {
  // ========== 通用功能（所有平台都可用） ==========
  
  // 聊天功能
  chat: true,
  privateChat: true,
  groupChat: true,
  friendAdd: true,
  
  // 商品功能
  goodsList: true,
  goodsDetail: true,
  goodsFavorite: true,
  
  // 帖子功能
  postList: true,
  postDetail: true,
  postPublish: true,
  postComment: true,
  
  // 用户功能
  userCenter: true,
  userInfo: true,
  userSetting: true,
  
  // 支付功能
  payment: true,
  orderList: true,
  
  // 消息推送
  push: true,
  
  // ========== 小程序限制功能（审核敏感） ==========
  
  // #ifdef MP-WEIXIN
  // 以下功能因小程序审核问题暂时隐藏
  auction: false,      // 拍卖/秒杀功能
  // #endif
  
  // ========== App 专属功能（可开放） ==========
  
  // #ifdef APP-PLUS
  auction: true,       // 拍卖/秒杀功能
  // #endif
}

/**
 * 判断功能是否可用
 */
export function isFeatureEnabled(feature: keyof typeof FeatureConfig): boolean {
  return FeatureConfig[feature] === true
}
