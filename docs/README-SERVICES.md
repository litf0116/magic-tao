# 魔力淘后台服务快速启动指南

## 服务概览

魔力淘项目包含以下主要服务：

1. **TtWork.Project.Web.Host** - 主API服务
   - 端口: 5000
   - 功能: 处理所有业务逻辑的Web API
   
2. **ImServer** - 即时通讯服务
   - 端口: 6001
   - 功能: WebSocket即时通讯服务
   
3. **TtWork.Project.Migrator** - 数据库迁移工具
   - 功能: 初始化和更新数据库结构

## 环境要求

- ✅ .NET 8.0 SDK
- ✅ MySQL 数据库
- ✅ Redis 缓存服务
- ✅ Visual Studio 2022 或 JetBrains Rider (可选)

## 快速启动

### 方式一：使用启动脚本 (推荐)

#### Windows 用户:
```bash
# 运行批处理脚本
./start-services.bat
```

#### Linux/Mac 用户:
```bash
# 给脚本执行权限
chmod +x start-services.sh

# 运行脚本
./start-services.sh
```

### 方式二：手动命令行启动

#### 1. 运行数据库迁移
```bash
cd src/TtWork.Project.Migrator
dotnet run
```

#### 2. 启动即时通讯服务
```bash
cd FreeIM/ImServer
dotnet run --urls=http://*:6001
```

#### 3. 启动主API服务
```bash
cd src/TtWork.Project.Web.Host
dotnet run --urls=http://*:5000
```

### 方式三：使用 dotnet CLI (开发模式)

#### 恢复依赖包
```bash
dotnet restore
```

#### 启动多个项目
```bash
# 在不同终端窗口中运行:

# 终端1 - IM服务
cd FreeIM/ImServer && dotnet run

# 终端2 - API服务  
cd src/TtWork.Project.Web.Host && dotnet run
```

## 服务验证

启动完成后，可以通过以下方式验证服务状态：

### 检查端口占用
```bash
# Windows
netstat -an | findstr ":5000"
netstat -an | findstr ":6001"

# Linux/Mac
netstat -an | grep ":5000"
netstat -an | grep ":6001"
```

### API服务验证
- 访问: http://localhost:5000
- Swagger文档: http://localhost:5000/swagger

### IM服务验证
- WebSocket地址: ws://localhost:6001

## 配置文件

### 主要配置文件位置:
- API服务配置: `src/TtWork.Project.Web.Host/appsettings.json`
- IM服务配置: `FreeIM/ImServer/appsettings.json`

### 关键配置项:
- 数据库连接字符串
- Redis连接配置
- 微信开发配置
- 又拍云OSS配置

## 故障排除

### 常见问题:

1. **端口被占用**
   ```bash
   # 查找占用进程
   netstat -ano | findstr ":5000"
   # 杀死进程
   taskkill /PID <进程ID> /F
   ```

2. **数据库连接失败**
   - 检查MySQL服务是否启动
   - 验证连接字符串配置
   - 确保数据库权限正确

3. **Redis连接失败**
   - 确保Redis服务已启动
   - 检查Redis连接配置

### 日志位置:
- API服务日志: `src/TtWork.Project.Web.Host/Logs/`
- IM服务日志: `FreeIM/ImServer/logs/`

## 开发建议

### 使用IDE启动 (推荐开发时使用):
1. 打开 `Molitao.sln` 解决方案
2. 设置多个启动项目:
   - TtWork.Project.Web.Host
   - ImServer
3. 按 F5 启动调试

### 生产部署:
```bash
# 发布API服务
dotnet publish src/TtWork.Project.Web.Host -c Release -o ./publish/api

# 发布IM服务
dotnet publish FreeIM/ImServer -c Release -o ./publish/im
```

## 服务管理命令

### 停止所有服务:
```bash
# 使用脚本停止
./start-services.sh  # 选择选项5

# 手动停止
pkill -f "TtWork.Project.Web.Host"
pkill -f "ImServer"
```

### 重启服务:
```bash
# 先停止再启动
./start-services.sh  # 选择选项5然后选择选项4
```
