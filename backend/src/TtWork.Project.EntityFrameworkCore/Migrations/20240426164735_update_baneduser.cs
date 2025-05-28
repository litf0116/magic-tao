using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class update_baneduser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_BanedUsers_UserId_EndTime",
                table: "T_BanedUsers");

            migrationBuilder.AddColumn<string>(
                name: "Chan",
                table: "T_BanedUsers",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_T_BanedUsers_UserId_EndTime_Chan",
                table: "T_BanedUsers",
                columns: new[] { "UserId", "EndTime", "Chan" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_BanedUsers_UserId_EndTime_Chan",
                table: "T_BanedUsers");

            migrationBuilder.DropColumn(
                name: "Chan",
                table: "T_BanedUsers");

            migrationBuilder.CreateIndex(
                name: "IX_T_BanedUsers_UserId_EndTime",
                table: "T_BanedUsers",
                columns: new[] { "UserId", "EndTime" },
                descending: new bool[0]);
        }
    }
}
