import { defineStore } from 'pinia'
import { asyncRouter, constantRoutes } from '../routes'
import { RouteRecordRaw } from 'vue-router'
import _ from 'lodash'

const hasPermission = (permissions: string[], roles: string[], route: RouteRecordRaw) => {
    if (route.meta) {
        let b1 = false
        let b2 = false
        if (route.meta.permissions) {
            if (permissions.length) b1 = permissions.some((p) => (route.meta as any).permissions.includes(p))
        } else b1 = true
        if (route.meta.roles) {
            if (roles.length) b2 = roles.some((role) => (route.meta as any).roles.includes(role))
        } else b2 = true
        return b1 && b2
    } else {
        return true
    }
}

export const filterAsyncRoutes = (routes: RouteRecordRaw[], permissions: string[], roles: string[]) => {
    const res: RouteRecordRaw[] = []
    _.forEach(routes, (route: RouteRecordRaw) => {
        const route_copy = { ...route }
        if (hasPermission(permissions, roles, route_copy)) {
            if (route_copy.children && route_copy.children.length) {
                route_copy.children = filterAsyncRoutes(route_copy.children, permissions, roles)
            }
            res.push(route_copy)
        }
    })

    return res
}

export const usePermissionStore = defineStore('permission', () => {
    const routes: Ref<RouteRecordRaw[]> = ref([])

    const dynamicRoutes: Ref<RouteRecordRaw[]> = ref([])

    function generateRoutes(input: { permissions: string[]; roles: string[] }) {
        if (input.permissions !== undefined) {
            const accessedRoutes = filterAsyncRoutes(asyncRouter, input.permissions!, input.roles!)
            routes.value = [...constantRoutes!, ...accessedRoutes]
        }
    }

    const visibleRouter = computed(() => routes.value.filter((x) => x.meta && !x.meta.hidden))

    return {
        routes,
        dynamicRoutes,
        visibleRouter,
        generateRoutes,
    }
})
