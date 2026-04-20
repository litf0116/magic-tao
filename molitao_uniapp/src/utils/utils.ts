import {
    getToken,
    getRefreshToken,
    isTokenExpiringSoon,
    setIsRefreshing,
    getIsRefreshing,
    refreshAccessToken,
    onTokenRefreshed,
    subscribeTokenRefresh,
    clearAuthTokens,
} from './tokenManager'

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
        return new Promise<T>(async (resolve, reject) => {
            const originalRequest = async (token?: string) => {
                if (token && isTokenExpiringSoon(3600) && getRefreshToken() && !getIsRefreshing()) {
                    setIsRefreshing(true)
                    const newToken = await refreshAccessToken()
                    setIsRefreshing(false)
                    if (newToken) {
                        options!.header = options!.header || {}
                        options!.header.Authorization = `Bearer ${newToken}`
                        onTokenRefreshed(newToken)
                    }
                }

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
                    if (data && data.success === true) {
                        resolve(data.result)
                    } else if (data && data.success === false) {
                        if (statusCode === 401 || data.unAuthorizedRequest) {
                            handleUnauthorized(options)
                            return
                        }
                        const err = data.error
                        console.log('[API Error]', options!.url, { err, data })
                        errorPrompt(err)
                        reject(err?.details || err?.message || '请求失败')
                        return
                    } else if (data && data.__abp) {
                        if (data.success === true) {
                            resolve(data.result)
                        } else {
                            const err = data.error
                            errorPrompt(err)
                            reject(err?.message || '请求失败')
                        }
                    } else {
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
            }

            await originalRequest()
        })
    }
}

const handleUnauthorized = async (options?: any) => {
    if (getRefreshToken() && !getIsRefreshing()) {
        setIsRefreshing(true)
        const newToken = await refreshAccessToken()
        setIsRefreshing(false)
        if (newToken && options) {
            options.header = options.header || {}
            options.header.Authorization = `Bearer ${newToken}`
            uni.request(options)
            return
        }
    } else if (getIsRefreshing() && options) {
        subscribeTokenRefresh((token: string) => {
            options.header = options.header || {}
            options.header.Authorization = `Bearer ${token}`
            uni.request(options)
        })
        return
    }

    clearAuthTokens()

    const pages = getCurrentPages()
    const currentPage = pages[pages.length - 1]
    const currentPath = currentPage?.route || ''

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
