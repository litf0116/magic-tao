# 推送通知权限管理

## 🔐 权限概述

推送通知需要用户授权才能正常工作。iOS 和 Android 平台对通知权限有不同的管理机制。

### 权限类型

| 平台 | 权限类型 | 必需性 | 说明 |
|------|----------|--------|------|
| iOS | `UNAuthorization` | ✅ 必需 | 用户必须明确授权 |
| Android (13+) | `POST_NOTIFICATIONS` | ✅ 必需 | 用户必须明确授权 |
| Android (<13) | `VIBRATE` | ❌ 可选 | 震动权限 |
| Android | `WAKE_LOCK` | ❌ 可选 | 唤醒锁权限 |

## 📱 iOS 权限管理

### 权限申请流程

```
1. App 启动
   ↓
2. 检查权限状态
   ↓
3. 弹出授权对话框
   ↓
4. 用户选择
   ├─ 授权 → 开始接收推送
   └─ 拒绝 → 引导用户去设置开启
```

### 权限状态枚举

```swift
import UserNotifications

enum AuthorizationStatus {
    case notDetermined      // 未确定（首次）
    case denied            // 已拒绝
    case authorized        // 已授权
    case provisional       // 临时授权（iOS 14+）
    case ephemeral         // 临时授权（iOS 15+）
}
```

### 权限申请实现

#### Swift (iOS 端)

```swift
import UserNotifications

class PushPermissionManager {
    
    /// 获取当前权限状态
    func getAuthorizationStatus() -> UNAuthorizationStatus {
        let center = UNUserNotificationCenter.current()
        var settings: UNNotificationSettings?
        let semaphore = DispatchSemaphore(value: 0)
        
        center.getNotificationSettings { currentSettings in
            settings = currentSettings
            semaphore.signal()
        }
        
        semaphore.wait()
        return settings?.authorizationStatus ?? .notDetermined
    }
    
    /// 申请通知权限
    func requestAuthorization(completion: @escaping (Bool) -> Void) {
        let center = UNUserNotificationCenter.current()
        
        center.requestAuthorization(options: [.alert, .sound, .badge]) { granted, error in
            if let error = error {
                print("Authorization error: \(error)")
                completion(false)
                return
            }
            
            if granted {
                DispatchQueue.main.async {
                    UIApplication.shared.registerForRemoteNotifications()
                }
                print("Authorization granted")
                completion(true)
            } else {
                print("Authorization denied")
                completion(false)
            }
        }
    }
    
    /// 检查是否已授权
    func isAuthorized() -> Bool {
        let status = getAuthorizationStatus()
        return status == .authorized || 
               status == .provisional || 
               status == .ephemeral
    }
    
    /// 引导用户去设置开启权限
    func openSettings() {
        if let url = URL(string: UIApplication.openSettingsURLString) {
            UIApplication.shared.open(url)
        }
    }
    
    /// 显示权限提示对话框
    func showPermissionPrompt(on viewController: UIViewController) {
        let alert = UIAlertController(
            title: "开启通知权限",
            message: "开启通知权限可以及时接收拍卖出价、拍卖结束等重要消息",
            preferredStyle: .alert
        )
        
        alert.addAction(UIAlertAction(title: "暂不开启", style: .default))
        alert.addAction(UIAlertAction(title: "去开启", style: .default) { _ in
            self.requestAuthorization { granted in
                if !granted {
                    self.showPermissionDeniedPrompt(on: viewController)
                }
            }
        })
        
        viewController.present(alert, animated: true)
    }
    
    /// 显示权限被拒绝提示
    func showPermissionDeniedPrompt(on viewController: UIViewController) {
        let alert = UIAlertController(
            title: "通知权限已关闭",
            message: "请前往设置开启通知权限，以便接收重要消息",
            preferredStyle: .alert
        )
        
        alert.addAction(UIAlertAction(title: "取消", style: .default))
        alert.addAction(UIAlertAction(title: "去设置", style: .default) { _ in
            self.openSettings()
        })
        
        viewController.present(alert, animated: true)
    }
}
```

### 在 AppDelegate 中使用

