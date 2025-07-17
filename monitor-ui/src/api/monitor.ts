import request from @/utils/request
import type { 
  SystemPerformance, 
  HealthCheck, 
  SlowRequest, 
  ErrorStatistic 
} from @/types'

// 系统健康检查
export const getHealthCheck = () => [object Object]return request.get<HealthCheck>('/api/monitor/health')
}

// 系统性能统计
export const getPerformance = () => [object Object]return request.get<SystemPerformance>('/api/monitor/performance')
}

// 获取慢请求列表
export const getSlowRequests = () => [object Object]return request.get<{ SlowRequests: SlowRequest[] }>(/api/monitor/slow-requests')
}

// 获取错误统计
export const getErrorStatistics = () => [object Object]return request.get<ErrorStatistic>('/api/monitor/errors')
} 