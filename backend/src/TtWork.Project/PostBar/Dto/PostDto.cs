using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Project.PostBar.Dto
{
    public class PostDto
    {
        /// <summary>
		/// 帖子ID
		/// </summary>
        public long postId { get; set; }
        /// <summary>
        /// 类型ID
        /// </summary>
        public string categoryId { get; set; }
        /// <summary>
        /// 发帖用户ID
        /// </summary>
        public long userId { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public decimal? price { get; set; }
        /// <summary>
        /// 浏览数
        /// </summary>
        public int viewCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int likeCount { get; set; }
        /// <summary>
        /// 回复数
        /// </summary>
        public int replyCount { get; set; }
        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool isTop { get; set; }
        /// <summary>
        /// 是否精华
        /// </summary>
        public bool isEssence { get; set; }
        /// <summary>
        /// 状态:1正常,2关闭,3删除
        /// </summary>
        public bool status { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime createdAt { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime updatedAt { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string categoryName { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string userAvatar { get; set; }
        /// <summary>
        /// 微信
        /// </summary>
        public string wechat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string qq { get; set; }
        /// <summary>
        /// IM编号
        /// </summary>
        public int LastModifierUserId { get; set; }
    }
}
