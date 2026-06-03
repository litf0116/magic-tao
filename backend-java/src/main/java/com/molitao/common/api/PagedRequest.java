package com.molitao.common.api;

/**
 * ABP-兼容的分页请求参数。
 *
 * <p>序列化为: {@code { sorting: "...", skipCount: 0, maxResultCount: 20 }}</p>
 */
public class PagedRequest {

    /** 排序字段 + 方向，如 "creationTime desc"、"price asc" */
    private String sorting;

    /** 跳过的记录数 = (pageIndex - 1) * maxResultCount */
    private int skipCount;

    /** 每页条数（默认 20） */
    private int maxResultCount = 20;

    public PagedRequest() {}

    // ========== 便捷构造 ==========

    public static PagedRequest of(int pageIndex, int pageSize) {
        PagedRequest req = new PagedRequest();
        req.skipCount = (pageIndex - 1) * pageSize;
        req.maxResultCount = pageSize;
        return req;
    }

    public static PagedRequest of(int pageIndex, int pageSize, String sorting) {
        PagedRequest req = of(pageIndex, pageSize);
        req.sorting = sorting;
        return req;
    }

    // ========== Getters / Setters ==========

    public String getSorting() { return sorting; }
    public void setSorting(String sorting) { this.sorting = sorting; }

    public int getSkipCount() { return skipCount; }
    public void setSkipCount(int skipCount) { this.skipCount = skipCount; }

    public int getMaxResultCount() { return maxResultCount; }
    public void setMaxResultCount(int maxResultCount) { this.maxResultCount = maxResultCount; }
}
