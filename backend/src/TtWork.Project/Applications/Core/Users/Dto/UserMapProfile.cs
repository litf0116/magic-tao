using AutoMapper;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Project.Applications.Core.Users.Dto;

namespace TtWork.Project.Applications.Users.Dto {
    public class UserMapProfile : Profile {
        public UserMapProfile() {
            CreateMap<UserDto, User>();
            CreateMap<UserDto, User>()
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.CreationTime, opt => opt.Ignore());

            CreateMap<CreateUserDto, User>();
            CreateMap<CreateUserDto, User>().ForMember(x => x.Roles, opt => opt.Ignore());
        }
    }
}