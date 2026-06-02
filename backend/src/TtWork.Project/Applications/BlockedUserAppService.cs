using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
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

        base.GetAllPermissionName = AppPermissions.Pages.ChatManager;
        base.CreatePermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;
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

    protected override BlockedUser MapToEntity(CreateBlockedUserDto createInput)
    {
        var currentUserId = AbpSession.UserId ?? 0;
        return new BlockedUser(currentUserId, createInput.BlockedUserId, createInput.Reason);
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
        return await base.CreateAsync(input);
    }

    [AbpAuthorize]
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

    private new async Task<BlockedUserDto> MapToEntityDto(BlockedUser entity)
    {
        var dto = base.MapToEntityDto(entity);

        // Lookup blocked user info for display
        var blockedUserCache = await _userCache.GetAsync(entity.BlockedUserId);
        if (blockedUserCache != null)
        {
            dto.BlockedUserName = blockedUserCache.Name ?? blockedUserCache.UserName ?? "用户";
            dto.BlockedUserAvatar = blockedUserCache.HeadImgUrl;
        }

        return dto;
    }
}

public class CreateBlockedUserDto : EntityDto<long>
{
    public long BlockedUserId { get; set; }
    public string Reason { get; set; }
}

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