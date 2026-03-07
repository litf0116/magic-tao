using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TtWork.Project.Migrations
{
    /// <summary>
    /// 优化拍卖品列表查询性能的数据库索引
    /// </summary>
    public partial class AddAuctionItemIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 状态索引 - 最常用的查询条件
            migrationBuilder.CreateIndex(
                name: "IX_T_AuctionItem_Status",
                table: "T_AuctionItem",
                column: "Status");

            // 混合状态排序索引 - 用于上架和拍卖中商品按 Order, Id 排序
            migrationBuilder.CreateIndex(
                name: "IX_T_AuctionItem_Status_Order_Id",
                table: "T_AuctionItem",
                columns: new[] { "Status", "Order", "Id" });

            // 已成交排序索引 - 用于已成交商品按 DealTime 降序排列
            migrationBuilder.CreateIndex(
                name: "IX_T_AuctionItem_Status_DealTime",
                table: "T_AuctionItem",
                columns: new[] { "Status", "DealTime" },
                descending: new[] { false, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 删除创建的索引
            migrationBuilder.DropIndex(
                name: "IX_T_AuctionItem_Status",
                table: "T_AuctionItem");

            migrationBuilder.DropIndex(
                name: "IX_T_AuctionItem_Status_Order_Id",
                table: "T_AuctionItem");

            migrationBuilder.DropIndex(
                name: "IX_T_AuctionItem_Status_DealTime",
                table: "T_AuctionItem");
        }
    }
}
