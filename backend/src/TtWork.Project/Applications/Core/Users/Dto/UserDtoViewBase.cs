using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;

namespace TtWork.Project.Users.Dto {
    [AutoMapFrom(typeof(User))]
    public class UserDtoViewBase : EntityDto<long> {
        public string UserName { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }
    }
}