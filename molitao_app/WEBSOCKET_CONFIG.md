# WebSocket 配置参数分析

## 一、配置概览

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        WebSocket 架构                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   Flutter App                          后端服务                              │
│   ┌──────────────────┐                ┌──────────────────┐                 │
│   │ WebSocketService │ ────WS───────▶ │ FreeIM ImServer  │                 │
│   │                  │                │   (端口 6001)     │                 │
│   └──────────────────┘                └────────┬─────────┘                 │
│           │                                    │                            │
│           │ pre-connect                         │ Redis Pub/Sub             │
│           ▼                                    ▼                            │
│   ┌──────────────────┐                ┌──────────────────┐                 │
│   │  API Server      │                │  Redis           │                 │
│   │ www.molitao.top  │                │  172.24.38.98    │                 │
│   └──────────────────┘                └──────────────────┘                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 二、前端配置 (Flutter App)

### 2.1 WebSocketService 配置

**文件**: `lib/data/services/websocket_service.dart`

| 参数 | 值 | 说明 |
|------|-----|------|
| `_httpBaseUrl` | `https://www.molitao.top` | API 基础地址 |
| `_reconnectDelaySeconds` | `5` | 重连延迟（秒） |
| WebSocket 端点 | `/ws/pre-connect` | 获取 WebSocket 连接信息的 API |

**当前问题**：
- ❌ 无心跳机制（ping/pong）
- ❌ 无连接超时配置
- ❌ 无最大重连次数限制
- ❌ 重连间隔固定，无指数退避

### 2.2 ChatStore 配置

**文件**: `lib/presentation/providers/chat_store.dart`

| 参数 | 值 | 说明 |
|------|-----|------|
| 默认频道 | `-1_auction` | 秒杀场频道 |
| 消息上限 | `800` 条 | 最多保留消息数 |
| 消息清理 | 保留最新 `750` 条 | 超出时删除旧消息 |

**当前问题**：
- ❌ WebSocket 重连后不自动重新 joinChannel
- ❌ 无连接状态监控和 UI 反馈

---

## 三、后端配置 (FreeIM)

### 3.1 ImServer 配置

**开发环境**: `backend/FreeIM/ImServer/appsettings.json`

```json
{
  "Urls": "http://*:6001",
  "ImServerOption": {
    "RedisClient": "127.0.0.1:6379,poolsize=10,defaultDatabase=0",
    "Servers": "127.0.0.1:6001",
    "Server": "127.0.0.1:6001"
  }
}
```

**生产环境**: `backend/FreeIM/ImServer/appsettings.Production.json`

```json
{
  "ImServerOption": {
    "RedisClient": "172.24.38.98:6379,poolsize=10,password=7yD3Ddd34,defaultDatabase=0",
    "Servers": "ws.molitao.top",
    "Server": "ws.molitao.top"
  }
}
```

| 参数 | 开发环境 | 生产环境 | 说明 |
|------|---------|---------|------|
| `Urls` | `http://*:6001` | - | 监听端口 |
| `Servers` | `127.0.0.1:6001` | `ws.molitao.top` | WebSocket 服务器地址 |
| `Server` | `127.0.0.1:6001` | `ws.molitao.top` | 当前服务器标识 |
| `RedisClient` | `127.0.0.1:6379` | `172.24.38.98:6379` | Redis 连接 |

### 3.2 API Server FreeIM 配置

**文件**: `backend/src/TtWork.Project.Web.Host/appsettings.json`

```json
{
  "FreeIm": {
    "Servers": "127.0.0.1"  // 开发环境
  }
}
```

**生产环境**: `appsettings.Production.json`

```json
{
  "FreeIm": {
    "Servers": "127.0.0.1"  // 注意：生产环境也指向本地，通过 Nginx 代理
  }
}
```

---

## 四、Redis 配置

### 4.1 开发环境

