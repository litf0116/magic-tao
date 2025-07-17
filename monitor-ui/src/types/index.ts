// 系统性能统计接口
export interface SystemPerformance {
  result: {
    system: {
      workingSet: number; // 工作集内存 (MB)
      privateMemory: number; // 私有内存 (MB)
      cpuTime: number; // CPU时间
      threadCount: number; // 线程数
      handleCount: number; // 句柄数
      startTime: string; // 启动时间
      uptime: string; // 运行时间
    };
    gc: {
      generation0: number; // GC代数统计
      generation1: number;
      generation2: number;
      totalMemory: number; // 托管内存 (MB)
    };
    apiStatistics: Record<string, ApiStatistic>;
  };
  targetUrl: string | null;
  success: boolean;
  error: string | null;
  unAuthorizedRequest: boolean;
  __abp: boolean;
}

// API统计接口
export interface ApiStatistic {
  endpoint: string;
  totalCalls: number;
  errorCount: number;
  errorRate: number;
  avgResponseTime: number;
  maxResponseTime: number;
  minResponseTime: number;
  lastCallTime: string;
  callsPerMinute: number;
}

// 系统健康检查接口
export interface HealthCheck {
  Status: string;
  TotalDuration: number;
  Checks: HealthCheckItem[];
}

// 健康检查项接口
export interface HealthCheckItem {
  Name: string;
  Status: string;
  Description: string;
  Duration: number;
  Exception?: string;
}

// 慢请求接口
export interface SlowRequest {
  endpoint: string; // 接口路径
  responseTime: number; // 响应时间 (ms)
  requestTime: string; // 请求时间
  method: string; // HTTP方法
  statusCode: number; // 状态码
  userId?: string; // 用户ID（可选）
  ipAddress?: string; // IP地址（可选）
}

// 慢请求列表响应接口
export interface SlowRequestsResponse {
  slowRequests: SlowRequest[];
  totalCount: number;
  threshold: number; // 慢请求阈值 (ms)
}

// 错误统计接口
export interface ErrorStatistic {
  TotalErrors: number;
  ErrorRate: number;
  ErrorDetails: ErrorDetail[];
}

// 错误详情接口
export interface ErrorDetail {
  Endpoint: string;
  ErrorCount: number;
  LastErrorTime: string;
  ErrorMessage: string;
}

// 图表数据接口
export interface ChartData {
  label: string;
  count: number;
  [key: string]: any;
}

// API响应接口
export interface ApiResponse<T = any> {
  success: boolean;
  result?: T;
  data?: T;
  message?: string;
  code?: number;
} 