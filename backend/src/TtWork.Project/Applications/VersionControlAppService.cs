using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.Configuration;
using Abp.UI;
using TtWork.Project.Core;

namespace TtWork.Project.Applications;

/// <summary>
/// 版本控制管理服务
/// </summary>
public class VersionControlAppService : ApplicationService
{
    private readonly ISettingManager _settingManager;

    public VersionControlAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    /// <summary>
    /// 获取当前稳定版本号
    /// </summary>
    public async Task<string> GetLatestStableVersion()
    {
        return await SettingManager.GetSettingValueAsync(
            AppSettings.VersionControl.LatestStableVersion
        );
    }

    /// <summary>
    /// 更新稳定版本号
    /// </summary>
    /// <param name="version">版本号 (格式: YYYYMMDD@主.次.补)</param>
    public async Task UpdateLatestStableVersion(string version)
    {
        // 验证版本格式
        if (!IsValidVersionFormat(version))
        {
            throw new UserFriendlyException("版本格式无效，正确格式: YYYYMMDD@主.次.补");
        }

        await SettingManager.ChangeSettingForApplicationAsync(
            AppSettings.VersionControl.LatestStableVersion,
            version
        );
    }

    /// <summary>
    /// 验证版本格式
    /// </summary>
    private bool IsValidVersionFormat(string version)
    {
        if (string.IsNullOrEmpty(version))
            return false;

        var parts = version.Split('@');
        if (parts.Length != 2)
            return false;

        // 验证日期部分 (8位数字)
        if (parts[0].Length != 8 || !int.TryParse(parts[0], out _))
            return false;

        // 验证语义化版本部分
        return System.Version.TryParse(parts[1], out _);
    }
}