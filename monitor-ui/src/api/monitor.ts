import request from '@/utils/request'
import type { 
  HealthCheck, 
  SlowRequest, 
  ErrorStatistic 
} from '@/types'

// 系统健康检查
export const getHealthCheck = () => {
  return request.get<HealthCheck>('/api/monitor/health')
}

// 系统性能统计 - 对应 /api/monitor/performance 接口
export const getPerformance = () => {
  return request.get<{
    system: {
      workingSet: number;
      privateMemory: number;
      cpuTime: number;
      threadCount: number;
      handleCount: number;
      startTime: string;
      uptime: string;
    };
    gc: {
      generation0: number;
      generation1: number;
      generation2: number;
      totalMemory: number;
    };
    apiStatistics: Record<string, {
      endpoint: string;
      totalCalls: number;
      errorCount: number;
      errorRate: number;
      avgResponseTime: number;
      maxResponseTime: number;
      minResponseTime: number;
      lastCallTime: string;
      callsPerMinute: number;
    }>;
  }>('/api/monitor/performance')
}

// 获取慢请求列表
export const getSlowRequests = () => {
  return request.get<{ SlowRequests: SlowRequest[] }>('/api/monitor/slow-requests')
}

// 获取错误统计
export const getErrorStatistics = () => {
  return request.get<ErrorStatistic>('/api/monitor/errors')
}

 