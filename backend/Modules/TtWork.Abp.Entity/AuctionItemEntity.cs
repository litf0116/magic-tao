using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Abp.Entity
{
    /// <summary>
    /// 拍卖商品信息
    /// </summary>
    [SugarTable("t_auctionitem")]
    public class AuctionItemEntity: AutoIncrementEntity
    {
        public enum AuctionStatusEnum
        {
            草稿 = 0,
            上架 = 1,
            拍卖中 = 2,
            已成交 = 4,
            交易成功 = 8,
            卖家失约 = 16,
            买家失约 = 32,
            交易关闭 = 128,
        }
        public string Name { get; set; }
        public AuctionStatusEnum Status { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int StartingPrice { get; set; }
        /// <summary>
        /// 当前价格
        /// </summary>
        public int? CurrentPrice { get; set; }
        /// <summary>
        /// 当前出价人编号
        /// </summary>
        public long? CurrentPriceUserId { get; set; }
        /// <summary>
        /// /当前出价人
        /// </summary>
        public string CurrentPriceUserName { get; set; }
       
        public int? FinalPrice { get; set; }
        public DateTime? DealTime { get; set; }
        public long? DealUserId { get; set; }
        public string DealUserName { get; set; } //成交人

        public string SellerInfo { get; set; }

        public long? SellerId { get; set; }  //出售人
                                             //排序
        public int Order { get; set; }
     
    }
}
