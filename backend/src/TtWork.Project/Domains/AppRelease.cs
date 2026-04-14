using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace TtWork.Project.Domains
{
    [Table("AppReleases")]
    public class AppRelease : FullAuditedEntity<long>
    {
        [MaxLength(50)]
        public string VersionName { get; set; }

        public int VersionCode { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string DownloadUrl { get; set; }

        [MaxLength(50)]
        public string FileName { get; set; }

        public long FileSize { get; set; }

        public bool IsForceUpdate { get; set; }

        [MaxLength(20)]
        public string Platform { get; set; }

        public DateTime ReleaseDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}