/**
 * PWA Service Worker
 * 功能：
 * 1. 离线缓存支持
 * 2. 推送消息接收
 * 3. 通知点击处理
 */

const CACHE_NAME = 'molitao-pwa-v20260410'
const API_CACHE_NAME = 'molitao-api-v1'
const STATIC_CACHE_LIST = [
  './',
  './index.html',
  './manifest.webmanifest',
  './static/logo.png',
  './static/icons/icon-192x192.png',
  './static/icons/icon-512x512.png'
]

// 安装事件：缓存静态资源
self.addEventListener('install', (event) => {
  console.log('[SW] Service Worker 安装中...')
  // 立即激活新 SW，不等待旧页面关闭
  self.skipWaiting()
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      console.log('[SW] 缓存静态资源')
      return cache.addAll(STATIC_CACHE_LIST)
    }).then(() => {
      console.log('[SW] Service Worker 安装完成')
    })
  )
})

// 激活事件：清理旧缓存
self.addEventListener('activate', (event) => {
  console.log('[SW] Service Worker 激活')
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames
          .filter((cacheName) => cacheName !== CACHE_NAME && cacheName !== API_CACHE_NAME)
          .map((cacheName) => {
            console.log('[SW] 删除旧缓存:', cacheName)
            return caches.delete(cacheName)
          })
      )
    }).then(() => {
      console.log('[SW] Service Worker 激活完成')
    })
  )
})

// 拦截网络请求：实现离线缓存策略
self.addEventListener('fetch', (event) => {
  const { request } = event
  const url = new URL(request.url)

  // 只处理 GET 请求
  if (request.method !== 'GET') {
    return
  }

  // API 请求使用网络优先策略
  if (url.pathname.startsWith('/api/')) {
    event.respondWith(
      caches.open(API_CACHE_NAME).then(async (cache) => {
        try {
          // 先尝试网络
          const networkResponse = await fetch(request)
          // 缓存响应
          cache.put(request, networkResponse.clone())
          return networkResponse
        } catch (error) {
          // 网络失败，尝试缓存
          const cachedResponse = await cache.match(request)
          if (cachedResponse) {
            console.log('[SW] API 请求使用缓存:', url.pathname)
            return cachedResponse
          }
          // 抛出错误让浏览器显示默认错误页
          throw error
        }
      })
    )
    return
  }

  // 静态资源使用缓存优先策略
  event.respondWith(
    caches.match(request).then((response) => {
      if (response) {
        return response
      }
      return fetch(request).then((networkResponse) => {
        // 缓存新资源
        if (networkResponse.ok) {
          const responseClone = networkResponse.clone()
          caches.open(CACHE_NAME).then((cache) => {
            cache.put(request, responseClone)
          })
        }
        return networkResponse
      })
    })
  )
})

// 推送消息接收
self.addEventListener('push', (event) => {
  console.log('[SW] 收到推送消息:', event)

  try {
    const data = event.data.json()
    console.log('[SW] 推送数据:', data)

    const options = {
      body: data.content || '',
      icon: './static/icons/icon-192x192.png',
      badge: './static/icons/icon-72x72.png',
      vibrate: [200, 100, 200],
      tag: data.messageId || Date.now().toString(),
      data: {
        url: data.url || './',
        messageId: data.messageId,
        extras: data.extras || {}
      },
      actions: [
        { action: 'view', title: '查看' },
        { action: 'close', title: '关闭' }
      ],
      requireInteraction: false
    }

    event.waitUntil(
      self.registration.showNotification(data.title || '魔力淘通知', options)
    )
  } catch (error) {
    console.error('[SW] 推送处理失败:', error)
  }
})

// 通知点击事件
self.addEventListener('notificationclick', (event) => {
  console.log('[SW] 通知被点击:', event)

  event.notification.close()

  if (event.action === 'view') {
    const url = event.notification.data?.url || './'
    console.log('[SW] 跳转到:', url)

    event.waitUntil(
      clients.matchAll({
        type: 'window',
        includeUncontrolled: true
      }).then((clientList) => {
        // 找到或打开一个窗口
        for (const client of clientList) {
          if (client.url === url || client.url === './') {
            client.focus()
            return
          }
        }
        // 没有匹配的窗口，打开新窗口
        return clients.openWindow(url)
      })
    )
  } else if (event.action === 'close') {
    console.log('[SW] 用户关闭了通知')
  }
})

// 后台同步事件（可选，用于后台数据同步）
self.addEventListener('sync', (event) => {
  console.log('[SW] 后台同步:', event.tag)
  // 实现后台数据同步逻辑
})

// 消息处理（来自页面的消息）
self.addEventListener('message', (event) => {
  console.log('[SW] 收到页面消息:', event.data)

  if (event.data && event.data.type === 'SKIP_WAITING') {
    event.waitUntil(
      self.skipWaiting().then(() => {
        console.log('[SW] 跳过等待，立即激活新的 SW')
      })
    )
  }
})
