export default [
    {
        path: 'audit/add',
        name: 'AuditAdd',
        component: () => import('@/views/admin/audit/edit.vue'),
        meta: {
            title: '创建审核流程',
            permissions: ['Pages.Administration'],
            icon: 'i-icon-park-audit',
        },
        props: (route: any) => ({
            name: route.query.name,
            providerName: route.query.providerName,
            providerKey: route.query.providerKey,
        }),
    },
    {
        path: 'audit/list',
        name: 'AuditList',
        component: () => import('@/views/admin/audit/list.vue'),
        meta: {
            title: '审核流程列表',
            permissions: ['Pages.Administration'],
        },
    },
    {
        path: 'audit/edit/:id',
        name: 'AuditEdit',
        component: () => import('@/views/admin/audit/edit.vue'),
        meta: {
            title: '编辑表单',
            hidden: true,
            permissions: ['Pages.Administration'],
        },
    },
]
