<template>
  <div class="performance-page">
    <div class="page-header">
      <h1 class="page-title">API性能监控</h1>
      <p class="page-description">实时监控API调用性能，识别性能瓶颈</p>
    </div>
    
    <!-- 性能概览 -->
    <div class="overview-grid">
      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <TrendCharts />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">总API调用</div>
            <div class="overview-value">{{ overallStats.totalCalls.toLocaleString() }}</div>
          </div>
        </div>
      </el-card>

      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Timer />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">平均响应时间</div>
            <div class="overview-value">{{ overallStats.avgResponseTime.toFixed(2) }}ms</div>
          </div>
        </div>
      </el-card>

      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Warning />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">错误率</div>
            <div class="overview-value" :class="overallStats.errorRate > 5 ? 'text-red-500' : 'text-green-500'">
              {{ overallStats.errorRate.toFixed(2) }}%
            </div>
          </div>
        </div>
      </el-card>

      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Connection />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">活跃API</div>
            <div class="overview-value">{{ activeApiCount }}</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 性能分析 -->
    <div class="analysis-grid">
      <el-card class="analysis-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Timer />
            </el-icon>
            <span>响应时间排行</span>
          </div>
        </template>
        <div class="analysis-content">
          <div v-if="slowestApis.length === 0" class="no-data">
            暂无API性能数据
          </div>
          <div v-else v-for="(item, index) in slowestApis" :key="index" class="api-item">
            <div class="api-rank">{{ index + 1 }}</div>
            <div class="api-info">
              <div class="api-name">{{ getApiName(item.endpoint) }}</div>
              <div class="api-method">{{ getMethod(item.endpoint) }}</div>
            </div>
            <div class="api-stats">
              <div class="api-time" :class="getResponseTimeClass(item.avgResponseTime)">
                {{ item.avgResponseTime.toFixed(2) }}ms
              </div>
              <div class="api-calls">{{ item.totalCalls.toLocaleString() }} 次</div>
              <div v-if="item.errorRate > 0" class="api-error">{{ item.errorRate.toFixed(2) }}% 错误</div>
            </div>
          </div>
        </div>
      </el-card>

      <el-card class="analysis-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <TrendCharts />
            </el-icon>
            <span>调用频率排行</span>
          </div>
        </template>
        <div class="analysis-content">
          <div v-if="topFrequencyApis.length === 0" class="no-data">
            暂无API调用频率数据
          </div>
          <div v-else v-for="(item, index) in topFrequencyApis" :key="index" class="api-item">
            <div class="api-rank">{{ index + 1 }}</div>
            <div class="api-info">
              <div class="api-name">{{ getApiName(item.endpoint) }}</div>
              <div class="api-method">{{ getMethod(item.endpoint) }}</div>
            </div>
            <div class="api-stats">
              <div class="api-frequency">{{ item.callsPerMinute.toFixed(1) }}/分钟</div>
              <div class="api-calls">{{ item.totalCalls.toLocaleString() }} 次</div>
              <div class="api-time">{{ item.avgResponseTime.toFixed(2) }}ms</div>
            </div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 慢请求列表 -->
    <div class="slow-requests-section">
      <el-card class="slow-requests-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Warning />
            </el-icon>
            <span>慢请求详情</span>
            <el-button 
              type="primary" 
              size="small" 
              @click="loadSlowRequests"
              :loading="slowRequestsLoading"
              style="margin-left: auto;"
            >
              刷新
            </el-button>
          </div>
        </template>
        <div class="slow-requests-content">
          <div v-if="slowRequestsLoading" class="loading">
            <el-icon class="is-loading"><Loading /></el-icon>
            加载中...
          </div>
          <div v-else-if="slowRequests.length === 0" class="no-data">
            暂无慢请求记录
          </div>
          <div v-else class="slow-requests-list">
            <div v-for="(item, index) in slowRequests" :key="index" class="slow-request-item">
              <div class="request-header">
                <div class="request-method" :class="getMethodClass(item.method)">
                  {{ item.method }}
                </div>
                <div class="request-endpoint">{{ item.endpoint }}</div>
                <div class="request-time" :class="getResponseTimeClass(item.responseTime)">
                  {{ item.responseTime.toFixed(2) }}ms
                </div>
              </div>
              <div class="request-details">
                <div class="detail-item">
                  <span class="detail-label">请求时间:</span>
                  <span class="detail-value">{{ formatTime(item.requestTime) }}</span>
                </div>
                <div class="detail-item">
                  <span class="detail-label">状态码:</span>
                  <span class="detail-value" :class="getStatusCodeClass(item.statusCode)">
                    {{ item.statusCode }}
                  </span>
                </div>
                <div v-if="item.userId" class="detail-item">
                  <span class="detail-label">用户ID:</span>
                  <span class="detail-value">{{ item.userId }}</span>
                </div>
                <div v-if="item.ipAddress" class="detail-item">
                  <span class="detail-label">IP地址:</span>
                  <span class="detail-value">{{ item.ipAddress }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getPerformance, getSlowRequests } from '@/api/monitor'
