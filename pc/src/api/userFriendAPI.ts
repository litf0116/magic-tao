import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 获取用户申请好友请求记录数量
 * @returns
 */
export function GetUserFriendCount() {
    return axios({
        url: '/api/services/app/UserFriend/GetUserFriendCount',
        method: 'get',
    })
}
