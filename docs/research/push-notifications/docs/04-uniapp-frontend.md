# UniApp 前端实现方案

## 📱 UniApp 推送通知概述

UniApp 提供了统一的推送通知 API，支持 iOS 和 Android 两个平台。通过官方插件和第三方推送服务（如极光推送、个推），可以实现跨平台的推送通知功能。

### 核心特性

- **跨平台**: 一套代码同时支持 iOS 和 Android
- **插件生态**: 官方推送插件 + 第三方推送服务
- **权限管理**: 自动处理平台权限申请
- **消息处理**: 统一的消息接收和处理接口
- **离线推送**: 支持 App 在后台或关闭时的推送

## 🔧 推送服务选择

### 方案对比

| 方案 | 优点 | 缺点 | 适用场景 |
|------|------|------|----------|
| **UniPush 2.0** | 官方支持，集成简单 | 依赖 DCloud 服务 | 国内业务为主 |
| **极光推送** | 功能完善，文档齐全 | 需要第三方账号 | 功能需求复杂 |
| **个推** | 厂商通道保活率高 | 配置较复杂 | 重视到达率 |
| **原生集成** | 完全自主可控 | 开发成本高 | 特殊需求 |

### 推荐方案

基于项目需求，推荐以下方案：

1. **国内版本**: 使用 UniPush 2.0（DCloud 官方服务）
2. **海外版本**: 分别集成 APNs + FCM
3. **统一方案**: 使用极光推送（同时支持海内外）

## 📦 安装配置

### UniPush 2.0 配置

#### 1. 开通 UniPush 服务

