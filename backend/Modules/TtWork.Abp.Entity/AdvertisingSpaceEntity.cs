using SqlSugar;

namespace TtWork.Abp.Entity
{
	/// <summary>
	/// 广告位
	/// </summary>
	[SugarTable("t_advertisingSpace")]
	public class AdvertisingSpaceEntity
	{
		/// <summary>
		/// 广告位编号
		/// </summary>
        [SugarColumn(ColumnName="Id" ,IsPrimaryKey = true )]
		public long Id { get; set; }
		/// <summary>
		/// 标题
		/// </summary>
        [SugarColumn(ColumnName="Title"  )]
		public string Title { get; set; }
		/// <summary>
		/// 类型：1 首页 2 贴吧
		/// </summary>
        [SugarColumn(ColumnName="Type"  )]
		public int? Type { get; set; }
        /// <summary>
        /// 状态：1正常 0 禁用
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 跳转地址
        /// </summary>
        [SugarColumn(ColumnName="Url"  )]
		public string Url { get; set; }
		/// <summary>
		/// 图片地址
		/// </summary>
        [SugarColumn(ColumnName= "ImageUrl")]
		public string ImageUrl { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
        [SugarColumn(ColumnName="CreateTime"  )]
		public DateTime? CreateTime { get; set; }
	}
}