```swift
import UIKit
import UserNotifications

@main
class AppDelegate: UIResponder, UIApplicationDelegate {

    func application(_ application: UIApplication, 
                     didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        
        // 配置推送代理
        UNUserNotificationCenter.current().delegate = self
        
        // 检查权限
        let permissionManager = PushPermissionManager()
        if permissionManager.isAuthorized() {
            // 已授权，注册推送
            application.registerForRemoteNotifications()
        } else {
            // 未授权，延迟申请权限
            DispatchQueue.main.asyncAfter(deadline: .now() + 2.0) {
                if let windowScene = application.connectedScenes.first as? UIWindowScene,
                   let rootViewController = windowScene.windows.first?.rootViewController {
                    permissionManager.showPermissionPrompt(on: rootViewController)
                }
            }
        }
        
        return true
    }
    
    // 注册推送成功
    func application(_ application: UIApplication, 
                     didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
        let token = deviceToken.map { String(format: "%02.2hhx", $0) }.joined()
        print("Device Token: \(token)")
        
        // 上传到后端
        uploadDeviceToken(token: token)
    }
    
    // 注册推送失败
    func application(_ application: UIApplication, 
                     didFailToRegisterForRemoteNotificationsWithError error: Error) {
        print("Failed to register for remote notifications: \(error)")
    }
}

extension AppDelegate: UNUserNotificationCenterDelegate {
    
    // 前台收到通知
    func userNotificationCenter(_ center: UNUserNotificationCenter, 
                               willPresent notification: UNNotification, 
                               withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        
        // 在前台也显示通知
        completionHandler([.banner, .sound, .badge])
    }
    
    // 点击通知
    func userNotificationCenter(_ center: UNUserNotificationCenter, 
                               didReceive response: UNNotificationResponse, 
                               withCompletionHandler completionHandler: @escaping () -> Void) {
        
        let userInfo = response.notification.request.content.userInfo
        print("Notification clicked: \(userInfo)")
        
        // 处理点击事件
        handleNotificationClick(userInfo)
        
        completionHandler()
    }
    
    private func handleNotificationClick(_ userInfo: [AnyHashable: Any]) {
        // 根据消息类型导航到对应页面
        guard let type = userInfo["type"] as? String else { return }
        
        switch type {
        case "bid_placed", "new_bid":
            if let auctionId = userInfo["auctionId"] as? String {
                navigateToAuctionDetail(auctionId: auctionId)
            }
        case "auction_ended":
            if let auctionId = userInfo["auctionId"] as? String {
                navigateToAuctionResult(auctionId: auctionId)
            }
        default:
            break
        }
    }
    
    private func navigateToAuctionDetail(auctionId: String) {
        // 导航到拍卖详情页
        // ...
    }
    
    private func navigateToAuctionResult(auctionId: String) {
        // 导航到拍卖结果页
        // ...
    }
    
    private func uploadDeviceToken(token: String) {
        // 上传到后端 API
        // ...
    }
}
```

## 🤖 Android 权限管理

### 权限申请流程

```
1. App 启动
   ↓
2. 检查权限状态
   ↓
3. 弹出授权对话框（Android 13+）
   ↓
4. 用户选择
   ├─ 授权 → 开始接收推送
   └─ 拒绝 → 引导用户去设置开启
```

### 权限申请实现

#### Kotlin (Android 端)

