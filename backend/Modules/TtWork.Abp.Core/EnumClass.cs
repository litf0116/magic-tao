using System.ComponentModel.DataAnnotations;

namespace TtWork
{
    public interface IHaveState
    {
        TtWork.EnumClass.DefaultListStatus State { get; set; }
    }


    public class EnumClass
    {
        public enum DefaultListStatus
        {
            [Display(Name = "不通过")] Deleted = -1,
            [Display(Name = "草稿")] Saved = 0,
            [Display(Name = "退回")] SendBacked = 1,
            [Display(Name = "导入")] Import = 2,
            [Display(Name = "提交")] Submitted = 5,
            [Display(Name = "审批通过")] Approved = 6
        }
    }
}