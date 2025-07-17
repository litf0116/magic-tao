<template>
  <div class="global-refresh-control">
    <div class="control-panel">
      <!-- 自动刷新开关 -->
      <div class="control-item">
        <el-switch 
          v-model="autoRefresh" 
          @change="handleAutoRefreshChange"
          size="small"
        />
        <span class="control-label">自动刷新</span>
      </div>

      <!-- 刷新间隔选择 -->
      <div class="control-item">
        <el-select 
          v-model="refreshInterval" 
          :disabled="!autoRefresh"
          size="small"
          style="width: 100px"
          @change="handleIntervalChange"
        >
          <el-option label="5秒" :value="5000" />
          <el-option label="10秒" :value="10000" />
          <el-option label="30秒" :value="30000" />
          <el-option label="1分钟" :value="60000" />
          <el-option label="2分钟" :value="120000" />
          <el-option label="5分钟" :value="300000" />
        </el-select>
      </div>

      <!-- 手动刷新按钮 -->
      <div class="control-item">
        <el-button 
          @click="handleManualRefresh"
          :loading="isLoading"
          size="small"
          type="primary"
        >
          立即刷新
        </el-button>
      </div>

      <!-- 倒计时显示 -->
      <div v-if="autoRefresh && !isPaused" class="control-item countdown">
        <el-icon class="countdown-icon"><Timer /></el-icon>
        <span class="countdown-text">{{ countdownText }}</span>
      </div>

      <!-- 状态指示器 -->
      <div class="control-item status">
        <el-icon v-if="isLoading" class="loading-icon"><Loading /></el-icon>
        <el-icon v-else-if="isPaused" class="paused-icon"><VideoPause /></el-icon>
        <el-icon v-else class="active-icon"><VideoPlay /></el-icon>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { globalRefreshManager } from '@/utils/refreshManager'

// 响应式数据
const autoRefresh = ref(true)
const refreshInterval = ref(30000)
const isLoading = ref(false)
const isPaused = ref(false)
const countdownText = ref('')



// 初始化
onMounted(() => {
  const state = globalRefreshManager.getState()
  autoRefresh.value = state.settings.autoRefresh
  refreshInterval.value = state.settings.interval
  isLoading.value = state.isLoading
  isPaused.value = state.isPaused
  countdownText.value = state.countdownText

  // 监听状态变化
  updateState()
  
  // 设置定时器更新状态
  const stateTimer = setInterval(updateState, 1000)
  
  // 清理定时器
  onUnmounted(() => {
    clearInterval(stateTimer)
  })
})

// 更新状态
const updateState = () => {
  const state = globalRefreshManager.getState()
  isLoading.value = state.isLoading
  isPaused.value = state.isPaused
  countdownText.value = state.countdownText
}

// 处理自动刷新开关
const handleAutoRefreshChange = (enabled: boolean) => {
  globalRefreshManager.toggleAutoRefresh(enabled)
  ElMessage.success(enabled ? '已开启自动刷新' : '已关闭自动刷新')
}

// 处理间隔变化
const handleIntervalChange = (interval: number) => {
  globalRefreshManager.setInterval(interval)
  ElMessage.success(`刷新间隔已设置为 ${getIntervalText(interval)}`)
}

// 处理手动刷新
const handleManualRefresh = async () => {
  try {
    // 触发全局刷新事件
    window.dispatchEvent(new CustomEvent('manual-refresh'))
    ElMessage.success('数据刷新成功')
  } catch (error) {
    ElMessage.error('数据刷新失败')
  }
}

// 获取间隔文本
const getIntervalText = (interval: number) => {
  const seconds = interval / 1000
  if (seconds < 60) {
    return `${seconds}秒`
  } else if (seconds < 3600) {
    return `${seconds / 60}分钟`
  } else {
    return `${seconds / 3600}小时`
  }
}
</script>

<style scoped>
.global-refresh-control {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 1000;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border: 1px solid #e4e7ed;
  padding: 12px;
}

.control-panel {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
}

.control-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.control-label {
  font-size: 12px;
  color: #606266;
  white-space: nowrap;
}

.countdown {
  background: #f0f9ff;
  padding: 4px 8px;
  border-radius: 4px;
  border: 1px solid #bae6fd;
}

.countdown-icon {
  font-size: 12px;
  color: #0ea5e9;
}

.countdown-text {
  font-size: 12px;
  color: #0ea5e9;
  font-weight: 500;
}

.status {
  padding: 4px;
}

.loading-icon {
  font-size: 14px;
  color: #409eff;
  animation: spin 1s linear infinite;
}

.paused-icon {
  font-size: 14px;
  color: #909399;
}

.active-icon {
  font-size: 14px;
  color: #67c23a;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .global-refresh-control {
    top: 10px;
    right: 10px;
    left: 10px;
    padding: 8px;
  }
  
  .control-panel {
    gap: 8px;
  }
  
  .control-item {
    gap: 4px;
  }
}
</style> 