import { createApp, watch } from 'vue'
import { createPinia } from 'pinia'
import { MotionPlugin } from '@vueuse/motion'
import screenShort from 'vue-web-screen-shot'
import App from './App.vue'
import router from './routes/index'
import { useUserStore } from './stores/userStore'

import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import 'virtual:uno.css'

import './style.scss'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

import Schema from 'async-validator'
// 设置报错提示信息
Schema.messages.required = (fieldName) => {
    return '该项为必填项'
}

const app = createApp(App)
app.use(router).use(createPinia()).use(ElementPlus).use(MotionPlugin)
app.use(screenShort, {
    enableWebRtc: true, // 启用WebRTC
    level: 9999, // 截图层级
    clickCutFullScreen: false, // 禁用单击全屏截图
    hiddenToolIco: {
        save: false,
        undo: false,
        confirm: false,
    },
    writeBase64: true, // 写入剪贴板
    wrcWindowMode: false, // 窗口模式
})

app.mount('#app')

for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}

app.config.globalProperties.$filters = {
    // payType: (value: unknown, arg: string | undefined) => {
    //     return PayTypeEnum[value]
    // },
    // orderState: (value: unknown, arg: string | undefined) => {
    //     return OrderStateEnum[value]
    // },
}

app.directive('permission', {
    mounted(el: any, binding: any) {
        const useStore: any = useUserStore()
        const permission = binding.value
        if (!permission) return

        // 初始检查
        const checkPermission = () => {
            const permissions = useStore.permissions || []
            const hasPermission = permissions.includes(permission)
            if (!hasPermission) {
                el.style.display = 'none'
            } else {
                el.style.display = ''
            }
        }

        checkPermission()

        // 监听权限变化
        el._permissionWatcher = watch(
            () => useStore.permissions,
            () => checkPermission(),
            { deep: true }
        )
    },
    updated(el: any, binding: any) {
        const useStore: any = useUserStore()
        const permission = binding.value
        if (!permission) return

        const permissions = useStore.permissions || []
        const hasPermission = permissions.includes(permission)
        if (!hasPermission) {
            el.style.display = 'none'
        } else {
            el.style.display = ''
        }
    },
    unmounted(el: any) {
        // 清理 watcher
        if (el._permissionWatcher) {
            el._permissionWatcher()
        }
    },
})
