package com.molitao.common.api;

import com.fasterxml.jackson.annotation.JsonInclude;
import java.util.List;

/**
 * ABP-兼容的错误信息结构。
 *
 * <p>序列化为: {@code { code: 0, message: "...", details: "...", validationErrors: [...] }}</p>
 */
@JsonInclude(JsonInclude.Include.NON_NULL)
public class ErrorInfo {

    /** 错误码（保留 ABP 兼容字段，暂不使用） */
    private int code;

    /** 用户可见的错误消息 */
    private String message;

    /** 详细错误信息（调试用） */
    private String details;

    /** 字段级验证错误列表 */
    private List<ValidationError> validationErrors;

    public ErrorInfo() {}

    public ErrorInfo(String message, String details,
                     List<ValidationError> validationErrors) {
        this.code = 0;
        this.message = message;
        this.details = details;
        this.validationErrors = validationErrors;
    }

    // ========== Getters / Setters ==========

    public int getCode() { return code; }
    public void setCode(int code) { this.code = code; }

    public String getMessage() { return message; }
    public void setMessage(String message) { this.message = message; }

    public String getDetails() { return details; }
    public void setDetails(String details) { this.details = details; }

    public List<ValidationError> getValidationErrors() { return validationErrors; }
    public void setValidationErrors(List<ValidationError> validationErrors) {
        this.validationErrors = validationErrors;
    }
}
