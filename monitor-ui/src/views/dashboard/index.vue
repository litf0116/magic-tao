<template>
  <div class="dashboard-page">
    <div class="page-header">
      <h1 class="page-title">系统监控概览</h1>
      <p class="page-description">实时监控系统关键指标和性能数据</p>
    </div>
    
    <!-- 关键指标卡片 -->
    <div class="metrics-grid">
      <el-card class="metric-card">
        <div class="metric-content">
          <div class="metric-icon">
            <el-icon class="icon">
              <Monitor />
            </el-icon>
          </div>
          <div class="metric-info">
            <div class="metric-label">系统状态</div>
            <div class="metric-value status-healthy">健康</div>
          </div>
        </div>
      </el-card>
      
      <el-card class="metric-card">
        <div class="metric-content">
          <div class="metric-icon">
            <el-icon class="icon">
              <Cpu />
            </el-icon>
          </div>
          <div class="metric-info">
            <div class="metric-label">私有内存</div>
            <div class="metric-value">{{ systemOverview.privateMemory || 0 }} MB</div>
          </div>
        </div>
      </el-card>
      
      <el-card class="metric-card">
        <div class="metric-content">
          <div class="metric-icon">
            <el-icon class="icon">
              <Connection />
            </el-icon>
          </div>
          <div class="metric-info">
            <div class="metric-label">线程数量</div>
            <div class="metric-value">{{ systemOverview.threadCount || 0 }}</div>
          </div>
        </div>
      </el-card>
      
      <el-card class="metric-card">
        <div class="metric-content">
          <div class="metric-icon">
            <el-icon class="icon">
              <Timer />
            </el-icon>
          </div>
          <div class="metric-info">
            <div class="metric-label">总API调用</div>
            <div class="metric-value">{{ overallStats.totalCalls.toLocaleString() }}</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 统计概览 -->
    <div class="stats-grid">
      <el-card class="stat-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <TrendCharts />
            </el-icon>
            <span>API调用统计</span>
          </div>
        </template>
        <div class="stat-content">
          <div class="stat-item">
            <span class="stat-label">总调用次数:</span>
            <span class="stat-value">{{ overallStats.totalCalls.toLocaleString() }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">平均响应时间:</span>
            <span class="stat-value">{{ overallStats.avgResponseTime.toFixed(2) }}ms</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">错误率:</span>
            <span class="stat-value" :class="overallStats.errorRate > 5 ? 'text-red-500' : 'text-green-500'">
              {{ overallStats.errorRate.toFixed(2) }}%
            </span>
          </div>
          <div class="stat-item">
            <span class="stat-label">活跃API数量:</span>
            <span class="stat-value">{{ activeApiCount }}</span>
          </div>
        </div>
      </el-card>

      <el-card class="stat-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Monitor />
            </el-icon>
            <span>系统资源</span>
          </div>
        </template>
        <div class="stat-content">
          <div class="stat-item">
            <span class="stat-label">工作集内存:</span>
            <span class="stat-value">{{ systemOverview.workingSet || 0 }} MB</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">托管内存:</span>
            <span class="stat-value">{{ systemOverview.totalMemory || 0 }} MB</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">句柄数量:</span>
            <span class="stat-value">{{ systemOverview.handleCount || 0 }}</span>
          </div>
          <div class="stat-item">
            <span class="stat-label">运行时长:</span>
            <span class="stat-value">{{ formatUptime(systemOverview.uptime) }}</span>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 图表区域 -->
    <div class="charts-grid">
      <el-card class="chart-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <TrendCharts />
            </el-icon>
            <span>API调用排行</span>
          </div>
        </template>
        <div class="chart-content">
          <div v-if="topApis.length === 0" class="no-data">
            暂无API调用数据
          </div>
          <div v-else v-for="(item, index) in topApis" :key="index" class="api-item">
            <div class="api-rank">{{ index + 1 }}</div>
            <div class="api-info">
              <div class="api-name">{{ item.label }}</div>
              <div class="api-stats">
                <span class="api-calls">{{ item.count.toLocaleString() }} 次</span>
                <span class="api-time">{{ item.avgTime.toFixed(2) }}ms</span>
                <span v-if="item.errorRate > 0" class="api-error">{{ item.errorRate.toFixed(2) }}% 错误</span>
              </div>
            </div>
          </div>
        </div>
      </el-card>
      
      <el-card class="chart-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Warning />
            </el-icon>
            <span>错误率排行</span>
          </div>
        </template>
        <div class="chart-content">
          <div v-if="topErrorApis.length === 0" class="no-data">
            暂无错误数据
          </div>
          <div v-else v-for="(item, index) in topErrorApis" :key="index" class="api-item">
            <div class="api-rank error-rank">{{ index + 1 }}</div>
            <div class="api-info">
              <div class="api-name">{{ getApiName(item.endpoint) }}</div>
              <div class="api-stats">
                <span class="api-calls">{{ item.totalCalls.toLocaleString() }} 次</span>
                <span class="api-error">{{ item.errorRate.toFixed(2) }}% 错误</span>
              </div>
            </div>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { getPerformance } from '@/api/monitor'
import { ElMessage } from 'element-plus'
import { globalRefreshManager } from '@/utils/refreshManager'

// 获取当前路由
const route = useRoute()

// 响应式数据
const systemOverview = ref({
  workingSet: 0,
  privateMemory: 0,
  threadCount: 0,
  uptime: '',
  totalMemory: 0,
  generation0: 0,
  generation1: 0,
  generation2: 0,
  handleCount: 0
})

const overallStats = ref({
  totalCalls: 0,
  totalErrors: 0,
  errorRate: 0,
  avgResponseTime: 0
})

const topApis = ref<Array<{ label: string; count: number; errorRate: number; avgTime: number }>>([])
const topErrorApis = ref<any[]>([])

// 计算活跃API数量
const activeApiCount = computed(() => {
  return topApis.value.length
})

// 格式化运行时间
const formatUptime = (uptime: string) => {
  if (!uptime) return '未知'
  // 解析类似 "10:16:53.9275692" 的时间格式
  const parts = uptime.split(':')
  if (parts.length >= 3) {
    const hours = parseInt(parts[0])
    const minutes = parseInt(parts[1])
    
    if (hours > 24) {
      const days = Math.floor(hours / 24)
      const remainingHours = hours % 24
      return `${days}天 ${remainingHours}小时`
    } else {
      return `${hours}小时 ${minutes}分钟`
    }
  }
  return uptime
}

// 获取API名称
const getApiName = (endpoint: string) => {
  if (!endpoint) return '未知接口'
  const parts = endpoint.split('/')
  return parts[parts.length - 1] || endpoint
}

// 加载数据
const loadData = async () => {
  try {
    // 调用真实接口
    const response = await getPerformance()
    console.log('Dashboard - 真实接口调用成功:', response)
    
    // 处理真实接口返回的数据
    if (response && typeof response === 'object') {
      const { system, gc, apiStatistics } = response as any
      
      // 更新系统概览数据
      systemOverview.value = {
        workingSet: system?.workingSet || 0,
        privateMemory: system?.privateMemory || 0,
        threadCount: system?.threadCount || 0,
        uptime: system?.uptime || '',
        totalMemory: gc?.totalMemory || 0,
        generation0: gc?.generation0 || 0,
        generation1: gc?.generation1 || 0,
        generation2: gc?.generation2 || 0,
        handleCount: system?.handleCount || 0
      }
      
      // 计算总体统计
      if (apiStatistics && Object.keys(apiStatistics).length > 0) {
        const totalCalls = Object.values(apiStatistics).reduce((sum: number, stat: any) => sum + (stat.totalCalls || 0), 0)
        const totalErrors = Object.values(apiStatistics).reduce((sum: number, stat: any) => sum + (stat.errorCount || 0), 0)
        const avgResponseTime = Object.values(apiStatistics).reduce((sum: number, stat: any) => sum + (stat.avgResponseTime || 0), 0) / Object.keys(apiStatistics).length
        
        overallStats.value = {
          totalCalls,
          totalErrors,
          errorRate: totalCalls > 0 ? (totalErrors / totalCalls) * 100 : 0,
          avgResponseTime: Math.round(avgResponseTime * 100) / 100
        }
        
        // 获取API调用排行
        topApis.value = Object.entries(apiStatistics)
          .map(([key, stat]: [string, any]) => ({
            label: getApiName(stat.endpoint || key),
            count: stat.totalCalls || 0,
            errorRate: stat.errorRate || 0,
            avgTime: stat.avgResponseTime || 0
          }))
          .sort((a, b) => b.count - a.count)
          .slice(0, 10)
        
        // 获取错误率最高的API
        topErrorApis.value = Object.values(apiStatistics)
          .filter((stat: any) => stat.errorRate > 0)
          .sort((a: any, b: any) => b.errorRate - a.errorRate)
          .slice(0, 10)
      } else {
        // 如果没有API统计数据，设置默认值
        overallStats.value = {
          totalCalls: 0,
          totalErrors: 0,
          errorRate: 0,
          avgResponseTime: 0
        }
        topApis.value = []
        topErrorApis.value = []
      }
    }
  } catch (error) {
    console.error('加载数据失败:', error)
    ElMessage.error('加载监控数据失败，请检查网络连接')
  }
}

// 页面激活状态
const isPageActive = ref(false)

// 页面激活/失活处理
const handlePageVisibilityChange = () => {
  if (document.hidden) {
    isPageActive.value = false
    globalRefreshManager.pause()
  } else if (route.path === '/dashboard') {
    isPageActive.value = true
    globalRefreshManager.resume()
    // 重新启动自动刷新
    globalRefreshManager.startAutoRefresh(loadData)
  }
}

// 手动刷新处理
const handleManualRefresh = () => {
  if (route.path === '/dashboard') {
    globalRefreshManager.manualRefresh(loadData)
  }
}

onMounted(() => {
  loadData()
  
  // 如果当前页面是dashboard，启动自动刷新
  if (route.path === '/dashboard') {
    isPageActive.value = true
    globalRefreshManager.startAutoRefresh(loadData)
  }
  
  // 监听页面可见性变化
  document.addEventListener('visibilitychange', handlePageVisibilityChange)
  
  // 监听全局刷新事件
  window.addEventListener('refresh-all-pages', handleManualRefresh)
})

onUnmounted(() => {
  document.removeEventListener('visibilitychange', handlePageVisibilityChange)
  window.removeEventListener('refresh-all-pages', handleManualRefresh)
})

// 监听路由变化，重新加载数据
watch(() => route.path, () => {
  if (route.path === '/dashboard') {
    console.log('Dashboard页面路由变化，重新加载数据')
    isPageActive.value = true
    globalRefreshManager.startAutoRefresh(loadData)
  } else {
    isPageActive.value = false
    globalRefreshManager.stopAutoRefresh()
  }
})
</script>

<style scoped>
.dashboard-page {
  min-height: 100%;
}

.page-header {
  margin-bottom: 32px;
}

.page-title {
  font-size: 28px;
  font-weight: 600;
  color: #303133;
  margin: 0 0 8px 0;
}

.page-description {
  font-size: 14px;
  color: #909399;
  margin: 0;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
}

.metric-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.metric-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12);
}

