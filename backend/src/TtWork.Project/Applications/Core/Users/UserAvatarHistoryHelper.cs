using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Logging;
using TtWork.Abp.Authorization.Users;
using TtWork.Project.Domains.Pays;

namespace TtWork.Project.Applications.Core.Users
{
    /// <summary>
    /// 用户头像历史记录助手
    /// </summary>
    public class UserAvatarHistoryHelper
    {
        private readonly IRepository<UserAvatarHistory, long> _historyRepository;
        private readonly ILogger<UserAvatarHistoryHelper> _logger;

        public UserAvatarHistoryHelper(
            IRepository<UserAvatarHistory, long> historyRepository,
            ILogger<UserAvatarHistoryHelper> logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        /// <summary>
        /// 记录头像修改历史
        /// </summary>
        public async Task RecordAvatarHistoryAsync(long userId, string oldAvatarUrl, string changeSource = "User")
        {
            if (string.IsNullOrEmpty(oldAvatarUrl))
            {
                return; // 如果没有旧头像，不记录
            }

            var history = new UserAvatarHistory
            {
                UserId = userId,
                PreviousHeadImgUrl = oldAvatarUrl,
                ChangeTime = DateTime.Now,
                ChangeSource = changeSource
            };

            await _historyRepository.InsertAsync(history);

            // 清理超过 5 条的旧记录
            var oldHistories = await _historyRepository.GetAll()
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ChangeTime)
                .Skip(5)
                .ToListAsync();

            if (oldHistories.Any())
            {
                await _historyRepository.HardDeleteManyAsync(oldHistories);
            }

            _logger.LogInformation("记录用户头像历史: UserId={UserId}, OldAvatar={OldAvatar}, Source={Source}",
                userId, oldAvatarUrl, changeSource);
        }

        /// <summary>
        /// 获取用户最近的一条历史记录
        /// </summary>
        public async Task<UserAvatarHistory> GetLastHistoryAsync(long userId)
        {
            return await _historyRepository.GetAll()
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ChangeTime)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// 回退用户头像到上一个状态
        /// </summary>
        public async Task<string> RollbackAvatarAsync(long userId, User user)
        {
            // 获取最近的历史记录
            var lastHistory = await GetLastHistoryAsync(userId);
            if (lastHistory == null)
            {
                return null; // 没有可回退的记录
            }

            var oldAvatar = user.HeadImgUrl;
            var newAvatar = lastHistory.PreviousHeadImgUrl;

            // 恢复头像
            user.HeadImgUrl = newAvatar;

            // 删除已使用的历史记录
            await _historyRepository.DeleteAsync(lastHistory);

            _logger.LogInformation("用户头像已回退: UserId={UserId}, 从={OldAvatar}, 到={NewAvatar}",
                userId, oldAvatar, newAvatar);

            return newAvatar;
        }
    }
}
