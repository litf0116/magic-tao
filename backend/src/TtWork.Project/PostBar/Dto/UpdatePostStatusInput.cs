using System.ComponentModel.DataAnnotations;

namespace TtWork.Project.PostBar.Dto
{
    /// <summary>
    /// 更新帖子状态输入参数
    /// </summary>
    public class UpdatePostStatusInput
    {
        /// <summary>
        /// 帖子ID
        /// </summary>
        [Required]
        public long PostId { get; set; }

        /// <summary>
        /// 帖子状态：1=正常, 2=关闭, 3=删除
        /// </summary>
        [Required]
        [Range(1, 3, ErrorMessage = "状态值只能为1（正常）、2（关闭）或3（删除）")]
        public byte Status { get; set; }
    }
}