@echo off
echo 快速启动所有服务...
echo.

echo 1. 运行数据库迁移...
cd /d "%~dp0src\TtWork.Project.Migrator"
dotnet run
if %errorlevel% neq 0 (
    echo 数据库迁移失败!
    pause
    exit /b 1
)

echo.
echo 2. 启动IM服务...
cd /d "%~dp0FreeIM\ImServer"
start "IM服务" cmd /k "dotnet run --urls=http://*:6001"

echo.
echo 3. 启动API服务...
cd /d "%~dp0src\TtWork.Project.Web.Host"
start "API服务" cmd /k "dotnet run --urls=http://*:5000"

echo.
echo 所有服务启动完成!
echo API服务: http://localhost:5000
echo IM服务: http://localhost:6001
echo.
pause