```kotlin
import android.Manifest
import android.app.NotificationManager
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import android.provider.Settings
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat

class PushPermissionManager(private val context: Context) {
    
    companion object {
        private const val REQUEST_NOTIFICATION_PERMISSION = 1001
    }
    
    /// 检查是否已授权
    fun isAuthorized(): Boolean {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            return ContextCompat.checkSelfPermission(
                context,
                Manifest.permission.POST_NOTIFICATIONS
            ) == PackageManager.PERMISSION_GRANTED
        }
        return true // Android 13 以下不需要权限
    }
    
    /// 申请通知权限
    fun requestPermission(activity: AppCompatActivity, callback: (Boolean) -> Unit) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            when {
                ContextCompat.checkSelfPermission(
                    context,
                    Manifest.permission.POST_NOTIFICATIONS
                ) == PackageManager.PERMISSION_GRANTED -> {
                    // 已授权
                    callback(true)
                }
                
                ActivityCompat.shouldShowRequestPermissionRationale(
                    activity,
                    Manifest.permission.POST_NOTIFICATIONS
                ) -> {
                    // 用户之前拒绝过，显示解释
                    showPermissionRationale(activity, callback)
                }
                
                else -> {
                    // 首次申请
                    requestPermissionLauncher.launch(callback)
                }
            }
        } else {
            // Android 13 以下不需要权限
            callback(true)
        }
    }
    
    /// 显示权限解释对话框
    private fun showPermissionRationale(
        activity: AppCompatActivity,
        callback: (Boolean) -> Unit
    ) {
        AlertDialog.Builder(activity)
            .setTitle("需要通知权限")
            .setMessage("开启通知权限可以及时接收拍卖出价、拍卖结束等重要消息")
            .setPositiveButton("去开启") { _, _ ->
                requestPermissionLauncher.launch(callback)
            }
            .setNegativeButton("取消", null)
            .show()
    }
    
    /// 权限请求 Launcher
    private val requestPermissionLauncher = activity.registerForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        callback?.invoke(isGranted)
    }
    
    private var callback: ((Boolean) -> Unit)? = null
    
    /// 检查通知是否启用
    fun areNotificationsEnabled(): Boolean {
        val notificationManager = context.getSystemService(Context.NOTIFICATION_SERVICE) 
            as NotificationManager
        
        return notificationManager.areNotificationsEnabled()
    }
    
    /// 检查特定渠道的通知是否启用
    fun isChannelEnabled(channelId: String): Boolean {
        val notificationManager = context.getSystemService(Context.NOTIFICATION_SERVICE) 
            as NotificationManager
        
        val channel = notificationManager.getNotificationChannel(channelId)
        return channel?.importance != NotificationManager.IMPORTANCE_NONE
    }
    
    /// 引导用户去设置开启权限
    fun openSettings() {
        val intent = Intent().apply {
            action = Settings.ACTION_APPLICATION_DETAILS_SETTINGS
            data = Uri.fromParts("package", context.packageName, null)
        }
        context.startActivity(intent)
    }
    
    /// 显示权限被拒绝提示
    fun showPermissionDeniedDialog(activity: AppCompatActivity) {
        AlertDialog.Builder(activity)
            .setTitle("通知权限已关闭")
            .setMessage("请前往设置开启通知权限，以便接收重要消息")
            .setPositiveButton("去设置") { _, _ ->
                openSettings()
            }
            .setNegativeButton("取消", null)
            .show()
    }
}
```

### 在 MainActivity 中使用

```kotlin
import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity

class MainActivity : AppCompatActivity() {
    
    private lateinit var permissionManager: PushPermissionManager
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        
        permissionManager = PushPermissionManager(this)
        
        // 检查权限
        checkNotificationPermission()
    }
    
    private fun checkNotificationPermission() {
        if (permissionManager.isAuthorized()) {
            // 已授权，初始化 FCM
            initFirebaseMessaging()
        } else {
            // 未授权，延迟申请权限
            Handler(Looper.getMainLooper()).postDelayed({
                permissionManager.requestPermission(this) { granted ->
                    if (granted) {
                        initFirebaseMessaging()
                    } else {
                        permissionManager.showPermissionDeniedDialog(this)
                    }
                }
            }, 2000)
        }
    }
    
    private fun initFirebaseMessaging() {
        // 初始化 FCM
        FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
            if (task.isSuccessful) {
                val token = task.result
                Log.d("FCM", "Firebase token: $token")
                
                // 上传到后端
                uploadRegistrationToken(token)
            }
        }
    }
    
    private fun uploadRegistrationToken(token: String) {
        // 上传到后端 API
        // ...
    }
    
    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        
        when (requestCode) {
            REQUEST_NOTIFICATION_PERMISSION -> {
                if (grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                    // 权限已授予
                    initFirebaseMessaging()
                } else {
                    // 权限被拒绝
                    permissionManager.showPermissionDeniedDialog(this)
                }
            }
        }
    }
}
```

## 🔒 UniApp 权限管理

### 权限申请实现

#### TypeScript (UniApp 端)

