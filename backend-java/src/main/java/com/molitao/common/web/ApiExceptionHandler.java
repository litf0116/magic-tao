package com.molitao.common.web;

import com.molitao.common.api.ApiResponse;
import com.molitao.common.api.ValidationError;
import com.molitao.common.exception.UserFriendlyException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.HttpStatus;
import org.springframework.http.converter.HttpMessageNotReadableException;
import org.springframework.security.access.AccessDeniedException;
import org.springframework.validation.FieldError;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.MissingServletRequestParameterException;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.List;

/**
 * 全局异常处理器。
 *
 * <p>将各类异常统一转换为 ABP-兼容的 {@link ApiResponse} 格式。</p>
 *
 * <h3>处理策略</h3>
 * <ul>
 *   <li>{@link UserFriendlyException} → HTTP 200 + {@code success: false}（业务错误，用户可见）</li>
 *   <li>{@code MethodArgumentNotValidException} → HTTP 200 + 字段级 validationErrors</li>
 *   <li>{@code AccessDeniedException} → HTTP 401（由 Spring Security 处理）</li>
 *   <li>未知异常 → HTTP 500（服务器内部错误）</li>
 * </ul>
 */
@RestControllerAdvice
public class ApiExceptionHandler {

    private static final Logger log = LoggerFactory.getLogger(ApiExceptionHandler.class);

    /**
     * 用户友好的业务异常 — 返回 HTTP 200 + success: false。
     *
     * <p>与 ABP 行为一致：业务错误也在 HTTP 200 中返回，
     * 前端通过 success=false 识别错误。</p>
     */
    @ExceptionHandler(UserFriendlyException.class)
    @ResponseStatus(HttpStatus.OK)
    public ApiResponse<Void> handleUserFriendly(UserFriendlyException e) {
        return ApiResponse.fail(e.getMessage(), e.getDetails());
    }

    /**
     * Spring Validation 验证错误 — 返回 HTTP 200 + 字段级验证错误。
     *
     * <p>ABP-兼容的 validationErrors 格式：{ message: "...", members: ["fieldName"] }</p>
     */
    @ExceptionHandler(MethodArgumentNotValidException.class)
    @ResponseStatus(HttpStatus.OK)
    public ApiResponse<Void> handleValidation(MethodArgumentNotValidException e) {
        List<ValidationError> errors = e.getBindingResult().getFieldErrors().stream()
                .map(f -> new ValidationError(f.getDefaultMessage(), List.of(f.getField())))
                .toList();
        return ApiResponse.fail("验证失败", null, errors);
    }

    /**
     * 缺少请求参数 — HTTP 400。
     *
     * <p>请求参数拼写错误或缺失时触发。</p>
     */
    @ExceptionHandler(MissingServletRequestParameterException.class)
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    public ApiResponse<Void> handleMissingParam(MissingServletRequestParameterException e) {
        return ApiResponse.fail("缺少必要参数: " + e.getParameterName());
    }

    /**
     * 请求体格式错误 — HTTP 400。
     *
     * <p>JSON 语法错误或类型不匹配时触发。</p>
     */
    @ExceptionHandler(HttpMessageNotReadableException.class)
    @ResponseStatus(HttpStatus.BAD_REQUEST)
    public ApiResponse<Void> handleMessageNotReadable(HttpMessageNotReadableException e) {
        return ApiResponse.fail("请求数据格式错误", e.getMostSpecificCause().getMessage());
    }

    /**
     * 鉴权错误 — HTTP 401。
     *
     * <p>没有登录或 token 过期，由 Spring Security 触发。</p>
     */
    @ExceptionHandler(AccessDeniedException.class)
    @ResponseStatus(HttpStatus.UNAUTHORIZED)
    public ApiResponse<Void> handleAccessDenied(AccessDeniedException e) {
        return ApiResponse.fail("未授权访问，请先登录");
    }

    /**
     * 兜底：未捕获异常 — HTTP 500。
     *
     * <p>仅返回用户可见的消息，详细异常信息记日志。</p>
     */
    @ExceptionHandler(Exception.class)
    @ResponseStatus(HttpStatus.INTERNAL_SERVER_ERROR)
    public ApiResponse<Void> handleUnknown(Exception e) {
        log.error("未预期的服务器错误", e);
        return ApiResponse.fail("服务器内部错误，请稍后重试",
                e.getClass().getSimpleName() + ": " + e.getMessage());
    }
}
