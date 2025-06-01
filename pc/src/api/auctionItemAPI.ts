import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 获取数据详情
 * @returns
 */
export function GetDetail(id) {
    return axios({
        url: '/api/AuctionItem/GetDetail?id=' + id,
        method: 'get',
    })
}

/**
 * 设置卡秒状态
 */
export function setKasecStatus(auctionItemId, isKasec) {
    return axios({
        url: '/api/AuctionItem/SetKasecStatus',
        method: 'post',
        data: { auctionItemId, isKasec },
    })
}

/**
 * 获取卡秒状态
 */
export function getKasecStatus(auctionItemId) {
    return axios({
        url: '/api/AuctionItem/GetKasecStatus?auctionItemId=' + auctionItemId,
        method: 'get',
    })
}
