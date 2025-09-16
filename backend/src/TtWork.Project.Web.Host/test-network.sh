#!/bin/bash

# 网络连接测试和诊断脚本
echo "=== Docker构建网络问题诊断 ==="

# 测试基本网络连接
echo "1. 测试基本网络连接..."
ping -c 3 8.8.8.8

echo -e "\n2. 测试DNS解析..."
nslookup api.nuget.org

echo -e "\n3. 测试NuGet源连接..."
curl -I https://api.nuget.org/v3/index.json

echo -e "\n4. 测试备用NuGet源..."
curl -I https://www.nuget.org/api/v2/

echo -e "\n5. 检查当前DNS配置..."
cat /etc/resolv.conf

echo -e "\n6. 测试dotnet restore命令..."
cd /tmp
dotnet new console -o TestProject
cd TestProject
timeout 60 dotnet restore --verbosity normal