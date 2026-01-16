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
 * 获取公开拍卖品列表（匿名访问）
 * @param params 查询参数
 * @returns
 */
export function GetPublicListAnonymous(
    params: {
        maxResultCount?: number
        skipCount?: number
        keyword?: string
        sorting?: string
    } = {}
) {
    return axios({
        url: '/api/AuctionItem/GetPublicListAnonymous',
        method: 'get',
        params: {
            MaxResultCount: params.maxResultCount,
            SkipCount: params.skipCount,
            Keyword: params.keyword,
            Sorting: params.sorting,
        },
    })
}

/**
 * 设置卡秒状态
 */
export function setKasecStatus(auctionItemId, isKasec) {
    return axios({
        url: '/api/services/app/AuctionItem/SetKasecStatus',
        method: 'post',
        data: { auctionItemId, isKasec },
    })
}

/**
 * 获取卡秒状态
 */
export function getKasecStatus(auctionItemId) {
    return axios({
        url: '/api/services/app/AuctionItem/GetKasecStatus?auctionItemId=' + auctionItemId,
        method: 'get',
    })
}
