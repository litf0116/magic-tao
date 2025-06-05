#!/bin/bash

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo -e "${BLUE}======================================"
echo -e "魔力淘后台服务启动脚本"
echo -e "======================================${NC}"
echo

# 检查 .NET 8.0 是否安装
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}错误: 未找到 .NET SDK${NC}"
        echo "请先安装 .NET 8.0 SDK"
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    echo -e "${GREEN}✓ .NET SDK 版本: $dotnet_version${NC}"
}

# 检查端口是否被占用
check_port() {
    local port=$1
    if netstat -an 2>/dev/null | grep ":$port " > /dev/null; then
        return 0  # 端口被占用
    else
        return 1  # 端口未被占用
    fi
}

# 启动API服务
start_api() {
    echo -e "${YELLOW}正在启动主API服务...${NC}"
    cd "$SCRIPT_DIR/src/TtWork.Project.Web.Host"
    
    if check_port 5000; then
        echo -e "${RED}警告: 端口 5000 已被占用${NC}"
        read -p "是否继续启动? (y/n): " choice
        if [[ $choice != "y" && $choice != "Y" ]]; then
            return 1
        fi
    fi
    
    echo -e "${GREEN}启动 API 服务在端口 5000...${NC}"
    dotnet run --urls=http://*:5000 &
    local pid=$!
    echo "API服务 PID: $pid"
    echo $pid > "$SCRIPT_DIR/api.pid"
    sleep 3
    
    if check_port 5000; then
        echo -e "${GREEN}✓ API服务启动成功${NC}"
    else
        echo -e "${RED}✗ API服务启动失败${NC}"
    fi
}

# 启动IM服务
start_im() {
    echo -e "${YELLOW}正在启动即时通讯服务...${NC}"
    cd "$SCRIPT_DIR/FreeIM/ImServer"
    
    if check_port 6001; then
        echo -e "${RED}警告: 端口 6001 已被占用${NC}"
        read -p "是否继续启动? (y/n): " choice
        if [[ $choice != "y" && $choice != "Y" ]]; then
            return 1
        fi
    fi
    
    echo -e "${GREEN}启动 IM 服务在端口 6001...${NC}"
    dotnet run --urls=http://*:6001 &
    local pid=$!
    echo "IM服务 PID: $pid"
    echo $pid > "$SCRIPT_DIR/im.pid"
    sleep 3
    
    if check_port 6001; then
        echo -e "${GREEN}✓ IM服务启动成功${NC}"
    else
        echo -e "${RED}✗ IM服务启动失败${NC}"
    fi
}

# 运行数据库迁移
run_migration() {
    echo -e "${YELLOW}正在运行数据库迁移...${NC}"
    cd "$SCRIPT_DIR/src/TtWork.Project.Migrator"
    
    dotnet run
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 数据库迁移完成${NC}"
    else
        echo -e "${RED}✗ 数据库迁移失败${NC}"
        return 1
    fi
}

# 停止服务
stop_services() {
    echo -e "${YELLOW}正在停止服务...${NC}"
    
    if [ -f "$SCRIPT_DIR/api.pid" ]; then
        local api_pid=$(cat "$SCRIPT_DIR/api.pid")
        if kill -0 $api_pid 2>/dev/null; then
            kill $api_pid
            echo -e "${GREEN}✓ API服务已停止${NC}"
        fi
        rm -f "$SCRIPT_DIR/api.pid"
    fi
    
    if [ -f "$SCRIPT_DIR/im.pid" ]; then
        local im_pid=$(cat "$SCRIPT_DIR/im.pid")
        if kill -0 $im_pid 2>/dev/null; then
            kill $im_pid
            echo -e "${GREEN}✓ IM服务已停止${NC}"
        fi
        rm -f "$SCRIPT_DIR/im.pid"
    fi
}

# 检查服务状态
check_status() {
    echo -e "${YELLOW}检查服务状态...${NC}"
    echo
    
    if check_port 5000; then
        echo -e "${GREEN}✓ API服务 (端口 5000) 正在运行${NC}"
    else
        echo -e "${RED}✗ API服务 (端口 5000) 未运行${NC}"
    fi
    
    if check_port 6001; then
        echo -e "${GREEN}✓ IM服务 (端口 6001) 正在运行${NC}"
    else
        echo -e "${RED}✗ IM服务 (端口 6001) 未运行${NC}"
    fi
    echo
}

# 显示菜单
show_menu() {
    echo "请选择要执行的操作:"
    echo "1. 启动主API服务"
    echo "2. 启动即时通讯服务"
    echo "3. 运行数据库迁移"
    echo "4. 启动所有服务"
    echo "5. 停止所有服务"
    echo "6. 检查服务状态"
    echo "7. 退出"
    echo
}

# 主程序
main() {
    check_dotnet
    
    while true; do
        echo
        show_menu
        read -p "请输入选项 (1-7): " choice
        
        case $choice in
            1)
                start_api
                ;;
            2)
                start_im
                ;;
            3)
                run_migration
                ;;
            4)
                run_migration
                start_im
                sleep 2
                start_api
                echo
                echo -e "${GREEN}所有服务已启动!${NC}"
                echo -e "- API服务: ${BLUE}http://localhost:5000${NC}"
                echo -e "- IM服务: ${BLUE}http://localhost:6001${NC}"
                ;;
            5)
                stop_services
                ;;
            6)
                check_status
                ;;
            7)
                stop_services
                echo -e "${GREEN}退出脚本${NC}"
                exit 0
                ;;
            *)
                echo -e "${RED}无效选项，请重新输入${NC}"
                ;;
        esac
    done
}

# 捕获 Ctrl+C 信号
trap 'echo -e "\n${YELLOW}正在停止服务...${NC}"; stop_services; exit 0' INT

# 运行主程序
main
