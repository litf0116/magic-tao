using System;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Applications.Core.Users
{
    /// <summary>
    /// 用户头像历史 API 服务
    /// </summary>
    [AbpAuthorize]
    public class UserAvatarHistoryAppService
    {
        private readonly IRepository<User, long> _userRepository;
        private readonly UserManager _userManager;
        private readonly UserAvatarHistoryHelper _historyHelper;
        private readonly ILogger<UserAvatarHistoryAppService> _logger;

        public UserAvatarHistoryAppService(
            IRepository<User, long> userRepository,
            UserManager userManager,
            UserAvatarHistoryHelper historyHelper,
            ILogger<UserAvatarHistoryAppService> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _historyHelper = historyHelper;
            _logger = logger;
        }

        /// <summary>
        /// 回退用户头像到上一个状态（仅管理员）
        /// </summary>
        [AbpAuthorize]
        public async Task<string> RollbackAvatar(long userId)
        {
            var user = await _userManager.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new UserFriendlyException("用户不存在");
            }

            var result = await _historyHelper.RollbackAvatarAsync(userId, user);
            if (result == null)
            {
                throw new UserFriendlyException("没有可回退的头像记录");
            }

            await _userManager.UpdateAsync(user);
            return result;
        }
    }
}
