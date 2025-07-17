using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Controllers;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TtWork.Abp.Definitions;
using TtWork.Project.Domains;
using TtWork.Abp.Entity;
using FreeIM;

namespace TtWork.Project.Controllers
{
    /// <summary>
    /// 消息数据修复控制器
    /// </summary>
    [Route("api/message-repair")]
    [DisableAuditing]
    public class MessageRepairController : AbpController
    {
        private readonly IRepository<Message, Guid> _messageRepository;
        private readonly IRepository<AuctionItem, long> _auctionItemRepository;
        private readonly ILogger<MessageRepairController> _logger;

        public MessageRepairController(
            IRepository<Message, Guid> messageRepository,
            IRepository<AuctionItem, long> auctionItemRepository,
            ILogger<MessageRepairController> logger)
        {
            _messageRepository = messageRepository;
            _auctionItemRepository = auctionItemRepository;
            _logger = logger;
        }

        /// <summary>
        /// 消息Payload修复请求参数
        /// </summary>
        public class MessageRepairInput
        {
            /// <summary>
            /// 开始时间
            /// </summary>
            public DateTime StartTime { get; set; }

            /// <summary>
            /// 结束时间
            /// </summary>
            public DateTime EndTime { get; set; }

            /// <summary>
            /// 消息类型 (1010=拍卖完成, 1011=交易成功)
            /// </summary>
            public List<ChatMessageType> MessageTypes { get; set; } = new List<ChatMessageType> { ChatMessageType.AuctionEnd, ChatMessageType.AuctionDeal };

            /// <summary>
            /// 每次处理的数量限制
            /// </summary>
            public int BatchSize { get; set; } = 100;

            /// <summary>
            /// 是否只预览，不实际修复
            /// </summary>
            public bool PreviewOnly { get; set; } = true;
        }

        /// <summary>
        /// 修复结果统计
        /// </summary>
        public class RepairResult
        {
            /// <summary>
            /// 总消息数
            /// </summary>
            public int TotalMessages { get; set; }

            /// <summary>
            /// 已修复数量
            /// </summary>
            public int FixedCount { get; set; }

            /// <summary>
            /// 未修复数量
            /// </summary>
            public int UnfixedCount { get; set; }

            /// <summary>
            /// 修复率
            /// </summary>
            public double FixRate => TotalMessages > 0 ? (double)FixedCount / TotalMessages * 100 : 0;

            /// <summary>
            /// 处理时间
            /// </summary>
            public TimeSpan ProcessingTime { get; set; }

            /// <summary>
            /// 详细信息
            /// </summary>
            public List<MessageRepairDetail> Details { get; set; } = new List<MessageRepairDetail>();
        }

        /// <summary>
        /// 修复详情
        /// </summary>
        public class MessageRepairDetail
        {
            /// <summary>
            /// 消息ID
            /// </summary>
            public Guid MessageId { get; set; }

            /// <summary>
            /// 消息类型
            /// </summary>
            public ChatMessageType MessageType { get; set; }

            /// <summary>
            /// 消息内容
            /// </summary>
            public string MessageContent { get; set; }

            /// <summary>
            /// 拍品名称
            /// </summary>
            public string AuctionName { get; set; }

            /// <summary>
            /// 成交价格
            /// </summary>
            public int? FinalPrice { get; set; }

            /// <summary>
            /// 成交人
            /// </summary>
            public string DealUserName { get; set; }

            /// <summary>
            /// 匹配到的拍品ID
            /// </summary>
            public long? MatchedAuctionId { get; set; }

            /// <summary>
            /// 匹配策略
            /// </summary>
            public string MatchStrategy { get; set; }

            /// <summary>
            /// 是否修复成功
            /// </summary>
            public bool IsFixed { get; set; }

            /// <summary>
            /// 修复时间
            /// </summary>
            public DateTime? RepairTime { get; set; }
        }

        /// <summary>
        /// 拍品匹配信息
        /// </summary>
        private class AuctionMatchInfo
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int? FinalPrice { get; set; }
            public string DealUserName { get; set; }
            public DateTime? DealTime { get; set; }
            public string MatchStrategy { get; set; }
            
            public DateTime? CreationTime { get; set; }

        }

