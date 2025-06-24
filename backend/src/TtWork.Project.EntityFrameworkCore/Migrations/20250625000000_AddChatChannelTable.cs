using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <summary>
    /// 添加聊天频道表以优化聊天列表查询性能
    /// </summary>
    public partial class AddChatChannelTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 创建聊天频道表
            migrationBuilder.CreateTable(
                name: "T_ChatChannel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    User1Id = table.Column<long>(type: "bigint", nullable: true),
                    User2Id = table.Column<long>(type: "bigint", nullable: true),
                    LastMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastMessageContent = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    LastMessageFromId = table.Column<long>(type: "bigint", nullable: true),
                    LastMessageFromName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    LastMessageFromAvatar = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    LastMessageTime = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    MessageCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ChatChannel", x => x.Id);
                });

            // 创建唯一索引确保频道ID不重复
            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_ChannelId",
                table: "T_ChatChannel",
                column: "ChannelId",
                unique: true);

            // 创建索引优化查询性能
            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_ChannelType_IsActive",
                table: "T_ChatChannel",
                columns: new[] { "ChannelType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_User1Id_User2Id",
                table: "T_ChatChannel",
                columns: new[] { "User1Id", "User2Id" });

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_LastMessageTime",
                table: "T_ChatChannel",
                column: "LastMessageTime");

            // 初始化系统频道数据
            migrationBuilder.InsertData(
                table: "T_ChatChannel",
                columns: new[] { "ChannelId", "ChannelType", "ChannelName", "IsActive", "SortOrder", "CreationTime" },
                values: new object[,]
                {
                    { "-1_auction", 2, "拍卖频道", true, 99, DateTime.Now },
                    { "0_lobby", 2, "大厅", true, 100, DateTime.Now }
                });

            // 根据现有消息数据生成聊天频道记录
            migrationBuilder.Sql(@"
                -- 1. 生成系统频道记录（基于Chan字段）
                INSERT INTO T_ChatChannel (
                    ChannelId, ChannelType, ChannelName, User1Id, User2Id,
                    LastMessageId, LastMessageContent, LastMessageFromId, 
                    LastMessageFromName, LastMessageFromAvatar, LastMessageTime, 
                    IsActive, SortOrder, MessageCount, CreationTime
                )
                SELECT 
                    m.Chan as ChannelId,
                    2 as ChannelType,  -- 系统频道
                    CASE 
                        WHEN m.Chan = 'auction' THEN '拍卖频道'
                        WHEN m.Chan = 'lobby' THEN '大厅'
                        WHEN m.Chan = '0' THEN '大厅'
                        WHEN m.Chan = '-1' THEN '拍卖频道'
                        ELSE CONCAT('频道-', m.Chan)
                    END as ChannelName,
                    NULL as User1Id,
                    NULL as User2Id,
                    latest.Id as LastMessageId,
                    latest.Msg as LastMessageContent,
                    latest.From as LastMessageFromId,
                    latest.FromName as LastMessageFromName,
                    latest.Avatar as LastMessageFromAvatar,
                    latest.Time as LastMessageTime,
                    1 as IsActive,
                    CASE 
                        WHEN m.Chan = 'lobby' OR m.Chan = '0' THEN 100
                        WHEN m.Chan = 'auction' OR m.Chan = '-1' THEN 99
                        ELSE 0
                    END as SortOrder,
                    msg_count.MessageCount,
                    NOW() as CreationTime
                FROM (
                    SELECT DISTINCT Chan 
                    FROM t_message 
                    WHERE Chan IS NOT NULL AND Chan != ''
                ) m
                LEFT JOIN (
                    SELECT Chan, COUNT(*) as MessageCount
                    FROM t_message
                    WHERE Chan IS NOT NULL AND Chan != ''
                    GROUP BY Chan
                ) msg_count ON m.Chan = msg_count.Chan
                LEFT JOIN (
                    SELECT m1.*
                    FROM t_message m1
                    INNER JOIN (
                        SELECT Chan, MAX(Time) as MaxTime
                        FROM t_message
                        WHERE Chan IS NOT NULL AND Chan != ''
                        GROUP BY Chan
                    ) m2 ON m1.Chan = m2.Chan AND m1.Time = m2.MaxTime
                ) latest ON m.Chan = latest.Chan
                WHERE NOT EXISTS (
                    SELECT 1 FROM T_ChatChannel cc WHERE cc.ChannelId = m.Chan
                );

                -- 2. 生成私聊频道记录（基于From和To字段）
                INSERT INTO T_ChatChannel (
                    ChannelId, ChannelType, ChannelName, User1Id, User2Id,
                    LastMessageId, LastMessageContent, LastMessageFromId, 
                    LastMessageFromName, LastMessageFromAvatar, LastMessageTime, 
                    IsActive, SortOrder, MessageCount, CreationTime
                )
                SELECT 
                    CONCAT('private_', 
                           CASE WHEN m.User1Id < m.User2Id THEN m.User1Id ELSE m.User2Id END,
                           '_',
                           CASE WHEN m.User1Id > m.User2Id THEN m.User1Id ELSE m.User2Id END
                    ) as ChannelId,
                    1 as ChannelType,  -- 私聊频道
                    NULL as ChannelName,
                    CASE WHEN m.User1Id < m.User2Id THEN m.User1Id ELSE m.User2Id END as User1Id,
                    CASE WHEN m.User1Id > m.User2Id THEN m.User1Id ELSE m.User2Id END as User2Id,
                    latest.Id as LastMessageId,
                    latest.Msg as LastMessageContent,
                    latest.From as LastMessageFromId,
                    latest.FromName as LastMessageFromName,
                    latest.Avatar as LastMessageFromAvatar,
                    latest.Time as LastMessageTime,
                    1 as IsActive,
                    0 as SortOrder,
                    msg_count.MessageCount,
                    NOW() as CreationTime
                FROM (
                    SELECT DISTINCT
                        From as User1Id,
                        To as User2Id
                    FROM t_message 
                    WHERE Chan IS NULL AND To IS NOT NULL
                    UNION
                    SELECT DISTINCT
                        To as User1Id,
                        From as User2Id
                    FROM t_message 
                    WHERE Chan IS NULL AND To IS NOT NULL
                ) m
                LEFT JOIN (
                    SELECT 
                        CASE WHEN From < To THEN From ELSE To END as User1Id,
                        CASE WHEN From > To THEN From ELSE To END as User2Id,
                        COUNT(*) as MessageCount
                    FROM t_message
                    WHERE Chan IS NULL AND To IS NOT NULL
                    GROUP BY 
                        CASE WHEN From < To THEN From ELSE To END,
                        CASE WHEN From > To THEN From ELSE To END
                ) msg_count ON 
                    CASE WHEN m.User1Id < m.User2Id THEN m.User1Id ELSE m.User2Id END = msg_count.User1Id
                    AND CASE WHEN m.User1Id > m.User2Id THEN m.User1Id ELSE m.User2Id END = msg_count.User2Id
                LEFT JOIN (
                    SELECT m1.*
                    FROM t_message m1
                    INNER JOIN (
                        SELECT 
                            CASE WHEN From < To THEN From ELSE To END as User1Id,
                            CASE WHEN From > To THEN From ELSE To END as User2Id,
                            MAX(Time) as MaxTime
                        FROM t_message
                        WHERE Chan IS NULL AND To IS NOT NULL
                        GROUP BY 
                            CASE WHEN From < To THEN From ELSE To END,
                            CASE WHEN From > To THEN From ELSE To END
                    ) m2 ON 
                        CASE WHEN m1.From < m1.To THEN m1.From ELSE m1.To END = m2.User1Id
                        AND CASE WHEN m1.From > m1.To THEN m1.From ELSE m1.To END = m2.User2Id
                        AND m1.Time = m2.MaxTime
                    WHERE m1.Chan IS NULL AND m1.To IS NOT NULL
                ) latest ON 
                    CASE WHEN m.User1Id < m.User2Id THEN m.User1Id ELSE m.User2Id END = 
                    CASE WHEN latest.From < latest.To THEN latest.From ELSE latest.To END
                    AND CASE WHEN m.User1Id > m.User2Id THEN m.User1Id ELSE m.User2Id END = 
                    CASE WHEN latest.From > latest.To THEN latest.From ELSE latest.To END
                WHERE m.User1Id != m.User2Id  -- 排除自己和自己的对话
                AND NOT EXISTS (
                    SELECT 1 FROM T_ChatChannel cc 
                    WHERE cc.ChannelId = CONCAT('private_', 
                                               CASE WHEN m.User1Id < m.User2Id THEN m.User1Id ELSE m.User2Id END,
                                               '_',
                                               CASE WHEN m.User1Id > m.User2Id THEN m.User1Id ELSE m.User2Id END)
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_ChatChannel");
        }
    }
}
