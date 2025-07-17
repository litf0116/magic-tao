import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/dashboard/index.vue'),
    meta: {
      title: '系统概览'
    }
  },
  {
    path: '/performance',
    name: 'Performance',
    component: () => import('@/views/performance/index.vue'),
    meta: {
      title: '性能监控'
    }
  },
  {
    path: '/system',
    name: 'System',
    component: () => import('@/views/system/index.vue'),
    meta: {
      title: '系统资源'
    }
  },
  {
    path: '/errors',
    name: 'Errors',
    component: () => import('@/views/errors/index.vue'),
    meta: {
      title: '错误统计'
    }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router 