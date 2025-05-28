import { RouteRecordRaw } from 'vue-router'

export default {
    path: '/chat',
    component: () => import('@/layouts/layoutChat.vue'),
    meta: { hidden: true },
    redirect: '/chat/index',
    children: [
        {
            path: 'index',
            name: 'chatIndex',
            redirect: '/chat/index/lobby',
            meta: { title: '聊天室' },
            component: () => import('@/views/chat/index.vue'),
            children: [
                // {
                //     path: 'lobby',
                //     meta: { title: '勇者招募所' },
                //     name: 'lobby',
                //     component: () => import('@/views/chat/lobby.vue'),
                // },
                {
                    path: 'privateChat/:id',
                    meta: { title: '私聊' },
                    component: () => import('@/views/chat/privateChat.vue'),
                },
                {
                    path: 'groupChat/:id',
                    meta: { title: '群聊' },
                    name: 'groupChat',
                    component: () => import('@/views/chat/groupChat.vue'),
                },
                {
                    path: 'auction',
                    meta: { title: '拍卖行' },
                    component: () => import('@/views/chat/auction.vue'),
                },
            ],
        },
        {
            path: 'auction',
            name: 'auction',
            redirect: '/chat/auction/auction',
            meta: { title: '聊天室' },
            component: () => import('@/views/chat/index.vue'),
            children: [
                {
                    path: 'auction',
                    meta: { title: '拍卖行' },
                    component: () => import('@/views/chat/auction.vue'),
                },
            ],
        },
        {
            path: 'contacts',
            meta: { title: '联系人' },
            component: () => import('@/views/chat/contacts.vue'),
        },
        {
            path: 'account',
            meta: { title: '账户' },
            component: () => import('@/views/chat/account.vue'),
        },
    ],
} as RouteRecordRaw
