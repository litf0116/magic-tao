using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FreeRedis;
using Newtonsoft.Json;

namespace FreeIM;

/// <summary>
/// im 核心类实现
/// </summary>
public class ImClient {
    protected readonly RedisClient Redis;
    private readonly string[] _servers;
    protected readonly string RedisPrefix;
    private readonly string _pathMatch;

    /// <summary>
    /// 推送消息的事件，可审查推向哪个Server节点
    /// </summary>
    public EventHandler<ImSendEventArgs> OnSend;

    /// <summary>
    /// 初始化 imclient
    /// </summary>
    /// <param name="options"></param>
    public ImClient(ImClientOptions options) {
        if (options.Redis == null) throw new ArgumentException("ImClientOptions.Redis 参数不能为空");
        Redis = options.Redis;
        _servers = options.Servers;
        RedisPrefix = $"wsim{options.PathMatch.Replace('/', '_')}";
        _pathMatch = options.PathMatch ?? "/ws";
    }

    /// <summary>
    /// 负载分区规则：取clientId后四位字符，转成10进制数字0-65535，求模
    /// </summary>
    /// <param name="clientId">客户端id</param>
    /// <returns></returns>
    protected string SelectServer(long clientId) {
        // var serversIdx = int.Parse(clientId.ToString("N").Substring(28), NumberStyles.HexNumber) % _servers.Length;
        // if (serversIdx >= _servers.Length) serversIdx = 0;
        return _servers[0];
    }

