import { RouteRecordRaw } from 'vue-router'

export default {
    path: '/auth',
    component: () => import('@/layouts/layout.vue'),
    meta: { hidden: true },
    children: [
        {
            path: 'login',
            name: 'login',
            meta: { title: '登录' },
            component: () => import('@/views/auth/login.vue'),
        },
    ],
} as RouteRecordRaw
