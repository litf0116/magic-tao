using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <summary>
    /// 添加高级群等级设置
    /// 支持第7级：成交额满158888 - 军神の李贝留斯（黑底金边效果）
    /// 支持第8级：成交额满308888 - 主神の阿尔杰斯（彩虹渐变边框效果）
    /// </summary>
    public partial class AddPremiumGroupLevels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 插入第7级等级设置：军神の李贝留斯
            migrationBuilder.Sql(@"
                INSERT INTO t_GroupChatLevelSettings (Name, Level, AmountRequired, BorderColor, RightBorderColor)
                VALUES ('军神の李贝留斯', 7, 158888.00, '#FFD700', '#000000');
            ");

            // 插入第8级等级设置：主神の阿尔杰斯
            migrationBuilder.Sql(@"
                INSERT INTO t_GroupChatLevelSettings (Name, Level, AmountRequired, BorderColor, RightBorderColor)
                VALUES ('主神の阿尔杰斯', 8, 308888.00, 'linear-gradient(45deg, #FF6B6B, #4ECDC4, #45B7D1, #FFA07A, #98D8C8, #F7DC6F)', '#FFD700');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除新增的高级等级记录（如果需要回滚）
            migrationBuilder.Sql(@"
                DELETE FROM t_GroupChatLevelSettings
                WHERE Level IN (7, 8);
            ");
        }
    }
}