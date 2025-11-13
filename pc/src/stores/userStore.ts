import { defineStore } from 'pinia'
import { getToken, removeToken, setToken } from '../utils/cookies'
import { usePermissionStore } from './permissionStore'
import api from '@/api'
import { generateTokenForUser } from '@/api/devAuthAPI'
import { UserLoginInfoDto } from '@/api/appService'
import { convertImageUrl } from '@/utils/imageUrlConverter'

export interface IUserModuleState {
    user: UserLoginInfoDto
    token: string
    roles: string[]
    permissions: string[]
}

const defaultUser: UserLoginInfoDto = { id: -1 }

// main is the name of the store. It is unique across your application
// and will appear in devtools
export const useUserStore = defineStore('user', () => {
    // a function that returns a fresh state
    const permissionStore = usePermissionStore()

    const user: Ref<UserLoginInfoDto> = ref(Object.assign({}, defaultUser, JSON.parse(localStorage.getItem('user')!)))

    // 处理用户头像URL转换
    const processedUser = computed(() => {
        if (!user.value || !user.value.headImgUrl) return user.value
        return {
            ...user.value,
            headImgUrl: convertImageUrl(user.value.headImgUrl)
        }
    })
    const token = ref(getToken() || '')
    const roles = ref([] as string[])
    const permissions = ref([] as string[])

    // Getter
    const isLogin: Ref<boolean> = computed(() => !!~user.value.id!)
    const isAdmin: Ref<boolean> = computed(() => !!~roles.value.indexOf('Admin'))

    const isChatAdmin: Ref<boolean> = computed(
        () => !!~roles.value.indexOf('Manager') || !!~roles.value.indexOf('Admin')
    )

    const isAuctionAdmin: Ref<boolean> = computed(
        () => !!~roles.value.indexOf('AuctionManager') || !!~roles.value.indexOf('Admin')
    )

    // optional actions

    function login(payload: { username: string; password: string; rememberClient: boolean }) {
        let { username } = payload
        const { password } = payload
        username = username.trim()
        return new Promise((resolve, reject) => {
            api.tokenAuth
                .authenticate({
                    body: {
                        userNameOrEmailAddress: username.trim(),
                        password: password,
                    },
                })
                .then(
                    async (res: any) => {
                        token.value = res.accessToken!
                        setToken(res.accessToken!)
                        resolve(res.accessToken!)
                    },
                    ({ error }) => {
                        reject(error)
                    }
                )
        })
    }

    // 开发调试登录 - 使用用户ID直接生成token
    function devLogin(userId: number) {
        return new Promise((resolve, reject) => {
            // 检查是否为开发环境
            if (import.meta.env.MODE !== 'development') {
                reject(new Error('开发调试登录仅在开发环境中可用'))
                return
            }

            console.log('🔧 UserStore.devLogin 开始为用户', userId, '生成token...')

            generateTokenForUser(userId)
                .then(
                    async (res: any) => {
                        console.log('📊 UserStore.devLogin 收到API响应:', res)

                        // 修复：res.data 包含实际的token数据
                        const tokenData = res.data || res;
                        console.log('📋 tokenData.accessToken:', tokenData.accessToken)
                        console.log('📋 tokenData.accessToken类型:', typeof tokenData.accessToken)
                        console.log('📋 tokenData.accessToken长度:', tokenData.accessToken?.length || 0)

                        if (!tokenData.accessToken) {
                            console.error('❌ UserStore.devLogin: accessToken为空!')
                            console.error('res对象:', JSON.stringify(res, null, 2))
                            reject(new Error('API返回的accessToken为空'))
                            return
                        }

                        token.value = tokenData.accessToken!
                        setToken(tokenData.accessToken!)
                        console.log('✅ UserStore.devLogin: token已设置')
                        resolve(tokenData.accessToken!)
                    },
                    (error) => {
                        console.error('❌ UserStore.devLogin: API调用失败:', error)
                        reject(error)
                    }
                )
        })
    }

    function SET_USER(payload: UserLoginInfoDto) {
        user.value = payload
        localStorage.setItem('user', JSON.stringify(payload))
    }

    function SET_TOKEN(payload: string) {
        token.value = payload
        setToken(payload)
    }
    function SET_ROLES(payload: string[]) {
        roles.value = payload
    }

    function RESET_TOKEN() {
        removeToken()
        SET_TOKEN('')
    }

    // 拉取用户信息
    const getUserInfo = () => {
        api.session
            .getCurrentLoginInformations()
            .then(async (res: any) => {
                if (!res.user) {
                    SET_USER({})
                    RESET_TOKEN()
                    SET_ROLES([])
                } else {
                    SET_USER(res.user)
                }
                if (res.permissions) permissions.value = res.permissions
                if (res.roles) roles.value = res.roles
                permissionStore.generateRoutes({
                    permissions: res.permissions!,
                    roles: res.roles,
                })
            })
            .catch(() => {
                logout()
            })
    }

    const chatStore = useChatStore()

    const logout = () => {
        console.log('logout action')
        api.tokenAuth
            .logOut()
            .then(() => {
                clear()
            })
            .catch((e) => {
                clear()
            })
    }

    function clear() {
        token.value = ''
        removeToken()
        roles.value = []
        permissions.value = []
        user.value = Object.assign({}, defaultUser)
        localStorage.removeItem('user')
        localStorage.removeItem('upyun')
        localStorage.removeItem('to')
        chatStore.clear()

        //重新生成路由
        const permissionStore = usePermissionStore()
        permissionStore.generateRoutes({ permissions: [], roles: [] })
    }

    return {
        user: processedUser,
        token,
        roles,
        permissions,
        isLogin,
        isAdmin,
        isChatAdmin,
        isAuctionAdmin,
        getUserInfo,
        SET_TOKEN,
        login,
        devLogin,
        logout,
    }
})
