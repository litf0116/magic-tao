using Abp.Application.Services.Dto;

namespace TtWork.Abp.Applications.Dtos {
    public class UserDtoBase : EntityDto<long> {
        public string UserName { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string Surname { get; set; }

        public string HeadImgUrl { get; set; }

        public string Qq { get; set; }

        public string Wx { get; set; }
    }
}