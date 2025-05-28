using Abp.Application.Services.Dto;

namespace TtWork.Project.Applications.Dtos {
    public class EntityEventDto<TPrimaryKey> : EntityDto<TPrimaryKey> {
        public int EventType { get; set; }

        //"delete" "create"
        public string Event { get; set; }
    }
}