#!/bin/bash
# 快速启动IM服务
cd "$(dirname "$0")/FreeIM/ImServer"
echo "启动IM服务在端口 6001..."
dotnet run --urls=http://*:6001
