<script setup lang="ts">
import { computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import GlobalRefreshControl from '@/components/GlobalRefreshControl.vue'
import { globalRefreshManager } from '@/utils/refreshManager'

const route = useRoute()

const activeMenu = computed(() => route.path)

// 页面可见性监听
const handleVisibilityChange = () => {
  if (document.hidden) {
    globalRefreshManager.pause()
  } else {
    globalRefreshManager.resume()
  }
}

// 手动刷新事件监听
const handleManualRefresh = () => {
  // 触发所有页面的刷新
  window.dispatchEvent(new CustomEvent('refresh-all-pages'))
}

onMounted(() => {
  // 监听页面可见性变化
  document.addEventListener('visibilitychange', handleVisibilityChange)
  
  // 监听手动刷新事件
  window.addEventListener('manual-refresh', handleManualRefresh)
})

onUnmounted(() => {
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  window.removeEventListener('manual-refresh', handleManualRefresh)
  globalRefreshManager.destroy()
})
</script>

<template>
  <div id="app">
    <el-container class="app-container">
      <!-- 顶部导航栏 -->
      <el-header class="app-header">
        <div class="header-content">
          <div class="header-left">
            <el-icon class="header-icon">
              <Monitor />
            </el-icon>
            <h1 class="header-title">魔力淘监控系统</h1>
          </div>
          <div class="header-right">
            <!-- 全局刷新控制组件 -->
            <GlobalRefreshControl />
          </div>
        </div>
      </el-header>
      
      <el-container class="main-container">
        <!-- 侧边栏 -->
        <el-aside width="240" class="app-sidebar">
          <el-menu
            :default-active="activeMenu"
            class="sidebar-menu"
            router
          >
            <el-menu-item index="/dashboard" class="menu-item">
              <el-icon><Monitor /></el-icon>
              <span>系统概览</span>
            </el-menu-item>
            <el-menu-item index="/performance" class="menu-item">
              <el-icon><TrendCharts /></el-icon>
              <span>性能监控</span>
            </el-menu-item>
            <el-menu-item index="/system" class="menu-item">
              <el-icon><Cpu /></el-icon>
              <span>系统资源</span>
            </el-menu-item>
            <el-menu-item index="/errors" class="menu-item">
              <el-icon><Warning /></el-icon>
              <span>错误统计</span>
            </el-menu-item>
          </el-menu>
        </el-aside>
        
        <!-- 主内容区域 -->
        <el-main class="app-main">
          <router-view />
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<style scoped>
.app-container {
  height: 100vh;
  background: #f5f7fa;
}

.app-header {
  background: #ffffff;
  border-bottom: 1px solid #e4e7ed;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  padding: 0;
  height: 64px;
}

.header-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 100%;
  padding: 0 24px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-icon {
  font-size: 24px;
  color: #409eff;
}

.header-title {
  font-size: 20px;
  font-weight: 600;
  color: #303133;
  margin: 0;
}

.header-right {
  display: flex;
  align-items: center;
}

.refresh-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  height: 36px;
  padding: 0 16px;
  border-radius: 8px;
}

.main-container {
  height: calc(100vh - 64px);
}

.app-sidebar {
  background: #ffffff;
  border-right: 1px solid #e4e7ed;
  box-shadow: 2px 0 8px rgba(0, 0, 0, 0.06);
}

.sidebar-menu {
  border-right: none;
  padding: 16px 0;
}

.menu-item {
  margin: 4px 16px;
  border-radius: 8px;
  height: 48px;
  line-height: 48px;
}

.menu-item:hover {
  background-color: #f0f9ff;
  color: #409eff;
}

.menu-item.is-active {
  background-color: #e6f7ff;
  color: #409eff;
  border-right: 3px solid #409eff;
}

.app-main {
  background: #f5f7fa;
  padding: 24px;
  overflow-y: auto;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .app-sidebar {
    width: 200px !important;
  }
  
  .header-title {
    font-size: 18px;
  }
  
  .app-main {
    padding: 16px;
  }
}
</style>
