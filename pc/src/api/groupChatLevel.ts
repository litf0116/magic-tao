import { useRequest } from '@/utils/request'
const axios = useRequest()

// 用户群等级实体
export interface UserGroupLevelEntity {
    id: number;
    userId: number;
    groupChatId: number;
    cumulativeAmount: number;
}

// 群聊等级配置实体
export interface GroupChatLevelSettingsEntity {
    id: number;
    name: string;
    level: number;
    amountRequired: number;
    borderColor: string;
    rightBorderColor: string;
}

// 用户等级信息（包含等级配置）
export interface UserLevelInfo {
    userLevel: UserGroupLevelEntity
    levelSettings: GroupChatLevelSettingsEntity
}

/**
 * 添加用户群聊等级信息
 * @returns
 */
export function GroupChatLevelAdd(data) {
    return axios({
        url: '/api/GroupChatLevelSettings/Add',
        method: 'post',
        data: data,
    })
}

/**
 * 获取用户群聊等级信息
 * @param id 用户ID
 * @returns Promise<UserGroupLevelEntity | null>
 */
export function GetUserGroupLevel(id: number): Promise<{ data: UserGroupLevelEntity | null }> {
    return axios({
        url: '/api/GroupChatLevelSettings/GetUserGroupLevel/' + id,
        method: 'get',
    })
}

/**
 * 获取用户等级信息（包含等级配置）
 * @param id 用户ID
 * @returns Promise<UserLevelInfo | null>
 */
export function GetUserLevelInfo(id: number): Promise<{ data: UserLevelInfo | null }> {
    return axios({
        url: '/api/GroupChatLevelSettings/GetUserLevelInfo/' + id,
        method: 'get',
    })
}

/**
 * 获取群聊等级信息列表
 * @returns
 */
export function GetList() {
    return axios({
        url: '/api/GroupChatLevelSettings/GetList',
        method: 'get',
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
        data: data,
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
        data: data,
    })
}

/**
 * 删除群聊等级信息列表
 * @returns
 */
export function DeleteGroupChatLevelSetting(id) {
    return axios({
        url: '/api/GroupChatLevelSettings/DeleteGroupChatLevelSetting/' + id,
        method: 'get',
    })
}

/**
 * 获取用户群聊等级
 * @returns
 */
export function wsGetUserGroupLevel(id) {
    return axios({
        url: '/ws/GetUserGroupLevel/' + id,
        method: 'get',
    })
}
