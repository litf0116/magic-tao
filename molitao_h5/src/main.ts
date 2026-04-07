import { createSSRApp } from 'vue'
import App from './App.vue'
import * as Pinia from 'pinia'
import { h5PushService } from './utils/pushH5'

import 'uno.css'
import '@/css/app.scss'
// import "@/uno.scss"

export function createApp() {
    const app = createSSRApp(App)
    app.use(Pinia.createPinia())

    // #ifdef H5
    h5PushService.init().catch((error) => {
        console.error('[Main] H5 推送服务初始化失败:', error)
    })
    // #endif

    return {
        app,
        Pinia,
    }
}
