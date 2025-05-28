const errorPrompt = (err: any) => {
    console.log('errorPrompt', err)
    if (err.validationErrors && err.validationErrors.length) {
        const info = err.validationErrors.reduce((c: any, o: any) => (c += `${o.message}\n`), '')
        uni.showModal({
            title: err.message,
            content: info,
            showCancel: false,
            success: function (res) {
                if (res.confirm) {
                    // console.log('用户点击确定')
                } else if (res.cancel) {
                    // console.log('用户点击取消')
                }
            },
        })
    } else {
        const msg = err.details || err.message
        if (err.code === 1) Tips.noCancelModal(msg)
        else Tips.info(msg)
    }
}

const httpsPromisify = <T>(fn: (opt: any) => void) => {
    return function (options: any | undefined) {
        return new Promise<T>((resolve, reject) => {
            options!.success = ({ data }: any) => {
                uni.hideLoading()
                uni.hideNavigationBarLoading()
                if (data.success) {
                    resolve(data.result)
                } else {
                    if (data.unAuthorizedRequest) {
                        uni.navigateTo({
                            url: '/pages/index/login',
                        })
                        return
                    }
                    errorPrompt(data.error)
                    reject(data.error.details || data.error.message)
                    return
                }
            }
            options!.fail = (err: any) => {
                console.log('httpsPromisify', err)
                uni.hideLoading()
                uni.hideNavigationBarLoading()
                return reject(err)
            }
            fn(options)
        })
    }
}

export default {
    httpsPromisify,
}
