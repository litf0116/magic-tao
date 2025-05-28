using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class ad_ip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ip",
                table: "T_Message",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_T_Message_Chan",
                table: "T_Message",
                column: "Chan");

            migrationBuilder.CreateIndex(
                name: "IX_T_Message_Time",
                table: "T_Message",
                column: "Time",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Message_Chan",
                table: "T_Message");

            migrationBuilder.DropIndex(
                name: "IX_T_Message_Time",
                table: "T_Message");

            migrationBuilder.DropColumn(
                name: "Ip",
                table: "T_Message");
        }
    }
}
