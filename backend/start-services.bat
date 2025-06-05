@echo off
chcp 65001 >nul
echo ====================================
echo 魔力淘后台服务启动脚本
echo ====================================
echo.

:menu
echo 请选择要启动的服务:
echo 1. 启动主API服务 (TtWork.Project.Web.Host)
echo 2. 启动即时通讯服务 (ImServer)
echo 3. 运行数据库迁移 (Migrator)
echo 4. 启动所有服务
echo 5. 检查服务状态
echo 6. 退出
echo.
set /p choice=请输入选项 (1-6): 

if "%choice%"=="1" goto start_api
if "%choice%"=="2" goto start_im
if "%choice%"=="3" goto migrate
if "%choice%"=="4" goto start_all
if "%choice%"=="5" goto check_status
if "%choice%"=="6" goto exit
goto menu

:start_api
echo.
echo 正在启动主API服务...
cd /d "%~dp0src\TtWork.Project.Web.Host"
start "魔力淘 API 服务" cmd /k "dotnet run --urls=http://*:5000"
echo API服务已启动在端口 5000
echo.
pause
goto menu

:start_im
echo.
echo 正在启动即时通讯服务...
cd /d "%~dp0FreeIM\ImServer"
start "魔力淘 IM 服务" cmd /k "dotnet run --urls=http://*:6001"
echo IM服务已启动在端口 6001
echo.
pause
goto menu

:migrate
echo.
echo 正在运行数据库迁移...
cd /d "%~dp0src\TtWork.Project.Migrator"
dotnet run
echo 数据库迁移完成
echo.
pause
goto menu

:start_all
echo.
echo 正在启动所有服务...
echo.

echo 1. 运行数据库迁移...
cd /d "%~dp0src\TtWork.Project.Migrator"
dotnet run
echo.

echo 2. 启动即时通讯服务...
cd /d "%~dp0FreeIM\ImServer"
start "魔力淘 IM 服务" cmd /k "dotnet run --urls=http://*:6001"
timeout /t 3 >nul
echo.

echo 3. 启动主API服务...
cd /d "%~dp0src\TtWork.Project.Web.Host"
start "魔力淘 API 服务" cmd /k "dotnet run --urls=http://*:5000"
echo.

echo 所有服务已启动!
echo - API服务: http://localhost:5000
echo - IM服务: http://localhost:6001
echo.
pause
goto menu

:check_status
echo.
echo 检查服务状态...
echo.
netstat -an | findstr ":5000" >nul
if %errorlevel%==0 (
    echo ✓ API服务 (端口 5000) 正在运行
) else (
    echo ✗ API服务 (端口 5000) 未运行
)

netstat -an | findstr ":6001" >nul
if %errorlevel%==0 (
    echo ✓ IM服务 (端口 6001) 正在运行
) else (
    echo ✗ IM服务 (端口 6001) 未运行
)
echo.
pause
goto menu

:exit
echo 退出脚本
exit /b 0