    /// <summary>
    /// ImServer 连接前的负载、授权，返回 ws 目标地址，使用该地址连接 websocket 服务端
    /// </summary>
    /// <param name="clientId">客户端id</param>
    /// <param name="clientMetaData">客户端相关信息，比如ip</param>
    /// <returns>websocket 地址：ws://xxxx/ws?token=xxx</returns>
    public string PrevConnectServer(long clientId, string clientMetaData) {
        var server = SelectServer(clientId);
        var token = $"{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");
        Redis.Set($"{RedisPrefix}Token{token}",
            JsonConvert.SerializeObject(new TokenEvent(clientId, clientMetaData)), 60);
#if DEBUG
        return $"ws://{server}{_pathMatch}?token={token}";
#else
        return $"wss://{server}{_pathMatch}?token={token}";
#endif
    }

    /// <summary>
    /// 向指定的多个客户端id发送消息
    /// </summary>
    public void SendMessage(MessageEvent msg) {
        Console.WriteLine($"[ImClient] SendMessage开始: SenderClientId={msg.SenderClientId}, ReceiverCount={msg.ReceiverClientIds.Count}, MessageType={msg.Message.type}");
        
        //有可能掉线了不在chanlist里.把自己加上
        msg.ReceiverClientIds.Add(msg.Message.from);
        Console.WriteLine($"[ImClient] 添加发送者到接收列表: From={msg.Message.from}");

        msg.ReceiverClientIds = msg.ReceiverClientIds.Distinct().ToList();
        Console.WriteLine($"[ImClient] 去重后接收者数量: {msg.ReceiverClientIds.Count}");
        
        Dictionary<string, ImSendEventArgs> dic = new();

        foreach (var clientId in msg.ReceiverClientIds) {
            // string server = SelectServer(clientId);
            string server = "Local";
            if (dic.ContainsKey(server) == false)
                dic.Add(server, new ImSendEventArgs(server, msg.SenderClientId, msg.Message, msg.Receipt));
            dic[server].ReceiveClientIds.Add(clientId);
        }

        Console.WriteLine($"[ImClient] 按服务器分组: {dic.Count}个服务器");

        // var messageJson = JsonConvert.SerializeObject(message);
        foreach (var sendArgs in dic.Values) {
            Console.WriteLine($"[ImClient] 发布消息到Redis: Server={sendArgs.Server}, ReceiverCount={sendArgs.ReceiveClientIds.Count}");
            
            OnSend?.Invoke(this, sendArgs);
            
            var redisChannel = $"{RedisPrefix}Server_Local";
            var messageEvent = new MessageEvent(msg.SenderClientId, sendArgs.ReceiveClientIds, msg.Message, sendArgs.Receipt);
            var messageJson = JsonConvert.SerializeObject(messageEvent);
            
            Console.WriteLine($"[ImClient] Redis发布: Channel={redisChannel}, Message={messageJson}");
            
            Redis.Publish(redisChannel, messageJson);
            
            Console.WriteLine($"[ImClient] Redis发布完成");
        }
        
        Console.WriteLine($"[ImClient] SendMessage完成");
    }

    /// <summary>
    /// 获取所在线客户端id
    /// </summary>
    public IEnumerable<long> GetClientListByOnline() {
        return Redis.HKeys($"{RedisPrefix}Online")
            .Select(a => long.TryParse(a, out var tryguid) ? tryguid : 0).Where(a => a != 0);
    }

    /// <summary>
    /// 判断客户端是否在线
    /// </summary>
    public bool HasOnline(long clientId) {
        return Redis.HGet<int>($"{RedisPrefix}Online", clientId.ToString()) > 0;
    }

    /// <summary>
    /// 强制下线
    /// </summary>
    public void ForceOffline(long clientId) {
        string server = SelectServer(clientId);
        Redis.Publish($"{RedisPrefix}Server_Local", $"__FreeIM__(ForceOffline){clientId}");
    }

    /// <summary>
    /// 事件订阅
    /// </summary>
    public void EventBus(
        Action<TokenEvent> online,
        Action<TokenEvent> offline) {
        var chanOnline = $"evt_{RedisPrefix}Online";
        var chanOffline = $"evt_{RedisPrefix}Offline";
        Redis.Subscribe([chanOnline, chanOffline], (chan, msg) => {
            if (chan == chanOnline)
                online(JsonConvert.DeserializeObject<TokenEvent>(msg as string));
            if (chan == chanOffline)
                offline(JsonConvert.DeserializeObject<TokenEvent>(msg as string));
        });
    }

    #region 群聊频道，每次上线都必须重新加入

    /// <summary>
    /// 加入群聊频道，每次上线都必须重新加入
    /// </summary>
    /// <param name="clientId">客户端id</param>
    /// <param name="chan">群聊频道名</param>
    public void JoinChan(long clientId, string chan) {
        using (var pipe = Redis.StartPipe()) {
            pipe.HSet($"{RedisPrefix}Chan{chan}", clientId.ToString(), 0);
            pipe.HSet($"{RedisPrefix}Client{clientId}", chan, 0);
            pipe.HIncrBy($"{RedisPrefix}ListChan", chan, 1);
            pipe.EndPipe();
        }

        return;


        using (var pipe = Redis.StartPipe()) {
            pipe.HSet($"{RedisPrefix}Chan{chan}", clientId.ToString(), 0);
            // pipe.HSet($"{_redisPrefix}Chan{chan}", clientId.ToString(), 0);

            // pipe.HIncrBy($"{_redisPrefix}ListChan", chan, 1);
            pipe.EndPipe();
        }

        var count = Redis.HKeys($"{RedisPrefix}Chan{chan}").Length;
        Console.WriteLine($"{RedisPrefix}Chan{chan} length {count}");
        Redis.HSet($"{RedisPrefix}ListChan", chan, count);
        // if (_redis.HGet($"{_redisPrefix}Chan{chan}", clientId.ToString()) == null) {
        //     _redis.HIncrBy($"{_redisPrefix}ListChan", chan, 1);
        // }
    }

    public void DeleteChan(string chan) {
        Redis.Del($"{RedisPrefix}Chan{chan}");
        Redis.HDel($"{RedisPrefix}ListChan", chan);
    }

    /// <summary>
    /// 离开群聊频道
    /// </summary>
    /// <param name="clientId">客户端id</param>
    /// <param name="chans">群聊频道名</param>
    public void LeaveChan(long clientId, params string[] chans) {
        if (chans?.Any() != true) return;
        using var pipe = Redis.StartPipe();
        foreach (var chan in chans) {
            pipe.HDel($"{RedisPrefix}Chan{chan}", clientId.ToString());
            pipe.HDel($"{RedisPrefix}Client{clientId}", chan);
            pipe.Eval(
                $"if redis.call('HINCRBY', KEYS[1], '{chan}', '-1') <= 0 then redis.call('HDEL', KEYS[1], '{chan}') end return 1",
                new[] { $"{RedisPrefix}ListChan" });
        }

        pipe.EndPipe();
    }

    /// <summary>
    /// 获取群聊频道所有客户端id（测试）
    /// </summary>
    /// <param name="chan">群聊频道名</param>
    /// <returns></returns>
    public long[] GetChanClientList(string chan) {
        return Redis.HKeys($"{RedisPrefix}Chan{chan}").Select(a => long.Parse(a)).ToArray();
    }

    /// <summary>
    /// 清理群聊频道的离线客户端（测试）
    /// </summary>
    /// <param name="chan">群聊频道名</param>
    public void ClearChanClient(string chan) {
        var websocketIds = Redis.HKeys($"{RedisPrefix}Chan{chan}");
        var offline = new List<string>();
        var span = websocketIds.AsSpan();
        var start = span.Length;
        while (start > 0) {
            start = start - 10;
            var length = 10;
            if (start < 0) {
                length = start + 10;
                start = 0;
            }

            var slice = span.Slice(start, length);
            var hvals = Redis.HMGet($"{RedisPrefix}Online", slice.ToArray().Select(b => b.ToString()).ToArray());
            for (var a = length - 1; a >= 0; a--) {
                if (string.IsNullOrEmpty(hvals[a])) {
                    offline.Add(span[start + a]);
                    span[start + a] = null;
                }
            }
        }

        //删除离线订阅
        if (offline.Any()) Redis.HDel($"{RedisPrefix}Chan{chan}", offline.ToArray());
    }

    /// <summary>
    /// 获取所有群聊频道和在线人数
    /// </summary>
    /// <returns>频道名和在线人数</returns>
    public IEnumerable<(string chan, long online)> GetChanList() {
        var ret = Redis.HGetAll<long>($"{RedisPrefix}ListChan");
        return ret.Select(a => (a.Key, a.Value));
    }

    /// <summary>
    /// 获取用户参与的所有群聊频道
    /// </summary>
    /// <param name="clientId">客户端id</param>
    /// <returns></returns>
    public string[] GetChanListByClientId(long clientId) {
        return Redis.HKeys($"{RedisPrefix}Client{clientId}");
    }

    /// <summary>
    /// 获取群聊频道的在线人数
    /// </summary>
    /// <param name="chan">群聊频道名</param>
    /// <returns>在线人数</returns>
    public long GetChanOnline(string chan) {
        return Redis.HGet<long>($"{RedisPrefix}ListChan", chan);
    }

    /// <summary>
    /// 发送群聊消息，所有在线的用户将收到消息
    /// </summary>
    /// <param name="senderClientId">发送者的客户端id</param>
    /// <param name="chan">群聊频道名</param>
    /// <param name="message">消息</param>
    public void SendChanMessage(long senderClientId, string chan, ChatMessage message) {
        Console.WriteLine($"[ImClient] SendChanMessage开始: senderClientId={senderClientId}, chan={chan}, messageType={message.type}");
        
        var websocketIds = Redis.HKeys($"{RedisPrefix}Chan{chan}");
        Console.WriteLine($"[ImClient] 获取频道用户列表: 频道={chan}, 用户数量={websocketIds.Length}");
        
        var validClientIds = websocketIds.Where(a => !string.IsNullOrEmpty(a))
            .Select(a => long.TryParse(a, out var tryuuid) ? tryuuid : 0).Where(x => x != 0).ToList();
        
        Console.WriteLine($"[ImClient] 有效用户ID列表: {string.Join(",", validClientIds)}");
        
        var messageEvent = new MessageEvent(senderClientId, validClientIds, message);
        Console.WriteLine($"[ImClient] 创建MessageEvent: SenderClientId={messageEvent.SenderClientId}, ReceiverCount={messageEvent.ReceiverClientIds.Count}");
        
        SendMessage(messageEvent);
        Console.WriteLine($"[ImClient] SendChanMessage完成");
    }

    #endregion
}