import request from '@/utils/request'

export interface PayDepositNativeResponse {
    code_url: string
    outTradeNo: string
    amount: number
}

export interface OrderStatusResponse {
    orderId: string
    outTradeNo: string
    status: string
    amount: number
    paidTime?: string
    tradeNo?: string
    message: string
}

export const payApi = {
    payDepositNative: (amount?: number): Promise<PayDepositNativeResponse> => {
        return new Promise((resolve, reject) => {
            const url = '/api/services/app/Client/PayDepositNative'
            const config = {
                method: 'get',
                url,
                params: {
                    amount: amount ?? 51,
                },
            }

            request(config)
                .then((response) => {
                    resolve(response.data)
                })
                .catch((error) => {
                    reject(error)
                })
        })
    },

    getOrderStatus: (outTradeNo: string): Promise<OrderStatusResponse> => {
        return new Promise((resolve, reject) => {
            const url = '/api/services/app/Client/GetPayOrderStatus'
            const config = {
                method: 'get',
                url,
                params: { outTradeNo },
            }

            request(config)
                .then((response) => {
                    resolve(response.data)
                })
                .catch((error) => {
                    reject(error)
                })
        })
    },
}
