import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 获取数据详情
 * @returns 
 */
export function GetDetail(id) {
    return axios({
        url: '/api/AuctionItem/GetDetail?id='+id,
        method: 'get',
    })
}