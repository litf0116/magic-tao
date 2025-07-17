<template>
  <div class="errors-page">
    <div class="page-header">
      <h1 class="page-title">错误统计</h1>
      <p class="page-description">监控系统错误和异常情况</p>
    </div>
    
    <!-- 错误概览 -->
    <div class="overview-grid">
      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Warning />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">总错误数</div>
            <div class="overview-value text-red-500">{{ overallStats.totalErrors.toLocaleString() }}</div>
          </div>
        </div>
      </el-card>

      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <TrendCharts />
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
            <div class="overview-label">总调用数</div>
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
    </div>

    <!-- 错误分析 -->
    <div class="analysis-grid">
      <el-card class="analysis-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Warning />
            </el-icon>
            <span>错误率排行</span>
          </div>
        </template>
        <div class="analysis-content">
          <div v-if="topErrorApis.length === 0" class="no-data">
            暂无错误数据
          </div>
          <div v-else v-for="(item, index) in topErrorApis" :key="index" class="api-item">
            <div class="api-rank error-rank">{{ index + 1 }}</div>
            <div class="api-info">
              <div class="api-name">{{ getApiName(item.endpoint) }}</div>
              <div class="api-method">{{ getMethod(item.endpoint) }}</div>
            </div>
            <div class="api-stats">
              <div class="api-error">{{ item.errorRate.toFixed(2) }}% 错误</div>
              <div class="api-calls">{{ item.totalCalls.toLocaleString() }} 次</div>
              <div class="api-errors">{{ item.errorCount.toLocaleString() }} 次错误</div>
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
            <span>错误分布</span>
          </div>
        </template>
        <div class="analysis-content">
          <div v-if="errorDistribution.length === 0" class="no-data">
            暂无错误分布数据
          </div>
          <div v-else class="error-distribution">
            <div v-for="(item, index) in errorDistribution" :key="index" class="error-item">
              <div class="error-rank">{{ index + 1 }}</div>
              <div class="error-info">
                <div class="error-endpoint">{{ item.endpoint }}</div>
                <div class="error-stats">
                  <span class="error-count">{{ item.errorCount.toLocaleString() }} 次错误</span>
                  <span class="error-rate">{{ item.errorRate.toFixed(2) }}% 错误率</span>
                </div>
              </div>
              <div class="error-bar">
                <div class="error-progress" :style="{ width: (item.errorRate / maxErrorRate * 100) + '%' }"></div>
              </div>
            </div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 错误详情 -->
    <div class="detail-section">
      <el-card class="detail-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Document />
            </el-icon>
            <span>错误详情统计</span>
          </div>
        </template>
        <div class="detail-content">
          <div v-if="topErrorApis.length === 0" class="no-data">
            暂无错误详情数据
          </div>
          <div v-else class="error-details">
            <div v-for="(item, index) in topErrorApis" :key="index" class="error-detail-item">
              <div class="detail-header">
                <div class="detail-method" :class="getMethodClass(item.endpoint)">
                  {{ getMethod(item.endpoint) }}
                </div>
                <div class="detail-endpoint">{{ item.endpoint }}</div>
                <div class="detail-error-rate" :class="item.errorRate > 10 ? 'text-red-500' : 'text-orange-500'">
                  {{ item.errorRate.toFixed(2) }}%
                </div>
              </div>
              <div class="detail-stats">
                <div class="stat-item">
                  <span class="stat-label">总调用:</span>
                  <span class="stat-value">{{ item.totalCalls.toLocaleString() }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">错误次数:</span>
                  <span class="stat-value text-red-500">{{ item.errorCount.toLocaleString() }}</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">平均响应时间:</span>
                  <span class="stat-value">{{ item.avgResponseTime.toFixed(2) }}ms</span>
                </div>
                <div class="stat-item">
                  <span class="stat-label">最后调用:</span>
                  <span class="stat-value">{{ formatTime(item.lastCallTime) }}</span>
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
import { getPerformance } from '@/api/monitor'
import { ElMessage } from 'element-plus'

// 获取当前路由
const route = useRoute()

// 响应式数据
const overallStats = ref({
  totalCalls: 0,
  totalErrors: 0,
  errorRate: 0,
  avgResponseTime: 0
})

const topErrorApis = ref<any[]>([])

// 计算错误分布数据
const errorDistribution = computed(() => {
  return topErrorApis.value.map(item => ({
    endpoint: getApiName(item.endpoint),
    errorCount: item.errorCount,
    errorRate: item.errorRate
  }))
})

// 计算最大错误率（用于进度条显示）
const maxErrorRate = computed(() => {
  if (errorDistribution.value.length === 0) return 1
  return Math.max(...errorDistribution.value.map(item => item.errorRate))
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

// 获取HTTP方法样式类
const getMethodClass = (endpoint: string) => {
  const method = getMethod(endpoint)
  const methodMap: Record<string, string> = {
    'GET': 'method-get',
    'POST': 'method-post',
    'PUT': 'method-put',
    'DELETE': 'method-delete',
    'PATCH': 'method-patch'
  }
  return methodMap[method.toUpperCase()] || 'method-default'
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

// 加载数据
const loadData = async () => {
  try {
    // 调用真实接口
    const response = await getPerformance()
    console.log('Errors - 真实接口调用成功:', response)
    
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
        
        // 获取错误率最高的API
        topErrorApis.value = Object.values(apiStatistics)
          .filter((stat: any) => stat.errorRate > 0)
          .sort((a: any, b: any) => b.errorRate - a.errorRate)
          .slice(0, 20)
      } else {
        // 如果没有API统计数据，设置默认值
        overallStats.value = {
          totalCalls: 0,
          totalErrors: 0,
          errorRate: 0,
          avgResponseTime: 0
        }
        topErrorApis.value = []
      }
    }
  } catch (error) {
    console.error('加载数据失败:', error)
    ElMessage.error('加载错误数据失败，请检查网络连接')
  }
}

// 监听路由变化，重新加载数据
watch(() => route.path, () => {
  if (route.path === '/errors') {
    console.log('Errors页面路由变化，重新加载数据')
    loadData()
  }
})

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.errors-page {
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

.api-error {
  font-size: 14px;
  font-weight: 600;
  color: #f56c6c;
}

.api-calls {
  font-size: 12px;
  color: #909399;
}

.api-errors {
  font-size: 12px;
  color: #f56c6c;
}

.error-distribution {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.error-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}

.error-item:last-child {
  border-bottom: none;
}

.error-info {
  flex: 1;
  min-width: 0;
}

.error-endpoint {
  font-size: 14px;
  font-weight: 500;
  color: #303133;
  margin-bottom: 4px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.error-stats {
  display: flex;
  gap: 12px;
  font-size: 12px;
  color: #909399;
}

.error-count {
  color: #f56c6c;
}

.error-rate {
  color: #e6a23c;
}

.error-bar {
  width: 100px;
  height: 8px;
  background: #f0f0f0;
  border-radius: 4px;
  overflow: hidden;
  flex-shrink: 0;
}

.error-progress {
  height: 100%;
  background: linear-gradient(90deg, #f56c6c, #e6a23c);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.detail-section {
  margin-bottom: 32px;
}

.detail-card {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.detail-content {
  padding: 16px 0;
}

.error-details {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.error-detail-item {
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  padding: 16px;
  background: #fafafa;
}

.detail-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}

.detail-method {
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

.detail-endpoint {
  flex: 1;
  font-size: 14px;
  font-weight: 500;
  color: #303133;
  font-family: 'Courier New', monospace;
}

.detail-error-rate {
  font-size: 14px;
  font-weight: 600;
}

.detail-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 8px;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 8px;
}

.stat-label {
  font-size: 12px;
  color: #909399;
}

.stat-value {
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
</style> 