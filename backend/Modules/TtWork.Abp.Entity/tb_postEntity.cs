using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 帖子表
	/// </summary>
	[SugarTable("tb_post")]
	public class tb_post
	{
		/// <summary>
		/// 帖子ID
		/// </summary>
        [SugarColumn(ColumnName="postId" ,IsPrimaryKey = true ,IsIdentity = true)]
		public long postId { get; set; }
		/// <summary>
		/// 类型ID
		/// </summary>
        [SugarColumn(ColumnName="categoryId"  )]
		public string categoryId { get; set; }
		/// <summary>
		/// 发帖用户ID
		/// </summary>
        [SugarColumn(ColumnName="userId"  )]
		public long userId { get; set; }
		/// <summary>
		/// 标题
		/// </summary>
        [SugarColumn(ColumnName="title"  )]
		public string title { get; set; }
		/// <summary>
		/// 内容
		/// </summary>
        [SugarColumn(ColumnName="content"  )]
		public string content { get; set; }
		/// <summary>
		/// 价格
		/// </summary>
        [SugarColumn(ColumnName="price"  )]
		public decimal? price { get; set; }
		/// <summary>
		/// 浏览数
		/// </summary>
        [SugarColumn(ColumnName="viewCount"  )]
		public int viewCount { get; set; }
		/// <summary>
		/// 点赞数
		/// </summary>
        [SugarColumn(ColumnName="likeCount"  )]
		public int likeCount { get; set; }
		/// <summary>
		/// 回复数
		/// </summary>
        [SugarColumn(ColumnName="replyCount"  )]
		public int replyCount { get; set; }
		/// <summary>
		/// 是否置顶
		/// </summary>
        [SugarColumn(ColumnName="isTop"  )]
		public bool isTop { get; set; }
		/// <summary>
		/// 是否精华
		/// </summary>
        [SugarColumn(ColumnName="isEssence"  )]
		public bool isEssence { get; set; }
		/// <summary>
		/// 状态:1正常,2关闭,3删除
		/// </summary>
        [SugarColumn(ColumnName="status"  )]
		public bool status { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
        [SugarColumn(ColumnName="createdAt"  )]
		public DateTime createdAt { get; set; }
		/// <summary>
		/// 更新时间
		/// </summary>
        [SugarColumn(ColumnName="updatedAt"  )]
		public DateTime updatedAt { get; set; }
	}
}