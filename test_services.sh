#!/bin/bash

# 功能测试脚本 - 验证拍卖消息和 Channel 创建

echo "================================"
echo "拍卖消息发送功能测试"
echo "================================"
echo ""

BASE_URL="http://localhost:12580"

echo "1. 检查后端服务状态..."
API_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/api/services/app/Session/GetCurrentLoginInformations" 2>/dev/null)
if [ "$API_STATUS" == "200" ] || [ "$API_STATUS" == "401" ]; then
    echo "   ✅ 后端服务响应正常 (状态码: $API_STATUS)"
else
    echo "   ⚠️  后端服务响应异常 (状态码: $API_STATUS)"
fi

echo ""
echo "2. 检查 API 文档可访问性..."
SWAGGER_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}/swagger/index.html" 2>/dev/null)
if [ "$SWAGGER_STATUS" == "200" ]; then
    echo "   ✅ API 文档可访问"
else
    echo "   ⚠️  API 文档可能需要登录 (状态码: $SWAGGER_STATUS)"
fi

echo ""
echo "3. 检查数据库连接..."
# 获取数据库连接信息
MYSQL_STATUS=$(mysql -h 127.0.0.1 -u root -proot -e "SELECT 1" 2>/dev/null && echo "connected" || echo "failed")
if [ "$MYSQL_STATUS" == "connected" ]; then
    echo "   ✅ MySQL 连接正常"
    
    # 检查 ChatChannel 表是否存在
    TABLE_EXISTS=$(mysql -h 127.0.0.1 -u root -proot www_molitao_top -e "SHOW TABLES LIKE 'T_ChatChannel'" 2>/dev/null | grep -c "T_ChatChannel" || echo "0")
    if [ "$TABLE_EXISTS" -gt 0 ]; then
        echo "   ✅ ChatChannel 表存在"
    else
        echo "   ⚠️  ChatChannel 表不存在，需要运行迁移"
    fi
else
    echo "   ❌ MySQL 连接失败"
fi

echo ""
echo "4. 检查 Redis 连接..."
REDIS_STATUS=$(redis-cli -h 127.0.0.1 -p 6379 ping 2>/dev/null || echo "failed")
if [ "$REDIS_STATUS" == "PONG" ]; then
    echo "   ✅ Redis 连接正常"
else
    echo "   ❌ Redis 连接失败"
fi

echo ""
echo "5. 检查 FreeIM 消息服务..."
IM_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "http://localhost:6001" 2>/dev/null)
if [ "$IM_STATUS" == "200" ] || [ "$IM_STATUS" == "404" ]; then
    echo "   ✅ FreeIM 服务运行中"
else
    echo "   ⚠️  FreeIM 服务状态未知 (状态码: $IM_STATUS)"
fi

echo ""
echo "================================"
echo "基础环境检查完成"
echo "================================"
echo ""
echo "测试访问地址:"
echo "  - PC 前端: http://localhost:4200"
echo "  - API 文档: http://localhost:12580/swagger"
echo "  - 后端服务: http://localhost:12580"
echo ""

# 输出当前进程状态
echo "运行中的服务进程:"
ps aux | grep -E "dotnet.*TtWork|node.*vite|dotnet.*ImServer" | grep -v grep | awk '{print "  PID: " $2 " - " $11 " " $12}'
