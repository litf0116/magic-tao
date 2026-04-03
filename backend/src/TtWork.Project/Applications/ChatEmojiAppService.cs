using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using TtWork.Abp;
using TtWork.Abp.Applications.Dtos;
using TtWork.Project.Domains;

namespace TtWork.Project.Applications;

public class ChatEmojiAppService(
    IRepository<ChatEmoji, long> repository,
    IocManager iocManager)
    : AbpAsyncCrudAppService<ChatEmoji, ChatEmojiDto, long, AppResultRequestDto,
        ChatEmojiDto, ChatEmojiDto>(repository,
        iocManager) {
    public override Task<ChatEmojiDto> UpdateAsync(ChatEmojiDto input) {
        throw new Exception("not allow");
        // return base.UpdateAsync(input);
    }

    [AbpAuthorize]
    public override async Task<PagedResultDto<ChatEmojiDto>> GetAllAsync(AppResultRequestDto input) {
        input.Sorting ??= "creationTime desc";
        input.MaxResultCount = 100;
        return await base.GetAllAsync(input);
    }

    [AbpAuthorize]
    public override Task<ChatEmojiDto> CreateAsync(ChatEmojiDto input) {
        return base.CreateAsync(input);
    }

    [AbpAuthorize]
    public override async Task DeleteAsync(EntityDto<long> input) {
        var entity = await Repository.GetAsync(input.Id);
        if (!await IsAdminAsync())
            if (entity.CreatorUserId != AbpSession.UserId)
                throw new AbpAuthorizationException("无权删除");

        await base.DeleteAsync(input);
    }

    protected override IQueryable<ChatEmoji> CreateFilteredQuery(AppResultRequestDto input) {
        return base.CreateFilteredQuery(input)
                .WhereIf(input.UserId.HasValue, x => x.CreatorUserId == input.UserId.Value)
            ;
    }
}