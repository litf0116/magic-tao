using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <summary>
    /// 为ChatChannel表添加用户状态字段
    /// 支持在单个记录中管理两个用户的删除状态，优化查询性能
    /// </summary>
    public partial class AddUserStatusFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 添加用户状态字段
            migrationBuilder.AddColumn<int>(
                name: "User1Status",
                table: "T_ChatChannel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "User2Status",
                table: "T_ChatChannel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // 创建优化查询的复合索引
            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_UserStatus_Optimized",
                table: "T_ChatChannel",
                columns: new[] { "User1Id", "User1Status", "User2Id", "User2Status", "ChannelType", "IsActive", "LastMessageTime" },
                descending: new bool[] { false, false, false, false, false, false, true });

            // 迁移现有数据：将t_chatlistdelete中的删除状态迁移到ChatChannel
            migrationBuilder.Sql(@"
                UPDATE c SET
                    c.User1Status = CASE WHEN d.UserId = c.User1Id THEN 1 ELSE 0 END,
                    c.User2Status = CASE WHEN d.UserId = c.User2Id THEN 1 ELSE 0 END
                FROM T_ChatChannel c
                INNER JOIN t_chatlistdelete d ON (
                    c.ChannelType = 1 AND
                    ((c.User1Id = d.UserId AND c.User2Id = d.ToUserId) OR
                     (c.User2Id = d.UserId AND c.User1Id = d.ToUserId))
                )
                WHERE c.IsActive = 1 AND (d.UserId IS NOT NULL);
            ");

            // 为私聊频道初始化用户状态
            migrationBuilder.Sql(@"
                UPDATE T_ChatChannel
                SET User1Status = 0, User2Status = 0
                WHERE ChannelType = 1 AND (User1Status IS NULL OR User2Status IS NULL);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 如果需要回滚，可以恢复删除表的数据
            migrationBuilder.Sql(@"
                -- 创建临时表存储当前的用户状态
                CREATE TABLE #TempDeleteList (
                    UserId BIGINT,
                    ToUserId BIGINT
                );

                -- 从ChatChannel恢复删除状态到临时表
                INSERT INTO #TempDeleteList (UserId, ToUserId)
                SELECT
                    c.User1Id as UserId,
                    c.User2Id as ToUserId
                FROM T_ChatChannel c
                WHERE c.ChannelType = 1 AND c.User1Status = 1

                UNION ALL

                SELECT
                    c.User2Id as UserId,
                    c.User1Id as ToUserId
                FROM T_ChatChannel c
                WHERE c.ChannelType = 1 AND c.User2Status = 1;

                -- 从临时表恢复到t_chatlistdelete（避免重复）
                INSERT INTO t_chatlistdelete (UserId, ToUserId, CreationTime)
                SELECT t.UserId, t.ToUserId, GETDATE()
                FROM #TempDeleteList t
                WHERE NOT EXISTS (
                    SELECT 1 FROM t_chatlistdelete d
                    WHERE d.UserId = t.UserId AND d.ToUserId = t.ToUserId
                );

                -- 清理临时表
                DROP TABLE #TempDeleteList;
            ");

            migrationBuilder.DropIndex(
                name: "IX_T_ChatChannel_UserStatus_Optimized",
                table: "T_ChatChannel");

            migrationBuilder.DropColumn(
                name: "User2Status",
                table: "T_ChatChannel");

            migrationBuilder.DropColumn(
                name: "User1Status",
                table: "T_ChatChannel");
        }
    }
}