import { ElMessage } from 'element-plus'
import type { SlowRequest } from '@/types'

// 获取当前路由
const route = useRoute()

// 响应式数据
const overallStats = ref({
  totalCalls: 0,
  totalErrors: 0,
  errorRate: 0,
  avgResponseTime: 0
})

const slowestApis = ref<any[]>([])
const topFrequencyApis = ref<any[]>([])
const slowRequests = ref<SlowRequest[]>([])
const slowRequestsLoading = ref(false)

// 计算活跃API数量
const activeApiCount = computed(() => {
  return slowestApis.value.length
})

// 获取HTTP方法
const getMethod = (endpoint: string) => {
  if (!endpoint) return 'GET'
  const parts = endpoint.split(' ')
  return parts[0] || 'GET'
}

// 获取API名称
const getApiName = (endpoint: string) => {
  if (!endpoint) return '未知接口'
  const parts = endpoint.split(' ')
  const path = parts[1] || endpoint
  const pathParts = path.split('/')
  return pathParts[pathParts.length - 1] || path
}

// 获取响应时间样式类
const getResponseTimeClass = (time: number) => {
  if (time > 1000) return 'text-red-500 font-bold'
  if (time > 500) return 'text-orange-500'
  if (time > 100) return 'text-yellow-500'
  return 'text-green-500'
}

// 获取HTTP方法样式类
const getMethodClass = (method: string) => {
  const methodMap: Record<string, string> = {
    'GET': 'method-get',
    'POST': 'method-post',
    'PUT': 'method-put',
    'DELETE': 'method-delete',
    'PATCH': 'method-patch'
  }
  return methodMap[method.toUpperCase()] || 'method-default'
}

// 获取状态码样式类
const getStatusCodeClass = (statusCode: number) => {
  if (statusCode >= 500) return 'status-error'
  if (statusCode >= 400) return 'status-warning'
  if (statusCode >= 300) return 'status-redirect'
  return 'status-success'
}

// 格式化时间
const formatTime = (timeStr: string) => {
  if (!timeStr) return '未知'
  try {
    const date = new Date(timeStr)
    return date.toLocaleString('zh-CN')
  } catch (error) {
    return timeStr
  }
}

// 加载性能数据
const loadData = async () => {
  try {
    // 调用真实接口
    const response = await getPerformance()
    console.log('Performance - 真实接口调用成功:', response)
    
    // 处理真实接口返回的数据
    if (response && typeof response === 'object') {
      const { apiStatistics } = response as any
      
      if (apiStatistics && Object.keys(apiStatistics).length > 0) {
        // 计算总体统计
        const totalCalls = Object.values(apiStatistics).reduce((sum: number, stat: any) => sum + (stat.totalCalls || 0), 0)
        const totalErrors = Object.values(apiStatistics).reduce((sum: number, stat: any) => sum + (stat.errorCount || 0), 0)
        const avgResponseTime = Object.values(apiStatistics).reduce((sum: number, stat: any) => sum + (stat.avgResponseTime || 0), 0) / Object.keys(apiStatistics).length
        
        overallStats.value = {
          totalCalls,
          totalErrors,
          errorRate: totalCalls > 0 ? (totalErrors / totalCalls) * 100 : 0,
          avgResponseTime: Math.round(avgResponseTime * 100) / 100
        }
        
        // 获取响应时间最慢的API
        slowestApis.value = Object.values(apiStatistics)
          .sort((a: any, b: any) => b.avgResponseTime - a.avgResponseTime)
          .slice(0, 20)
        
        // 获取调用频率最高的API
        topFrequencyApis.value = Object.values(apiStatistics)
          .sort((a: any, b: any) => b.callsPerMinute - a.callsPerMinute)
          .slice(0, 20)
      } else {
        // 如果没有API统计数据，设置默认值
        overallStats.value = {
          totalCalls: 0,
          totalErrors: 0,
          errorRate: 0,
          avgResponseTime: 0
        }
        slowestApis.value = []
        topFrequencyApis.value = []
      }
    }
  } catch (error) {
    console.error('加载性能数据失败:', error)
    ElMessage.error('加载性能数据失败，请检查网络连接')
  }
}

