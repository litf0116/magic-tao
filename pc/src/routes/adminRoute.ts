import { RouteRecordRaw } from 'vue-router'
// import audit from './modules/audit'

export default {
    path: '/admin',
    redirect: '/admin/dashboard',
    component: () => import('@/layouts/adminLayout.vue'),
    meta: { title: '管理后台', icon: 'House' },
    children: [
        {
            path: '/admin/dashboard',
            meta: { title: 'dashboard', icon: 'House' },
            component: () => import('@/views/dashboard/index.vue'),
        },
        {
            path: '/admin/auction',
            meta: { title: '拍卖管理', icon: 'Monitor' },
            children: [
                {
                    path: 'auctionItem',
                    name: 'auctionItemList',
                    meta: {
                        title: '拍卖列表',
                    },
                    component: () => import('@/views/admin/auction/auctionItem/list.vue'),
                },
                {
                    path: 'auctionItemDeal',
                    name: 'auctionItemDealList',
                    meta: {
                        title: '成交记录',
                    },
                    component: () => import('@/views/admin/auction/auctionItem/dealList.vue'),
                },
                {
                    path: 'bidHistory',
                    name: 'bidHistoryList',
                    meta: {
                        title: '出价历史',
                    },
                    component: () => import('@/views/admin/auction/bidHistory/list.vue'),
                },
            ],
        },
        {
            path: '/admin/website',
            meta: { title: '内容管理', icon: 'Monitor' },
            children: [
                {
                    path: 'chatGroup',
                    name: 'chatGroupList',
                    meta: {
                        title: '群聊列表',
                    },
                    component: () => import('@/views/admin/website/chatGroup/list.vue'),
                },
                {
                    path: 'groupLevelSettings',
                    name: 'groupLevelSettings',
                    meta: {
                        title: '群聊等级',
                    },
                    component: () => import('@/views/admin/website/GroupLevelSettings.vue'),
                },
                {
                    path: 'banedUser',
                    name: 'banedUserList',
                    meta: {
                        title: '禁言用户列表',
                    },
                    component: () => import('@/views/admin/website/banedUser/list.vue'),
                },
                {
                    path: 'advertisement',
                    name: 'advertisementList',
                    meta: {
                        title: '广告管理',
                    },
                    component: () => import('@/views/admin/website/advertisement/list.vue'),
                },
                {
                    path: 'announcement',
                    name: 'announcementList',
                    meta: {
                        title: '公告管理',
                    },
                    component: () => import('@/views/admin/website/announcement/list.vue'),
                },
                {
                    path: 'advertisingSpace',
                    name: 'advertisingSpaceList',
                    meta: {
                        title: '广告位管理',
                    },
                    component: () => import('@/views/admin/advertisingSpace/list.vue'),
                },
            ],
        },
        {
            path: '/admin/system',
            redirect: '/admin/system/user',
            meta: { title: '系统管理', icon: 'Setting' },
            children: [
                {
                    path: 'sensitiveWord',
                    name: 'sensitiveWordList',
                    meta: {
                        title: '敏感词管理',
                    },
                    component: () => import('@/views/admin/system/sensitiveWord/list.vue'),
                },
                {
                    path: 'user',
                    name: 'userList',
                    meta: {
                        title: '用户列表',
                    },
                    component: () => import('@/views/admin/system/user/list.vue'),
                },
                {
                    path: 'role',
                    name: 'roleList',
                    meta: {
                        title: '角色列表',
                    },
                    component: () => import('@/views/admin/system/role/list.vue'),
                },
                // {
                //     path: 'test',
                //     name: 'Test',
                //     meta: {
                //         title: 'Test',
                //     },
                //     component: () => import('@/views/admin/index.vue'),
                // },
            ],
        },
        {
            path: '/admin/postBar',
            meta: { title: '贴吧管理', icon: 'Monitor' },
            children: [
                {
                    path: 'postBarBulletin',
                    name: 'bulletinList',
                    meta: {
                        title: '贴吧公告',
                    },
                    component: () => import('@/views/admin/postBar/bulletin/list.vue'),
                },
                {
                    path: 'category',
                    name: 'categoryList',
                    meta: {
                        title: '类型管理',
                    },
                    component: () => import('@/views/admin/postBar/category/list.vue'),
                },
                {
                    path: 'post',
                    name: 'postList',
                    meta: {
                        title: '帖子管理',
                    },
                    component: () => import('@/views/admin/postBar/post/list.vue'),
                },
                {
                    path: 'hotWords',
                    name: 'hotWordsList',
                    meta: {
                        title: '热词管理',
                    },
                    component: () => import('@/views/admin/postBar/hotWords/list.vue'),
                },
            ],
        },
        // ...audit,
    ],
} as RouteRecordRaw
