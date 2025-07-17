import { ref } from 'vue'

export interface RefreshSettings {
  autoRefresh: boolean
  interval: number
  lastRefreshTime: number
}

export class RefreshManager {
  private timer: number | null = null
  private countdownTimer: number | null = null
  private isPaused = ref(false)
  private currentCountdown = ref(0)
  private isLoading = ref(false)
  private settings = ref<RefreshSettings>({
    autoRefresh: true,
    interval: 30000, // 30秒
    lastRefreshTime: Date.now()
  })

  // 计算属性
  get countdownText() {
    const seconds = Math.ceil(this.currentCountdown.value / 1000)
    if (seconds < 60) {
      return `${seconds}秒`
    } else {
      const minutes = Math.floor(seconds / 60)
      const remainingSeconds = seconds % 60
      return `${minutes}分${remainingSeconds}秒`
    }
  }

  // 初始化设置
  constructor() {
    this.loadSettings()
  }

  // 加载设置
  private loadSettings() {
    const REFRESH_SETTINGS_KEY = 'monitor_refresh_settings'
    try {
      const saved = localStorage.getItem(REFRESH_SETTINGS_KEY)
      if (saved) {
        const parsed = JSON.parse(saved)
        this.settings.value = { ...this.settings.value, ...parsed }
      }
    } catch (error) {
      console.error('加载刷新设置失败:', error)
    }
  }

  // 保存设置
  private saveSettings() {
    const REFRESH_SETTINGS_KEY = 'monitor_refresh_settings'
    try {
      localStorage.setItem(REFRESH_SETTINGS_KEY, JSON.stringify(this.settings.value))
    } catch (error) {
      console.error('保存刷新设置失败:', error)
    }
  }

  // 开始自动刷新
  startAutoRefresh(callback: () => Promise<void>) {
    if (!this.settings.value.autoRefresh || this.isPaused.value) {
      return
    }

    this.stopAutoRefresh()
    
    const executeRefresh = async () => {
      if (this.isPaused.value) return
      
      try {
        this.isLoading.value = true
        await callback()
        this.settings.value.lastRefreshTime = Date.now()
      } catch (error) {
        console.error('自动刷新失败:', error)
      } finally {
        this.isLoading.value = false
      }
    }

    // 立即执行一次
    executeRefresh()
    
    // 设置定时器
    this.timer = setInterval(executeRefresh, this.settings.value.interval)
    this.startCountdown()
  }

  // 停止自动刷新
  stopAutoRefresh() {
    if (this.timer) {
      clearInterval(this.timer)
      this.timer = null
    }
    this.stopCountdown()
  }

  // 开始倒计时
  private startCountdown() {
    this.stopCountdown()
    
    const updateCountdown = () => {
      if (this.isPaused.value) return
      
      const elapsed = Date.now() - this.settings.value.lastRefreshTime
      const remaining = Math.max(0, this.settings.value.interval - elapsed)
      this.currentCountdown.value = remaining
      
      if (remaining <= 0) {
        this.currentCountdown.value = this.settings.value.interval
      }
    }

    updateCountdown()
    this.countdownTimer = setInterval(updateCountdown, 1000)
  }

  // 停止倒计时
  private stopCountdown() {
    if (this.countdownTimer) {
      clearInterval(this.countdownTimer)
      this.countdownTimer = null
    }
    this.currentCountdown.value = 0
  }

  // 手动刷新
  async manualRefresh(callback: () => Promise<void>) {
    try {
      this.isLoading.value = true
      await callback()
      this.settings.value.lastRefreshTime = Date.now()
      this.currentCountdown.value = this.settings.value.interval
    } catch (error) {
      console.error('手动刷新失败:', error)
      throw error
    } finally {
      this.isLoading.value = false
    }
  }

  // 切换自动刷新
  toggleAutoRefresh(enabled: boolean) {
    this.settings.value.autoRefresh = enabled
    this.saveSettings()
    
    if (enabled && !this.isPaused.value) {
      // 重新启动刷新（需要外部调用 startAutoRefresh）
    } else {
      this.stopAutoRefresh()
    }
  }

  // 设置刷新间隔
  setInterval(interval: number) {
    this.settings.value.interval = interval
    this.saveSettings()
    
    if (this.settings.value.autoRefresh && !this.isPaused.value) {
      // 重新启动刷新（需要外部调用 startAutoRefresh）
    }
  }

  // 暂停刷新
  pause() {
    this.isPaused.value = true
    this.stopAutoRefresh()
  }

  // 恢复刷新
  resume() {
    this.isPaused.value = false
    // 需要外部调用 startAutoRefresh 来重新启动
  }

  // 获取状态
  getState() {
    return {
      settings: this.settings.value,
      isPaused: this.isPaused.value,
      isLoading: this.isLoading.value,
      countdown: this.currentCountdown.value,
      countdownText: this.countdownText
    }
  }

  // 销毁
  destroy() {
    this.stopAutoRefresh()
  }
}

// 创建全局实例
export const globalRefreshManager = new RefreshManager() 