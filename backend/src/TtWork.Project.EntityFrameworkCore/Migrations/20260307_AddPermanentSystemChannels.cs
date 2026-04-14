using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <summary>
    /// 添加常驻系统频道：系统公告和新手版主群聊
    /// 这些频道将始终显示，不受版本控制限制
    /// </summary>
    public partial class AddPermanentSystemChannels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 插入系统公告频道
            migrationBuilder.InsertData(
                table: "T_ChatChannel",
                columns: new[] { 
                    "ChannelId", "ChannelType", "ChannelName", 
                    "IsActive", "SortOrder", "CreationTime",
                    "LastMessageTime", "MessageCount"
                },
                values: new object[,]
                {
                    { 
                        "-10_announcement", 2, "系统公告", 
                        true, 98, DateTime.Now,
                        0L, 0
                    },
                    { 
                        "-11_newbie", 2, "新手版主群聊", 
                        true, 97, DateTime.Now,
                        0L, 0
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除这两个系统频道
            migrationBuilder.DeleteData(
                table: "T_ChatChannel",
                keyColumn: "ChannelId",
                keyValues: new object[] { "-10_announcement", "-11_newbie" });
        }
    }
}