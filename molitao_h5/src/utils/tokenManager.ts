const TOKEN_KEY = 'token'
const REFRESH_TOKEN_KEY = 'refreshToken'
const TOKEN_EXPIRE_TIME_KEY = 'tokenExpireTime'

export const getToken = (): string => {
    return uni.getStorageSync(TOKEN_KEY) || ''
}

export const setToken = (token: string): void => {
    uni.setStorageSync(TOKEN_KEY, token)
}

export const removeToken = (): void => {
    uni.removeStorageSync(TOKEN_KEY)
}

export const getRefreshToken = (): string => {
    return uni.getStorageSync(REFRESH_TOKEN_KEY) || ''
}

export const setRefreshToken = (refreshToken: string): void => {
    uni.setStorageSync(REFRESH_TOKEN_KEY, refreshToken)
}

export const removeRefreshToken = (): void => {
    uni.removeStorageSync(REFRESH_TOKEN_KEY)
}

export const getTokenExpireTime = (): number | null => {
    const time = uni.getStorageSync(TOKEN_EXPIRE_TIME_KEY)
    return time ? parseInt(time, 10) : null
}

export const setTokenExpireTime = (expireInSeconds: number): void => {
    const expireTime = Date.now() + expireInSeconds * 1000
    uni.setStorageSync(TOKEN_EXPIRE_TIME_KEY, expireTime.toString())
}

export const removeTokenExpireTime = (): void => {
    uni.removeStorageSync(TOKEN_EXPIRE_TIME_KEY)
}

export const isTokenExpiringSoon = (thresholdSeconds = 3600): boolean => {
    const expireTime = getTokenExpireTime()
    if (!expireTime) return false
    return Date.now() + thresholdSeconds * 1000 > expireTime
}

export const clearAuthTokens = (): void => {
    removeToken()
    removeRefreshToken()
    removeTokenExpireTime()
}

let isRefreshing = false
let refreshSubscribers: ((token: string) => void)[] = []

export const subscribeTokenRefresh = (cb: (token: string) => void): void => {
    refreshSubscribers.push(cb)
}

export const onTokenRefreshed = (token: string): void => {
    refreshSubscribers.forEach((cb) => cb(token))
    refreshSubscribers = []
}

export const setIsRefreshing = (value: boolean): void => {
    isRefreshing = value
}

export const getIsRefreshing = (): boolean => {
    return isRefreshing
}

let host = 'https://www.molitao.top'
// #ifdef H5
if (import.meta.env.DEV) {
    host = ''
}
// #endif

export const refreshAccessToken = async (): Promise<string | null> => {
    const refreshToken = getRefreshToken()
    if (!refreshToken) {
        return null
    }

    try {
        const res = await new Promise<any>((resolve, reject) => {
            uni.request({
                url: host + '/api/TokenAuth/RefreshToken',
                method: 'POST',
                data: { refreshToken },
                header: {
                    'Content-Type': 'application/json',
                    'Abp.Tenantid': 1,
                },
                success: (response) => resolve(response),
                fail: (error) => reject(error),
            })
        })

        if (res.statusCode === 200 && res.data?.success && res.data.result?.accessToken) {
            const { accessToken, expireInSeconds } = res.data.result
            setToken(accessToken)
            setTokenExpireTime(expireInSeconds || 604800)
            return accessToken
        }
    } catch (error) {
        console.error('Refresh token failed:', error)
        clearAuthTokens()
        handleUnauthorized()
    }
    return null
}

const handleUnauthorized = () => {
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