// 加载慢请求数据
const loadSlowRequests = async () => {
  try {
    slowRequestsLoading.value = true
    const response = await getSlowRequests()
    console.log('Slow Requests - 真实接口调用成功:', response)
    
    if (response && typeof response === 'object') {
      const { slowRequests: requests } = response as any
      slowRequests.value = requests || []
    } else {
      slowRequests.value = []
    }
  } catch (error) {
    console.error('加载慢请求数据失败:', error)
    ElMessage.error('加载慢请求数据失败，请检查网络连接')
    slowRequests.value = []
  } finally {
    slowRequestsLoading.value = false
  }
}

onMounted(() => {
  loadData()
  loadSlowRequests()
})

// 监听路由变化，重新加载数据
watch(() => route.path, () => {
  if (route.path === '/performance') {
    console.log('Performance页面路由变化，重新加载数据')
    loadData()
    loadSlowRequests()
  }
})
</script>

<style scoped>
.performance-page {
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

.overview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
}

.overview-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.overview-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.12);
}

.overview-content {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 8px 0;
}

.overview-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: linear-gradient(135deg, #409eff, #67c23a);
  display: flex;
  align-items: center;
  justify-content: center;
}

.overview-icon .icon {
  font-size: 24px;
  color: white;
}

.overview-info {
  flex: 1;
}

.overview-label {
  font-size: 14px;
  color: #909399;
  margin-bottom: 4px;
}

.overview-value {
  font-size: 24px;
  font-weight: 600;
  color: #303133;
}

.analysis-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
}

.analysis-card {
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

.analysis-content {
  padding: 16px 0;
}

.no-data {
  text-align: center;
  color: #909399;
  padding: 40px 0;
  font-size: 14px;
}

.loading {
  text-align: center;
  color: #909399;
  padding: 40px 0;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
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

.api-method {
  font-size: 12px;
  color: #909399;
}

.api-stats {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
}

.api-time {
  font-size: 14px;
  font-weight: 600;
}

.api-frequency {
  font-size: 14px;
  font-weight: 600;
  color: #409eff;
}

.api-calls {
  font-size: 12px;
  color: #909399;
}

.api-error {
  font-size: 12px;
  color: #f56c6c;
}

.slow-requests-section {
  margin-bottom: 32px;
}

.slow-requests-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.slow-requests-content {
  padding: 16px 0;
}

.slow-requests-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.slow-request-item {
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  padding: 16px;
  background: #fafafa;
}

.request-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.request-method {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
  color: white;
}

.method-get {
  background: #67c23a;
}

.method-post {
  background: #409eff;
}

.method-put {
  background: #e6a23c;
}

.method-delete {
  background: #f56c6c;
}

.method-patch {
  background: #909399;
}

.method-default {
  background: #909399;
}

.request-endpoint {
  flex: 1;
  font-size: 14px;
  font-weight: 500;
  color: #303133;
  font-family: 'Courier New', monospace;
}

.request-time {
  font-size: 14px;
  font-weight: 600;
}

.request-details {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 8px;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.detail-label {
  font-size: 12px;
  color: #909399;
}

.detail-value {
  font-size: 12px;
  color: #303133;
  font-weight: 500;
}

.text-red-500 {
  color: #f56c6c;
}

.text-green-500 {
  color: #67c23a;
}

.text-orange-500 {
  color: #e6a23c;
}

.text-yellow-500 {
  color: #f0ad4e;
}

.font-bold {
  font-weight: 700;
}

.status-success {
  color: #67c23a;
}

.status-warning {
  color: #e6a23c;
}

.status-error {
  color: #f56c6c;
}

.status-redirect {
  color: #409eff;
}
</style> 