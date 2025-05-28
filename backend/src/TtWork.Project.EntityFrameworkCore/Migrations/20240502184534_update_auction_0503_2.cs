using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class update_auction_0503_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousPrice",
                table: "T_AuctionItem");

            migrationBuilder.AddColumn<string>(
                name: "Previous",
                table: "T_AuctionItem",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Previous",
                table: "T_AuctionItem");

            migrationBuilder.AddColumn<int>(
                name: "PreviousPrice",
                table: "T_AuctionItem",
                type: "int",
                nullable: true);
        }
    }
}
