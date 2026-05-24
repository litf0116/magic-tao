import { RouteRecordRaw } from 'vue-router'

export default {
    path: '/android-download',
    name: 'androidDownload',
    meta: {
        title: 'Android 下载',
        hidden: true,
        requiresAuth: false,
    },
    component: () => import('@/views/android-download/index.vue'),
} as RouteRecordRaw
