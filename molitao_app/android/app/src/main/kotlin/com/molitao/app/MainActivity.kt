package com.molitao.app

import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import android.os.Build
import io.flutter.embedding.android.FlutterActivity

class MainActivity : FlutterActivity() {
    override fun onCreate(savedInstanceState: android.os.Bundle?) {
        super.onCreate(savedInstanceState)
        createNotificationChannels()
    }

    private fun createNotificationChannels() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val manager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager

            // 拍卖通知渠道 - 高优先级，锁屏可见
            val auctionChannel = NotificationChannel(
                "auction_notify",
                "拍卖通知",
                NotificationManager.IMPORTANCE_HIGH
            ).apply {
                description = "拍品开拍、竞价等通知"
                enableLights(true)
                enableVibration(true)
                setShowBadge(true)
                // 锁屏显示
                lockscreenVisibility = Notification.VISIBILITY_PUBLIC
            }

            // 系统通知渠道 - 默认优先级
            val systemChannel = NotificationChannel(
                "system_notify",
                "系统通知",
                NotificationManager.IMPORTANCE_DEFAULT
            ).apply {
                description = "订单、消息等系统通知"
                enableLights(true)
                enableVibration(true)
                setShowBadge(true)
            }

            manager.createNotificationChannels(listOf(auctionChannel, systemChannel))
        }
    }
}
