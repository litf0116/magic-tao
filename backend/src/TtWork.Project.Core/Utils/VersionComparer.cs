using System;

namespace TtWork.Project.Core.Utils;

/// <summary>
/// 版本比较工具类
/// 版本格式: YYYYMMDD@主.次.补 (例: 20260224@1.1.21)
/// </summary>
public static class VersionComparer
{
    /// <summary>
    /// 比较两个版本号
    /// </summary>
    /// <param name="version1">版本1 (格式: YYYYMMDD@主.次.补)</param>
    /// <param name="version2">版本2 (格式: YYYYMMDD@主.次.补)</param>
    /// <returns>
    /// -1: version1 &lt; version2
    /// 0: version1 == version2
    /// 1: version1 &gt; version2
    /// </returns>
    public static int Compare(string? version1, string? version2)
    {
        if (string.IsNullOrEmpty(version1) && string.IsNullOrEmpty(version2))
            return 0;
        if (string.IsNullOrEmpty(version1))
            return -1;
        if (string.IsNullOrEmpty(version2))
            return 1;

        try
        {
            var (date1, semantic1) = ParseVersion(version1);
            var (date2, semantic2) = ParseVersion(version2);

            // 先比较日期部分
            int dateCompare = date1.CompareTo(date2);
            if (dateCompare != 0)
                return dateCompare;

            // 日期相同则比较语义化版本
            return semantic1.CompareTo(semantic2);
        }
        catch
        {
            // 解析失败时按字符串比较
            return string.Compare(version1, version2, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// 检查是否应该显示拍卖场
    /// 逻辑：当前版本 <= 稳定版本 = 已发布版本 = 显示拍卖
    ///      当前版本 > 稳定版本 = 审核中版本 = 隐藏拍卖
    /// </summary>
    /// <param name="currentVersion">当前版本</param>
    /// <param name="stableVersion">稳定版本</param>
    /// <returns>是否允许显示拍卖场</returns>
    public static bool ShouldShowAuction(string? currentVersion, string? stableVersion)
    {
        // 策略A: 无版本号默认显示（保护旧版本用户体验）
        if (string.IsNullOrEmpty(currentVersion))
            return true;
        
        if (string.IsNullOrEmpty(stableVersion))
            return true;

        // 当前版本 <= 稳定版本 = 已发布版本 = 显示拍卖
        // 当前版本 > 稳定版本 = 审核中版本 = 隐藏拍卖
        return Compare(currentVersion, stableVersion) <= 0;
    }

    /// <summary>
    /// 解析版本号
    /// </summary>
    private static (int Date, Version SemanticVersion) ParseVersion(string version)
    {
        var parts = version.Split('@');
        if (parts.Length != 2)
            throw new ArgumentException($"无效的版本格式: {version}");

        int date = int.Parse(parts[0]);
        Version semanticVersion = new Version(parts[1]);

        return (date, semanticVersion);
    }
}