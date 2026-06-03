package com.molitao.common.exception;

/**
 * 用户可见的业务异常。
 *
 * <p>抛出此异常时，全局异常处理器会返回 HTTP 200 + {@code { success: false, error: { message: "..." } }}，
 * 而非 HTTP 4xx，以保持与 ABP 框架行为一致。</p>
 *
 * <p>使用示例:</p>
 * <pre>{@code
 * if (user == null) {
 *     throw new UserFriendlyException("用户不存在或已注销");
 * }
 * }</pre>
 */
public class UserFriendlyException extends RuntimeException {

    /** 详细调试信息（可选） */
    private final String details;

    public UserFriendlyException(String message) {
        super(message);
        this.details = null;
    }

    public UserFriendlyException(String message, String details) {
        super(message);
        this.details = details;
    }

    public UserFriendlyException(String message, String details, Throwable cause) {
        super(message, cause);
        this.details = details;
    }

    public String getDetails() {
        return details;
    }
}
