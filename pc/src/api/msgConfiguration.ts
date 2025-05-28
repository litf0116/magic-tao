import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 获取数据
 * @returns 
 */
export function GetList() {
    return axios({
        url: '/api/MsgConfiguration/GetList',
        method: 'get',
    })
}
/**
 * 创建消息配置
 * @param data 
 * @returns 
 */
export function Add(data) {
    return axios({
        url: '/api/MsgConfiguration/Add',
        method: 'post',
        data: data
    })
}