/**
 * H5 推送权限管理 Composable
 * 统一管理推送权限请求流程
 */

import { ref } from 'vue'
import { h5PushService } from '@/utils/pushH5'

interface PushPermissionState {
  canRequest: boolean
  permissionStatus: 'default' | 'granted' | 'denied'
  isSubscribed: boolean
}

const state = ref<PushPermissionState>({
  canRequest: false,
  permissionStatus: 'default',
  isSubscribed: false
})

const showDialog = ref(false)

/**
 * H5 推送权限管理 Hook
 */
export function usePushPermission() {
  /**
   * 初始化推送权限状态
   */
  const initPermissionState = async () => {
    // #ifdef H5
    const status = await h5PushService.getPermissionStatus()
    state.value.permissionStatus = status

    const subscription = h5PushService.getSubscription()
    state.value.isSubscribed = subscription !== null

    state.value.canRequest = status === 'default' || status === 'granted'
    // #endif
  }

  /**
   * 显示推送权限请求对话框
   */
  const showPermissionDialog = () => {
    showDialog.value = true
  }

  /**
   * 隐藏推送权限请求对话框
   */
  const hidePermissionDialog = () => {
    showDialog.value = false
  }

  /**
   * 请求推送权限并订阅
   */
  const requestPermission = async (): Promise<boolean> => {
    // #ifdef H5
    try {
      const success = await h5PushService.requestPermission()
      
      if (success) {
        state.value.permissionStatus = 'granted'
        state.value.isSubscribed = true
        uni.showToast({
          title: '推送通知已开启',
          icon: 'success'
        })
      } else {
        state.value.permissionStatus = 'denied'
        state.value.isSubscribed = false
        uni.showToast({
          title: '未能开启推送通知',
          icon: 'none'
        })
      }
      
      return success
    } catch (error) {
      console.error('[PushPermission] 请求推送权限失败:', error)
      uni.showToast({
        title: '请求推送权限失败',
        icon: 'none'
      })
      return false
    }
    // #endif

    // #ifndef H5
    return false
    // #endif
  }

  /**
   * 取消推送订阅
   */
  const unsubscribe = async () => {
    // #ifdef H5
    try {
      await h5PushService.unsubscribe()
      state.value.isSubscribed = false
      
      uni.showToast({
        title: '已关闭推送通知',
        icon: 'success'
      })
    } catch (error) {
      console.error('[PushPermission] 取消订阅失败:', error)
      uni.showToast({
        title: '操作失败',
        icon: 'none'
      })
    }
    // #endif
  }

  /**
   * 检查是否应该显示权限请求对话框
   * 用于首次访问时的智能提示
   */
  const checkShouldShowDialog = async (): Promise<boolean> => {
    // #ifdef H5
    await initPermissionState()

    const storageKey = 'push_permission_dialog_shown'
    const dialogShown = uni.getStorageSync(storageKey)

    if (state.value.permissionStatus === 'default' && !dialogShown) {
      return true
    }
    // #endif

    return false
  }

  /**
   * 标记对话框已显示
   */
  const markDialogShown = () => {
    const storageKey = 'push_permission_dialog_shown'
    uni.setStorageSync(storageKey, true)
  }

  return {
    state,
    showDialog,
    initPermissionState,
    showPermissionDialog,
    hidePermissionDialog,
    requestPermission,
    unsubscribe,
    checkShouldShowDialog,
    markDialogShown
  }
}
