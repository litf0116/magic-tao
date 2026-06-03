package com.molitao.common.api;

import java.util.List;

/**
 * ABP-兼容的字段级验证错误。
 *
 * <p>序列化为: {@code { message: "...", members: ["FieldName"] }}</p>
 */
public class ValidationError {

    /** 验证失败消息 */
    private String message;

    /** 失败字段名列表（ABP 风格，PascalCase 字段名） */
    private List<String> members;

    public ValidationError() {}

    public ValidationError(String message, List<String> members) {
        this.message = message;
        this.members = members;
    }

    // ========== Getters / Setters ==========

    public String getMessage() { return message; }
    public void setMessage(String message) { this.message = message; }

    public List<String> getMembers() { return members; }
    public void setMembers(List<String> members) { this.members = members; }
}
