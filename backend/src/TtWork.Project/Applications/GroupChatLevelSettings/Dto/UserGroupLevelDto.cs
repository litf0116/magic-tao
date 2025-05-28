using FreeIM;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Project.Applications.GroupChatLevelSettings.Dto
{
    public class UserGroupLevelDto
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 群聊等级编号
        /// </summary>
        public int GroupChatId { get; set; }
        /// <summary>
        /// 累计金额
        /// </summary>
        public decimal CumulativeAmount { get; set; }
    }

    public class GroupChatLevelSettingsDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 等级名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 等级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// <summary>
        /// 所需金额
        /// </summary>
        public decimal AmountRequired { get; set; }
        /// <summary>
        /// 左边框颜色
        /// </summary>
        public string BorderColor { get; set; }
        /// <summary>
        /// 右边框颜色
        /// </summary>
        public string RightBorderColor { get; set; }
    }
}
