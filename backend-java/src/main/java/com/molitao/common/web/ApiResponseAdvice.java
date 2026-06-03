package com.molitao.common.web;

import com.molitao.common.api.ApiResponse;
import org.springframework.core.MethodParameter;
import org.springframework.core.io.Resource;
import org.springframework.http.MediaType;
import org.springframework.http.converter.HttpMessageConverter;
import org.springframework.http.server.ServerHttpRequest;
import org.springframework.http.server.ServerHttpResponse;
import org.springframework.web.bind.annotation.RestControllerAdvice;
import org.springframework.web.servlet.mvc.method.annotation.ResponseBodyAdvice;

/**
 * 全局响应包装器。
 *
 * <p>所有 {@code @RestController} 的返回值自动包装为 ABP 兼容的 {@link ApiResponse} 格式。
 * Spring Security 的异常（401/403）由 {@link ApiExceptionHandler} 处理，不经此包装。</p>
 *
 * <h3>包装规则</h3>
 * <ul>
 *   <li>返回值已是 {@code ApiResponse} → 跳过，不重复包装</li>
 *   <li>返回值是 {@code String} → 跳过（由 StringHttpMessageConverter 处理）</li>
 *   <li>返回值是 {@code Resource}（文件下载） → 跳过</li>
 *   <li>返回 {@code void} / {@code ResponseEntity} → 跳过</li>
 *   <li>其他 → 自动 {@code ApiResponse.ok(result)}</li>
 * </ul>
 */
@RestControllerAdvice
public class ApiResponseAdvice implements ResponseBodyAdvice<Object> {

    @Override
    public boolean supports(MethodParameter returnType,
                            Class<? extends HttpMessageConverter<?>> converterType) {
        // 返回值类型为 ApiResponse → 不重复包装
        if (returnType.getParameterType() == ApiResponse.class) {
            return false;
        }
        // String → StringHttpMessageConverter 处理，不包装
        if (returnType.getParameterType() == String.class) {
            return false;
        }
        return true;
    }

    @Override
    public Object beforeBodyWrite(Object body,
                                  MethodParameter returnType,
                                  MediaType selectedContentType,
                                  Class<? extends HttpMessageConverter<?>> selectedConverterType,
                                  ServerHttpRequest request,
                                  ServerHttpResponse response) {

        // 已包装过或 null body → 直接返回
        if (body instanceof ApiResponse) {
            return body;
        }

        // Resource（文件下载）→ 不包装
        if (body instanceof Resource) {
            return body;
        }

        return ApiResponse.ok(body);
    }
}
