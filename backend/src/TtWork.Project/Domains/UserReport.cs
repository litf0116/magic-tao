using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains;

[Table("T_UserReports")]
public class UserReport : Entity<long>, ICreationAudited {
    /// <summary>
    /// Who filed the report
    /// </summary>
    public long ReporterId { get; set; }

    /// <summary>
    /// The reported message ID
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// The user who sent the message
    /// </summary>
    public long ReportedUserId { get; set; }

    /// <summary>
    /// Channel for locating message context
    /// </summary>
    [MaxLength(200)]
    public string Chan { get; set; }

    /// <summary>
    /// Report reason
    /// </summary>
    [MaxLength(500)]
    public string Reason { get; set; }

    /// <summary>
    /// Detailed explanation (nullable)
    /// </summary>
    [MaxLength(2000)]
    public string? Evidence { get; set; }

    /// <summary>
    /// Report status: Pending=0, Processed=1, Rejected=2
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Admin processing note (nullable)
    /// </summary>
    [MaxLength(500)]
    public string? AdminNote { get; set; }

    /// <summary>
    /// When the report was processed (nullable)
    /// </summary>
    public DateTime? ProcessedTime { get; set; }

    public DateTime CreationTime { get; set; }

    public long? CreatorUserId { get; set; }
}
