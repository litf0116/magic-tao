<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const activeMenu = computed(() => route.path)

const refreshData = () => {
  // 刷新数据的逻辑
  console.log('刷新数据')
}
</script>

<template>
  <div id="app">
    <el-container>
      <el-header class="header">
        <div class="flex items-center justify-between h-full">
          <div class="flex items-center">
            <el-icon class="text-2xl mr-3">
              <Monitor />
            </el-icon>
            <h1 class="text-xl font-bold">魔力淘监控系统</h1>
          </div>
          <div class="flex items-center space-x-4">
            <el-button type="primary" @click="refreshData">
              <el-icon class="mr-1">
                <Refresh />
              </el-icon>
              刷新数据
            </el-button>
          </div>
        </div>
      </el-header>
      
      <el-container>
        <el-aside width="200" class="sidebar">
          <el-menu
            :default-active="activeMenu"
            class="sidebar-menu"
            router
          >
            <el-menu-item index="/dashboard">
              <el-icon><Monitor /></el-icon>
              <span>系统概览</span>
            </el-menu-item>
            <el-menu-item index="/performance">
              <el-icon><TrendCharts /></el-icon>
              <span>性能监控</span>
            </el-menu-item>
            <el-menu-item index="/system">
              <el-icon><Cpu /></el-icon>
              <span>系统资源</span>
            </el-menu-item>
            <el-menu-item index="/errors">
              <el-icon><Warning /></el-icon>
              <span>错误统计</span>
            </el-menu-item>
          </el-menu>
        </el-aside>
        
        <el-main class="main-content">
          <router-view />
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<style scoped>
.header {
  background: #fff;
  border-bottom: 1px solid #e4e7d;
  padding: 0 20px;
}

.sidebar {
  background: #f5f7fa;
  border-right: 1px solid #e40000000;
}

.sidebar-menu {
  border-right: none;
}

.main-content {
  background: #f57a;
  padding: 20;
}
</style>