        /// <summary>
        /// 修复消息Payload中缺少的拍品ID
        /// </summary>
        /// <param name="input">修复参数</param>
        /// <returns>修复结果</returns>
        [HttpPost("repair-payload")]
        // [AbpAuthorize(AppPermissions.Pages.ChatManager)]
        public async Task<RepairResult> RepairMessagePayload([FromBody] MessageRepairInput input)
        {
            var startTime = DateTime.Now;
            _logger.LogInformation("开始修复消息Payload，时间范围: {StartTime} - {EndTime}, 类型: {MessageTypes}, 批次大小: {BatchSize}, 预览模式: {PreviewOnly}",
                input.StartTime, input.EndTime, string.Join(",", input.MessageTypes), input.BatchSize, input.PreviewOnly);

            var result = new RepairResult();

            try
            {
                // 转换时间戳 - 使用消息的Time字段进行过滤
                var startTimestamp = new DateTimeOffset(input.StartTime).ToUnixTimeMilliseconds();
                var endTimestamp = new DateTimeOffset(input.EndTime).ToUnixTimeMilliseconds();

                // 查询需要修复的消息 - 使用Time字段而不是CreateTime
                var allMessages = await _messageRepository.GetAll()
                    .Where(m => m.Time >= startTimestamp && m.Time <= endTimestamp)
                    .Where(m => input.MessageTypes.Contains(m.Type))
                    .Where(m => m.Payload != null && m.Payload != "{}" && m.Payload != "")
                    .Take(input.BatchSize * 2) // 多取一些，因为后面会过滤
                    .ToListAsync();

                // 在内存中过滤出需要修复的消息
                var messagesToRepair = allMessages
                    .Where(m => !HasValidAuctionId(m.Payload))
                    .Take(input.BatchSize)
                    .ToList();

                result.TotalMessages = messagesToRepair.Count;
                _logger.LogInformation("查询到 {TotalCount} 条消息，其中 {RepairCount} 条需要修复", allMessages.Count, result.TotalMessages);

                if (result.TotalMessages == 0)
                {
                    result.ProcessingTime = DateTime.Now - startTime;
                    return result;
                }

                // 获取所有相关的拍品信息
                var auctionItems = await _auctionItemRepository.GetAll()
                    // .Where(ai => ai.DealTime.HasValue)
                    .Select(ai => new AuctionMatchInfo
                    {
                        Id = ai.Id,
                        Name = ai.Name,
                        FinalPrice = ai.FinalPrice,
                        DealUserName = ai.DealUserName,
                        DealTime = ai.DealTime,
                        CreationTime = ai.CreationTime
                    }).OrderByDescending(r=>r.CreationTime)
                    .ToListAsync();

                _logger.LogInformation("获取到 {Count} 个拍品信息", auctionItems.Count);

                // 处理每条消息
                foreach (var message in messagesToRepair)
                {
                    var detail = await ProcessMessage(message, auctionItems, input.PreviewOnly);
                    result.Details.Add(detail);

                    if (detail.IsFixed)
                    {
                        result.FixedCount++;
                    }
                    else
                    {
                        result.UnfixedCount++;
                    }
                }

                result.ProcessingTime = DateTime.Now - startTime;

                _logger.LogInformation("修复完成，总计: {Total}, 修复: {Fixed}, 未修复: {Unfixed}, 修复率: {FixRate:F2}%, 耗时: {ProcessingTime}",
                    result.TotalMessages, result.FixedCount, result.UnfixedCount, result.FixRate, result.ProcessingTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "修复消息Payload时发生错误");
                throw new UserFriendlyException("修复过程中发生错误: " + ex.Message);
            }
        }

        /// <summary>
        /// 获取修复统计信息
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>统计信息</returns>
        [HttpPost("statistics")]
        // [AbpAuthorize(AppPermissions.Pages.ChatManager)]
        public async Task<object> GetRepairStatistics([FromBody] MessageRepairInput input)
        {
            // 转换时间戳 - 使用消息的Time字段进行过滤
            var startTimestamp = new DateTimeOffset(input.StartTime).ToUnixTimeMilliseconds();
            var endTimestamp = new DateTimeOffset(input.EndTime).ToUnixTimeMilliseconds();

            // 先查询所有符合条件的消息
            var allMessages = await _messageRepository.GetAll()
                .Where(m => m.Time >= startTimestamp && m.Time <= endTimestamp)
                .Where(m => input.MessageTypes.Contains(m.Type))
                .Where(m => m.Payload != null && m.Payload != "{}" && m.Payload != "")
                .ToListAsync();

            // 在内存中统计
            var totalMessages = allMessages.Count;
            var messagesWithoutId = allMessages.Count(m => !HasValidAuctionId(m.Payload));

            return new
            {
                TotalMessages = totalMessages,
                MessagesWithoutId = messagesWithoutId,
                MessagesWithId = totalMessages - messagesWithoutId,
                FixRate = totalMessages > 0 ? (double)(totalMessages - messagesWithoutId) / totalMessages * 100 : 0
            };
        }

