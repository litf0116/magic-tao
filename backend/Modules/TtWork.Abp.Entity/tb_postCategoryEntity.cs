using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 帖子类型表
	/// </summary>
	[SugarTable("tb_postCategory")]
	public class tb_postCategory
	{
		/// <summary>
		/// 类型ID
		/// </summary>
        [SugarColumn(ColumnName="categoryId" ,IsPrimaryKey = true ,IsIdentity = true)]
		public int categoryId { get; set; }
		/// <summary>
		/// 类型名称
		/// </summary>
        [SugarColumn(ColumnName="name"  )]
		public string name { get; set; }
		/// <summary>
		/// 排序
		/// </summary>
        [SugarColumn(ColumnName="sort"  )]
		public int sort { get; set; }
		/// <summary>
		/// 状态:1启用,2禁用
		/// </summary>
        [SugarColumn(ColumnName="status"  )]
		public int status { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime createdAt { get; set; }
		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime updatedAt { get; set; }
	}
}