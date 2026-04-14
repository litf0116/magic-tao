// pages.config.ts
import { defineUniPages } from '@uni-helper/vite-plugin-uni-pages'

export default defineUniPages({
    easycom: {
        autoscan: true,
        custom: {
            '^(?!z-paging-refresh|z-paging-load-more)z-paging(.*)': 'z-paging/components/z-paging$1/z-paging$1.vue',
            '^uv-(.*)': '@climblee/uv-ui/components/uv-$1/uv-$1.vue',
        },
    },
    // 你也可以定义 pages 字段，它具有最高的优先级。
    pages: [],
    globalStyle: {
        navigationBarTextStyle: 'white',
        navigationBarTitleText: '魔力淘',
        navigationBarBackgroundColor: '#F4835a',
    },
    tabBar: {
        selectedColor: '#F4835a',
        color: '#999999',
        borderStyle: 'black',
        list: [
            {
                pagePath: 'pages/index/index',
                iconPath: 'static/images/tab1_b.png',
                selectedIconPath: 'static/images/tab1.png',
                text: '首页',
            },
            {
                pagePath: 'pages/chat/index',
                iconPath: 'static/images/tab2_b.png',
                selectedIconPath: 'static/images/tab2.png',
                text: '会话列表',
            },
            {
                pagePath: 'pages/chat/contacts',
                iconPath: 'static/images/tab3_b.png',
                selectedIconPath: 'static/images/tab3.png',
                text: '通讯录',
            },
            {
                pagePath: 'pages/index/my',
                iconPath: 'static/images/tab4_b.png',
                selectedIconPath: 'static/images/tab4.png',
                text: '个人中心',
            },
        ],
    },
})
