plugins {
    id("com.android.application")
    id("kotlin-android")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
}

android {
    namespace = "com.molitao.app"
    compileSdk = flutter.compileSdkVersion
    ndkVersion = flutter.ndkVersion

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }

    kotlinOptions {
        jvmTarget = JavaVersion.VERSION_11.toString()
    }

    defaultConfig {
        applicationId = "com.molitao.app"
        minSdk = flutter.minSdkVersion
        targetSdk = flutter.targetSdkVersion
        versionCode = flutter.versionCode
        versionName = flutter.versionName

        // 多ABI支持
        ndk {
            abiFilters "armeabi-v7a", "arm64-v8a", "x86_64"
        }

        // 权限配置
        manifestPlaceholders["appName"] = "魔力淘"
    }

    signingConfigs {
        create("release") {
            storeFile = file("molitao-release.keystore")
            storePassword = "molitao123"
            keyAlias = "molitao"
            keyPassword = "molitao123"
        }
    }

    buildTypes {
        debug {
            signingConfig = signingConfigs.getByName("release")
            buildConfigField("boolean", "DEBUG_MODE", "true")
            manifestPlaceholders["debugMode"] = true
        }
        release {
            signingConfig = signingConfigs.getByName("release")
            buildConfigField("boolean", "DEBUG_MODE", "false")
            manifestPlaceholders["debugMode"] = false

            // 代码混淆
            isMinifyEnabled = true
            proguardFiles(getDefaultProguardFile("proguard-android-optimize.txt"), "proguard-rules.pro")

            // 移除调试符号
            isDebuggable = false
        }
    }

    buildFeatures {
        buildConfig = true
        compose = false
    }

    packaging {
        resources {
            excludes += "/META-INF/{AL2.0,LGPL2.1}"
        }
    }

    lint {
        abortOnError = false
        checkReleaseBuilds = false
    }
}

flutter {
    source = "../.."
}
