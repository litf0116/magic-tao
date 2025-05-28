using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace FreeIM;

public static class FreeImServerExtensions {
    private static bool _isUseWebSockets;

    /// <summary>
    /// 启用 ImServer 服务端
    /// </summary>
    public static void UseFreeImServer(this IApplicationBuilder app, ImServerOptions options) {
        app.Map(options.PathMatch, build => {
            var imServer = new ImServer(options);
            if (_isUseWebSockets == false) {
                _isUseWebSockets = true;
                build.UseWebSockets();
            }

            build.Use((ctx, next) =>
                imServer.Acceptor(ctx, next));
        });
    }
}

/// <summary>
/// im 核心类实现的配置所需
/// </summary>
public class ImServerOptions : ImClientOptions {
    /// <summary>
    /// 设置服务名称，它应该是 servers 内的一个
    /// </summary>
    public string Server { get; set; }
}

class ImServer : ImClient {
    private string Server { get; set; }

    public ImServer(ImServerOptions options) : base(options) {
        Server = options.Server;
        Redis.Subscribe($"{RedisPrefix}Server_Local", RedisSubscribeMessage);
    }

    private const int BufferSize = 4096;

    private readonly ConcurrentDictionary<long, ConcurrentDictionary<Guid, ImServerClient>> _clients =
        new();

    private class ImServerClient(WebSocket socket, long clientId) {
        public readonly WebSocket Socket = socket;

        // ReSharper disable once UnusedMember.Local
        public long ClientId = clientId;
    }

    // ReSharper disable once UnusedParameter.Global
    internal async Task Acceptor(HttpContext context, Func<Task> next) {
        if (!context.WebSockets.IsWebSocketRequest) return;
        string token = context.Request.Query["token"];
        if (string.IsNullOrEmpty(token)) return;
        var tokenValue = await Redis.GetAsync($"{RedisPrefix}Token{token}");
        if (string.IsNullOrEmpty(tokenValue)) {
            Log.Error("token Error {@token}", token);
            throw new Exception("授权错误：用户需通过 ImHelper.PrevConnectServer 获得包含 token 的连接");
        }

        var data = JsonConvert.DeserializeObject<TokenEvent>(tokenValue);

        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var cli = new ImServerClient(socket, data.ClientId);
        var newid = Guid.NewGuid();

        var wsList = _clients.GetOrAdd(data.ClientId, _ => new ConcurrentDictionary<Guid, ImServerClient>());
        wsList.TryAdd(newid, cli);
        using (var pipe = Redis.StartPipe()) {
            pipe.HIncrBy($"{RedisPrefix}Online", data.ClientId.ToString(), 1);
            pipe.Publish($"evt_{RedisPrefix}Online", tokenValue);
            pipe.EndPipe();
        }

        var buffer = new byte[BufferSize];
        var seg = new ArraySegment<byte>(buffer);
        try {
            while (socket.State == WebSocketState.Open && _clients.ContainsKey(data.ClientId)) {
                var incoming = await socket.ReceiveAsync(seg, CancellationToken.None);
                // ReSharper disable once UnusedVariable
                var outgoing = new ArraySegment<byte>(buffer, 0, incoming.Count);
            }

            socket.Abort();
        }
        catch {
            // ignored
        }

        Log.Warning($"ImServer Acceptor End with User {data.ClientId} , socket.State {socket.State}");

        // ReSharper disable once UnusedVariable
        wsList.TryRemove(newid, out var oldCli);
        // ReSharper disable once UnusedVariable
        await Redis.EvalAsync(
            // ReSharper disable StringLiteralTypo
            $"if redis.call('HINCRBY', KEYS[1], '{data.ClientId}', '-1') <= 0 then redis.call('HDEL', KEYS[1], '{data.ClientId}') end return 1",
            [$"{RedisPrefix}Online"]);

        if (wsList.Count == 0) {
            // if (wsList.Any() == false) 
            _clients.TryRemove(data.ClientId, out var oldWsList);
            LeaveChan(data.ClientId, GetChanListByClientId(data.ClientId));
        }


        await Redis.PublishAsync($"evt_{RedisPrefix}Offline", tokenValue);
    }

    void RedisSubscribeMessage(string chan, object msg) {
        try {
            Log.Information("RedisSubscribeMessage ,{@o}", msg);
            var msgText = msg as string;
            if (msgText == null) return;
            if (msgText.StartsWith("__FreeIM__(ForceOffline)")) {
                if (long.TryParse(msgText.Substring(24), out var clientId)) {
                    if (_clients.TryRemove(clientId, out var oldClients)) {
                        foreach (var oldCli in oldClients) {
                            try {
                                oldCli.Value.Socket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable,
                                    "disconnect",
                                    CancellationToken.None).GetAwaiter().GetResult();
                            }
                            catch {
                                // ignored
                            }

                            try {
                                oldCli.Value.Socket.Abort();
                            }
                            catch {
                                // ignored
                            }

                            try {
                                oldCli.Value.Socket.Dispose();
                            }
                            catch {
                                // ignored
                            }
                        }
                    }
                }

                return;
            }

            //_redist.pub过来的是Json序列化的字符串
            var data = JsonConvert
                .DeserializeObject<MessageEvent>(msgText);
            //Console.WriteLine($"收到消息：{data.content}" + (data.receipt ? "【需回执】" : ""));

            var outgoing =
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data.Message)));
            foreach (var clientId in data.ReceiverClientIds) {
                if (_clients.TryGetValue(clientId, out var wsList) == false) {
                    //Console.WriteLine($"websocket{clientId} 离线了，{data.content}" + (data.receipt ? "【需回执】" : ""));
                    if (data.SenderClientId != 0 && clientId != data.SenderClientId && data.Receipt) {
                        SendMessage(new MessageEvent(clientId, [data.SenderClientId],
                            data.Message with { type = ChatMessageType.Receipt, receipt = "用户不在线" }));
                    }

                    continue;
                }

                var sockArray = wsList.Values.ToArray();

                //如果接收消息人是发送者，并且接收者只有1个以下，则不发送
                // if (clientId == data.SenderClientId && sockArray.Length <= 1) continue;
                //只有接收者为多端时，才转发消息通知其他端
                if (clientId == data.SenderClientId) {
                    //Console.WriteLine("自己发自己,不处理");
                    continue;
                }

                foreach (var sh in sockArray) {
                    try {
                        sh.Socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None)
                            .ContinueWith(async (t, state) => {
                                if (t.Exception != null) {
                                    if (state is not WebSocket ws) return;
                                    Log.Error(t.Exception, "sh.Socket.SendAsync ERROR");
                                    try {
                                        await ws.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "disconnect",
                                            CancellationToken.None);
                                    }
                                    catch {
                                        // ignored
                                    }

                                    try {
                                        ws.Abort();
                                    }
                                    catch {
                                        // ignored
                                    }

                                    try {
                                        ws.Dispose();
                                    }
                                    catch {
                                        // ignored
                                    }
                                }
                            }, sh.Socket);
                    }
                    catch (Exception e) {
                        Log.Error(e, "sh.Socket.SendAsync Error");
                    }
                }


                if (data.SenderClientId != 0 && clientId != data.SenderClientId && data.Receipt) {
                    SendMessage(
                        new MessageEvent(clientId, [data.SenderClientId],
                            data.Message with { type = ChatMessageType.Receipt, receipt = "发送成功" }));
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"FreeIM.ImServer 订阅方法出错了：{ex.Message}\r\n{ex.StackTrace}");
        }
    }
}