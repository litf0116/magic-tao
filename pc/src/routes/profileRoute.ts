import { RouteRecordRaw } from 'vue-router'

export default {
    path: '/profile',
    name: 'profile',
    meta: {
        title: '个人中心',
        hidden: true,
        requiresAuth: true,
    },
    component: () => import('@/views/profile/ProfilePage.vue'),
} as RouteRecordRaw
