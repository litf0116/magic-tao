<template>
    <ElConfigProvider :locale="zhCn">
        <RouterView v-slot="{ Component }">
            <template v-if="Component">
                <!-- <Suspense> -->
                <component :is="Component"></component>
                <!-- </Suspense> -->
            </template>
        </RouterView>
    </ElConfigProvider>
</template>

<script setup lang="ts">
import 'nprogress/nprogress.css'
import NProgress from 'nprogress'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import '@/style.scss'

const whiteList = ['/', '/index', '/auth/login', '/register', '/error-page', '/app-download']
const route = useRoute()
const router = useRouter()
const path = computed(() => route.path)

const userStore = useUserStore()
// 异步获取用户信息，避免阻塞页面加载
if (userStore.token) {
    userStore.getUserInfo().catch(() => {
        // 获取用户信息失败时不做处理，让路由守卫处理重定向
    })
}

provide('path', path)

router.beforeEach(async (to, from, next) => {
    NProgress.start()
    const userStore: any = useUserStore()
    const token = userStore.token
    const user = userStore.user
    // console.log('to', to, to.query)

    // Determine whether the user has logged in
    if (token) {
        // Check whether the user has obtained his permission roles
        if (!user) {
            // Remove token and redirect to login page
            userStore.logout()
            next(`/auth/login?redirect=${to.path}`)
        } else {
            next()
            return true
        }
    } else {
        if (whiteList.indexOf(to.path) !== -1) {
            // In the free login whitelist, go directly
            next()
            return true
        } else {
            // Other pages that do not have permission to access are redirected to the login page.
            next(`/auth/login?redirect=${to.path}`)
        }
    }
})

router.afterEach((to: any) => {
    // Finish progress bar
    NProgress.done()
    // set page title
    document.title = `${to.meta.title} - ${import.meta.env.VITE_APP_TITLE}`
})
</script>

<style lang="scss">
/*引入阿里字体图标*/
@import './assets/iconfont/iconfont.css';
</style>
