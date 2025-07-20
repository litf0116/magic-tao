using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus;
using Abp.Extensions;
using Abp.UI;
using FreeIM;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SqlSugar;
using TtWork.Abp.Applications.Dtos;
using TtWork.Abp.Caches;
using TtWork.Abp.Entity;
using TtWork.Project.Domains;
using TtWork.Project.EventHandlers;
using TtWork.Project.Events;
using TtWork.Project.Events.Commands;
using TtWork.Project.Services.Messaging.Models;

namespace TtWork.Project.Services.Messaging
{
    /// <summary>
    /// 统一消息发送服务实现
    /// </summary>
    public class MessageSendingService : IMessageSendingService, ITransientDependency
    {
        private readonly UserCache _userCache;
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IRepository<BanedUser, long> _banedUserRepository;
        private readonly IRepository<ChatListDelete> _chatListDeleteRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;
        private readonly ISqlSugarClient _sqlSugarClient;
        private readonly IMessageSequenceService _messageSequenceService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<MessageSendingService> _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public MessageSendingService(
            UserCache userCache,
            IRepository<Message, Guid> messageRepository,
            IRepository<BanedUser, long> banedUserRepository,
            IRepository<ChatListDelete> chatListDeleteRepository,
            IHttpContextAccessor httpContextAccessor,
            IMediator mediator,
            ISqlSugarClient sqlSugarClient,
            IMessageSequenceService messageSequenceService,
            IEventBus eventBus,
            ILogger<MessageSendingService> logger,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _userCache = userCache;
            _messageRepository = messageRepository;
            _banedUserRepository = banedUserRepository;
            _chatListDeleteRepository = chatListDeleteRepository;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _sqlSugarClient = sqlSugarClient;
            _messageSequenceService = messageSequenceService;
            _eventBus = eventBus;
            _logger = logger;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<SendMessageResult> SendChannelMessageAsync(long fromUserId, string channel, ChatMessage message, MessageSendOptions options = null)
        {
            _logger.LogInformation("=== SendChannelMessageAsync开始 === FromUserId={FromUserId}, Channel={Channel}, MessageType={MessageType}", 
                fromUserId, channel, message.type);

            options ??= new MessageSendOptions();
            
            try
            {
                // 1. 验证和增强消息
                _logger.LogInformation("开始验证和增强消息");
                var (isValid, errorMessage, enrichedMessage, userInfo) = await ValidateAndEnrichMessageAsync(fromUserId, message, channel, options);
                if (!isValid)
                {
                    _logger.LogError("消息验证失败: {ErrorMessage}", errorMessage);
                    return SendMessageResult.CreateFailure(errorMessage);
                }

                _logger.LogInformation("消息验证和增强成功: FromName={FromName}, FromAdmin={FromAdmin}, FromTag={FromTag}", 
                    enrichedMessage.fromName, enrichedMessage.fromAdmin, enrichedMessage.fromTag);

                // 2. 生成序列号
                _logger.LogInformation("生成序列号");
                var sequenceNumber = await _messageSequenceService.GetNextSequenceNumberForChannelAsync(channel);
                _logger.LogInformation("序列号生成成功: {SequenceNumber}", sequenceNumber);

                // 3. 持久化消息
                Message entity = null;
                if (options.PersistToDatabase && enrichedMessage.type != ChatMessageType.Welcome)
                {
                    _logger.LogInformation("开始持久化消息到数据库");
                    entity = new Message(enrichedMessage, sequenceNumber)
                    {
                        Ip = GetClientIp(),
                        FromAdmin = userInfo.isAdmin,
                        FromTag = userInfo.adminTag,
                        TagClass = userInfo.tagClass
                    };
                    
                    await _messageRepository.InsertAsync(entity);
                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    // 使用服务端生成的时间戳更新消息
                    enrichedMessage.time = entity.Time;
                    enrichedMessage.sequenceNumber = entity.SequenceNumber;

                    _logger.LogInformation("消息持久化成功: MessageId={MessageId}, Time={Time}", entity.Id, entity.Time);

                    // 触发聊天消息发送事件（异步执行，无需等待）
                    _eventBus.TriggerAsync(new ChatMessageSentEvent(entity.Id));
                    _logger.LogInformation("聊天消息发送事件已触发");
                }
                else
                {
                    _logger.LogInformation("跳过消息持久化: PersistToDatabase={PersistToDatabase}, MessageType={MessageType}", 
                        options.PersistToDatabase, enrichedMessage.type);
                }

                // 4. 投递消息
                if (options.SendImmediately)
                {
                    _logger.LogInformation("开始通过ImHelper发送消息: FromUserId={FromUserId}, Channel={Channel}", fromUserId, channel);
                    
                    // 在发送前检查是否需要编码（卡秒消息）
                    var messageToSend = enrichedMessage;
                    if (enrichedMessage.type == ChatMessageType.KasecStatusChanged)
                    {
                        _logger.LogInformation("检测到卡秒消息，进行编码后发送");
                        messageToSend = EncodeKasecMessage(enrichedMessage);
                    }
                    
                    ImHelper.SendChanMessage(fromUserId, channel, messageToSend);
                    _logger.LogInformation("ImHelper.SendChanMessage调用完成");
                }
                else
                {
                    _logger.LogInformation("跳过立即发送: SendImmediately={SendImmediately}", options.SendImmediately);
                }

                var result = SendMessageResult.CreateSuccess(entity?.Id, sequenceNumber, 
                    entity != null ? DateTimeOffset.FromUnixTimeMilliseconds(entity.Time).DateTime : DateTime.Now, enrichedMessage);

                _logger.LogInformation("=== SendChannelMessageAsync完成 === Success={Success}, MessageId={MessageId}, SequenceNumber={SequenceNumber}", 
                    result.Success, result.MessageId, result.SequenceNumber);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送频道消息失败: FromUserId={FromUserId}, Channel={Channel}", fromUserId, channel);
                return SendMessageResult.CreateFailure($"发送失败: {ex.Message}");
            }
        }

        public async Task<SendMessageResult> SendPrivateMessageAsync(long fromUserId, long toUserId, ChatMessage message, bool isReceipt = false, MessageSendOptions options = null)
        {
            options ??= new MessageSendOptions();
            
            try
            {
                // 1. 验证和增强消息
                var (isValid, errorMessage, enrichedMessage, userInfo) = await ValidateAndEnrichMessageAsync(fromUserId, message, null, options);
                if (!isValid)
                {
                    return SendMessageResult.CreateFailure(errorMessage);
                }

                // 设置接收者
                enrichedMessage.to = toUserId;

                // 2. 生成序列号
                var sequenceNumber = await _messageSequenceService.GetNextSequenceNumberForPrivateAsync(fromUserId, toUserId);

                // 3. 持久化消息
                Message entity = null;
                if (options.PersistToDatabase)
                {
                    entity = new Message(enrichedMessage, sequenceNumber)
                    {
                        Ip = GetClientIp(),
                        FromAdmin = userInfo.isAdmin,
                        FromTag = userInfo.adminTag,
                        TagClass = userInfo.tagClass
                    };

                    await _messageRepository.InsertAsync(entity);
                    await _unitOfWorkManager.Current.SaveChangesAsync();

                    // 使用服务端生成的时间戳更新消息
                    enrichedMessage.time = entity.Time;
                    enrichedMessage.sequenceNumber = entity.SequenceNumber;

                    // 触发聊天消��发送事件
                    await _eventBus.TriggerAsync(new ChatMessageSentEvent(entity.Id));
                }

                // 4. 投递消息
                if (options.SendImmediately)
                {
                    // 在发送前检查是否需要编码（卡秒消息）
                    var messageToSend = enrichedMessage;
                    if (enrichedMessage.type == ChatMessageType.KasecStatusChanged)
                    {
                        _logger.LogInformation("检测到卡秒消息，进行编码后发送");
                        messageToSend = EncodeKasecMessage(enrichedMessage);
                    }
                    
                    ImHelper.SendMessage(fromUserId, [toUserId], messageToSend, isReceipt);
                }

                return SendMessageResult.CreateSuccess(entity?.Id, sequenceNumber, 
                    entity != null ? DateTimeOffset.FromUnixTimeMilliseconds(entity.Time).DateTime : DateTime.Now, enrichedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送私聊消息失败: FromUserId={FromUserId}, ToUserId={ToUserId}", fromUserId, toUserId);
                return SendMessageResult.CreateFailure($"发送失败: {ex.Message}");
            }
        }

        public async Task<SendMessageResult> SendSystemChannelMessageAsync(string channel, ChatMessage message, MessageSendOptions options = null)
        {
            options ??= new MessageSendOptions();
            options.SkipPermissionCheck = true;
            options.AddAdminTag = false;
            
            // 使用系统用户ID (假设为0或其他系统标识)
            return await SendChannelMessageAsync(0, channel, message, options);
        }

        public async Task<SendMessageResult> SendSystemPrivateMessageAsync(long toUserId, ChatMessage message, MessageSendOptions options = null)
        {
            options ??= new MessageSendOptions();
            options.SkipPermissionCheck = true;
            options.AddAdminTag = false;
            
            // 使用系统用户ID (假设为0或其他系统标识)
            return await SendPrivateMessageAsync(0, toUserId, message, false, options);
        }

        public async Task<BatchSendMessageResult> SendBatchMessagesAsync(IEnumerable<MessageSendRequest> requests)
        {
            var result = new BatchSendMessageResult();
            var requestList = requests.ToList();
            result.TotalCount = requestList.Count;

            foreach (var request in requestList)
            {
                try
                {
                    SendMessageResult sendResult;
                    
                    if (!string.IsNullOrEmpty(request.Channel))
                    {
                        // 频道消息
                        sendResult = await SendChannelMessageAsync(request.FromUserId, request.Channel, request.Message, request.Options);
                    }
                    else if (request.ToUserId.HasValue)
                    {
                        // 私聊消息
                        sendResult = await SendPrivateMessageAsync(request.FromUserId, request.ToUserId.Value, request.Message, request.IsReceipt, request.Options);
                    }
                    else
                    {
                        sendResult = SendMessageResult.CreateFailure("无效的消息发送请求：缺少频道或接收者");
                    }

                    result.Results.Add(sendResult);
                    
                    if (sendResult.Success)
                    {
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailureCount++;
                        result.Errors.Add(sendResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    result.FailureCount++;
                    result.Errors.Add($"发送失败: {ex.Message}");
                    result.Results.Add(SendMessageResult.CreateFailure(ex.Message));
                }
            }

            return result;
        }

        public async Task<SendMessageResult> SendAuctionMessageAsync(long fromUserId, long? toUserId, string channel, ChatMessage message, bool isSystemMessage = false)
        {
            _logger.LogInformation("=== SendAuctionMessageAsync开始 === FromUserId={FromUserId}, ToUserId={ToUserId}, Channel={Channel}, IsSystemMessage={IsSystemMessage}, MessageType={MessageType}", 
                fromUserId, toUserId, channel, isSystemMessage, message.type);

            var options = new MessageSendOptions
            {
                SkipPermissionCheck = isSystemMessage,
                // 修复：即使是系统消息，也应该显示管理员标签，只要有有效的用户ID
                AddAdminTag = fromUserId > 0,
                AddUserChatLevel = !isSystemMessage
            };

            _logger.LogInformation("消息发送选项: SkipPermissionCheck={SkipPermissionCheck}, AddAdminTag={AddAdminTag}, AddUserChatLevel={AddUserChatLevel}", 
                options.SkipPermissionCheck, options.AddAdminTag, options.AddUserChatLevel);

            SendMessageResult result;
            if (!string.IsNullOrEmpty(channel))
            {
                _logger.LogInformation("发送频道消息: Channel={Channel}", channel);
                result = await SendChannelMessageAsync(fromUserId, channel, message, options);
            }
            else if (toUserId.HasValue)
            {
                _logger.LogInformation("发送私聊消息: ToUserId={ToUserId}", toUserId.Value);
                result = await SendPrivateMessageAsync(fromUserId, toUserId.Value, message, false, options);
            }
            else
            {
                _logger.LogError("无效的拍卖消息发送请求：缺少频道或接收者");
                result = SendMessageResult.CreateFailure("无效的拍卖消息发送请求：缺少频道或接收者");
            }

            _logger.LogInformation("=== SendAuctionMessageAsync完成 === Success={Success}, MessageId={MessageId}, ErrorMessage={ErrorMessage}", 
                result.Success, result.MessageId, result.Message);

            return result;
        }

        #region 私有方法

        /// <summary>
        /// 验证和增强消息
        /// </summary>
        private async Task<(bool isValid, string errorMessage, ChatMessage enrichedMessage, (bool isAdmin, string adminTag, string tagClass) userInfo)> ValidateAndEnrichMessageAsync(
            long fromUserId, ChatMessage message, string channel, MessageSendOptions options)
        {
            try
            {
                // 克隆消息避免修改原始对象
                var enrichedMessage = new ChatMessage
                {
                    id = message.id ?? Guid.NewGuid(),
                    type = message.type,
                    msg = message.msg,
                    payload = message.payload,
                    chan = message.chan ?? channel,
                    from = fromUserId,
                    to = message.to,
                    fromName = message.fromName,
                    avatar = message.avatar,
                    time = message.time
                };

                // 获取用户信息 - 修复：即使跳过权限检查，也需要获取用户基本信息用于显示
                UserDto userInfo = null;
                if (fromUserId > 0)
                {
                    try
                    {
                        userInfo = await _userCache.GetAsync(fromUserId);
                        _logger.LogDebug("获取用户信息: UserId={UserId}, UserName={UserName}, HeadImgUrl={HeadImgUrl}", 
                            fromUserId, userInfo?.Name, userInfo?.HeadImgUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "获取用户缓存信息失败: UserId={UserId}", fromUserId);
                        userInfo = null;
                    }
                    
                    // 只有在非跳过权限检查时才验证用户状态
                    if (!options.SkipPermissionCheck && userInfo != null && !userInfo.IsActive)
                    {
                        return (false, "账号已被禁用", null, (false, "", ""));
                    }

                    // 设置用户基本信息 - ���复：无论是否跳过权限检查都设置基本信息
                    if (userInfo != null)
                    {
                        enrichedMessage.fromName = userInfo.Name;
                        enrichedMessage.avatar = userInfo.HeadImgUrl;
                        _logger.LogDebug("设置用户基本信息: fromName={FromName}, avatar={Avatar}", 
                            enrichedMessage.fromName, enrichedMessage.avatar);
                    }
                    else
                    {
                        _logger.LogWarning("用户信息为空: UserId={UserId}", fromUserId);
                    }
                }

                // 权限检查
                var (isAdmin, adminTag, tagClass) = (false, "", "");
                if (options.AddAdminTag && userInfo != null)
                {
                    (isAdmin, adminTag, tagClass) = await CheckIsChatAdmin(userInfo);
                    enrichedMessage.fromAdmin = isAdmin;
                    enrichedMessage.fromTag = adminTag;
                    enrichedMessage.tagClass = tagClass;
                    _logger.LogDebug("设置管理员信息: isAdmin={IsAdmin}, adminTag={AdminTag}, tagClass={TagClass}", 
                        isAdmin, adminTag, tagClass);
                }

                // 禁言检查
                if (!options.SkipPermissionCheck && !isAdmin && fromUserId > 0)
                {
                    var banedUser = await _banedUserRepository.FirstOrDefaultAsync(a =>
                        a.UserId == fromUserId && (a.Chan == null || a.Chan == channel) &&
                        a.EndTime > DateTime.Now);
                    if (banedUser != null)
                    {
                        return (false, $"您已被禁言,结束时间 {banedUser.EndTime:yyyy-MM-dd HH:mm:ss}", null, (isAdmin, adminTag, tagClass));
                    }
                }

                // 敏感词检查
                if (!options.SkipSensitiveWordCheck)
                {
                    var checkResult = await CheckMsgText(enrichedMessage);
                    if (!string.IsNullOrEmpty(checkResult.errorMessage))
                    {
                        return (false, checkResult.errorMessage, null, (isAdmin, adminTag, tagClass));
                    }
                    enrichedMessage = checkResult.message;
                }

                // 添加用户群聊等级信息
                if (options.AddUserChatLevel && fromUserId > 0)
                {
                    await AddUserChatLevelInfo(enrichedMessage, fromUserId);
                }

                _logger.LogDebug("消息增强完成: fromName={FromName}, fromAdmin={FromAdmin}, fromTag={FromTag}", 
                    enrichedMessage.fromName, enrichedMessage.fromAdmin, enrichedMessage.fromTag);

                return (true, null, enrichedMessage, (isAdmin, adminTag, tagClass));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证和增强消息失败: FromUserId={FromUserId}", fromUserId);
                return (false, $"消息处理失败: {ex.Message}", null, (false, "", ""));
            }
        }

        /// <summary>
        /// 检查用户管理员权限
        /// </summary>
        private async Task<(bool, string, string)> CheckIsChatAdmin(UserDto currentUser)
        {
            try
            {
                if (currentUser is { RoleNames.Length: > 0 })
                {
                    if (currentUser.RoleNames.Contains("AuctionManager"))
                        return (true, "拍卖师", "tag_AuctionManager");
                    if (currentUser.RoleNames.Contains("Manager"))
                        return (true, "管理员", "tag_Manager");
                    if (currentUser.RoleNames.Contains("AuctionUser"))
                        return (false, "竞拍用户", "tag_AudtionUser");
                    if (currentUser.RoleNames.Contains("Admin"))
                        return (true, "系统管理员", "tag_Admin");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "获取用户缓存信息失败");
            }

            return (false, "", "");
        }

        /// <summary>
        /// 检查消息文本
        /// </summary>
        private async Task<(ChatMessage message, string errorMessage)> CheckMsgText(ChatMessage message)
        {
            try
            {
                // 从Redis缓存中取出敏感词
                var sw = await _mediator.Send(new QueryCacheWords());

                var result = IndexOfFirstArray(message.msg, sw);
                if (result is not null)
                {
                    return (null, $"含有禁用词:{result}");
                }

                if (message.msg != null && message.msg.Length > 400)
                {
                    return (null, "消息过长");
                }

                message.msg = HttpUtility.HtmlEncode(message.msg);
                return (message, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查消息文本失败");
                return (null, "消息检查失败");
            }
        }

        /// <summary>
        /// 检查敏感词
        /// </summary>
        private string IndexOfFirstArray(string text, string[] needles)
        {
            if (string.IsNullOrEmpty(text) || needles == null) return null;
            
            ReadOnlySpan<char> haystatck = text;
            for (var i = 0; i < haystatck.Length; i++)
            {
                foreach (var needle in needles)
                {
                    if (!string.IsNullOrEmpty(needle))
                        if (haystatck[i..].StartsWith(needle, StringComparison.OrdinalIgnoreCase))
                        {
                            return needle;
                        }
                }
            }

            return null;
        }

        /// <summary>
        /// 添加用户群聊等级信息
        /// </summary>
        private async Task AddUserChatLevelInfo(ChatMessage message, long userId)
        {
            try
            {
                // 群聊等级信息
                var groupChatLevel = await _sqlSugarClient.Queryable<GroupChatLevelSettingsEntity>().FirstAsync(f => f.Level == 0);
                
                // 查询用户群聊等级
                var userGroupLevel = await _sqlSugarClient.Queryable<UserGroupLevelEntity>()
                    .LeftJoin<GroupChatLevelSettingsEntity>((a, b) => a.GroupChatId == b.Id)
                    .Where((a, b) => a.UserId == userId)
                    .Select((a, b) => new
                    {
                        a.UserId,
                        b.Name,
                        b.Level,
                        b.BorderColor,
                        b.RightBorderColor
                    })
                    .FirstAsync();

                // 设置用户群聊等级信息
                if (userGroupLevel != null)
                {
                    message.userChatLevel = new
                    {
                        userId = userGroupLevel.UserId,
                        name = userGroupLevel.Name,
                        level = userGroupLevel.Level,
                        borderColor = userGroupLevel.BorderColor,
                        rightBorderColor = userGroupLevel.RightBorderColor
                    };
                }
                else
                {
                    message.userChatLevel = new
                    {
                        userId = groupChatLevel.Id,
                        name = groupChatLevel.Name,
                        level = groupChatLevel.Level,
                        borderColor = groupChatLevel.BorderColor,
                        rightBorderColor = groupChatLevel.RightBorderColor
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加用户群聊等级信息失败: UserId={UserId}", userId);
                // 不抛出异常，只是无法设置等级信息
            }
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        private string GetClientIp()
        {
            try
            {
                return _httpContextAccessor!.HttpContext!.Request.Headers["X-Real-IP"].FirstOrDefault() ??
                       _httpContextAccessor!.HttpContext!.Request.HttpContext!.Connection!.RemoteIpAddress!
                           .ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 编码卡秒消息为AuctionBid类型
        /// </summary>
        /// <param name="kasecMessage">原始卡秒消息</param>
        /// <returns>编码后的消息</returns>
        private ChatMessage EncodeKasecMessage(ChatMessage kasecMessage)
        {
            _logger.LogInformation("开始编码卡秒消息: OriginalType={OriginalType}, Message={Message}", 
                kasecMessage.type, kasecMessage.msg);

            // 从原始payload中提取数据
            long? auctionItemId = null;
            bool? isKasec = null;

            if (kasecMessage.payload != null)
            {
                // 使用反射或动态访问来获取payload属性
                var payloadType = kasecMessage.payload.GetType();
                var auctionItemIdProperty = payloadType.GetProperty("auctionItemId");
                var isKasecProperty = payloadType.GetProperty("isKasec");

                if (auctionItemIdProperty != null)
                {
                    auctionItemId = (long?)auctionItemIdProperty.GetValue(kasecMessage.payload);
                }
                if (isKasecProperty != null)
                {
                    isKasec = (bool?)isKasecProperty.GetValue(kasecMessage.payload);
                }
            }

            var encodedMessage = new ChatMessage
            {
                type = ChatMessageType.AuctionBid,  // 使用AuctionBid作为载体类型
                chan = kasecMessage.chan,
                msg = kasecMessage.msg,
                from = kasecMessage.from,
                fromName = kasecMessage.fromName,
                avatar = kasecMessage.avatar,
                time = kasecMessage.time,
                payload = new
                {
                    // 原始卡秒消息的payload
                    auctionItemId = auctionItemId,
                    isKasec = isKasec,
                    // 编码标识
                    messageType = "KasecStatusChanged",
                    originalType = "KasecStatusChanged",
                    // 其他编码信息
                    encoded = true
                }
            };

            _logger.LogInformation("卡秒消息编码完成: EncodedType={EncodedType}, Payload={Payload}", 
                encodedMessage.type, System.Text.Json.JsonSerializer.Serialize(encodedMessage.payload));

            return encodedMessage;
        }

        #endregion
    }
}
