import Cookies from 'js-cookie'

// User
const tokenKey = 'token'
export const getToken = () => {
    // Cookies.get(tokenKey)
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

const ouKey = 'Abp.OrganizationUnitId'
export const getOu = () => localStorage.getItem(ouKey)

export const setOu = (id: string) => localStorage.setItem(ouKey, id)

export const removeOu = () => Cookies.remove(ouKey)
