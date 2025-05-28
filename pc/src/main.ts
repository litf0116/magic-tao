import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { MotionPlugin } from '@vueuse/motion'
import screenShort from 'vue-web-screen-shot'
import App from './App.vue'
import router from './routes/index'

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
        const permission = binding.value // 获取到 v-permission的值
        if (permission) {
            const permissions = computed(() => useStore.permissions || [])
            const hasPermission = !!~permissions.value.indexOf(permission)
            if (!hasPermission) {
                // 没有权限 移除Dom元素
                el.parentNode && el.parentNode.removeChild(el)
            }
        }
    },
})
