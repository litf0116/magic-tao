using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class _20260522_AddSmsVerificationCodeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundComplateTime",
                table: "Pays_PayOrder");

            migrationBuilder.AlterColumn<string>(
                name: "LastMessageFromAvatar",
                table: "T_ChatChannel",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "t_GroupChatLevelSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    AmountRequired = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BorderColor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RightBorderColor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_GroupChatLevelSettings", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "t_UserGroupLevel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GroupChatId = table.Column<int>(type: "int", nullable: false),
                    CumulativeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_UserGroupLevel", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_SmsVerificationCodes_PhoneNumber_Purpose_CreationTime",
                table: "SmsVerificationCodes",
                columns: new[] { "PhoneNumber", "Purpose", "CreationTime" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_t_GroupChatLevelSettings_Level",
                table: "t_GroupChatLevelSettings",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_t_UserGroupLevel_UserId",
                table: "t_UserGroupLevel",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_GroupChatLevelSettings");

            migrationBuilder.DropTable(
                name: "t_UserGroupLevel");

            migrationBuilder.DropIndex(
                name: "IX_SmsVerificationCodes_PhoneNumber_Purpose_CreationTime",
                table: "SmsVerificationCodes");

            migrationBuilder.AlterColumn<string>(
                name: "LastMessageFromAvatar",
                table: "T_ChatChannel",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(512)",
                oldMaxLength: 512,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundComplateTime",
                table: "Pays_PayOrder",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
