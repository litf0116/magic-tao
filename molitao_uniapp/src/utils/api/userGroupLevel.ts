import request from '../request'

export default {
    // 获取用户等级信息（包含等级配置）
    getUserLevelInfo(id: number) {
        return request.get(`/api/GroupChatLevelSettings/GetUserLevelInfo/${id}`)
    },

    // 获取用户群聊等级
    getUserGroupLevel(id: number) {
        return request.get(`/api/GroupChatLevelSettings/GetUserGroupLevel/${id}`)
    }
}
