using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 回复点赞表
	/// </summary>
	[SugarTable("tb_replyLike")]
	public class tb_replyLike
	{
		/// <summary>
		/// ID
		/// </summary>
        [SugarColumn(ColumnName="id" ,IsPrimaryKey = true ,IsIdentity = true)]
		public long id { get; set; }
		/// <summary>
		/// 回复ID
		/// </summary>
        [SugarColumn(ColumnName="replyId"  )]
		public long replyId { get; set; }
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