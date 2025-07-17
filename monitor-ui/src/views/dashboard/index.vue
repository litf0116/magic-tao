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
            <div class="metric-label">内存使用</div>
            <div class="metric-value">1,280 MB</div>
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
            <div class="metric-value">23</div>
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
            <div class="metric-label">运行时间</div>
            <div class="metric-value">2天 5小时</div>
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
            <span>API调用统计</span>
          </div>
        </template>
        <div class="chart-placeholder">
          <el-icon class="placeholder-icon">
            <TrendCharts />
          </el-icon>
          <p>图表加载中...</p>
        </div>
      </el-card>
      
      <el-card class="chart-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Warning />
            </el-icon>
            <span>错误统计</span>
          </div>
        </template>
        <div class="chart-placeholder">
          <el-icon class="placeholder-icon">
            <Warning />
          </el-icon>
          <p>图表加载中...</p>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
// 仪表板页面
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

.chart-placeholder {
  height: 300px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #c0c4cc;
}

.placeholder-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.chart-placeholder p {
  margin: 0;
  font-size: 14px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .metrics-grid {
    grid-template-columns: 1fr;
  }
  
  .charts-grid {
    grid-template-columns: 1fr;
  }
  
  .page-title {
    font-size: 24px;
  }
}
</style>