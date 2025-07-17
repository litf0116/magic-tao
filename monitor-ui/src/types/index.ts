// 系统性能统计接口
export interface SystemPerformance {
  System:[object Object]WorkingSet: number; // 工作集内存 (MB)
    PrivateMemory: number; // 私有内存 (MB)
    CpuTime: number; // CPU时间
    ThreadCount: number; // 线程数
    HandleCount: number; // 句柄数
    StartTime: string; // 启动时间
    Uptime: string; // 运行时间
  };
  GC: {
    Generation0 number; // GC代数统计
    Generation1: number;
    Generation2: number;
    TotalMemory: number; // 托管内存 (MB)
  };
  ApiStatistics: Record<string, ApiStatistic>;
}

// API统计接口
export interface ApiStatistic {
  Endpoint: string;
  TotalCalls: number;
  ErrorCount: number;
  ErrorRate: number;
  AvgResponseTime: number;
  MaxResponseTime: number;
  MinResponseTime: number;
  LastCallTime: string;
  CallsPerMinute: number;
}

// 系统健康检查接口
export interface HealthCheck [object Object]  Status: string;
  TotalDuration: number;
  Checks: HealthCheckItem
// 健康检查项接口
export interface HealthCheckItem [object Object]
  Name: string;
  Status: string;
  Description: string;
  Duration: number;
  Exception?: string;
}

// 慢请求接口
export interface SlowRequest {
  Endpoint: string;
  AvgResponseTime: number;
  TotalCalls: number;
  ErrorRate: number;
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
export interface ChartData[object Object]
  label: string;
  count: number;
  [key: string]: any;
}

// API响应接口
export interface ApiResponse<T = any> {
  success: boolean;
  data: T;
  message?: string;
  code?: number;
} 