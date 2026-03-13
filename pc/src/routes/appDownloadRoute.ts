import { RouteRecordRaw } from 'vue-router'

export default {
    path: '/app-download',
    name: 'appDownload',
    meta: {
        title: '应用下载',
        hidden: true,
        requiresAuth: false,
    },
    component: () => import('@/views/app-download/index.vue'),
} as RouteRecordRaw
