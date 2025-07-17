<template>
  <div class="system-page">
    <div class="page-header">
      <h1 class="page-title">系统资源监控</h1>
      <p class="page-description">实时监控系统资源使用情况</p>
    </div>
    
    <!-- 系统概览 -->
    <div class="overview-grid">
      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Cpu />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">工作集内存</div>
            <div class="overview-value">{{ systemOverview.workingSet || 0 }} MB</div>
          </div>
        </div>
      </el-card>

      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Monitor />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">私有内存</div>
            <div class="overview-value">{{ systemOverview.privateMemory || 0 }} MB</div>
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
            <div class="overview-label">线程数量</div>
            <div class="overview-value">{{ systemOverview.threadCount || 0 }}</div>
          </div>
        </div>
      </el-card>

      <el-card class="overview-card">
        <div class="overview-content">
          <div class="overview-icon">
            <el-icon class="icon">
              <Setting />
            </el-icon>
          </div>
          <div class="overview-info">
            <div class="overview-label">句柄数量</div>
            <div class="overview-value">{{ systemOverview.handleCount || 0 }}</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 详细监控 -->
    <div class="detail-grid">
      <el-card class="detail-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <TrendCharts />
            </el-icon>
            <span>内存使用情况</span>
          </div>
        </template>
        <div class="memory-info">
          <div class="memory-item">
            <div class="memory-label">工作集内存</div>
            <div class="memory-value">{{ systemOverview.workingSet || 0 }} MB</div>
            <div class="memory-bar">
              <div class="memory-progress" :style="{ width: getMemoryPercentage(systemOverview.workingSet, systemOverview.totalMemory) + '%' }"></div>
            </div>
          </div>
          <div class="memory-item">
            <div class="memory-label">私有内存</div>
            <div class="memory-value">{{ systemOverview.privateMemory || 0 }} MB</div>
            <div class="memory-bar">
              <div class="memory-progress" :style="{ width: getMemoryPercentage(systemOverview.privateMemory, systemOverview.totalMemory) + '%' }"></div>
            </div>
          </div>
          <div class="memory-item">
            <div class="memory-label">托管内存</div>
            <div class="memory-value">{{ systemOverview.totalMemory || 0 }} MB</div>
            <div class="memory-bar">
              <div class="memory-progress" :style="{ width: '100%' }"></div>
            </div>
          </div>
        </div>
      </el-card>

      <el-card class="detail-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Delete />
            </el-icon>
            <span>垃圾回收统计</span>
          </div>
        </template>
        <div class="gc-info">
          <div class="gc-item">
            <div class="gc-label">Generation 0</div>
            <div class="gc-value">{{ systemOverview.generation0 || 0 }}</div>
            <div class="gc-description">短期对象回收次数</div>
          </div>
          <div class="gc-item">
            <div class="gc-label">Generation 1</div>
            <div class="gc-value">{{ systemOverview.generation1 || 0 }}</div>
            <div class="gc-description">中期对象回收次数</div>
          </div>
          <div class="gc-item">
            <div class="gc-label">Generation 2</div>
            <div class="gc-value">{{ systemOverview.generation2 || 0 }}</div>
            <div class="gc-description">长期对象回收次数</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- 系统信息 -->
    <div class="info-grid">
      <el-card class="detail-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Timer />
            </el-icon>
            <span>系统信息</span>
          </div>
        </template>
        <div class="system-info">
          <div class="info-item">
            <span class="info-label">启动时间:</span>
            <span class="info-value">{{ formatStartTime(systemOverview.startTime) }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">运行时长:</span>
            <span class="info-value">{{ formatUptime(systemOverview.uptime) }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">CPU时间:</span>
            <span class="info-value">{{ formatCpuTime(systemOverview.cpuTime) }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">句柄数量:</span>
            <span class="info-value">{{ systemOverview.handleCount || 0 }}</span>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute } from 'vue-router'
import { getPerformance } from '@/api/monitor'
import { ElMessage } from 'element-plus'

// 获取当前路由
const route = useRoute()

// 响应式数据
const systemOverview = ref({
  workingSet: 0,
  privateMemory: 0,
  threadCount: 0,
  uptime: '',
  startTime: '',
  cpuTime: 0,
  handleCount: 0,
  totalMemory: 0,
  generation0: 0,
  generation1: 0,
  generation2: 0
})

// 格式化运行时间
const formatUptime = (uptime: string) => {
  if (!uptime) return '未知'
  const parts = uptime.split(':')
  if (parts.length >= 3) {
    const hours = parseInt(parts[0])
    const minutes = parseInt(parts[1])
    const seconds = parseInt(parts[2].split('.')[0])
    
    if (hours > 24) {
      const days = Math.floor(hours / 24)
      const remainingHours = hours % 24
      return `${days}天 ${remainingHours}小时 ${minutes}分钟`
    } else {
      return `${hours}小时 ${minutes}分钟 ${seconds}秒`
    }
  }
  return uptime
}

// 格式化启动时间
const formatStartTime = (startTime: string) => {
  if (!startTime) return '未知'
  try {
    const date = new Date(startTime)
    return date.toLocaleString('zh-CN')
  } catch (error) {
    return startTime
  }
}

// 格式化CPU时间
const formatCpuTime = (cpuTime: number) => {
  if (!cpuTime) return '0ms'
  return `${cpuTime.toLocaleString()}ms`
}

// 计算内存使用百分比
const getMemoryPercentage = (current: number, total: number) => {
  if (total === 0) return 0
  return Math.min((current / total) * 100, 100)
}

// 加载数据
const loadData = async () => {
  try {
    // 调用真实接口
    const response = await getPerformance()
    console.log('System - 真实接口调用成功:', response)
    
    // 处理真实接口返回的数据
    if (response && typeof response === 'object') {
      const { system, gc } = response as any
      
      // 更新系统概览数据
      systemOverview.value = {
        workingSet: system?.workingSet || 0,
        privateMemory: system?.privateMemory || 0,
        threadCount: system?.threadCount || 0,
        uptime: system?.uptime || '',
        startTime: system?.startTime || '',
        cpuTime: system?.cpuTime || 0,
        handleCount: system?.handleCount || 0,
        totalMemory: gc?.totalMemory || 0,
        generation0: gc?.generation0 || 0,
        generation1: gc?.generation1 || 0,
        generation2: gc?.generation2 || 0
      }
    }
  } catch (error) {
    console.error('加载数据失败:', error)
    ElMessage.error('加载系统数据失败，请检查网络连接')
  }
}

onMounted(() => {
  loadData()
})

// 监听路由变化，重新加载数据
watch(() => route.path, () => {
  if (route.path === '/system') {
    console.log('System页面路由变化，重新加载数据')
    loadData()
  }
})
</script>

<style scoped>
.system-page {
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

.detail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
}

.detail-card {
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

.memory-info {
  padding: 16px 0;
}

.memory-item {
  margin-bottom: 20px;
}

.memory-item:last-child {
  margin-bottom: 0;
}

.memory-label {
  font-size: 14px;
  color: #606266;
  margin-bottom: 8px;
}

.memory-value {
  font-size: 16px;
  font-weight: 600;
  color: #303133;
  margin-bottom: 8px;
}

.memory-bar {
  width: 100%;
  height: 8px;
  background: #f0f0f0;
  border-radius: 4px;
  overflow: hidden;
}

.memory-progress {
  height: 100%;
  background: linear-gradient(90deg, #409eff, #67c23a);
  border-radius: 4px;
  transition: width 0.3s ease;
}

.gc-info {
  padding: 16px 0;
}

.gc-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}

.gc-item:last-child {
  border-bottom: none;
}

.gc-label {
  font-size: 14px;
  color: #606266;
  flex: 1;
}

.gc-value {
  font-size: 18px;
  font-weight: 600;
  color: #409eff;
  margin: 0 16px;
}

.gc-description {
  font-size: 12px;
  color: #909399;
  flex: 1;
  text-align: right;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}

.system-info {
  padding: 16px 0;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
}

.info-item:last-child {
  border-bottom: none;
}

.info-label {
  font-size: 14px;
  color: #606266;
}

.info-value {
  font-size: 14px;
  font-weight: 500;
  color: #303133;
}
</style> 