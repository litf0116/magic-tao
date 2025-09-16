#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
# 配置DNS以避免网络问题
RUN echo "nameserver 8.8.8.8" >> /etc/resolv.conf && \
    echo "nameserver 8.8.4.4" >> /etc/resolv.conf
WORKDIR /src
COPY ["FreeIM/FreeIM/FreeIM.csproj", "FreeIM/FreeIM/"]
COPY ["Modules/Tt.HttpClient.Weixin/Tt.HttpClient.Weixin.csproj", "Modules/Tt.HttpClient.Weixin/"]
COPY ["Modules/TtWork.Abp.AppManagement/TtWork.Abp.AppManagement.csproj", "Modules/TtWork.Abp.AppManagement/"]
COPY ["Modules/TtWork.Abp.Core/TtWork.Abp.Core.csproj", "Modules/TtWork.Abp.Core/"]
COPY ["Modules/TtWork.Abp.Oss.UpYun/TtWork.Abp.Oss.UpYun.csproj", "Modules/TtWork.Abp.Oss.UpYun/"]
COPY ["Modules/TtWork.Lib/TtWork.Lib.csproj", "Modules/TtWork.Lib/"]
COPY ["src/TtWork.Project/TtWork.Project.csproj", "src/TtWork.Project/"]
COPY ["src/TtWork.Project.Web.Core/TtWork.Project.Web.Core.csproj", "src/TtWork.Project.Web.Core/"]
COPY ["src/TtWork.Project.Core/TtWork.Project.Core.csproj", "src/TtWork.Project.Core/"]
COPY ["src/TtWork.Project.EntityFrameworkCore/TtWork.Project.EntityFrameworkCore.csproj", "src/TtWork.Project.EntityFrameworkCore/"]
COPY ["src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj", "src/TtWork.Project.Web.Host/"]
COPY ["src/TtWork.Project.Web.Host/nuget.config", "src/TtWork.Project.Web.Host/"]
# 尝试多次restore以确保网络连接
RUN for i in 1 2 3; do \
    echo "Attempt $i: Restoring NuGet packages..." && \
    dotnet restore "./src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj" \
        --verbosity normal \
        --configfile "./src/TtWork.Project.Web.Host/nuget.config" && \
    break || \
    (echo "Restore failed, retrying in 10 seconds..." && sleep 10); \
done

COPY . .
WORKDIR "/src/src/TtWork.Project.Web.Host"

# 构建项目，添加重试机制
RUN for i in 1 2 3; do \
    echo "Attempt $i: Building project..." && \
    dotnet build "./TtWork.Project.Web.Host.csproj" \
        -c $BUILD_CONFIGURATION \
        -o /app/build \
        --verbosity normal && \
    break || \
    (echo "Build failed, retrying in 10 seconds..." && sleep 10); \
done

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TtWork.Project.Web.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 临时切换到root用户进行文件系统操作
USER root
RUN rm -rf /app/wwwroot
RUN mkdir -p /app/wwwroot
RUN mkdir -p /app/cert
RUN chmod -R 777 /app/wwwroot
RUN chown -R app:app /app/wwwroot /app/cert

# 切换回app用户以确保安全性
USER app
ENTRYPOINT ["dotnet", "TtWork.Project.Web.Host.dll"]