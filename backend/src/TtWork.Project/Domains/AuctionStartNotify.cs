using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

[Table("T_AuctionStartNotify")]
public class AuctionStartNotify : CreationAuditedEntity<long> {
    public long AuctionItemId { get; set; }

    public string openid { get; set; }
}