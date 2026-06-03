package com.molitao.common.api;

import java.util.Collections;
import java.util.List;

/**
 * ABP-兼容的分页响应体。
 *
 * <p>序列化为: {@code { totalCount: 100, items: [...] }}</p>
 *
 * <p>注意：PagedResult 本身不是完整的 ApiResponse，
 * 它作为 {@code ApiResponse< PagedResult<T> >.ok(result)} 的 result 嵌套在标准包装中。</p>
 */
public class PagedResult<T> {

    /** 满足过滤条件的总记录数 */
    private long totalCount;

    /** 当前页数据列表 */
    private List<T> items;

    public PagedResult() {}

    public PagedResult(long totalCount, List<T> items) {
        this.totalCount = totalCount;
        this.items = items != null ? items : Collections.emptyList();
    }

    /** 空分页结果 */
    public static <T> PagedResult<T> empty() {
        return new PagedResult<>(0, Collections.emptyList());
    }

    // ========== Getters / Setters ==========

    public long getTotalCount() { return totalCount; }
    public void setTotalCount(long totalCount) { this.totalCount = totalCount; }

    public List<T> getItems() { return items; }
    public void setItems(List<T> items) { this.items = items; }
}
