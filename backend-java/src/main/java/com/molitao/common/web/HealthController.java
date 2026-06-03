package com.molitao.common.web;

import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

import java.time.LocalDateTime;
import java.util.Map;

/**
 * 健康检查端点 — 验证响应包装器是否正常工作。
 *
 * <p>访问 {@code GET /api/health} 应返回:</p>
 * <pre>{@code
 * {
 *   "__abp": true,
 *   "success": true,
 *   "result": {
 *     "status": "UP",
 *     "timestamp": "2026-06-03 12:00:00"
 *   }
 * }
 * }</pre>
 */
@RestController
public class HealthController {

    @GetMapping("/api/health")
    public Map<String, Object> health() {
        return Map.of(
                "status", "UP",
                "timestamp", LocalDateTime.now().toString()
        );
    }

    /**
     * 测试业务错误 — 抛出 UserFriendlyException，应返回 200 + success:false。
     */
    @GetMapping("/api/health/error")
    public String testError() {
        throw new com.molitao.common.exception.UserFriendlyException(
                "这是一个测试业务错误", "详细调试信息"
        );
    }
}
