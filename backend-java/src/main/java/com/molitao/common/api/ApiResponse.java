package com.molitao.common.api;

import com.fasterxml.jackson.annotation.JsonInclude;

/**
 * ABP-兼容的全局响应包装器。
 *
 * <p>所有 Controller 返回值自动包装为 {@code { success: true, result: ..., error: null }} 格式，
 * 与现有 C# ABP Framework 的 AjaxResponse 格式完全一致。</p>
 *
 * <h3>核心规则</h3>
 * <ul>
 *   <li>成功: {@code ApiResponse.of(result)} → { success: true, result: ..., error: null }</li>
 *   <li>业务错误: {@code ApiResponse.fail(message)} → { success: false, error: { message: "..." } }</li>
 *   <li>未授权: 由 Spring Security 返回 401，不经过此包装</li>
 * </ul>
 */
@JsonInclude(JsonInclude.Include.NON_NULL)
public class ApiResponse<T> {

    /** ABP 框架标识字段，固定 true，前端兼容性保留 */
    private Boolean __abp = true;

    /** 是否成功 */
    private boolean success;

    /** 业务数据（成功时存在） */
    private T result;

    /** 错误信息（失败时存在） */
    private ErrorInfo error;

    /** 目标跳转 URL（ABP 兼容保留） */
    private String targetUrl;

    /** ABP 兼容字段 */
    private boolean unAuthorizedRequest;

    // ========== 构造 ==========

    private ApiResponse() {}

    // ========== 静态工厂：成功 ==========

    /** 成功，带业务数据 */
    public static <T> ApiResponse<T> ok(T result) {
        ApiResponse<T> resp = new ApiResponse<>();
        resp.success = true;
        resp.result = result;
        resp.error = null;
        return resp;
    }

    /** 成功，无数据（void 操作） */
    public static ApiResponse<Void> ok() {
        ApiResponse<Void> resp = new ApiResponse<>();
        resp.success = true;
        resp.result = null;
        resp.error = null;
        return resp;
    }

    // ========== 静态工厂：失败 ==========

    /** 普通业务错误 */
    public static ApiResponse<Void> fail(String message) {
        return fail(message, null);
    }

    /** 带详情的业务错误 */
    public static ApiResponse<Void> fail(String message, String details) {
        return fail(message, details, null);
    }

    /** 带字段级验证错误的业务错误 */
    public static ApiResponse<Void> fail(String message, String details,
                                          java.util.List<ValidationError> validationErrors) {
        ApiResponse<Void> resp = new ApiResponse<>();
        resp.success = false;
        resp.error = new ErrorInfo(message, details, validationErrors);
        return resp;
    }

    // ========== Getters ==========

    public Boolean get__abp() { return __abp; }
    public boolean isSuccess() { return success; }
    public T getResult() { return result; }
    public ErrorInfo getError() { return error; }
    public String getTargetUrl() { return targetUrl; }
    public boolean isUnAuthorizedRequest() { return unAuthorizedRequest; }

    // ========== Setters (Lombok 未启用时兼容) ==========

    public void set__abp(Boolean __abp) { this.__abp = __abp; }
    public void setSuccess(boolean success) { this.success = success; }
    public void setResult(T result) { this.result = result; }
    public void setError(ErrorInfo error) { this.error = error; }
    public void setTargetUrl(String targetUrl) { this.targetUrl = targetUrl; }
    public void setUnAuthorizedRequest(boolean unAuthorizedRequest) { this.unAuthorizedRequest = unAuthorizedRequest; }
}