```typescript
// utils/permission.ts
class PushPermissionManager {
  /**
   * 检查通知权限状态
   */
  async checkPermission(): Promise<boolean> {
    // #ifdef APP-PLUS
    const platform = uni.getSystemInfoSync().platform

    if (platform === 'ios') {
      const status = await this.getIOSAuthorizationStatus()
      return status === 'authorized' || status === 'provisional'
    } else if (platform === 'android') {
      return this.getAndroidPermission()
    }
    // #endif

    return false
  }

  /**
   * 获取 iOS 权限状态
   */
  private async getIOSAuthorizationStatus(): Promise<string> {
    // #ifdef APP-PLUS
    return new Promise((resolve) => {
      const JPush = plus.jpush
      JPush.getAuthorizationStatus((status: any) => {
        resolve(status.status)
      })
    })
    // #endif

    return 'notDetermined'
  }

  /**
   * 获取 Android 权限状态
   */
  private getAndroidPermission(): boolean {
    // #ifdef APP-PLUS
    const androidVersion = uni.getSystemInfoSync().system.split(' ')[1]
    
    if (parseInt(androidVersion) >= 13) {
      const permission = 'android.permission.POST_NOTIFICATIONS'
      const result = plus.android.checkPermission(permission)
      return result === 0
    }
    // #endif

    return true
  }

  /**
   * 申请通知权限
   */
  async requestPermission(): Promise<boolean> {
    // #ifdef APP-PLUS
    const platform = uni.getSystemInfoSync().platform

    if (platform === 'ios') {
      return await this.requestIOSPermission()
    } else if (platform === 'android') {
      return await this.requestAndroidPermission()
    }
    // #endif

    return false
  }

  /**
   * 申请 iOS 权限
   */
  private async requestIOSPermission(): Promise<boolean> {
    // #ifdef APP-PLUS
    return new Promise((resolve) => {
      const JPush = plus.jpush
      JPush.requestNotificationPermission((result: any) => {
        resolve(result.isEnable)
      })
    })
    // #endif

    return false
  }

  /**
   * 申请 Android 权限
   */
  private async requestAndroidPermission(): Promise<boolean> {
    // #ifdef APP-PLUS
    const androidVersion = uni.getSystemInfoSync().system.split(' ')[1]
    
    if (parseInt(androidVersion) >= 13) {
      const permission = 'android.permission.POST_NOTIFICATIONS'
      
      return new Promise((resolve) => {
        plus.android.requestPermissions([permission], (e: any) => {
          if (e.granted && e.granted.length > 0) {
            resolve(true)
          } else {
            resolve(false)
          }
        })
      })
    }
    // #endif

    return true
  }

  /**
   * 显示权限申请对话框
   */
  async showPermissionPrompt(): Promise<boolean> {
    return new Promise((resolve) => {
      uni.showModal({
        title: '开启通知权限',
        content: '开启通知权限可以及时接收拍卖出价、拍卖结束等重要消息',
        confirmText: '去开启',
        success: async (res) => {
          if (res.confirm) {
            const granted = await this.requestPermission()
            if (granted) {
              resolve(true)
            } else {
              this.showPermissionDeniedDialog()
              resolve(false)
            }
          } else {
            resolve(false)
          }
        }
      })
    })
  }

  /**
   * 显示权限被拒绝对话框
   */
  showPermissionDeniedDialog(): void {
    uni.showModal({
      title: '通知权限已关闭',
      content: '请前往设置开启通知权限，以便接收重要消息',
      confirmText: '去设置',
      success: (res) => {
        if (res.confirm) {
          this.openSettings()
        }
      }
    })
  }

  /**
   * 打开系统设置
   */
  openSettings(): void {
    // #ifdef APP-PLUS
    plus.runtime.openURL('app-settings:')
    // #endif
  }
}

// 导出单例
export const permissionManager = new PushPermissionManager()
```

### 在 App.vue 中使用

```vue
<script setup lang="ts">
import { onLaunch } from '@dcloudio/uni-app'
import { permissionManager } from '@/utils/permission'

onLaunch(async () => {
  console.log('App Launch')

  // 检查权限
  const hasPermission = await permissionManager.checkPermission()

  if (!hasPermission) {
    // 延迟申请权限
    setTimeout(async () => {
      const granted = await permissionManager.showPermissionPrompt()
      if (granted) {
        // 权限已授予，初始化推送
        initPushService()
      }
    }, 2000)
  } else {
    // 已有权限，初始化推送
    initPushService()
  }
})

const initPushService = () => {
  // 初始化推送服务
  // ...
}
</script>
```

## 📊 权限状态管理

### 状态存储

```typescript
// store/push.ts
import { defineStore } from 'pinia'

export const usePushStore = defineStore('push', {
  state: () => ({
    hasPermission: false,
    deviceToken: '',
    isPushReady: false
  }),

  actions: {
    setPermission(hasPermission: boolean) {
      this.hasPermission = hasPermission
    },

    setDeviceToken(token: string) {
      this.deviceToken = token
    },

    setPushReady(ready: boolean) {
      this.isPushReady = ready
    }
  }
})
```

### 在组件中使用

```vue
<template>
  <view class="permission-status">
    <view v-if="pushStore.hasPermission" class="success">
      ✓ 已开启通知权限
    </view>
    <view v-else class="warning" @click="requestPermission">
      ! 未开启通知权限，点击开启
    </view>
  </view>
</template>

<script setup lang="ts">
import { usePushStore } from '@/store/push'
import { permissionManager } from '@/utils/permission'

const pushStore = usePushStore()

const requestPermission = async () => {
  const granted = await permissionManager.requestPermission()
  pushStore.setPermission(granted)

  if (granted) {
    uni.showToast({
      title: '权限已开启',
      icon: 'success'
    })
  }
}
</script>
```

