// Android FCM 推送通知示例代码
// 文档: https://firebase.google.com/docs/admin/setup

using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TtWork.Project.PushNotifications.Examples
{
    /// <summary>
    /// FCM 推送通知示例
    /// </summary>
    public class FcmExample
    {
        private readonly FirebaseMessaging _messaging;

        public FcmExample(string serviceAccountPath, string projectId)
        {
            // 初始化 Firebase App
            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromFile(serviceAccountPath);

                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = projectId
                });
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }

        /// <summary>
        /// 示例 1: 发送基础通知
        /// </summary>
        public async Task Example1_SendBasicNotification(string registrationToken)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "出价成功",
                    Body = "您的出价 ¥10,000 已成功提交"
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 推送成功: {registrationToken}");
            }
            else
            {
                Console.WriteLine($"❌ 推送失败");
            }
        }

        /// <summary>
        /// 示例 2: 发送带自定义数据的推送
        /// </summary>
        public async Task Example2_SendNotificationWithData(string registrationToken)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "新出价提醒",
                    Body = "您关注的拍品刚刚有新出价"
                },
                Data = new Dictionary<string, string>
                {
                    { "type", "new_bid" },
                    { "auctionId", "12345" },
                    { "itemId", "67890" },
                    { "amount", "10000" },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "bids_channel",
                        Sound = "default",
                        NotificationCount = 1,
                        ClickAction = "OPEN_AUCTION_DETAIL"
                    }
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 带数据推送成功: {registrationToken}");
            }
        }

        /// <summary>
        /// 示例 3: 发送数据消息（仅传递数据，不显示通知）
        /// </summary>
        public async Task Example3_SendDataMessage(string registrationToken)
        {
            var message = new Message
            {
                Token = registrationToken,
                Data = new Dictionary<string, string>
                {
                    { "type", "auction_update" },
                    { "auctionId", "12345" },
                    { "itemId", "67890" },
                    { "currentBid", "10000" },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Uptime = 3600 // 消息 TTL（秒）
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 数据消息发送成功: {registrationToken}");
            }
        }

        /// <summary>
        /// 示例 4: 发送主题推送（订阅该主题的所有用户）
        /// </summary>
        public async Task Example4_SendTopicMessage()
        {
            var message = new Message
            {
                Topic = "auction_updates",
                Notification = new Notification
                {
                    Title = "新拍品上线",
                    Body = "刚刚发布了新的拍品！"
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "auction_channel",
                        Sound = "default"
                    }
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 主题推送成功: auction_updates");
            }
        }

        /// <summary>
        /// 示例 5: 批量发送推送
        /// </summary>
        public async Task Example5_BatchSend(string[] registrationTokens)
        {
            const int batchSize = 500; // FCM 支持一次最多 500 个 Token

            var batches = registrationTokens
                .Select((token, index) => new { token, index })
                .GroupBy(x => x.index / batchSize)
                .Select(g => g.Select(x => x.token));

            foreach (var batch in batches)
            {
                var message = new MulticastMessage
                {
                    Tokens = batch.ToList(),
                    Notification = new Notification
                    {
                        Title = "批量通知",
                        Body = "这是一条批量发送的通知"
                    },
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification
                        {
                            ChannelId = "default_channel",
                            Sound = "default"
                        }
                    }
                };

                var response = await _messaging.SendMulticastAsync(message);

                Console.WriteLine($"✅ 批量推送完成: {response.SuccessCount} 成功, {response.FailureCount} 失败");

                // 处理失败的 Token
                if (response.FailureCount > 0)
                {
                    for (int i = 0; i < response.Responses.Count; i++)
                    {
                        if (!response.Responses[i].IsSuccess)
                        {
                            var failedToken = batch.ElementAt(i);
                            var error = response.Responses[i].Exception.Message;
                            Console.WriteLine($"❌ 推送失败: {failedToken}, 原因: {error}");
                            
                            // TODO: 从数据库移除无效 Token
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 示例 6: 条件推送（满足特定条件的用户）
        /// </summary>
        public async Task Example6_ConditionalMessage()
        {
            var message = new Message
            {
                Condition = "'auction_updates' in topics && 'high_priority' in topics",
                Notification = new Notification
                {
                    Title = "重要通知",
                    Body = "仅发送给订阅了拍卖更新和高优先级的用户"
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 条件推送成功");
            }
        }

        /// <summary>
        /// 示例 7: 优先级控制
        /// </summary>
        public async Task Example7_PriorityControl(string registrationToken)
        {
            // 高优先级 - 立即发送
            var highPriorityMessage = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "紧急通知",
                    Body = "需要立即处理"
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            // 普通优先级 - 节省电量
            var normalPriorityMessage = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "普通通知",
                    Body = "非紧急消息"
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.Normal
                }
            };

            await _messaging.SendAsync(highPriorityMessage);
            await _messaging.SendAsync(normalPriorityMessage);
        }

        /// <summary>
        /// 示例 8: 消息 TTL（过期时间）
        /// </summary>
        public async Task Example8_MessageTtl(string registrationToken)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "限时优惠",
                    Body = "优惠将在 1 小时后过期"
                },
                Android = new AndroidConfig
                {
                    Uptime = 3600 // 1 小时（3600 秒）
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 带 TTL 的消息发送成功");
            }
        }

        /// <summary>
        /// 示例 9: 折叠键（相同折叠键的消息会互相覆盖）
        /// </summary>
        public async Task Example9_CollapseKey(string registrationToken)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "最新消息",
                    Body = "这是最新的拍卖状态"
                },
                CollapseKey = "auction_12345", // 相同的折叠键会互相覆盖
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 带折叠键的消息发送成功");
            }
        }

        /// <summary>
        /// 示例 10: 拍卖出价通知（实际业务场景）
        /// </summary>
        public async Task Example10_AuctionBidNotification(string registrationToken,
                                                          Guid auctionId,
                                                          Guid itemId,
                                                          decimal amount)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "出价成功",
                    Body = $"您的出价 ¥{amount:N0} 已成功提交"
                },
                Data = new Dictionary<string, string>
                {
                    { "type", "bid_placed" },
                    { "auctionId", auctionId.ToString() },
                    { "itemId", itemId.ToString() },
                    { "amount", amount.ToString() },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "bids_channel",
                        Sound = "default",
                        NotificationCount = 1,
                        ClickAction = "OPEN_AUCTION_DETAIL"
                    }
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 拍卖出价通知已发送: {registrationToken}");
                // TODO: 记录推送日志
            }
            else
            {
                Console.WriteLine($"❌ 拍卖出价通知发送失败");
                // TODO: 记录失败日志，加入重试队列
            }
        }

        /// <summary>
        /// 示例 11: 错误处理
        /// </summary>
        public async Task Example11_ErrorHandling(string registrationToken)
        {
            try
            {
                var message = new Message
                {
                    Token = registrationToken,
                    Notification = new Notification
                    {
                        Title = "测试推送",
                        Body = "这是一条测试消息"
                    }
                };

                var response = await _messaging.SendAsync(message);

                if (string.IsNullOrEmpty(response))
                {
                    Console.WriteLine($"❌ 推送失败: Token 可能无效");
                    // TODO: 从数据库移除无效 Token
                }
            }
            catch (FirebaseMessagingException ex)
            {
                Console.WriteLine($"❌ Firebase Messaging 错误: {ex.Message}");
                Console.WriteLine($"错误代码: {ex.MessagingErrorCode}");
                
                switch (ex.MessagingErrorCode)
                {
                    case MessagingErrorCode.Unregistered:
                        Console.WriteLine($"⚠️ Token 已注销，需要从数据库移除: {registrationToken}");
                        // TODO: 从数据库移除无效 Token
                        break;

                    case MessagingErrorCode.InvalidArgument:
                        Console.WriteLine($"⚠️ 参数无效");
                        break;

                    case MessagingErrorCode.SenderIdMismatch:
                        Console.WriteLine($"⚠️ Sender ID 不匹配");
                        break;

                    case MessagingErrorCode.TooManyRequests:
                        Console.WriteLine($"⚠️ 请求过于频繁，需要限流");
                        // TODO: 实现限流策略
                        break;

                    default:
                        Console.WriteLine($"❌ 未处理的错误: {ex.Message}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 发生异常: {ex.Message}");
                // TODO: 记录错误日志，发送告警
            }
        }

        /// <summary>
        /// 示例 12: 不同平台配置
        /// </summary>
        public async Task Example12_PlatformSpecificConfig(string registrationToken)
        {
            var message = new Message
            {
                Token = registrationToken,
                Notification = new Notification
                {
                    Title = "跨平台通知",
                    Body = "支持 iOS 和 Android"
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "default_channel",
                        Sound = "default",
                        NotificationCount = 1,
                        Color = "#ff0000", // 通知图标背景色
                        Tag = "notification_tag_123" // 通知标签
                    }
                },
                Apns = new ApnsConfig
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "apns-priority", "10" }
                    },
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = "跨平台通知",
                            Body = "支持 iOS 和 Android"
                        },
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var response = await _messaging.SendAsync(message);

            if (!string.IsNullOrEmpty(response))
            {
                Console.WriteLine($"✅ 跨平台推送成功");
            }
        }

        /// <summary>
        /// 主函数 - 运行所有示例
        /// </summary>
        public static async Task Main(string[] args)
        {
            // 配置（实际应用中从配置文件读取）
            var serviceAccountPath = "./certs/firebase-service-account.json";
            var projectId = "your-project-id";

            var example = new FcmExample(serviceAccountPath, projectId);

            // 测试设备 Token
            var registrationToken = "YOUR_REGISTRATION_TOKEN";

            try
            {
                // 运行示例
                await example.Example1_SendBasicNotification(registrationToken);
                await example.Example2_SendNotificationWithData(registrationToken);
                await example.Example3_SendDataMessage(registrationToken);
                await example.Example4_SendTopicMessage();
                await example.Example11_ErrorHandling(registrationToken);
                await example.Example10_AuctionBidNotification(registrationToken,
                    Guid.NewGuid(), Guid.NewGuid(), 10000);

                Console.WriteLine("✅ 所有示例运行完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 运行示例时发生错误: {ex.Message}");
            }
        }
    }
}
