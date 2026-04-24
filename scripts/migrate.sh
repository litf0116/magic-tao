#!/bin/bash
# 运行数据库迁移
cd "$(dirname "$0")/src/TtWork.Project.Migrator"
echo "运行数据库迁移..."
dotnet run
