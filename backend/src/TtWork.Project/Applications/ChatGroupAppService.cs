using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using FreeIM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Caches;
using TtWork.Abp.Dapper;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class ChatGroupAppService : AbpAsyncCrudAppService<ChatGroup, ChatGroupDto, long, AppResultRequestDto,
    ChatGroupCreateOrUpdateDto, ChatGroupCreateOrUpdateDto> {
    private readonly UserCache _userCache;
    private readonly IRepository<Message, Guid> _messageRepository;
    // private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ChatGroupAppService(
        UserCache userCache,
        IRepository<ChatGroup, long> repository,
        IRepository<Message, Guid> messageRepository,
        // ISqlConnectionFactory sqlConnectionFactory,
        IocManager iocManager) : base(repository, iocManager) {
        _userCache = userCache;
        _messageRepository = messageRepository;
        // _sqlConnectionFactory = sqlConnectionFactory;
        // base.GetAllPermissionName = AppPermissions.Pages.ChatManager;
        base.UpdatePermissionName = AppPermissions.Pages.ChatManager;

        base.GetCreatorUser = true;
        base.EnableGetEdit = true;
    }


    [HttpGet]
    [AbpAuthorize]
    public async Task<ChatGroupDto> ToggleHidden(long id) {
        var group = await Repository.GetAsync(id);
        if (!await IsAdminAsync()) {
            if (group.CreatorUserId != AbpSession.UserId!.Value) {
                throw new UserFriendlyException(1, "无权管理他人频道");
            }
        }

        group.IsHidden = !group.IsHidden;
        await Repository.UpdateAsync(group);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToEntityDto(group);
    }

    [HttpGet]
    [AbpAuthorize]
    public async Task<ListResultDto<UserDto>> GetGroupUser(string chan) {
        var list = ImHelper.GetChanClientList(chan);

        var result = new List<UserDto>();
        foreach (var u in list) {
            result.Add(await _userCache.GetAsync(u));
        }

        return new ListResultDto<UserDto>(result);
    }


    [AbpAuthorize]
    public override Task<ChatGroupDto> GetAsync(EntityDto<long> input) {
        return base.GetAsync(input);
    }

    [AbpAuthorize]
    public override async Task<ChatGroupDto> CreateAsync(ChatGroupCreateOrUpdateDto input) {
        var result = await base.CreateAsync(input);
        ImHelper.JoinChan(AbpSession.UserId!.Value, result.Chan);
        return result;
    }

    /// <summary>
    /// 删除组队频道
    /// </summary>
    /// <param name="input"></param>
    /// <exception cref="UserFriendlyException"></exception>
    [AbpAuthorize]
    public override async Task DeleteAsync(EntityDto<long> input) {
        var group = await base.GetAsync(input);

        if (!await IsAdminAsync() && group.CreatorUserId != AbpSession.UserId!.Value)
            throw new UserFriendlyException(1, "无权删除他人频道");

        ImHelper.SendChanMessage(group.CreatorUserId!.Value, group.Chan, new ChatMessage() { type = ChatMessageType.Goodbye, chan = group.Chan });
        ImHelper.DeleteChan(group.Chan);

        // using var conn = _sqlConnectionFactory.GetOpenConnection();
        // conn.Execute("delete from t_message where chan=@chan", new { group.Chan });

        await _messageRepository.GetAll().Where(x => x.Chan == group.Chan).ExecuteDeleteAsync();
        await base.DeleteAsync(input);
    }

    /// <summary>
    /// 踢出用户
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <exception cref="UserFriendlyException"></exception>
    [HttpGet]
    [AbpAuthorize]
    public async Task KickUser(long id, long userId) {
        var group = await Repository.GetAsync(id);
        if (!await IsAdminAsync()) {
            if (group.CreatorUserId != AbpSession.UserId!.Value) {
                throw new UserFriendlyException(1, "无权管理他人频道");
            }
        }

        var dto = MapToEntityDto(group);

        ImHelper.SendChanMessage(group.CreatorUserId!.Value, dto.Chan,
            new ChatMessage() { type = ChatMessageType.Goodbye, chan = dto.Chan, msg = $"{userId}" });
        Thread.Sleep(200);
        ImHelper.LeaveChan(userId, dto.Chan);
    }


    [HttpGet]
    [AbpAuthorize]
    public Task<PagedResultDto<ChatGroupDto>> GetAllPublic(AppResultRequestDto input) {
        input.Status = 1;
        input.MaxResultCount = 100;
        return base.GetAllAsync(input);
    }

    protected override IQueryable<ChatGroup> CreateFilteredQuery(AppResultRequestDto input) {
        return base.CreateFilteredQuery(input)
                .WhereIf(input.Status is 0, x => x.IsHidden)
                .WhereIf(input.Status is 1, x => !x.IsHidden)
            ;
    }
}