1. 登录 [DCloud 开发者中心](https://dev.dcloud.net.cn/)
2. 创建应用
3. 开通 UniPush 2.0 服务
4. 获取 AppID、AppKey、AppSecret

#### 2. manifest.json 配置

```json
{
  "app-plus": {
    "distribute": {
      "android": {
        "permissions": [
          "<uses-permission android:name=\"android.permission.RECEIVE_USER_PRESENT\"/>",
          "<uses-permission android:name=\"android.permission.VIBRATE\"/>"
        ],
        "push": {
          "unipush": {
            "enable": true
          }
        }
      },
      "ios": {
        "idfa": true,
        "push": {
          "unipush": {
            "enable": true
          }
        }
      }
    }
  },
  "mp-weixin": {
    // 微信小程序配置
  },
  "h5": {
    // H5 配置
  }
}
```

#### 3. pages.json 配置

```json
{
  "globalStyle": {
    "navigationBarTextStyle": "black",
    "navigationBarTitleText": "拍卖应用"
  }
}
```

### 极光推送配置

#### 1. 注册极光推送账号

1. 访问 [极光推送官网](https://www.jiguang.cn/)
2. 注册账号并创建应用
3. 配置 Android 和 iOS 推送
4. 获取 AppKey

#### 2. 安装插件

在 HBuilderX 中：
1. 打开插件市场
2. 搜索 "JG-JPush"
3. 导入插件到项目

#### 3. manifest.json 配置

```json
{
  "app-plus": {
    "distribute": {
      "android": {
        "plugins": {
          "JG-JPush": {
            "appkey": "YOUR_JPUSH_APPKEY"
          }
        }
      },
      "ios": {
        "plugins": {
          "JG-JPush": {
            "appkey": "YOUR_JPUSH_APPKEY"
          }
        }
      }
    }
  }
}
```

## 🎯 推送通知实现

### UniPush 2.0 实现

#### 1. 初始化推送

```typescript
// utils/push.ts
import type { JPushMessage } from '@/types/push'

class PushService {
  private clientId: string = ''
  private isReady: boolean = false

  /**
   * 初始化推送服务
   */
  async init(): Promise<void> {
    // #ifdef APP-PLUS
    return new Promise((resolve, reject) => {
      const pushManager = uni.getPushManager()

      pushManager.on('register', (result: any) => {
        console.log('Push registered:', result)
        this.clientId = result.cid
        this.isReady = true
        
        // 上传到后端
        this.uploadClientId(result.cid)
        resolve()
      })

      pushManager.on('message', (result: JPushMessage) => {
        console.log('Push message received:', result)
        this.handleMessage(result)
      })

      pushManager.on('click', (result: any) => {
        console.log('Push message clicked:', result)
        this.handleClick(result)
      })

      pushManager.on('error', (error: any) => {
        console.error('Push error:', error)
        reject(error)
      })

      // 启动推送
      pushManager.start()
    })
    // #endif

    // #ifndef APP-PLUS
    console.warn('Push is only available in APP')
    return Promise.resolve()
    // #endif
  }

  /**
   * 上传 Client ID 到后端
   */
  private async uploadClientId(clientId: string): Promise<void> {
    try {
      const platform = uni.getSystemInfoSync().platform === 'ios' ? 'iOS' : 'Android'
      
      await uni.request({
        url: `${import.meta.env.VITE_API_URL}/api/push/device-token/register`,
        method: 'POST',
        header: {
          'Authorization': `Bearer ${uni.getStorageSync('token')}`
        },
        data: {
          deviceToken: clientId,
          platform: platform,
          deviceInfo: JSON.stringify(uni.getSystemInfoSync())
        }
      })

      console.log('Client ID uploaded successfully')
    } catch (error) {
      console.error('Failed to upload client ID:', error)
    }
  }

  /**
   * 处理接收到的消息
   */
  private handleMessage(message: JPushMessage): void {
    const { title, content, extras } = message

    // 展示本地通知
    uni.showNotification({
      title: title || '新消息',
      content: content || '',
      success: () => {
        console.log('Notification shown')
      }
    })

    // 触发自定义事件
    uni.$emit('pushMessage', message)

    // 根据消息类型处理
    if (extras?.type) {
      this.handleMessageType(extras.type, extras)
    }
  }

  /**
   * 处理消息点击
   */
  private handleClick(result: any): void {
    const { extras } = result

    // 根据消息类型导航到对应页面
    if (extras?.type) {
      this.navigateToPage(extras)
    }
  }

  /**
   * 根据消息类型处理
   */
  private handleMessageType(type: string, data: any): void {
    switch (type) {
      case 'bid_placed':
        // 出价成功
        break
      case 'new_bid':
        // 新出价提醒
        break
      case 'auction_ended':
        // 拍卖结束
        break
      default:
        break
    }
  }

  /**
   * 导航到对应页面
   */
  private navigateToPage(data: any): void {
    switch (data.type) {
      case 'bid_placed':
      case 'new_bid':
        uni.navigateTo({
          url: `/pages/auction/detail?id=${data.auctionId}`
        })
        break
      case 'auction_ended':
        uni.navigateTo({
          url: `/pages/auction/result?id=${data.auctionId}`
        })
        break
      default:
        break
    }
  }

  /**
   * 获取 Client ID
   */
  getClientId(): string {
    return this.clientId
  }

  /**
   * 检查是否已初始化
   */
  isPushReady(): boolean {
    return this.isReady
  }

  /**
   * 设置别名（用于定向推送）
   */
  async setAlias(alias: string): Promise<void> {
    // #ifdef APP-PLUS
    const pushManager = uni.getPushManager()
    await pushManager.setAlias(alias)
    // #endif
  }

  /**
   * 设置标签（用于分组推送）
   */
  async setTags(tags: string[]): Promise<void> {
    // #ifdef APP-PLUS
    const pushManager = uni.getPushManager()
    await pushManager.setTags(tags)
    // #endif
  }

  /**
   * 清除通知
   */
  async clearAllNotifications(): Promise<void> {
    // #ifdef APP-PLUS
    const pushManager = uni.getPushManager()
    await pushManager.clearAllNotifications()
    // #endif
  }
}

// 导出单例
export const pushService = new PushService()
```

#### 2. 在 App.vue 中初始化

```vue
<script setup lang="ts">
import { onLaunch, onShow } from '@dcloudio/uni-app'
import { pushService } from '@/utils/push'

onLaunch(() => {
  console.log('App Launch')
  
  // 初始化推送服务
  pushService.init().catch(error => {
    console.error('Failed to init push service:', error)
  })
})

onShow(() => {
  console.log('App Show')
})
</script>

<style>
/* 全局样式 */
</style>
```

### 极光推送实现

#### 1. 初始化极光推送

```typescript
// utils/jpush.ts
import type { JPushMessage } from '@/types/push'

class JPushService {
  private clientId: string = ''
  private isReady: boolean = false

  /**
   * 初始化极光推送
   */
  async init(): Promise<void> {
    // #ifdef APP-PLUS
    return new Promise((resolve, reject) => {
      // 初始化 JPush
      const jpush = plus.jpush

      jpush.init()

      // 监听连接状态
      jpush.addEventListener('connectStatusChange', (result: any) => {
        console.log('JPush connect status:', result)
        if (result.code === 0) {
          this.isReady = true
          resolve()
        }
      })

      // 监听自定义消息
      jpush.addEventListener('customMessage', (message: JPushMessage) => {
        console.log('JPush custom message:', message)
        this.handleCustomMessage(message)
      })

      // 监听通知点击
      jpush.addEventListener('notificationClick', (message: JPushMessage) => {
        console.log('JPush notification clicked:', message)
        this.handleNotificationClick(message)
      })

      // 监听通知接收
      jpush.addEventListener('notificationReceive', (message: JPushMessage) => {
        console.log('JPush notification received:', message)
        this.handleNotificationReceive(message)
      })

      // 监听注册成功
      jpush.addEventListener('register', (result: any) => {
        console.log('JPush registered:', result)
        this.clientId = result.registrationId
        
        // 上传到后端
        this.uploadRegistrationId(result.registrationId)
      })

      // 设置别名和标签
      this.setUserInfo()

      // 启动推送
      jpush.startPush()
    })
    // #endif

    // #ifndef APP-PLUS
    console.warn('JPush is only available in APP')
    return Promise.resolve()
    // #endif
  }

  /**
   * 上传 Registration ID 到后端
   */
  private async uploadRegistrationId(registrationId: string): Promise<void> {
    try {
      const platform = uni.getSystemInfoSync().platform === 'ios' ? 'iOS' : 'Android'
      
      await uni.request({
        url: `${import.meta.env.VITE_API_URL}/api/push/device-token/register`,
        method: 'POST',
        header: {
          'Authorization': `Bearer ${uni.getStorageSync('token')}`
        },
        data: {
          deviceToken: registrationId,
          platform: platform,
          deviceInfo: JSON.stringify(uni.getSystemInfoSync())
        }
      })

      console.log('Registration ID uploaded successfully')
    } catch (error) {
      console.error('Failed to upload registration ID:', error)
    }
  }

  /**
   * 处理自定义消息
   */
  private handleCustomMessage(message: JPushMessage): void {
    const { extras } = message

    // 触发自定义事件
    uni.$emit('jpushCustomMessage', message)

    // 根据消息类型处理
    if (extras?.type) {
      this.handleMessageType(extras.type, extras)
    }
  }

  /**
   * 处理通知点击
   */
  private handleNotificationClick(message: JPushMessage): void {
    const { extras } = message

    // 导航到对应页面
    if (extras?.type) {
      this.navigateToPage(extras)
    }
  }

  /**
   * 处理通知接收
   */
  private handleNotificationReceive(message: JPushMessage): void {
    const { title, content, extras } = message

    // 触发自定义事件
    uni.$emit('jpushNotification', message)
  }

  /**
   * 设置别名和标签
   */
  private setUserInfo(): void {
    const userId = uni.getStorageSync('userId')
    if (userId) {
      // #ifdef APP-PLUS
      const jpush = plus.jpush
      jpush.setAlias({ alias: userId.toString() })
      jpush.setTags({ tags: ['auction'] })
      // #endif
    }
  }

  /**
   * 根据消息类型处理
   */
  private handleMessageType(type: string, data: any): void {
    switch (type) {
      case 'bid_placed':
        // 出价成功
        break
      case 'new_bid':
        // 新出价提醒
        break
      case 'auction_ended':
        // 拍卖结束
        break
      default:
        break
    }
  }

  /**
   * 导航到对应页面
   */
  private navigateToPage(data: any): void {
    switch (data.type) {
      case 'bid_placed':
      case 'new_bid':
        uni.navigateTo({
          url: `/pages/auction/detail?id=${data.auctionId}`
        })
        break
      case 'auction_ended':
        uni.navigateTo({
          url: `/pages/auction/result?id=${data.auctionId}`
        })
        break
      default:
        break
    }
  }

  /**
   * 获取 Registration ID
   */
  getRegistrationId(): string {
    return this.clientId
  }

  /**
   * 检查是否已初始化
   */
  isReady(): boolean {
    return this.isReady
  }

  /**
   * 设置别名
   */
  async setAlias(alias: string): Promise<void> {
    // #ifdef APP-PLUS
    const jpush = plus.jpush
    await jpush.setAlias({ alias })
    // #endif
  }

  /**
   * 设置标签
   */
  async setTags(tags: string[]): Promise<void> {
    // #ifdef APP-PLUS
    const jpush = plus.jpush
    await jpush.setTags({ tags })
    // #endif
  }

  /**
   * 清除所有通知
   */
  async clearAllNotifications(): Promise<void> {
    // #ifdef APP-PLUS
    const jpush = plus.jpush
    await jpush.clearAllNotifications()
    // #endif
  }

  /**
   * 获取通知权限状态
   */
  async getNotificationPermission(): Promise<boolean> {
    // #ifdef APP-PLUS
    const jpush = plus.jpush
    const result = await jpush.getNotificationPermission()
    return result.isEnable
    // #endif

    // #ifndef APP-PLUS
    return false
    // #endif
  }

  /**
   * 请求通知权限
   */
  async requestNotificationPermission(): Promise<boolean> {
    // #ifdef APP-PLUS
    const jpush = plus.jpush
    const result = await jpush.requestNotificationPermission()
    return result.isEnable
    // #endif

    // #ifndef APP-PLUS
    return false
    // #endif
  }
}

// 导出单例
export const jpushService = new JPushService()
```

## 🔐 权限管理

### iOS 权限申请

```typescript
// 在 App.vue 中申请权限
async requestNotificationPermission(): Promise<void> {
  // #ifdef APP-PLUS
  const jpush = plus.jpush
  
  // iOS 13+ 需要申请通知权限
  if (uni.getSystemInfoSync().platform === 'ios') {
    const result = await jpush.requestNotificationPermission()
    
    if (!result.isEnable) {
      uni.showModal({
        title: '通知权限',
        content: '请在设置中开启通知权限，以便接收重要消息',
        confirmText: '去设置',
        success: (res) => {
          if (res.confirm) {
            plus.runtime.openURL('app-settings:')
          }
        }
      })
    }
  }
  // #endif
}
```

### Android 权限申请

```typescript
// Android 13+ 需要申请 POST_NOTIFICATIONS 权限
async requestAndroidPermission(): Promise<void> {
  // #ifdef APP-PLUS
  if (uni.getSystemInfoSync().platform === 'android') {
    const androidVersion = uni.getSystemInfoSync().system.split(' ')[1]
    
    if (parseInt(androidVersion) >= 13) {
      const permission = 'android.permission.POST_NOTIFICATIONS'
      const result = await new Promise((resolve) => {
        plus.android.requestPermissions([permission], (e: any) => {
          if (e.granted && e.granted.length > 0) {
            resolve(true)
          } else {
            resolve(false)
          }
        })
      })

      if (!result) {
        uni.showModal({
          title: '通知权限',
          content: '请在设置中开启通知权限，以便接收重要消息',
          confirmText: '去设置',
          success: (res) => {
            if (res.confirm) {
              plus.runtime.openURL('app-settings:')
            }
          }
        })
      }
    }
  }
  // #endif
}
```

## 📨 消息处理

### 在页面中监听推送消息

```vue
<template>
  <view class="auction-detail">
    <!-- 页面内容 -->
  </view>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { pushService } from '@/utils/push'

// 监听推送消息
const handlePushMessage = (message: any) => {
  console.log('Received push message:', message)
  
  // 刷新页面数据
  if (message.extras?.auctionId === auctionId.value) {
    loadAuctionDetail()
  }
}

onMounted(() => {
  // 监听推送消息
  uni.$on('pushMessage', handlePushMessage)
})

onUnmounted(() => {
  // 移除监听
  uni.$off('pushMessage', handlePushMessage)
})
</script>
```

### 处理不同类型的消息

```typescript
// types/push.ts
export interface PushMessage {
  title?: string
  content?: string
  extras?: {
    type: string
    auctionId?: string
    itemId?: string
    amount?: number
    [key: string]: any
  }
}

// 拍卖出价消息处理器
export class AuctionMessageHandler {
  /**
   * 处理出价消息
   */
  static handleBidMessage(message: PushMessage): void {
    const { type, auctionId } = message.extras || {}

    switch (type) {
      case 'bid_placed':
        this.showBidSuccessNotification(message)
        break
      case 'new_bid':
        this.showNewBidNotification(message)
        break
      case 'outbid':
        this.showOutbidNotification(message)
        break
      case 'auction_ending':
        this.showAuctionEndingNotification(message)
        break
      case 'auction_ended':
        this.showAuctionEndedNotification(message)
        break
    }
  }

  /**
   * 显示出价成功通知
   */
  private static showBidSuccessNotification(message: PushMessage): void {
    uni.showToast({
      title: '出价成功',
      icon: 'success'
    })

    // 更新页面数据
    uni.$emit('auctionBidPlaced', message.extras)
  }

  /**
   * 显示新出价通知
   */
  private static showNewBidNotification(message: PushMessage): void {
    uni.showModal({
      title: '新出价提醒',
      content: message.content || '您关注的拍品刚刚有新出价',
      confirmText: '查看',
      success: (res) => {
        if (res.confirm && message.extras?.auctionId) {
          uni.navigateTo({
            url: `/pages/auction/detail?id=${message.extras.auctionId}`
          })
        }
      }
    })
  }

  /**
   * 显示被超出价通知
   */
  private static showOutbidNotification(message: PushMessage): void {
    uni.showModal({
      title: '出价被超越',
      content: '您的出价已被其他买家超越',
      confirmText: '查看',
      success: (res) => {
        if (res.confirm && message.extras?.auctionId) {
          uni.navigateTo({
            url: `/pages/auction/detail?id=${message.extras.auctionId}`
          })
        }
      }
    })
  }

  /**
   * 显示拍卖即将结束通知
   */
  private static showAuctionEndingNotification(message: PushMessage): void {
    uni.showModal({
      title: '拍卖即将结束',
      content: '您关注的拍品即将结束拍卖',
      confirmText: '查看',
      success: (res) => {
        if (res.confirm && message.extras?.auctionId) {
          uni.navigateTo({
            url: `/pages/auction/detail?id=${message.extras.auctionId}`
          })
        }
      }
    })
  }

  /**
   * 显示拍卖结束通知
   */
  private static showAuctionEndedNotification(message: PushMessage): void {
    uni.showModal({
      title: '拍卖已结束',
      content: '您关注的拍品拍卖已结束',
      confirmText: '查看结果',
      success: (res) => {
        if (res.confirm && message.extras?.auctionId) {
          uni.navigateTo({
            url: `/pages/auction/result?id=${message.extras.auctionId}`
          })
        }
      }
    })
  }
}
```

## 🎯 使用示例

### 在拍卖出价页面中使用

```vue
<template>
  <view class="bid-page">
    <button @click="placeBid">出价</button>
  </view>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { pushService } from '@/utils/push'
import { AuctionMessageHandler } from '@/types/push'

const bidAmount = ref(10000)

// 监听推送消息
const handlePushMessage = (message: any) => {
  AuctionMessageHandler.handleBidMessage(message)
}

// 出价
const placeBid = async () => {
  try {
    await uni.request({
      url: `${import.meta.env.VITE_API_URL}/api/auction/bid`,
      method: 'POST',
      header: {
        'Authorization': `Bearer ${uni.getStorageSync('token')}`
      },
      data: {
        auctionId: auctionId.value,
        amount: bidAmount.value
      }
    })

    uni.showToast({
      title: '出价成功',
      icon: 'success'
    })
  } catch (error) {
    uni.showToast({
      title: '出价失败',
      icon: 'none'
    })
  }
}

onMounted(() => {
  // 监听推送消息
  uni.$on('pushMessage', handlePushMessage)
})

onUnmounted(() => {
  // 移除监听
  uni.$off('pushMessage', handlePushMessage)
})
</script>
```

## 📊 调试与测试

### 测试推送功能

```typescript
// 在开发环境中测试推送
if (import.meta.env.DEV) {
  // 模拟推送消息
  setTimeout(() => {
    uni.$emit('pushMessage', {
      title: '测试推送',
      content: '这是一条测试推送消息',
      extras: {
        type: 'new_bid',
        auctionId: '12345',
        amount: 10000
      }
    })
  }, 3000)
}
```

### 查看推送日志

```typescript
// 开启推送调试日志
// #ifdef APP-PLUS
const jpush = plus.jpush
jpush.setDebugMode(true)
// #endif
```

## 🔧 常见问题

### 1. iOS 收不到推送

- 检查是否正确配置了 APNs 证书
- 检查是否申请了通知权限
- 检查设备是否在线
- 检查推送证书是否过期

### 2. Android 收不到推送

- 检查是否正确配置了 Firebase 或极光推送
- 检查是否申请了通知权限（Android 13+）
- 检查设备是否在线
- 检查厂商通道配置

### 3. 推送延迟

- 检查网络连接
- 检查推送队列是否拥堵
- 优化推送服务配置

### 4. Token 失效

- 定期检查 Token 有效性
- 重新注册 Token
- 上传新 Token 到后端

## 🔗 参考资料

- [UniApp 官方文档 - 推送](https://uniapp.dcloud.net.cn/api/plugins/push.html)
- [UniPush 2.0 文档](https://uniapp.dcloud.net.cn/unipush.html)
- [极光推送 UniApp 插件](https://ext.dcloud.net.cn/plugin?id=567)
- [极光推送官方文档](https://docs.jiguang.cn/)
