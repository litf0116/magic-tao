using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 回复表
	/// </summary>
	[SugarTable("tb_reply")]
	public class tb_reply
	{
		/// <summary>
		/// 回复ID
		/// </summary>
        [SugarColumn(ColumnName="replyId" ,IsPrimaryKey = true ,IsIdentity = true)]
		public long replyId { get; set; }
		/// <summary>
		/// 帖子ID
		/// </summary>
        [SugarColumn(ColumnName="postId"  )]
		public long postId { get; set; }
		/// <summary>
		/// 回复用户ID
		/// </summary>
        [SugarColumn(ColumnName="userId"  )]
		public long userId { get; set; }
		/// <summary>
		/// 回复内容
		/// </summary>
        [SugarColumn(ColumnName="content"  )]
		public string content { get; set; }
		/// <summary>
		/// 父回复ID,用于楼中楼
		/// </summary>
        [SugarColumn(ColumnName="parentId"  )]
		public long? parentId { get; set; }
		/// <summary>
		/// 点赞数
		/// </summary>
        [SugarColumn(ColumnName="likeCount"  )]
		public int likeCount { get; set; }
		/// <summary>
		/// 状态:1正常,2删除
		/// </summary>
        [SugarColumn(ColumnName="status"  )]
		public byte status { get; set; }
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