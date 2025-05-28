using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.EntityHistory;
using Abp.Events.Bus.Entities;

namespace TtWork.Project.Applications.Auditing.Dto
{
    [AutoMapFrom(typeof(EntityChange))]
    public class EntityChangeListDto : EntityDto<long>
    {
        public long? UserId { get; set; }

        public string UserName { get; set; }

        public DateTime ChangeTime { get; set; }

        public string EntityTypeFullName { get; set; }

        public EntityChangeType ChangeType { get; set; }

        public string ChangeTypeName => ChangeType.ToString();

        public long EntityChangeSetId { get; set; }
    }
}