using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TtWork.Abp;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications
{
    [Route("api/services/app/[controller]/[action]")]
    public class AppReleaseAppService : AbpAppServiceBase
    {
        private readonly IRepository<AppRelease, long> _appReleaseRepository;
        private readonly IWebHostEnvironment _env;

        public AppReleaseAppService(
            IRepository<AppRelease, long> appReleaseRepository,
            IWebHostEnvironment env)
        {
            _appReleaseRepository = appReleaseRepository;
            _env = env;
        }

        /// <summary>
        /// 上传并发布新版本
        /// </summary>
        [HttpPost]
        [AbpAuthorize(AppPermissions.Administration)]
        public async Task<long> PublishAppRelease(
            [FromForm] string versionName,
            [FromForm] int versionCode,
            [FromForm] string description,
            [FromForm] bool isForceUpdate,
            [FromForm] IFormFile file,
            [FromForm] string platform = "android")
        {
            if (file == null || file.Length == 0)
            {
                throw new UserFriendlyException("请选择APK文件");
            }

            // 验证文件类型
            if (!file.FileName.ToLower().EndsWith(".apk") && !file.FileName.ToLower().EndsWith(".wgt"))
            {
                throw new UserFriendlyException("仅支持APK或WGT文件");
            }

            // 创建uploads目录
            string uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "apps");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // 生成唯一文件名
            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{platform}_{versionCode}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
            string filePath = Path.Combine(uploadsDir, uniqueFileName);

            // 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 创建发布记录
            var appRelease = new AppRelease
            {
                VersionName = versionName,
                VersionCode = versionCode,
                Description = description,
                DownloadUrl = $"/uploads/apps/{uniqueFileName}",
                FileName = file.FileName,
                FileSize = file.Length,
                IsForceUpdate = isForceUpdate,
                Platform = platform,
                ReleaseDate = DateTime.Now,
                IsActive = true
            };

            await _appReleaseRepository.InsertAsync(appRelease);

            // 停用旧版本（同一平台的）
            var oldReleases = await _appReleaseRepository.GetAllListAsync(x =>
                x.Platform == platform && x.VersionCode < versionCode && x.IsActive);

            foreach (var oldRelease in oldReleases)
            {
                oldRelease.IsActive = false;
                await _appReleaseRepository.UpdateAsync(oldRelease);
            }

            return appRelease.Id;
        }

        /// <summary>
        /// 检查更新（无需登录）
        /// </summary>
        [HttpGet]
        public async Task<object> CheckUpdate(int currentVersionCode, string platform = "android")
        {
            var latestRelease = await _appReleaseRepository.GetAllListAsync(x =>
                x.Platform == platform && x.IsActive);

            var release = latestRelease.OrderByDescending(x => x.VersionCode).FirstOrDefault();

            if (release == null)
            {
                return new
                {
                    HasUpdate = false,
                    LatestVersionCode = 0,
                    LatestVersionName = ""
                };
            }

            return new
            {
                HasUpdate = release.VersionCode > currentVersionCode,
                LatestVersionCode = release.VersionCode,
                LatestVersionName = release.VersionName,
                Description = release.Description,
                DownloadUrl = release.DownloadUrl,
                FileName = release.FileName,
                FileSize = release.FileSize,
                IsForceUpdate = release.IsForceUpdate,
                ReleaseDate = release.ReleaseDate.ToString("yyyy-MM-dd HH:mm")
            };
        }

        /// <summary>
        /// 获取版本历史
        /// </summary>
        [HttpGet]
        public async Task<object> GetReleaseHistory(string platform = "android")
        {
            var releases = await _appReleaseRepository.GetAllListAsync(x => x.Platform == platform);

            return new
            {
                Items = releases.OrderByDescending(x => x.VersionCode).Select(x => new
                {
                    x.Id,
                    x.VersionName,
                    x.VersionCode,
                    x.Description,
                    x.FileName,
                    x.FileSize,
                    x.IsForceUpdate,
                    x.Platform,
                    x.ReleaseDate,
                    x.IsActive,
                    x.DownloadUrl,
                    x.CreationTime
                })
            };
        }

        /// <summary>
        /// 删除版本（管理员）
        /// </summary>
        [HttpDelete]
        [AbpAuthorize(AppPermissions.Administration)]
        public async Task DeleteRelease(long id)
        {
            var release = await _appReleaseRepository.GetAsync(id);
            await _appReleaseRepository.DeleteAsync(release);
        }

        /// <summary>
        /// 切换版本激活状态（管理员）
        /// </summary>
        [HttpPost]
        [AbpAuthorize(AppPermissions.Administration)]
        public async Task ToggleRelease(long id)
        {
            var release = await _appReleaseRepository.GetAsync(id);
            release.IsActive = !release.IsActive;
            await _appReleaseRepository.UpdateAsync(release);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}