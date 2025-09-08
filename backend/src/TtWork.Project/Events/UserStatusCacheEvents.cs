using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Events.Bus;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Json;
using Abp.Logging;
using Microsoft.Extensions.Logging;
using SqlSugar;
using TtWork.Abp.Authorization.Users;
using TtWork.Abp.Entity;
using TtWork.Project.Domains;
using TtWork.Project.Services;

namespace TtWork.Project.Events
{
    /// <summary>
    /// 用户信息更新事件
    /// </summary>
    public class UserUpdatedEvent : EventData
    {
        public User Entity { get; }
        
        public UserUpdatedEvent(User entity)
        {
            Entity = entity;
        }
    }

    /// <summary>
    /// 用户角色变更事件
    /// </summary>
    public class UserRoleChangedEvent : EventData
    {
        public User Entity { get; }
        
        public UserRoleChangedEvent(User entity)
        {
            Entity = entity;
        }
    }

    /// <summary>
    /// 用户被禁言事件
    /// </summary>
    public class UserBannedEvent : EventData
    {
        public BanedUser Entity { get; }
        public string Channel { get; }
        public DateTime? BanEndTime { get; }

        public UserBannedEvent(BanedUser entity)
        {
            Entity = entity;
            Channel = entity.Chan;
            BanEndTime = entity.EndTime;
        }
    }

    /// <summary>
    /// 用户禁言解除事件
    /// </summary>
    public class UserUnbannedEvent : EventData
    {
        public long UserId { get; }
        public string Channel { get; }

        public UserUnbannedEvent(long userId, string channel = null)
        {
            UserId = userId;
            Channel = channel;
        }
    }

    /// <summary>
    /// 用户群聊等级变更事件
    /// </summary>
    public class UserGroupLevelChangedEvent : EventData
    {
        public UserGroupLevelEntity Entity { get; }
        
        public UserGroupLevelChangedEvent(UserGroupLevelEntity entity)
        {
            Entity = entity;
        }
    }

    /// <summary>
    /// 缓存失效事件处理器
    /// </summary>
    public class UserStatusCacheInvalidationHandler : ITransientDependency
    {
        private readonly IUserStatusCacheService _userStatusCache;
        private readonly ILogger<UserStatusCacheInvalidationHandler> _logger;

        public UserStatusCacheInvalidationHandler(
            IUserStatusCacheService userStatusCache,
            ILogger<UserStatusCacheInvalidationHandler> logger)
        {
            _userStatusCache = userStatusCache;
            _logger = logger;
        }

        public async Task HandleUserUpdatedEventAsync(UserUpdatedEvent eventData)
        {
            try
            {
                await _userStatusCache.ClearUserCacheAsync(eventData.Entity.Id, clearAll: false);
                _logger.LogInformation("用户信息更新，缓存已清除: UserId={UserId}", eventData.Entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户信息更新事件失败: UserId={UserId}", eventData.Entity.Id);
            }
        }

        public async Task HandleUserRoleChangedEventAsync(UserRoleChangedEvent eventData)
        {
            try
            {
                await _userStatusCache.ClearUserCacheAsync(eventData.Entity.Id, clearAll: false);
                _logger.LogInformation("用户角色变更，缓存已清除: UserId={UserId}", eventData.Entity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户角色变更事件失败: UserId={UserId}", eventData.Entity.Id);
            }
        }

        public async Task HandleUserBannedEventAsync(UserBannedEvent eventData)
        {
            try
            {
                await _userStatusCache.ClearUserCacheAsync(eventData.Entity.UserId, clearAll: true);
                _logger.LogInformation("用户被禁言，缓存已清除: UserId={UserId}, Channel={Channel}", 
                    eventData.Entity.UserId, eventData.Channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户禁言事件失败: UserId={UserId}", eventData.Entity.UserId);
            }
        }

        public async Task HandleUserUnbannedEventAsync(UserUnbannedEvent eventData)
        {
            try
            {
                await _userStatusCache.ClearUserCacheAsync(eventData.UserId, clearAll: true);
                _logger.LogInformation("用户禁言解除，缓存已清除: UserId={UserId}, Channel={Channel}", 
                    eventData.UserId, eventData.Channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户禁言解除事件失败: UserId={UserId}", eventData.UserId);
            }
        }

        public async Task HandleUserGroupLevelChangedEventAsync(UserGroupLevelChangedEvent eventData)
        {
            try
            {
                await _userStatusCache.ClearUserCacheAsync(eventData.Entity.UserId, clearAll: false);
                _logger.LogInformation("用户群聊等级变更，缓存已清除: UserId={UserId}", eventData.Entity.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理用户群聊等级变更事件失败: UserId={UserId}", eventData.Entity.UserId);
            }
        }
    }

    /// <summary>
    /// 用户信息变更发布服务
    /// </summary>
    public class UserEventPublisher : ITransientDependency
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<UserEventPublisher> _logger;

        public UserEventPublisher(IEventBus eventBus, ILogger<UserEventPublisher> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        /// <summary>
        /// 发布用户信息更新事件
        /// </summary>
        public async Task PublishUserUpdatedAsync(User user)
        {
            try
            {
                await _eventBus.TriggerAsync(new UserUpdatedEvent(user));
                _logger.LogDebug("已发布用户信息更新事件: UserId={UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布用户信息更新事件失败: UserId={UserId}", user.Id);
            }
        }

        /// <summary>
        /// 发布用户角色变更事件
        /// </summary>
        public async Task PublishUserRoleChangedAsync(User user)
        {
            try
            {
                await _eventBus.TriggerAsync(new UserRoleChangedEvent(user));
                _logger.LogDebug("已发布用户角色变更事件: UserId={UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布用户角色变更事件失败: UserId={UserId}", user.Id);
            }
        }

        /// <summary>
        /// 发布用户禁言事件
        /// </summary>
        public async Task PublishUserBannedAsync(BanedUser banedUser)
        {
            try
            {
                await _eventBus.TriggerAsync(new UserBannedEvent(banedUser));
                _logger.LogDebug("已发布用户禁言事件: UserId={UserId}, Channel={Channel}", 
                    banedUser.UserId, banedUser.Chan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布用户禁言事件失败: UserId={UserId}", banedUser.UserId);
            }
        }

        /// <summary>
        /// 发布用户禁言解除事件
        /// </summary>
        public async Task PublishUserUnbannedAsync(long userId, string channel = null)
        {
            try
            {
                await _eventBus.TriggerAsync(new UserUnbannedEvent(userId, channel));
                _logger.LogDebug("已发布用户禁言解除事件: UserId={UserId}, Channel={Channel}", userId, channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布用户禁言解除事件失败: UserId={UserId}", userId);
            }
        }

        /// <summary>
        /// 发布用户群聊等级变更事件
        /// </summary>
        public async Task PublishUserGroupLevelChangedAsync(UserGroupLevelEntity userGroupLevel)
        {
            try
            {
                await _eventBus.TriggerAsync(new UserGroupLevelChangedEvent(userGroupLevel));
                _logger.LogDebug("已发布用户群聊等级变更事件: UserId={UserId}", userGroupLevel.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发布用户群聊等级变更事件失败: UserId={UserId}", userGroupLevel.UserId);
            }
        }
    }
}