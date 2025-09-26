using SqlSugar;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [SugarColumn(ColumnName="postId", IsPrimaryKey = true, IsIdentity = true)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long postId { get; set; }

		/// <summary>
		/// 类型ID
		/// </summary>
        [SugarColumn(ColumnName="categoryId")]
		[StringLength(255)]
		public string categoryId { get; set; }

		/// <summary>
		/// 发帖用户ID
		/// </summary>
        [SugarColumn(ColumnName="userId")]
		[Required]
		public long userId { get; set; }

		/// <summary>
		/// 标题
		/// </summary>
        [SugarColumn(ColumnName="title")]
		[Required]
		[StringLength(100)]
		public string title { get; set; }

		/// <summary>
		/// 内容
		/// </summary>
        [SugarColumn(ColumnName="content", ColumnDataType = "text")]
		[Required]
		[Column(TypeName = "text")]
		public string content { get; set; }

		/// <summary>
		/// 价格
		/// </summary>
        [SugarColumn(ColumnName="price", ColumnDataType = "decimal(10,2)")]
		[Column(TypeName = "decimal(10,2)")]
		public decimal? price { get; set; }

		/// <summary>
		/// 浏览数
		/// </summary>
        [SugarColumn(ColumnName="viewCount")]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int viewCount { get; set; } = 0;

		/// <summary>
		/// 点赞数
		/// </summary>
        [SugarColumn(ColumnName="likeCount")]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int likeCount { get; set; } = 0;

		/// <summary>
		/// 回复数
		/// </summary>
        [SugarColumn(ColumnName="replyCount")]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public int replyCount { get; set; } = 0;

		/// <summary>
		/// 是否置顶
		/// </summary>
        [SugarColumn(ColumnName="isTop", ColumnDataType = "tinyint(1)")]
		[Column(TypeName = "tinyint(1)")]
		public bool isTop { get; set; } = false;

		/// <summary>
		/// 是否精华
		/// </summary>
        [SugarColumn(ColumnName="isEssence", ColumnDataType = "tinyint(1)")]
		[Column(TypeName = "tinyint(1)")]
		public bool isEssence { get; set; } = false;

		/// <summary>
		/// 状态:1正常,2关闭,3删除
		/// </summary>
        [SugarColumn(ColumnName="status", ColumnDataType = "tinyint")]
		[Column(TypeName = "tinyint")]
		public byte status { get; set; } = 1;

		/// <summary>
		/// 创建时间
		/// </summary>
        [SugarColumn(ColumnName="createdAt")]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime createdAt { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
        [SugarColumn(ColumnName="updatedAt")]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public DateTime updatedAt { get; set; }
	}
}