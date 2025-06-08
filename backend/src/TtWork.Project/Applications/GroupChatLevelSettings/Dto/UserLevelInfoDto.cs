using TtWork.Abp.Entity;

namespace TtWork.Project.Applications.GroupChatLevelSettings.Dto
{
    /// <summary>
    /// 用户等级信息DTO
    /// </summary>
    public class UserLevelInfoDto
    {
        /// <summary>
        /// 用户等级信息
        /// </summary>
        public UserGroupLevelEntity UserLevel { get; set; }

        /// <summary>
        /// 等级配置信息
        /// </summary>
        public GroupChatLevelSettingsEntity LevelSettings { get; set; }
    }
} 