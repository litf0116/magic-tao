using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Caches;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class BlockedUserAppService : AbpAsyncCrudAppService<BlockedUser, BlockedUserDto, long, AppResultRequestDto,
    CreateBlockedUserDto, BlockedUserDto>
{
    private readonly IRepository<BlockedUser, long> _repository;
    private readonly UserCache _userCache;

    public BlockedUserAppService(
        IRepository<BlockedUser, long> repository,
        UserCache userCache,
        IocManager iocManager) : base(repository, iocManager)
    {
        _repository = repository;
        _userCache = userCache;

        base.GetAllPermissionName = null;
        base.CreatePermissionName = null;
        base.UpdatePermissionName = null;
        base.DeletePermissionName = null;
        base.GetPermissionName = null;
        base.GetUser = true;
        base.GetCreatorUser = true;
    }

    public override Task<BlockedUserDto> UpdateAsync(BlockedUserDto input) =>
        throw new UserFriendlyException("NOT SUPPORTED");

    public override async Task DeleteAsync(EntityDto<long> input)
    {
        var currentUserId = AbpSession.UserId ?? 0;
        var record = await _repository.GetAsync(input.Id);
        if (record.BlockerId != currentUserId)
            throw new UserFriendlyException("无权操作，该拉黑记录不属于当前用户");
        await _repository.DeleteAsync(input.Id);
    }

    public override async Task<BlockedUserDto> CreateAsync(CreateBlockedUserDto input)
    {
        var currentUserId = AbpSession.UserId ?? 0;
        var exists = await _repository.GetAll()
            .AnyAsync(b => b.BlockerId == currentUserId && b.BlockedUserId == input.BlockedUserId);
        if (exists)
            throw new UserFriendlyException("已拉黑该用户");
        if (currentUserId == input.BlockedUserId)
            throw new UserFriendlyException("不能拉黑自己");

        var entity = new BlockedUser(currentUserId, input.BlockedUserId, input.Reason);
        entity = await _repository.InsertAsync(entity);

        var dto = new BlockedUserDto {
            Id = entity.Id,
            BlockedUserId = entity.BlockedUserId,
            Reason = entity.Reason,
            CreationTime = entity.CreationTime
        };
        var blockedUserCache = await _userCache.GetAsync(entity.BlockedUserId);
        if (blockedUserCache != null)
        {
            dto.BlockedUserName = blockedUserCache.Name ?? blockedUserCache.UserName ?? "用户";
            dto.BlockedUserAvatar = blockedUserCache.HeadImgUrl;
        }
        return dto;
    }

    public override async Task<PagedResultDto<BlockedUserDto>> GetAllAsync(AppResultRequestDto input)
    {
        var query = CreateFilteredQuery(input);

        var total = await query.CountAsync();
        var items = await query.ToListAsync();

        var dtos = new System.Collections.Generic.List<BlockedUserDto>();
        foreach (var entity in items)
        {
            var dto = new BlockedUserDto {
                Id = entity.Id,
                BlockedUserId = entity.BlockedUserId,
                Reason = entity.Reason,
                CreationTime = entity.CreationTime
            };
            var blockedUserCache = await _userCache.GetAsync(entity.BlockedUserId);
            if (blockedUserCache != null)
            {
                dto.BlockedUserName = blockedUserCache.Name ?? blockedUserCache.UserName ?? "用户";
                dto.BlockedUserAvatar = blockedUserCache.HeadImgUrl;
            }
            dtos.Add(dto);
        }

        return new PagedResultDto<BlockedUserDto>(total, dtos);
    }

    [AbpAuthorize]
    [HttpGet]
    public async Task<CheckBlockedResultDto> CheckAsync(long blockedUserId)
    {
        var currentUserId = AbpSession.UserId ?? 0;
        var isBlocked = await _repository.GetAll()
            .AnyAsync(b => b.BlockerId == currentUserId && b.BlockedUserId == blockedUserId);
        return new CheckBlockedResultDto { IsBlocked = isBlocked };
    }

    protected override IQueryable<BlockedUser> CreateFilteredQuery(AppResultRequestDto input)
    {
        var currentUserId = AbpSession.UserId ?? 0;
        return base.CreateFilteredQuery(input)
            .Where(b => b.BlockerId == currentUserId);
    }
}

public class CreateBlockedUserDto : EntityDto<long>
{
    public long BlockedUserId { get; set; }
    public string Reason { get; set; }
}

[AutoMap(typeof(BlockedUser))]
public class BlockedUserDto : EntityDto<long>
{
    public long BlockedUserId { get; set; }
    public string BlockedUserName { get; set; }
    public string BlockedUserAvatar { get; set; }
    public string Reason { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CheckBlockedResultDto : EntityDto
{
    public bool IsBlocked { get; set; }
}
