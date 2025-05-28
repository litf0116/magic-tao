import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 添加用户群聊等级信息
 * @returns 
 */
export function GroupChatLevelAdd(data) {
    return axios({
        url: '/api/GroupChatLevelSettings/Add',
        method: 'post',
        data:data
    })
}
/**
 * 获取用户群聊等级信息
 * @param id 
 * @returns 
 */
export function GetUserGroupLevel(id) {
    return axios({
        url: '/api/GroupChatLevelSettings/GetUserGroupLevel/'+id,
        method: 'get'
    })
}

/**
 * 获取群聊等级信息列表
 * @returns 
 */
export function GetList() {
    return axios({
        url: '/api/GroupChatLevelSettings/GetList',
        method: 'get'
    })
}
/**
 * 添加群聊等级信息列表
 * @returns 
 */
export function AddGroupChatLevelSettings(data) {
    return axios({
        url: '/api/GroupChatLevelSettings/AddGroupChatLevelSettings',
        method: 'post',
        data:data
    })
}

/**
 * 修改群聊等级信息列表
 * @returns 
 */
export function EditGroupChatLevelSetting(data) {
    return axios({
        url: '/api/GroupChatLevelSettings/EditGroupChatLevelSetting',
        method: 'post',
        data:data
    })
}
/**
 * 删除群聊等级信息列表
 * @returns 
 */
export function DeleteGroupChatLevelSetting(id) {
    return axios({
        url: '/api/GroupChatLevelSettings/DeleteGroupChatLevelSetting/'+id,
        method: 'get'
    })
}

/**
 * 获取用户群聊等级
 * @returns 
 */
export function wsGetUserGroupLevel(id) {
    return axios({
        url: '/ws/GetUserGroupLevel/'+id,
        method: 'get'
    })
}