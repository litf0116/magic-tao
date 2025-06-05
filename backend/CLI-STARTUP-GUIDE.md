# 魔力淘后台服务命令行启动指南

## 📋 服务概览

| 服务名称 | 端口 | 功能描述 | 项目路径 |
|---------|------|----------|----------|
| **API服务** | 5000 | 主要业务逻辑API | `src/TtWork.Project.Web.Host` |
| **IM服务** | 6001 | WebSocket即时通讯 | `FreeIM/ImServer` |
| **数据库迁移** | - | 初始化数据库结构 | `src/TtWork.Project.Migrator` |

## 🚀 快速启动

### 方式一：一键启动所有服务

#### Windows用户：
```bash
# 运行交互式启动脚本
./start-services.bat

# 或快速启动所有服务
./quick-start.bat
```

#### Linux/Mac用户：
```bash
# 运行交互式启动脚本
./start-services.sh
```

### 方式二：单独启动服务

#### 启动API服务
```bash
# 使用脚本
./start-api.sh

# 或手动启动
cd src/TtWork.Project.Web.Host
dotnet run --urls=http://*:5000
```

#### 启动IM服务
```bash
# 使用脚本
./start-im.sh

# 或手动启动
cd FreeIM/ImServer
dotnet run --urls=http://*:6001
```

#### 运行数据库迁移
```bash
# 使用脚本
./migrate.sh

# 或手动运行
cd src/TtWork.Project.Migrator
dotnet run
```

## 🔧 开发模式启动

### 使用 dotnet watch（热重载）
```bash
# API服务热重载
cd src/TtWork.Project.Web.Host
dotnet watch run --urls=http://*:5000

# IM服务热重载
cd FreeIM/ImServer
dotnet watch run --urls=http://*:6001
```

### 使用 Visual Studio Code
```bash
# 在根目录打开VS Code
code .

# 或在具体项目目录打开
code src/TtWork.Project.Web.Host
code FreeIM/ImServer
```

## 📱 验证服务状态

### 检查端口占用
```bash
# 检查API服务端口
netstat -an | grep :5000

# 检查IM服务端口  
netstat -an | grep :6001
```

### 访问服务
```bash
# API服务健康检查
curl http://localhost:5000/health

# API Swagger文档
curl http://localhost:5000/swagger
```

在浏览器中访问：
- **API服务**: http://localhost:5000
- **Swagger文档**: http://localhost:5000/swagger
- **IM服务**: ws://localhost:6001

## 🛠️ 故障排除

### 常见问题及解决方案

#### 1. 端口被占用
```bash
# 查找占用端口的进程
netstat -ano | grep :5000
lsof -i :5000  # Linux/Mac

# 杀死进程（谨慎使用）
kill -9 <PID>
```

#### 2. 数据库连接失败
检查配置文件：`src/TtWork.Project.Web.Host/appsettings.json`
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=127.0.0.1;Database=www_molitao_top;User ID=root;Password=Jia05300329;..."
  }
}
```

确保：
- MySQL服务已启动
- 数据库 `www_molitao_top` 已创建
- 用户权限正确

#### 3. Redis连接失败
检查Redis配置：
```json
{
  "Redis": {
    "ConnectionString": "127.0.0.1:6379,syncTimeout=5000,abortConnect=false",
    "DatabaseId": 0
  }
}
```

确保Redis服务已启动：
```bash
# Windows (如果安装了Redis)
redis-server

# Linux/Mac
sudo systemctl start redis
# 或
redis-server /etc/redis/redis.conf
```

#### 4. .NET SDK版本问题
```bash
# 检查.NET版本
dotnet --version

# 应该显示8.x.x版本
# 如果没有，请安装.NET 8.0 SDK
```

## 📊 监控和日志

### 查看实时日志
```bash
# API服务日志
tail -f src/TtWork.Project.Web.Host/Logs/log.txt

# IM服务日志
tail -f FreeIM/ImServer/logs/log.txt
```

### 使用Seq日志聚合（可选）
访问：http://localhost:5341

## 🔄 服务管理

### 停止服务
```bash
# 使用脚本停止
./start-services.sh  # 选择停止选项

# 手动停止
pkill -f "TtWork.Project.Web.Host"
pkill -f "ImServer"

# Windows
taskkill /IM dotnet.exe /F
```

### 重启服务
```bash
# 先停止再启动
./start-services.sh  # 选择停止然后启动
```

## 🏗️ 生产部署

### 发布应用
```bash
# 发布API服务
dotnet publish src/TtWork.Project.Web.Host -c Release -o ./publish/api

# 发布IM服务
dotnet publish FreeIM/ImServer -c Release -o ./publish/im

# 运行发布版本
cd publish/api && dotnet TtWork.Project.Web.Host.dll
cd publish/im && dotnet ImServer.dll
```

### Docker部署
```bash
# 构建Docker镜像
docker build -t molitao-api .

# 运行容器
docker run -p 5000:5000 molitao-api
```

## 💡 开发提示

### 环境变量设置
```bash
# 设置开发环境
export ASPNETCORE_ENVIRONMENT=Development

# Windows
set ASPNETCORE_ENVIRONMENT=Development
```

### 配置文件优先级
1. `appsettings.json`
2. `appsettings.Development.json`（开发环境）
3. 环境变量
4. 命令行参数

### 调试配置
在 `launchSettings.json` 中配置启动参数：
```json
{
  "profiles": {
    "TtWork.Project.Web.Host": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "http://*:5000"
    }
  }
}
```

---

## 📞 技术支持

如果遇到问题，请检查：
1. 日志文件中的错误信息
2. 数据库和Redis连接状态
3. 端口占用情况
4. .NET SDK版本兼容性

**Happy Coding! 🎉**
