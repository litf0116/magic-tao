<template>
  <div class="errors-page">
    <div class="page-header">
      <h1 class="page-title">错误统计</h1>
      <p class="page-description">监控系统错误和异常情况</p>
    </div>
    
    <div class="content-area">
      <el-card class="info-card">
        <template #header>
          <div class="card-header">
            <el-icon class="card-icon">
              <Warning />
            </el-icon>
            <span>开发进度</span>
          </div>
        </template>
        <div class="card-content">
          <p>错误统计页面正在开发中...</p>
          <p>将包含以下功能：</p>
          <ul>
            <li>错误率趋势图表</li>
            <li>错误类型分布</li>
            <li>错误详情列表</li>
            <li>告警配置</li>
          </ul>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
// 错误统计页面
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

.content-area {
  display: grid;
  gap: 24px;
}

.info-card {
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

.card-content {
  padding: 16px 0;
}

.card-content p {
  margin: 0 0 16px 0;
  color: #606266;
  line-height: 1.6;
}

.card-content ul {
  margin: 0;
  padding-left: 20px;
  color: #606266;
}

.card-content li {
  margin-bottom: 8px;
  line-height: 1.6;
}
</style> 