#!/bin/bash
# 快速启动API服务
cd "$(dirname "$0")/src/TtWork.Project.Web.Host"
echo "启动API服务在端口 12580..."
dotnet run --urls=http://*:12580
