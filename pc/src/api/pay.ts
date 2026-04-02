import request from '@/utils/request'

export interface PayDepositNativeResponse {
    code_url: string
    outTradeNo: string
    amount: number
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
}
