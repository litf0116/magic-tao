import api from '@/utils/api'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

export interface IUser {
    id?: number
    isAuthenticated?: boolean
    userName?: string
    name?: string
    surname?: string
    headImgUrl?: string
    phoneNumber?: string
    phoneNumberConfirmed?: boolean
    emailAddress?: string
    tenantId?: string
    roles?: string[]
}

export interface IUserInfo {
    avatarUrl?: string
    city?: string
    country?: string
    gender?: number
    language?: string
    nickName?: string
    openid?: string
    province?: string
    unionid?: string
}

export const useUserStore = defineStore('userStore', () => {
    const user = ref<IUser>(uni.getStorageSync('user') || {})
    const userInfo = ref<IUserInfo>(uni.getStorageSync('userInfo') || { openid: '', unionid: '' })
    const roles = ref<string[]>(uni.getStorageSync('roles') || [])
    const openid = ref<string>(uni.getStorageSync('openid') || '')
    const unionid = ref<string>(uni.getStorageSync('unionid') || '')
    const token = ref<string>(uni.getStorageSync('token') || '')
    const sessionKey = ref<string>(uni.getStorageSync('sessionKey') || '')
    const sessionTime = ref<string>(uni.getStorageSync('sessionTime') || '')
    const phone = ref<string>(uni.getStorageSync('phone') || '')

    const isLogin = computed(() => user.value.id! > 0)
    const isAdmin = computed(() => roles.value.includes('Admin'))
    const isChatAdmin: Ref<boolean> = computed(
        () => !!~roles.value.indexOf('Manager') || !!~roles.value.indexOf('Admin')
    )
    const isAuctionAdmin: Ref<boolean> = computed(
        () => !!~roles.value.indexOf('AuctionManager') || !!~roles.value.indexOf('Admin')
    )

    const _logout = () => {
        // console.log("mutaction:LOGOUT")
        uni.removeStorageSync('token')
        uni.removeStorageSync('userInfo')
        uni.removeStorageSync('userid')
        uni.removeStorageSync('sessionKey')
        uni.removeStorageSync('phone')
        uni.removeStorageSync('roles')
        token.value = ''
        sessionKey.value = ''
        sessionTime.value = ''
        user.value = {}
        phone.value = ''
    }

    function SET_USER(payload: IUser) {
        user.value = payload
        uni.setStorageSync('user', payload)
        uni.setStorageSync('userid', payload.id)
    }
    function SET_PHONE(payload: string) {
        phone.value = payload
        uni.setStorageSync('phone', payload)
    }
    function SET_ROLES(payload: string[]) {
        roles.value = payload
        uni.setStorageSync('roles', payload)
    }

    function needLogin(forceCheck = false, backHome = true) {
        const { toLogin, toHome } = useTo()
        if (!token.value || forceCheck === true) {
            uni.showModal({
                content: '需要登录后才能继续',
                success: (e) => {
                    if (e.confirm) {
                        const pages: any[] = getCurrentPages() || []
                        setTimeout(() => {
                            toLogin(pages[pages.length - 1].route, pages[pages.length - 1].options)
                        }, 200)
                    } else {
                        if (backHome) toHome()
                    }
                },
            })
            return false
        }
        return true
    }

    const checkLogin = async (_needlogin = false, forceCheck = true) => {
        if (!token.value) {
            console.log('notlogin... to logout')
            _logout()
            if (_needlogin) needLogin()
            return
        }

        if (!forceCheck && user.value.id && token.value) {
            return
        }
        await api.getCurrentLoginInformations().then(
            (res: any) => {
                if (res && res.user) {
                    SET_USER(res.user)
                    if (res.user.phoneNumber) SET_PHONE(res.user.phoneNumber)
                    if (res.roles) SET_ROLES(res.roles)
                } else {
                    console.log('notlogin... to logout')
                    _logout()
                    if (_needlogin) {
                        needLogin()
                    }
                }
            },
            (err) => {
                console.log('notlogin', err)
                _logout()
            }
        )
    }

    const getCode = () => {
        return new Promise((resolve, reject) => {
            uni.login({
                provider: 'weixin',
                success: async (loginRes) => {
                    console.log('loginRes:', loginRes)
                    if (loginRes.errMsg === 'login:ok' && loginRes.code) {
                        console.log('code', loginRes.code)
                        return resolve(loginRes.code)
                    } else {
                        return reject()
                    }
                },
            })
        })
    }

    //LINK - 微信登录
    const wxLogin = async () => {
        return new Promise(async (resolve, reject) => {
            await getCode().then(async (code) => {
                console.log('getCode result:', code)
                await api
                    .weixinMiniAuthenticate({
                        code: code,
                    })
                    .then(async (res: any) => {
                        console.log(res)
                        if (res.accessToken) {
                            token.value = res.accessToken
                            uni.setStorageSync('token', res.accessToken)

                            if (res.extension) {
                                if (res.extension.openid) {
                                    openid.value = res.extension.openid
                                    await uni.setStorageSync('openid', res.extension.openid)
                                }
                                if (res.extension.session_key) {
                                    sessionKey.value = res.extension.session_key
                                    await uni.setStorageSync('sessionKey', res.extension.session_key)
                                    sessionTime.value = res.extension.time
                                    await uni.setStorageSync('sessionTime', res.extension.time)
                                }
                                if (res.extension.unionid) {
                                    await uni.setStorageSync('unionid', res.extension.unionid)
                                    unionid.value = res.extension.unionid
                                }
                            }
                            await checkLogin()
                            return resolve(res)
                        } else {
                            reject('登录失败')
                        }
                        return resolve(res)
                    })
            })
        })
    }

    // ANCHOR - 帐号密码登录
    const login = async (userNameOrEmailAddress: string, password: string) => {
        return new Promise(async (resolve, reject) => {
            await api
                .authenticate({ userNameOrEmailAddress, password })
                .then(async (res: any) => {
                    if (res.accessToken) {
                        token.value = res.accessToken
                        uni.setStorageSync('token', res.accessToken)
                        await checkLogin()
                        return resolve(res)
                    } else {
                        reject('登录失败')
                    }

                    return resolve(res)
                })
                .catch((err) => {
                    return reject(err)
                })
        })
    }

    const code2Session = () => {
        return new Promise((resolve, reject) => {
            uni.login({
                provider: 'weixin',
                success: async (loginRes) => {
                    // console.log("loginRes:", loginRes)
                    if (loginRes.errMsg === 'login:ok' && loginRes.code) {
                        console.log('code', loginRes.code)
                        await api.code2session({ code: loginRes.code }).then(async (res: any) => {
                            // console.log("code2session:", res)
                            if (res.openid) {
                                openid.value = res.openid
                                await uni.setStorageSync('openid', res.openid)
                            }
                            if (res.session_key) {
                                sessionKey.value = res.session_key
                                await uni.setStorageSync('sessionKey', res.session_key)
                                sessionTime.value = res.time
                                await uni.setStorageSync('sessionTime', res.session_key)
                            }
                            if (res.unionid) {
                                await uni.setStorageSync('unionid', res.unionid)
                                unionid.value = res.unionid
                            }
                        })
                        return resolve(loginRes)
                    } else {
                        return reject()
                    }
                },
            })
        })
    }

    const phoneLogin = async (data: { iv: string; encryptedData: string }) => {
        return new Promise(async (resolve, reject) => {
            await code2Session().then(async (res) => {
                console.log('code2Session result:', res)
                console.log('openid', openid.value)
                console.log('unionid', unionid.value)
                console.log('sessionKey', sessionKey.value)
                await api
                    .phoneAuth({
                        openid: openid.value,
                        unionid: unionid.value,
                        session_key: sessionKey.value,
                        iv: data.iv,
                        encryptedData: data.encryptedData,
                    })
                    .then(
                        async (res: any) => {
                            if (res.accessToken) {
                                token.value = res.accessToken
                                uni.setStorageSync('token', res.accessToken)

                                // if (res.user) {
                                //     SET_USER(res.user)
                                //     if (res.user.phoneNumber) {
                                //         SET_PHONE(res.user.phoneNumber)
                                //     }
                                // }

                                // if (res.user && res.user.weChatUserLogin) {
                                //     {
                                //         const v = res.user.weChatUserLogin
                                //         uni.setStorageSync("userInfo", v)
                                //         userInfo.value = v
                                //     }
                                // }
                                // if (res.roleNames) {
                                //     roles.value = res.roleNames
                                // }
                                await checkLogin()
                                return resolve(res)
                            } else {
                                return reject('获取登录失败')
                            }
                        },
                        (err) => {
                            return reject(err)
                        }
                    )
            })
        })
    }

    const chatStore = useChatStore()

    function logout() {
        uni.showModal({
            content: '确定要退出登录么',
            success: (e) => {
                if (e.confirm) {
                    api.tokenAuth
                        .Logout()
                        .then(() => {
                            clear()
                        })
                        .catch((e) => {
                            clear()
                        })
                }
            },
        })
    }

    function clear() {
        _logout()
        chatStore.clear()
    }

    //ANCHOR - return
    return {
        user,
        userInfo,
        roles,
        openid,
        unionid,
        token,
        sessionKey,
        phone,
        //compute
        isLogin,
        isAdmin,
        isChatAdmin,
        isAuctionAdmin,
        //action
        logout,
        clear,
        code2Session,
        wxLogin,
        login,
        phoneLogin,
        checkLogin,
        needLogin,
    }
})
