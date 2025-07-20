import { useRequest } from '@/utils/request'
const axios = useRequest()

/**
 * 查询拍卖中的商品
 * @param data
 * @returns
 */
export function GetAuctionMidList(data) {
    return axios({
        url: '/api/services/app/AuctionItem/GetAuctionMidList',
        method: 'post',
        data: data,
    })
}

/**
 * 分页查询提现记录
 * @param data
 * @returns
 */
export function PageWithdrawalAmount(data) {
    return axios({
        url: '/api/services/app/WithdrawalAmountService/Page',
        method: 'get',
        params: data,
    })
}