        /// <summary>
        /// 处理单条消息
        /// </summary>
        private async Task<MessageRepairDetail> ProcessMessage(Message message, List<AuctionMatchInfo> auctionItems, bool previewOnly)
        {
            var detail = new MessageRepairDetail
            {
                MessageId = message.Id,
                MessageType = message.Type,
                MessageContent = message.Msg,
                RepairTime = DateTime.Now
            };

            try
            {
                // 解析payload
                var payload = JObject.Parse(message.Payload);
                detail.AuctionName = payload["name"]?.ToString();
                detail.FinalPrice = payload["finalPrice"]?.Value<int>();
                detail.DealUserName = payload["dealUserName"]?.ToString();

                // 尝试匹配拍品
                var matchedAuction = FindMatchingAuction(auctionItems, detail.AuctionName, detail.FinalPrice, detail.DealUserName);
                
                if (matchedAuction != null)
                {
                    detail.MatchedAuctionId = matchedAuction.Id;
                    detail.MatchStrategy = matchedAuction.MatchStrategy;
                    detail.IsFixed = true;

                    if (!previewOnly)
                    {
                        // 实际修复数据
                        payload["id"] = matchedAuction.Id;
                        message.Payload = payload.ToString(Formatting.None);
                        await _messageRepository.UpdateAsync(message);
                        
                        _logger.LogInformation("成功修复消息 {MessageId}，匹配拍品 {AuctionId}，策略: {Strategy}",
                            message.Id, matchedAuction.Id, matchedAuction.MatchStrategy);
                    }
                }
                else
                {
                    detail.IsFixed = false;
                    _logger.LogWarning("无法为消息 {MessageId} 找到匹配的拍品，名称: {Name}, 价格: {Price}, 成交人: {DealUser}",
                        message.Id, detail.AuctionName, detail.FinalPrice, detail.DealUserName);
                }
            }
            catch (Exception ex)
            {
                detail.IsFixed = false;
                _logger.LogError(ex, "处理消息 {MessageId} 时发生错误", message.Id);
            }

            return detail;
        }

        /// <summary>
        /// 查找匹配的拍品
        /// </summary>
        private AuctionMatchInfo FindMatchingAuction(List<AuctionMatchInfo> auctionItems, string name, int? finalPrice, string dealUserName)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // 策略1：精确匹配（名称+价格+成交人）
            var exactMatch = auctionItems.FirstOrDefault(ai => 
                ai.Name == name && 
                ai.FinalPrice == finalPrice && 
                ai.DealUserName == dealUserName);
            
            if (exactMatch != null)
            {
                exactMatch.MatchStrategy = "精确匹配";
                return exactMatch;
            }

            // 策略2：名称+价格匹配
            var priceMatch = auctionItems.FirstOrDefault(ai => 
                ai.Name == name && 
                ai.FinalPrice == finalPrice);
            
            if (priceMatch != null)
            {
                priceMatch.MatchStrategy = "名称+价格匹配";
                return priceMatch;
            }

            // 策略3：仅名称匹配
            var nameMatch = auctionItems.FirstOrDefault(ai => ai.Name == name);
            
            if (nameMatch != null)
            {
                nameMatch.MatchStrategy = "仅名称匹配";
                return nameMatch;
            }

            return null;
        }

        /// <summary>
        /// 检查payload是否包含有效的拍品ID
        /// </summary>
        private bool HasValidAuctionId(string payload)
        {
            try
            {
                var json = JObject.Parse(payload);
                var id = json["id"];
                if (id == null)
                {   id = json["Id"];
                    
                }
                return id != null && id.Value<long>() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
} 