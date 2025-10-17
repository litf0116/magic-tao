using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class AddChatChannelUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "T_ChatChannel",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChannelId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChannelType = table.Column<int>(type: "int", nullable: false),
                    ChannelName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    User1Id = table.Column<long>(type: "bigint", nullable: true),
                    User2Id = table.Column<long>(type: "bigint", nullable: true),
                    User1Status = table.Column<int>(type: "int", nullable: false),
                    User2Status = table.Column<int>(type: "int", nullable: false),
                    LastMessageId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastMessageContent = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastMessageFromId = table.Column<long>(type: "bigint", nullable: true),
                    LastMessageFromName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastMessageFromAvatar = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastMessageTime = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_T_ChatChannel", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_ChannelId",
                table: "T_ChatChannel",
                column: "ChannelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_ChannelType_IsActive",
                table: "T_ChatChannel",
                columns: new[] { "ChannelType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_LastMessageTime",
                table: "T_ChatChannel",
                column: "LastMessageTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_T_ChatChannel_User1Id_User2Id",
                table: "T_ChatChannel",
                columns: new[] { "User1Id", "User2Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "T_ChatChannel");
        }
    }
}
