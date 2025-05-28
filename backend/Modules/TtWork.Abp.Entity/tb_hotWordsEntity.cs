using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 热词
	/// </summary>
	[SugarTable("tb_hotWords")]
	public class tb_hotWordsEntity
	{
		/// <summary>
		/// 热词编号
		/// </summary>
        [SugarColumn(ColumnName="Id" ,IsPrimaryKey = true )]
		public long Id { get; set; }
		/// <summary>
		/// 标题
		/// </summary>
        [SugarColumn(ColumnName="Title"  )]
		public string Title { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
        [SugarColumn(ColumnName="CreateTime"  )]
		public DateTime? CreateTime { get; set; }
	}
}