```json
{
  "Redis": {
    "ConnectionString": "127.0.0.1:6379",
    "DatabaseId": 0,
    "SyncTimeout": 5000,
    "AsyncTimeout": 5000,
    "ConnectTimeout": 5000,
    "MaxPoolSize": 50,
    "ConnectRetry": 3,
    "KeepAlive": 60,
    "AbortOnConnectFail": false
  }
}
```

### 4.2 生产环境

```json
{
  "Redis": {
    "ConnectionString": "172.24.38.98:6379,password=7yD3Ddd34,syncTimeout=5000,abortConnect=false",
    "DatabaseId": 0
  }
}
```

---

## 五、消息类型映射

**文件**: `lib/data/services/websocket_service.dart`

| 数值类型 | 字符串类型 | 说明 |
|---------|-----------|------|
| 1 | Text | 文本消息 |
| 2 | Image | 图片消息 |
| 3 | File | 文件消息 |
| 10 | Receipt | 回执 |
| 100 | Welcome | 用户进入 |
| 101 | Goodbye | 用户离开 |
| 102 | BanUser | 禁言 |
| 110 | Backout | 撤回 |
| **1000** | **AuctionStart** | **拍卖开始** |
| **1002** | **AuctionBid** | **出价消息** |
| **1010** | **AuctionEnd** | **拍卖结束** |
| **1011** | **AuctionDeal** | **成交消息** |
| 2000 | KasecStatusChanged | 状态变更 |

---

## 六、连接流程

```
1. App 调用 POST /ws/pre-connect (带 Authorization header)
         │
         ▼
2. 后端返回: { result: { server: "ws://xxx", websocketId: 123 } }
         │
         ▼
3. App 使用返回的 server 地址建立 WebSocket 连接
         │
         ▼
4. 连接成功后，App 发送订阅频道请求: joinChannel("-1_auction")
         │
         ▼
5. 后端通过 Redis Pub/Sub 广播消息到所有订阅者
```

---

## 七、建议优化项

### 7.1 前端优化

| 优化项 | 当前状态 | 建议 |
|--------|---------|------|
| 心跳机制 | ❌ 无 | 添加 30 秒 ping，检测死连接 |
| 重连策略 | 固定 5 秒 | 指数退避：1s → 2s → 4s → 8s → 16s |
| 最大重连次数 | ❌ 无限 | 限制 10 次，之后提示用户 |
| 连接超时 | ❌ 无 | 添加 10 秒连接超时 |
| 重连后订阅 | ❌ 丢失 | 自动重新 joinChannel |
| 连接状态 UI | ❌ 无 | 显示连接中断提示 |

### 7.2 后端优化

| 优化项 | 当前状态 | 建议 |
|--------|---------|------|
| WebSocket 超时 | 默认值 | 配置明确的读写超时 |
| 最大连接数 | 默认值 | 根据服务器资源配置 |
| 消息队列 | Redis | 可考虑消息持久化 |

---

## 八、配置参数建议

### 8.1 推荐的前端配置

```dart
class WebSocketConfig {
  static const String httpBaseUrl = 'https://www.molitao.top';
  static const String wsEndpoint = '/ws/pre-connect';
  
  static const int reconnectInitialDelay = 1;      // 初始重连延迟（秒）
  static const int reconnectMaxDelay = 30;          // 最大重连延迟（秒）
  static const int reconnectMaxAttempts = 10;       // 最大重连次数
  static const int connectionTimeout = 10;          // 连接超时（秒）
  static const int heartbeatInterval = 30;          // 心跳间隔（秒）
  static const int heartbeatTimeout = 10;           // 心跳超时（秒）
}
```

### 8.2 推荐的后端配置

```json
{
  "ImServerOption": {
    "RedisClient": "...",
    "Servers": "ws.molitao.top",
    "Server": "ws.molitao.top",
    "ConnectionTimeout": 10000,
    "ReadTimeout": 60000,
    "WriteTimeout": 10000,
    "MaxConnections": 10000
  }
}
```