.metric-content {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 8px 0;
}

.metric-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: linear-gradient(135deg, #409eff, #67c23a);
  display: flex;
  align-items: center;
  justify-content: center;
}

.metric-icon .icon {
  font-size: 24px;
  color: white;
}

.metric-info {
  flex: 1;
}

.metric-label {
  font-size: 14px;
  color: #909399;
  margin-bottom: 4px;
}

.metric-value {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
}

.status-healthy {
  color: #67c23a;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
}

.stat-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.stat-content {
  padding: 16px 0;
}

.stat-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.stat-item:last-child {
  border-bottom: none;
}

.stat-label {
  font-size: 14px;
  color: #606266;
}

.stat-value {
  font-size: 14px;
  font-weight: 500;
  color: #303133;
}

.charts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}

.chart-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
  color: #303133;
}

.card-icon {
  font-size: 18px;
  color: #409eff;
}

.chart-content {
  padding: 16px 0;
}

.no-data {
  text-align: center;
  color: #909399;
  padding: 40px 0;
  font-size: 14px;
}

.api-item {
  display: flex;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}

.api-item:last-child {
  border-bottom: none;
}

.api-rank {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: #409eff;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  margin-right: 16px;
  flex-shrink: 0;
}

.error-rank {
  background: #f56c6c;
}

.api-info {
  flex: 1;
  min-width: 0;
}

.api-name {
  font-size: 14px;
  font-weight: 500;
  color: #303133;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.api-stats {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: #909399;
}

.api-calls {
  color: #409eff;
}

.api-time {
  color: #67c23a;
}

.api-error {
  color: #f56c6c;
}

.text-red-500 {
  color: #f56c6c;
}

.text-green-500 {
  color: #67c23a;
}
</style>