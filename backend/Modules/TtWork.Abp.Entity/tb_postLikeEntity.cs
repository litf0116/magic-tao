using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 帖子点赞表
	/// </summary>
	[SugarTable("tb_postLike")]
	public class tb_postLike
	{
		/// <summary>
		/// ID
		/// </summary>
        [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true)]
		public long id { get; set; }
		/// <summary>
		/// 帖子ID
		/// </summary>
        [SugarColumn(ColumnName="postId"  )]
		public long postId { get; set; }
		/// <summary>
		/// 用户ID
		/// </summary>
        [SugarColumn(ColumnName="userId"  )]
		public long userId { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
        [SugarColumn(ColumnName="createdAt"  )]
		public DateTime createdAt { get; set; }
	}
}