#!/bin/bash

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}======================================"
echo -e "魔力淘环境依赖检查"
echo -e "======================================${NC}"
echo

# 检查.NET SDK
check_dotnet() {
    echo -n "检查 .NET SDK: "
    if command -v dotnet &> /dev/null; then
        local version=$(dotnet --version)
        if [[ $version =~ ^8\. ]]; then
            echo -e "${GREEN}✓ .NET $version${NC}"
            return 0
        else
            echo -e "${YELLOW}⚠ 发现 .NET $version，推荐使用 .NET 8.0${NC}"
            return 1
        fi
    else
        echo -e "${RED}✗ 未安装${NC}"
        return 1
    fi
}

# 检查MySQL
check_mysql() {
    echo -n "检查 MySQL 连接: "
    # 这里可以添加具体的MySQL连接测试
    if command -v mysql &> /dev/null; then
        echo -e "${GREEN}✓ MySQL 客户端已安装${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠ MySQL 客户端未安装（可选）${NC}"
        return 1
    fi
}

# 检查Redis
check_redis() {
    echo -n "检查 Redis: "
    if command -v redis-cli &> /dev/null; then
        # 尝试连接Redis
        if redis-cli ping &> /dev/null; then
            echo -e "${GREEN}✓ Redis 运行中${NC}"
            return 0
        else
            echo -e "${YELLOW}⚠ Redis 客户端已安装但服务未运行${NC}"
            return 1
        fi
    else
        echo -e "${YELLOW}⚠ Redis 客户端未安装（可选）${NC}"
        return 1
    fi
}

# 检查端口占用
check_ports() {
    echo "检查端口占用:"
    
    echo -n "  端口 5000 (API服务): "
    if netstat -an 2>/dev/null | grep ":5000 " > /dev/null; then
        echo -e "${YELLOW}⚠ 已被占用${NC}"
    else
        echo -e "${GREEN}✓ 可用${NC}"
    fi
    
    echo -n "  端口 6001 (IM服务): "
    if netstat -an 2>/dev/null | grep ":6001 " > /dev/null; then
        echo -e "${YELLOW}⚠ 已被占用${NC}"
    else
        echo -e "${GREEN}✓ 可用${NC}"
    fi
}

# 检查项目文件
check_project_files() {
    echo "检查项目文件:"
    
    echo -n "  API项目: "
    if [ -f "src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj" ]; then
        echo -e "${GREEN}✓ 存在${NC}"
    else
        echo -e "${RED}✗ 缺失${NC}"
    fi
    
    echo -n "  IM项目: "
    if [ -f "FreeIM/ImServer/ImServer.csproj" ]; then
        echo -e "${GREEN}✓ 存在${NC}"
    else
        echo -e "${RED}✗ 缺失${NC}"
    fi
    
    echo -n "  迁移项目: "
    if [ -f "src/TtWork.Project.Migrator/TtWork.Project.Migrator.csproj" ]; then
        echo -e "${GREEN}✓ 存在${NC}"
    else
        echo -e "${RED}✗ 缺失${NC}"
    fi
}

# 检查配置文件
check_config_files() {
    echo "检查配置文件:"
    
    echo -n "  API配置: "
    if [ -f "src/TtWork.Project.Web.Host/appsettings.json" ]; then
        echo -e "${GREEN}✓ 存在${NC}"
    else
        echo -e "${RED}✗ 缺失${NC}"
    fi
    
    echo -n "  IM配置: "
    if [ -f "FreeIM/ImServer/appsettings.json" ]; then
        echo -e "${GREEN}✓ 存在${NC}"
    else
        echo -e "${RED}✗ 缺失${NC}"
    fi
}

# 主函数
main() {
    check_dotnet
    echo
    check_mysql
    check_redis
    echo
    check_ports
    echo
    check_project_files
    echo
    check_config_files
    
    echo
    echo -e "${BLUE}======================================${NC}"
    echo -e "${GREEN}环境检查完成！${NC}"
    echo
    echo "如果发现问题，请参考 CLI-STARTUP-GUIDE.md 进行解决"
}

# 切换到脚本所在目录
cd "$(dirname "$0")"

# 运行主函数
main
