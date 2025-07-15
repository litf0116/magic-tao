using TtWork.Abp.Entity;

namespace TtWork.Project.Applications.GroupChatLevelSettings.Dto
{
    /// <summary>
    /// 用户等级数据DTO（用于兼容性）
    /// </summary>
    public class UserLevelDataDto
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

    /// <summary>
    /// 用户等级信息DTO
    /// 兼容性设计：同时支持新旧版本小程序的数据访问方式
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

        /// <summary>
        /// 兼容性属性：为了支持旧版本小程序，提供data属性
        /// 旧版本小程序使用 levelResponse.data 获取数据
        /// 新版本小程序使用 levelResponse.userLevel 和 levelResponse.levelSettings 获取数据
        /// </summary>
        public UserLevelDataDto Data => new UserLevelDataDto
        {
            UserLevel = UserLevel,
            LevelSettings = LevelSettings
        };

        /// <summary>
        /// 兼容性属性：为了支持旧版本小程序，提供level属性
        /// 旧版本小程序使用 levelResponse.data.levelSettings.level 获取等级
        /// </summary>
        public int? Level => LevelSettings?.Level;

        /// <summary>
        /// 兼容性属性：为了支持旧版本小程序，提供cumulativeAmount属性
        /// 旧版本小程序使用 levelResponse.data.userLevel.cumulativeAmount 获取累计金额
        /// </summary>
        public decimal? CumulativeAmount => UserLevel?.CumulativeAmount;
    }


} 