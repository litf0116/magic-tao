using System;
using System.Threading.Tasks;
using Abp.Dependency;
using TtWork.Project.Services;
using Microsoft.Extensions.Logging;

namespace TtWork.Project.Services;

/// <summary>
/// 聊天数据迁移服务
/// 用于将现有消息数据迁移到ChatChannel表
/// </summary>
public class ChatDataMigrationService : ITransientDependency
{
    private readonly ChatChannelService _chatChannelService;
    private readonly ILogger<ChatDataMigrationService> _logger;

    public ChatDataMigrationService(
        ChatChannelService chatChannelService,
        ILogger<ChatDataMigrationService> logger)
    {
        _chatChannelService = chatChannelService;
        _logger = logger;
    }

    /// <summary>
    /// 执行聊天数据迁移
    /// 这是一个一次性操作，用于将现有的消息数据迁移到新的频道表结构
    /// </summary>
    public async Task MigrateDataAsync()
    {
        try
        {
            _logger.LogInformation("开始迁移聊天数据到ChatChannel表...");

            await _chatChannelService.MigrateExistingMessagesToChannelsAsync();

            _logger.LogInformation("聊天数据迁移完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "聊天数据迁移失败");
            throw;
        }
    }
}
