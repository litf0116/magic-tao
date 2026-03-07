// iOS APNs 推送通知示例代码
// 文档: https://github.com/alexalok/dotAPNS

using DotAPNS;
using DotAPNS.Args;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TtWork.Project.PushNotifications.Examples
{
    /// <summary>
    /// APNs 推送通知示例
    /// </summary>
    public class ApnsExample
    {
        private readonly ApnsClient _apnsClient;

        public ApnsExample(string keyId, string teamId, string bundleId, string privateKeyPath)
        {
            var config = new ApnsConfig(
                keyId: keyId,
                teamId: teamId,
                bundleId: bundleId,
                privateKeyPath: privateKeyPath,
                useSandbox: false // 生产环境设为 false
            );

            _apnsClient = new ApnsClient(config);
        }

        /// <summary>
        /// 示例 1: 发送基础通知
        /// </summary>
        public async Task Example1_SendBasicNotification(string deviceToken)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "出价成功",
                        Body = "您的出价 ¥10,000 已成功提交"
                    },
                    Sound = "default",
                    Badge = 1
                }
            };

            var notification = new ApnsNotification(deviceToken, payload);
            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 推送成功: {deviceToken}");
            }
            else
            {
                Console.WriteLine($"❌ 推送失败: {response.Reason}");
            }
        }

        /// <summary>
        /// 示例 2: 发送带自定义数据的推送
        /// </summary>
        public async Task Example2_SendNotificationWithData(string deviceToken)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "新出价提醒",
                        Body = "您关注的拍品刚刚有新出价"
                    },
                    Sound = "default",
                    Badge = 1,
                    Category = "NEW_BID"
                },
                Custom = new Dictionary<string, object>
                {
                    { "type", "new_bid" },
                    { "auctionId", "12345" },
                    { "itemId", "67890" },
                    { "amount", 10000 },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                }
            };

            var notification = new ApnsNotification(deviceToken, payload);
            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 带数据推送成功: {deviceToken}");
            }
        }

        /// <summary>
        /// 示例 3: 发送静默推送（后台更新）
        /// </summary>
        public async Task Example3_SendSilentPush(string deviceToken)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    ContentAvailable = 1 // 静默推送关键参数
                },
                Custom = new Dictionary<string, object>
                {
                    { "type", "auction_update" },
                    { "auctionId", "12345" },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                }
            };

            var notification = new ApnsNotification(deviceToken, payload);
            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 静默推送成功: {deviceToken}");
            }
        }

        /// <summary>
        /// 示例 4: 发送富媒体通知
        /// </summary>
        public async Task Example4_SendRichMediaNotification(string deviceToken)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "新拍品上线",
                        Body = "限量版古董手表正在拍卖"
                    },
                    MutableContent = 1, // 允许 App 扩展通知内容
                    Sound = "default"
                },
                Custom = new Dictionary<string, object>
                {
                    { "image-url", "https://image.molitao.top/auctions/12345.jpg" },
                    { "type", "new_auction" },
                    { "auctionId", "12345" }
                }
            };

            var notification = new ApnsNotification(deviceToken, payload);
            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 富媒体推送成功: {deviceToken}");
            }
        }

        /// <summary>
        /// 示例 5: 批量发送推送
        /// </summary>
        public async Task Example5_BatchSend(string[] deviceTokens)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "拍卖即将结束",
                        Body = "您关注的拍品将在 10 分钟后结束拍卖"
                    },
                    Sound = "default",
                    Badge = 1
                }
            };

            var tasks = new List<Task>();

            foreach (var token in deviceTokens)
            {
                var notification = new ApnsNotification(token, payload);
                tasks.Add(Task.Run(async () =>
                {
                    var response = await _apnsClient.SendAsync(notification);
                    if (response.IsSuccess)
                    {
                        Console.WriteLine($"✅ 批量推送成功: {token}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ 批量推送失败: {token}, 原因: {response.Reason}");
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 示例 6: 错误处理
        /// </summary>
        public async Task Example6_ErrorHandling(string deviceToken)
        {
            try
            {
                var payload = new ApnsPayload
                {
                    Aps = new Aps
                    {
                        Alert = new ApsAlert
                        {
                            Title = "测试推送",
                            Body = "这是一条测试消息"
                        },
                        Sound = "default"
                    }
                };

                var notification = new ApnsNotification(deviceToken, payload);
                var response = await _apnsClient.SendAsync(notification);

                if (!response.IsSuccess)
                {
                    // 处理特定错误
                    switch (response.Reason)
                    {
                        case "Unregistered":
                            Console.WriteLine($"⚠️ Token 无效，需要从数据库移除: {deviceToken}");
                            // TODO: 从数据库移除无效 Token
                            break;

                        case "BadDeviceToken":
                            Console.WriteLine($"⚠️ Token 格式错误: {deviceToken}");
                            // TODO: 标记 Token 为无效
                            break;

                        case "TooManyRequests":
                            Console.WriteLine($"⚠️ 请求过于频繁，需要限流");
                            // TODO: 实现限流策略
                            break;

                        default:
                            Console.WriteLine($"❌ 推送失败: {response.Reason}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 发生异常: {ex.Message}");
                // TODO: 记录错误日志，发送告警
            }
        }

        /// <summary>
        /// 示例 7: 拍卖出价通知（实际业务场景）
        /// </summary>
        public async Task Example7_AuctionBidNotification(string deviceToken, 
                                                         Guid auctionId, 
                                                         Guid itemId, 
                                                         decimal amount)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "出价成功",
                        Body = $"您的出价 ¥{amount:N0} 已成功提交"
                    },
                    Sound = "default",
                    Badge = 1,
                    Category = "BID_SUCCESS"
                },
                Custom = new Dictionary<string, object>
                {
                    { "type", "bid_placed" },
                    { "auctionId", auctionId.ToString() },
                    { "itemId", itemId.ToString() },
                    { "amount", amount },
                    { "timestamp", DateTime.UtcNow.ToString("o") }
                }
            };

            var notification = new ApnsNotification(deviceToken, payload);
            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 拍卖出价通知已发送: {deviceToken}");
                // TODO: 记录推送日志
            }
            else
            {
                Console.WriteLine($"❌ 拍卖出价通知发送失败: {response.Reason}");
                // TODO: 记录失败日志，加入重试队列
            }
        }

        /// <summary>
        /// 示例 8: 优先级控制
        /// </summary>
        public async Task Example8_PriorityControl(string deviceToken)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "紧急通知",
                        Body = "需要立即处理"
                    },
                    Sound = "default",
                    Priority = 10 // 10 = 立即发送，5 = 节省电量
                }
            };

            var notification = new ApnsNotification(deviceToken, payload);
            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 高优先级推送成功");
            }
        }

        /// <summary>
        /// 示例 9: 过期时间设置
        /// </summary>
        public async Task Example9_Expiration(string deviceToken)
        {
            var payload = new ApnsPayload
            {
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = "限时优惠",
                        Body = "优惠将在 1 小时后过期"
                    },
                    Sound = "default"
                }
            };

            var notification = new ApnsNotification(deviceToken, payload)
            {
                Expiration = DateTime.UtcNow.AddHours(1) // 1 小时后过期
            };

            var response = await _apnsClient.SendAsync(notification);

            if (response.IsSuccess)
            {
                Console.WriteLine($"✅ 带过期时间的推送成功");
            }
        }

        /// <summary>
        /// 主函数 - 运行所有示例
        /// </summary>
        public static async Task Main(string[] args)
        {
            // 配置（实际应用中从配置文件读取）
            var keyId = "YOUR_KEY_ID";
            var teamId = "YOUR_TEAM_ID";
            var bundleId = "com.molitao.app";
            var privateKeyPath = "./certs/AuthKey_YOUR_KEY_ID.p8";

            var example = new ApnsExample(keyId, teamId, bundleId, privateKeyPath);

            // 测试设备 Token
            var deviceToken = "YOUR_DEVICE_TOKEN";

            try
            {
                // 运行示例
                await example.Example1_SendBasicNotification(deviceToken);
                await example.Example2_SendNotificationWithData(deviceToken);
                await example.Example3_SendSilentPush(deviceToken);
                await example.Example4_SendRichMediaNotification(deviceToken);
                await example.Example6_ErrorHandling(deviceToken);
                await example.Example7_AuctionBidNotification(deviceToken, 
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
