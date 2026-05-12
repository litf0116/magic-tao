# Add project specific ProGuard rules here.
# You can control the set of applied configuration files using the
# proguardFiles setting in build.gradle.kts.
#
# For more details, see
#   http://developer.android.com/guide/developing/tools/proguard.html

# If your project uses WebView with JS, uncomment the following
# and specify the fully qualified class name to the JavaScript interface
# class:
#-keepclassmembers class fqcn.of.javascript.interface.for.webview {
#   public *;
#}

# Uncomment this to preserve the line number information for
# debugging stack traces.
#-keepattributes SourceFile,LineNumberTable

# If you keep the line number information, uncomment this to
# hide the original source file name.
#-renamesourcefileattribute SourceFile

# Flutter wrapper
-keep class io.flutter.app.** { *; }
-keep class io.flutter.plugin.**  { *; }
-keep class io.flutter.util.**  { *; }
-keep class io.flutter.view.**  { *; }
-keep class io.flutter.**  { *; }
-keep class io.flutter.plugins.**  { *; }

# Kotlin 元数据保护（支持 Freezed、JSON Serializable 等）
-keep class kotlin.Metadata { *; }
-keepclassmembers class **$WhenMappings {
    <fields>;
}
-keepclassmembers class kotlin.Metadata {
    public <methods>;
}
-keep class kotlin.** { *; }
-keepclassmembers class **$**$WhenMappings {
    <methods>;
}

# 保持注解
-keepattributes Annotation, InnerClasses, EnclosingMethod
-keepattributes Signature, Exceptions, MethodParameters

# 保持反射相关的类
-keepattributes *Annotation*, Reflection, InnerClasses

# Dio HTTP client
-keep class com.google.gson.** { *; }
-keep class okhttp3.** { *; }
-keep class okio.** { *; }

# 微信SDK
-keep class com.tencent.mm.sdk.** { *; }
-keep class com.tencent.wxop.** { *; }

# 极光推送
-keep class cn.jpush.** { *; }
-keep class cn.jiguang.** { *; }

# 权限管理
-keep class com.karumi.dexter.** { *; }

# 图片处理
-keep class com.bumptech.glide.** { *; }

# 二维码扫描
-keep class com.google.zxing.** { *; }
-keep class com.google.mlkit.** { *; }

# 音频播放
-keep class org.videolan.libvlc.** { *; }

# 加密
-keep class org.bouncycastle.** { *; }

# 设备信息
-keep class android.os.Build { *; }

# 网络状态
-keep class android.net.ConnectivityManager { *; }

# 文件系统
-keep class android.os.Environment { *; }

# 忽略警告
-ignorewarnings
-optimizationpasses 5
-dontusemixedcaseclassnames
-dontskipnonpubliclibraryclasses
-dontpreverify
-verbose
-optimizations !code/simplification/arithmetic,!field/*,!class/merging/*