## 🔔 通知渠道管理 (Android)

### 创建通知渠道

```kotlin
// AndroidManifest.xml
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
```

```kotlin
class NotificationChannelManager(private val context: Context) {
    
    companion object {
        const val CHANNEL_DEFAULT = "default_channel"
        const val CHANNEL_BIDS = "bids_channel"
        const val CHANNEL_AUCTION = "auction_channel"
        const val CHANNEL_SYSTEM = "system_channel"
    }
    
    /// 创建所有通知渠道
    fun createAllChannels() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            createDefaultChannel()
            createBidsChannel()
            createAuctionChannel()
            createSystemChannel()
        }
    }
    
    /// 创建默认渠道
    private fun createDefaultChannel() {
        val channelId = CHANNEL_DEFAULT
        val channelName = "默认通知"
        val channelDescription = "应用默认通知"
        
        val importance = NotificationManager.IMPORTANCE_HIGH
        val channel = NotificationChannel(channelId, channelName, importance).apply {
            description = channelDescription
            enableLights(true)
            enableVibration(true)
            vibrationPattern = longArrayOf(0, 200, 100, 200)
        }
        
        val manager = context.getSystemService(NotificationManager::class.java)
        manager.createNotificationChannel(channel)
    }
    
    /// 创建出价渠道
    private fun createBidsChannel() {
        val channelId = CHANNEL_BIDS
        val channelName = "出价通知"
        val channelDescription = "拍卖出价相关通知"
        
        val importance = NotificationManager.IMPORTANCE_HIGH
        val channel = NotificationChannel(channelId, channelName, importance).apply {
            description = channelDescription
            enableLights(true)
            enableVibration(true)
            vibrationPattern = longArrayOf(0, 300, 200, 300)
        }
        
        val manager = context.getSystemService(NotificationManager::class.java)
        manager.createNotificationChannel(channel)
    }
    
    /// 创建拍卖渠道
    private fun createAuctionChannel() {
        val channelId = CHANNEL_AUCTION
        val channelName = "拍卖通知"
        val channelDescription = "拍卖相关通知"
        
        val importance = NotificationManager.IMPORTANCE_DEFAULT
        val channel = NotificationChannel(channelId, channelName, importance).apply {
            description = channelDescription
            enableLights(true)
            enableVibration(true)
            vibrationPattern = longArrayOf(0, 100, 50, 100)
        }
        
        val manager = context.getSystemService(NotificationManager::class.java)
        manager.createNotificationChannel(channel)
    }
    
    /// 创建系统渠道
    private fun createSystemChannel() {
        val channelId = CHANNEL_SYSTEM
        val channelName = "系统通知"
        val channelDescription = "系统相关通知"
        
        val importance = NotificationManager.IMPORTANCE_LOW
        val channel = NotificationChannel(channelId, channelName, importance).apply {
            description = channelDescription
            enableLights(false)
            enableVibration(false)
        }
        
        val manager = context.getSystemService(NotificationManager::class.java)
        manager.createNotificationChannel(channel)
    }
}
```

### 在 MainActivity 中创建渠道

```kotlin
class MainActivity : AppCompatActivity() {
    
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)
        
        // 创建通知渠道
        NotificationChannelManager(this).createAllChannels()
        
        // 其他初始化
        // ...
    }
}
```

## 📝 最佳实践

### 1. 合理的权限申请时机

- ✅ 在用户首次需要使用推送功能时申请
- ✅ 在 App 启动后延迟申请（避免影响启动体验）
- ❌ 不要在 App 启动时立即申请权限

### 2. 提供清晰的权限说明

- ✅ 解释为什么需要这个权限
- ✅ 说明开启权限后的好处
- ❌ 不要强制要求用户授权

### 3. 优雅地处理权限被拒绝

- ✅ 提供引导去设置开启权限的入口
- ✅ 记录权限拒绝事件用于分析
- ❌ 不要因为权限被拒绝而阻止用户使用 App

### 4. 定期检查权限状态

- ✅ 在关键操作前检查权限
- ✅ 提供权限状态提示
- ❌ 不要假设权限状态永远不变

## 🔗 参考资料

- [Apple Developer - UserNotifications](https://developer.apple.com/documentation/usernotifications)
- [Android Developer - Notifications](https://developer.android.com/develop/ui/views/notifications)
- [UniApp 权限文档](https://uniapp.dcloud.net.cn/api/system/permission.html)
