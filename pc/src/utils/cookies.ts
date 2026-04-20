import Cookies from 'js-cookie'

// User
const tokenKey = 'token'
const refreshTokenKey = 'refreshToken'
const tokenExpireTimeKey = 'tokenExpireTime'

export const getToken = () => {
    return localStorage.getItem(tokenKey)
}
export const setToken = (token: string) => {
    localStorage.setItem(tokenKey, token)
    Cookies.set(tokenKey, token)
}
export const removeToken = () => {
    localStorage.removeItem(tokenKey)
    Cookies.remove(tokenKey)
}

export const getRefreshToken = () => {
    return localStorage.getItem(refreshTokenKey)
}
export const setRefreshToken = (refreshToken: string) => {
    localStorage.setItem(refreshTokenKey, refreshToken)
}
export const removeRefreshToken = () => {
    localStorage.removeItem(refreshTokenKey)
}

export const getTokenExpireTime = () => {
    const time = localStorage.getItem(tokenExpireTimeKey)
    return time ? parseInt(time, 10) : null
}
export const setTokenExpireTime = (expireInSeconds: number) => {
    const expireTime = Date.now() + expireInSeconds * 1000
    localStorage.setItem(tokenExpireTimeKey, expireTime.toString())
}
export const removeTokenExpireTime = () => {
    localStorage.removeItem(tokenExpireTimeKey)
}

export const isTokenExpiringSoon = (thresholdSeconds = 3600) => {
    const expireTime = getTokenExpireTime()
    if (!expireTime) return false
    return Date.now() + thresholdSeconds * 1000 > expireTime
}

export const clearAuthTokens = () => {
    removeToken()
    removeRefreshToken()
    removeTokenExpireTime()
}

const ouKey = 'Abp.OrganizationUnitId'
export const getOu = () => localStorage.getItem(ouKey)

export const setOu = (id: string) => localStorage.setItem(ouKey, id)

export const removeOu = () => Cookies.remove(ouKey)
