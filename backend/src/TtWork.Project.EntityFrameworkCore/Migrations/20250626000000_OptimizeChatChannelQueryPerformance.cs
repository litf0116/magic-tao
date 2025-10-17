using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <summary>
    /// 优化聊天频道查询性能的数据库索引
    /// </summary>
    public partial class OptimizeChatChannelQueryPerformance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 为 ChatChannel 表添加复合索引
            // 支持按用户查询活跃频道的复合索引
            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_UserChannels",
                table: "T_ChatChannel",
                columns: new[] { "ChannelType", "IsActive", "User1Id", "User2Id", "LastMessageTime" },
                descending: new[] { false, false, false, false, true });

            // 为 ChatListDelete 表添加复合索引
            // 支持按用户查询删除列表的索引
            migrationBuilder.CreateIndex(
                name: "IX_t_chatlistdelete_UserId_ToUserId",
                table: "t_chatlistdelete",
                columns: new[] { "UserId", "ToUserId" });

            // 为 Message 表添加复合索引优化消息查询（如果频繁查询最后消息）
            migrationBuilder.CreateIndex(
                name: "IX_t_Message_Chan_Time_LastMsg",
                table: "t_message",
                columns: new[] { "Chan", "Time" },
                descending: new bool[] { false, true });

            // 为 Message 表的私聊消息添加索引
            migrationBuilder.CreateIndex(
                name: "IX_t_Message_Private_LastMsg",
                table: "t_message",
                columns: new[] { "From", "To", "Time" },
                descending: new bool[] { false, false, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除创建的索引
            migrationBuilder.DropIndex(
                name: "IX_T_ChatChannel_UserChannels",
                table: "T_ChatChannel");

            migrationBuilder.DropIndex(
                name: "IX_t_chatlistdelete_UserId_ToUserId",
                table: "t_chatlistdelete");

            migrationBuilder.DropIndex(
                name: "IX_t_Message_Chan_Time_LastMsg",
                table: "t_message");

            migrationBuilder.DropIndex(
                name: "IX_t_Message_Private_LastMsg",
                table: "t_message");
        }
    }
}