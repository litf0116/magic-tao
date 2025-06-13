using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageSequenceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SequenceNumber",
                table: "T_Message",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // 为现有消息设置序列号（基于时间戳排序）
            migrationBuilder.Sql(@"
                SET @row_number = 0;
                UPDATE T_Message 
                SET SequenceNumber = (@row_number := @row_number + 1)
                ORDER BY Time ASC;
            ");

            // 添加索引以优化查询性能
            migrationBuilder.CreateIndex(
                name: "IX_T_Message_Chan_SequenceNumber",
                table: "T_Message",
                columns: new[] { "Chan", "SequenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_T_Message_From_To_SequenceNumber",
                table: "T_Message",
                columns: new[] { "From", "To", "SequenceNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_T_Message_Chan_SequenceNumber",
                table: "T_Message");

            migrationBuilder.DropIndex(
                name: "IX_T_Message_From_To_SequenceNumber",
                table: "T_Message");

            migrationBuilder.DropColumn(
                name: "SequenceNumber",
                table: "T_Message");
        }
    }
}