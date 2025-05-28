using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class modifyUser0610 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignInToken",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "SignInTokenExpireTimeUtc",
                table: "AbpUsers");

            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "AbpUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositBalance",
                table: "AbpUsers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Balance",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "DepositBalance",
                table: "AbpUsers");

            migrationBuilder.AddColumn<string>(
                name: "SignInToken",
                table: "AbpUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SignInTokenExpireTimeUtc",
                table: "AbpUsers",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
