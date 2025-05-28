using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 贴吧公告
	/// </summary>
	[SugarTable("tb_postBulletin")]
	public class tb_postBulletinEntity
    {
		/// <summary>
		/// 贴吧公告编号
		/// </summary>
        [SugarColumn(ColumnName="Id" ,IsPrimaryKey = true ,IsIdentity = true)]
		public int Id { get; set; }
		/// <summary>
		/// 标题
		/// </summary>
        [SugarColumn(ColumnName="Title"  )]
		public string Title { get; set; }
		/// <summary>
		/// 内容
		/// </summary>
        [SugarColumn(ColumnName="Content"  )]
		public string Content { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
        [SugarColumn(ColumnName="CreateTime"  )]
		public DateTime? CreateTime { get; set; }
	}
}