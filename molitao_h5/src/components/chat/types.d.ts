export interface ChatOptions {
    enableAudio?: boolean // 是否启用语音功能
    enableEmoji?: boolean // 是否启用表情功能
    enableImage?: boolean // 是否启用图片功能
    maxTextLength?: number // 最大文本长度
    chatType?: 'private' | 'group' | 'auction' // 聊天类型
    showUserInfo?: boolean // 是否显示用户信息
    enableLongPress?: boolean // 是否启用长按功能
    autoScroll?: boolean // 是否自动滚动到底部
    historyLoadSize?: number // 历史消息加载数量
}
