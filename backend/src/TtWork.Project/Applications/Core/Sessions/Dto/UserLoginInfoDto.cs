using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using TtWork.Abp.Applications.Dtos;

namespace TtWork.Project.Applications.Sessions.Dto {
    //Mapped to/from User in CustomDtoMapper
    [AutoMapFrom(typeof(UserDto))]
    public class UserLoginInfoDto : EntityDto<long> {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }

        public string HeadImgUrl { get; set; }

        public string PhoneNumber { get; set; }

        /// <summary>
        /// 帐户余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 保证金
        /// </summary>
        public decimal DepositBalance { get; set; }

        public bool NeedProfileCompletion { get; set; }

        public bool SkipProfileCompletion { get; set; }
    }
}