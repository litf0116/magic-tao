import { createRouter, RouteRecordRaw, createWebHashHistory } from 'vue-router'
import authRoute from './authRoute'

import Layout from '@/layouts/layout.vue'
import adminRoute from './adminRoute'
import chatRoute from './chatRoute'
import appDownloadRoute from './appDownloadRoute'
import androidDownloadRoute from './androidDownloadRoute'
export const constantRoutes: RouteRecordRaw[] = [
    {
        path: '/',
        component: Layout,
        redirect: '/index',
        meta: { title: '魔力淘', icon: 'i-mdi-edit' },
        children: [
            {
                path: 'index',
                name: 'index',
                meta: { title: '魔力淘' },
                component: () => import('@/views/home/index.vue'),
            },
            {
                path: 'a',
                name: 'a',
                meta: { title: '拍卖行' },
                component: () => import('@/views/home/a.vue'),
            },
            {
                path: 'b',
                name: 'b',
                meta: { title: '魔力淘橱窗' },
                component: () => import('@/views/home/a.vue'),
            },
            {
                path: 'c',
                name: 'c',
                meta: { title: '魔力宝贝官网' },
                component: () => import('@/views/home/a.vue'),
            },
            {
                path: 'd',
                name: 'd',
                meta: { title: '魔力百科资料站' },
                component: () => import('@/views/home/a.vue'),
            },
            {
                path: 'e',
                name: 'e',
                meta: { title: '自助中介系统' },
                component: () => import('@/views/home/a.vue'),
            },
        ],
    },
    {
        path: '/payment',
        name: 'Payment',
        meta: { title: '支付' },
        component: () => import('@/views/payment/PaymentPage.vue'),
    },
    appDownloadRoute,
    androidDownloadRoute,
    {
        path: '/forum',
        component: () => import('@/layouts/layout2.vue'),
        meta: { hidden: true },
        redirect: '/forum/index',
        children: [
            {
                path: 'tradingPost',
                name: 'tradingPost',
                meta: { title: '交易站' },
                component: () => import('@/views/home/tradingPost.vue'),
            },
            {
                path: 'postDetail/:id',
                name: 'postDetail',
                meta: { title: '帖子详情' },
                component: () => import('@/views/home/components/postDetail.vue'),
            },
        ],
    },
]

export const asyncRouter: RouteRecordRaw[] = [
    chatRoute,
    authRoute,
    adminRoute,

    {
        path: '/.*',
        redirect: '/404',
        meta: { hidden: true },
    },
]

const router = createRouter({
    history: createWebHashHistory(),
    routes: [...constantRoutes, ...asyncRouter],
})

export default router
