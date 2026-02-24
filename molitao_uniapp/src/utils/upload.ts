import Upyun from './upyun-wxapp-sdk.js'
const upyun = new Upyun.Upyun({
    bucket: 'molitao',
    domainHost: 'http://image.molitao.top',
    getSignatureUrl: import.meta.env.VITE_APP_BASE_API + '/api/services/app/Upload/GetSignature',
})

export function uploadImage(file) {
    return new Promise((resolve, reject) => {
        // 验证文件类型
        const fileName = file.toString()
        const fileExtension = fileName.split('.').pop()?.toLowerCase()
        const allowedExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp']

        if (!fileExtension || !allowedExtensions.includes(fileExtension)) {
            uni.showToast({
                title: '只支持 JPG、PNG、GIF、WEBP 格式的图片!',
                icon: 'none',
            })
            return reject('只支持 JPG、PNG、GIF、WEBP 格式的图片!')
        }

        const imageSrc = file
        // const fileExt = imageSrc.replace(/.+\./, '')
        // const fileName = dayjs(new Date()).format('YYYYMMHHmmss') + '.' + fileExt
        const path = `wxapp/${uni.getStorageSync('unionid') || uni.getStorageSync('openid') || 'unknow'}/`
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

                // 验证文件类型
                const fileName = imageSrc.toString()
                const fileExtension = fileName.split('.').pop()?.toLowerCase()
                const allowedExtensions = ['jpg', 'jpeg', 'png', 'gif', 'webp']

                if (!fileExtension || !allowedExtensions.includes(fileExtension)) {
                    uni.showToast({
                        title: '只支持 JPG、PNG、GIF、WEBP 格式的图片!',
                        icon: 'none',
                    })
                    return reject('只支持 JPG、PNG、GIF、WEBP 格式的图片!')
                }

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
