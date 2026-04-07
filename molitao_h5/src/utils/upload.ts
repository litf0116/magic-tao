import Upyun from './upyun-wxapp-sdk.js'

// 统一使用正式服务器地址
const baseApi = 'https://www.molitao.top'

const upyun = new Upyun.Upyun({
    bucket: 'molitao',
    operator: 'molitao',
    domainHost: 'https://image.molitao.top',
    getSignatureUrl: baseApi + '/api/services/app/Upload/GetSignature',
})

export function uploadImage(file) {
    return new Promise((resolve, reject) => {
        const imageSrc = file
        // 使用时间戳+随机字符串作为路径，避免unionid/openid为空导致undefined
        const timestamp = Date.now()
        const unionid = uni.getStorageSync('unionid')
        const openid = uni.getStorageSync('openid')
        const userId =
            unionid && unionid !== 'undefined' && unionid !== ''
                ? unionid
                : openid && openid !== 'undefined' && openid !== ''
                ? openid
                : `guest${timestamp}`
        const path = `wxapp/${userId}/`
        upyun.upload({
            localPath: imageSrc,
            remotePath: path,
            success: (res) => {
                if (res.statusCode == 401) {
                    uni.removeStorageSync(Upyun.CACHE_KEY)
                    return reject('上传失败，请重新上传')
                } else {
                    const jsonData = JSON.parse(res.data)
                    return resolve(`${upyun.domainHost}${jsonData.url}`)
                }
            },
            fail: ({ errMsg }) => {
                return reject(errMsg)
            },
        })
    })
}

export function upload(count = 1) {
    return new Promise<object>((resolve, reject) => {
        uni.chooseImage({
            count: count,
            // sizeType: ["compressed"],
            sourceType: ['album', 'camera'],
            //成功
            success: (res) => {
                const imageSrc = res!.tempFilePaths![0]
                upyun.upload({
                    localPath: imageSrc,
                    success: (res: any) => {
                        const jsonData = JSON.parse(res.data)
                        return resolve(jsonData)
                    },
                    fail: ({ errMsg }: any) => {
                        return reject(errMsg)
                    },
                })
            },
            //失败
            fail: ({ errMsg }: any) => {
                return reject(errMsg)
            },
        })
    })
}
export default {
    upload,
}
