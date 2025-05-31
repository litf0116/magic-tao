using System;
using Abp.Application.Services.Dto;
using Abp.Extensions;
using Abp.Runtime.Validation;
using JetBrains.Annotations;

namespace TtWork.Abp.Applications.Dtos {
    //custom PagedResultRequestDto
    public class AppResultRequestDto : PagedResultRequestDto, IShouldNormalize, ISortedResultRequest {
        [CanBeNull] public string AppName { get; set; }
        public int? Type { get; set; }
        public bool? Self { get; set; }
        public bool? IsTop { get; set; }

        public long? OrganizationUnitId { get; set; }
        public long? ShopId { get; set; }
        public int? Status { get; set; }

        public long? UserId { get; set; }
        public int? Pid { get; set; }
        public Guid? GPid { get; set; }
        public string Keyword { get; set; }
        public bool? IsActive { get; set; }
        public string Sorting { get; set; }

        public DateTime? From { get; set; } // javascript date within timezone

        public DateTime? To { get; set; } // javascript date within timezone

        public bool? PublicHidden { get; set; }

        public virtual void Normalize() {
            if (Sorting.IsNullOrWhiteSpace()) {
                Sorting = "Id descending";
            }


            if (MaxResultCount > 2000)
                MaxResultCount = 2000;
        }
    }
}