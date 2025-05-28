using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class update_message_index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Message_Chan",
                table: "T_Message");

            migrationBuilder.DropIndex(
                name: "IX_T_Message_Time",
                table: "T_Message");

            migrationBuilder.CreateIndex(
                name: "IX_T_Message_Chan_Time",
                table: "T_Message",
                columns: new[] { "Chan", "Time" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_T_Message_From_To_Time",
                table: "T_Message",
                columns: new[] { "From", "To", "Time" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Message_Chan_Time",
                table: "T_Message");

            migrationBuilder.DropIndex(
                name: "IX_T_Message_From_To_Time",
                table: "T_Message");

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
    }
}
