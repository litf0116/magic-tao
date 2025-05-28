using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Core.Authorization.Users;
using TtWork.Abp.Domains;

namespace TtWork.Project.Domains;

[Table("T_BanedUsers")]
public class BanedUser : Entity<long>, ICreationAudited {
    public DateTime EndTime { get; private set; }
    public long UserId { get; private set; }

    public string Chan { get; private set; }

    public DateTime CreationTime { get; set; }

    public long? CreatorUserId { get; set; }

    public BanedUser() {
    }

    public BanedUser(long userId, long minutes, string chan) {
        UserId = userId;
        EndTime = DateTime.Now.AddMinutes(minutes);
        Chan = string.IsNullOrWhiteSpace(chan) ? null : chan;
    }
}

[AutoMapFrom(typeof(BanedUser))]
[AutoMapTo(typeof(BanedUser))]
public class BanedUserDto : EntityDto<long>, ICreationAudited, IHaveUser, IHaveCreatorUser {
    public DateTime EndTime { get; private set; }
    public long UserId { get; set; }

    public string Chan { get; private set; }

    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public UserDtoBase CreatorUser { get; set; }
    public UserDtoBase User { get; set; }
}