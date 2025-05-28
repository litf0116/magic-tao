using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Organizations;
using TtWork.Project.Applications.Roles.Dto;
using TtWork.Project.Roles.Dto;
using TtWork.Project.Users.Dto;
using AutoMapper;
using FreeIM;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Authorization.Roles;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Lib.Extensions;
using TtWork.Project.Applications.Core.Users.Dto;
using TtWork.Project.Applications.Sessions.Dto;
using TtWork.Project.Applications.Users.Dto;
using TtWork.Project.Domains;

namespace TtWork.Project {
    internal static class CustomDtoMapper {
        public static void CreateMappings(IMapperConfigurationExpression configuration) {
            configuration.CreateMap<Message, ChatMessage>();


            configuration.CreateMap<UserDto, UserDtoBase>();
            configuration.CreateMap<User, UserLoginInfoDto>();
            configuration.CreateMap<User, UserDtoBase>();
            configuration.CreateMap<User, CreateUserDto>();

            #region 用户

            // User
            configuration.CreateMap<UserDto, User>()
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.CreationTime, opt => opt.Ignore());

            configuration.CreateMap<CreateUserDto, User>()
                .ForMember(x => x.Roles, opt => opt.Ignore());

            configuration.CreateMap<User, UserEditDto>()
                .ForMember(z => z.Password, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(z => z.Password, opt => opt.Ignore());

            // Role and permission
            configuration.CreateMap<Permission, string>().ConvertUsing(r => r.Name);
            configuration.CreateMap<RolePermissionSetting, string>().ConvertUsing(r => r.Name);

            configuration.CreateMap<CreateRoleDto, Role>();

            configuration.CreateMap<RoleDto, Role>();

            configuration.CreateMap<Role, RoleDto>().ForMember(x => x.GrantedPermissions,
                opt => opt.MapFrom(x => x.Permissions.Where(p => p.IsGranted)));

            configuration.CreateMap<Role, RoleListDto>();
            configuration.CreateMap<Role, RoleEditDto>();
            configuration.CreateMap<Permission, FlatPermissionDto>();

            #endregion
        }

        /// <summary>
        /// 取得枚举存放INT的各int位值的LIST  input: 11 = > 1011 =>  output: [8,2,1]
        /// </summary>
        /// <param name="tags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static List<int> GetTagsIntList<T>(int tags) where T : Enum {
            var result = new List<int>();
            foreach (int v in typeof(T).GetEnumValues()) {
                if ((v & tags) == v)
                    result.Add(v);
            }

            return result;
        }

        /// <summary>
        /// 取得枚举存放INT的各key位值的LIST  input: 11 = > 1011 =>  output: ["8的Key","2的Key","1的key"]
        /// </summary>
        /// <param name="tags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static List<string> GetTagsNameList<T>(int tags) {
            var result = new List<string>();
            foreach (int v in typeof(T).GetEnumValues()) {
                if ((v & tags) == v)
                    result.Add(EnumHelper<T>.GetDisplayValue(EnumHelper<T>.Parse(v.ToString())));
            }

            return result;
        }

        private static int TagsListToInt(List<int> tags) {
            return tags.Aggregate(0, (current, v) => current | v);
        }
    }
}