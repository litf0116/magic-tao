const errorPrompt = (err: any) => {
    if (!err) {
        console.warn('Error prompt received undefined error')
        return
    }
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

        // 检查是否是出价规则错误，如果是则触发自定义弹窗事件
        if (err.code === 1 && msg && msg.includes('出价必须大于最低加价')) {
            // 触发全局事件，让页面组件处理
            uni.$emit('showBidRulesModal', {
                message: msg,
                // 页面组件需要自己获取并传递价格信息
                needPriceInfo: true,
            })
        } else if (err.code === 1) {
            Tips.noCancelModal(msg)
        } else {
            Tips.info(msg)
        }
    }
}

const httpsPromisify = <T>(fn: (opt: any) => void) => {
    return function (options: any | undefined) {
        return new Promise<T>((resolve, reject) => {
            options!.success = ({ data, statusCode }: any) => {
                try {
                    uni.hideLoading()
                    // #ifndef H5
                    uni.hideNavigationBarLoading()
                    // #endif
                } catch (error) {
                    // 忽略隐藏加载状态时的错误
                }
                console.log('[API Response]', options!.url, { data, statusCode })
                // 兼容标准格式和简化格式: success 字段判断
                if (data && data.success === true) {
                    resolve(data.result)
                } else if (data && data.success === false) {
                    // 处理 401 未授权
                    if (statusCode === 401 || data.unAuthorizedRequest) {
                        handleUnauthorized()
                        return
                    }
                    // 处理 HTTP 404 等错误，data.error 可能不存在
                    const err = data.error
                    console.log('[API Error]', options!.url, { err, data })
                    errorPrompt(err)
                    reject(err?.details || err?.message || '请求失败')
                    return
                } else if (data && data.__abp) {
                    // ABP 格式只有 __abp 标记
                    if (data.success === true) {
                        resolve(data.result)
                    } else {
                        const err = data.error
                        errorPrompt(err)
                        reject(err?.message || '请求失败')
                    }
                } else {
                    // 非标准格式，直接返回 data
                    resolve(data)
                }
            }
            options!.fail = (err: any) => {
                try {
                    uni.hideLoading()
                    // #ifndef H5
                    uni.hideNavigationBarLoading()
                    // #endif
                } catch (error) {
                    // 忽略隐藏加载状态时的错误
                }
                return reject(err)
            }
            fn(options)
        })
    }
}

const handleUnauthorized = () => {
    const pages = getCurrentPages()
    const currentPage = pages[pages.length - 1]
    const currentPath = currentPage?.route || ''

    // 如果已经在登录页，不跳转
    if (currentPath.includes('login')) {
        return
    }

    uni.navigateTo({
        url: '/pages/index/login',
    })
}

export default {
    httpsPromisify,
}
