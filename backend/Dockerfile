#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM gitlab.somall.top:8090/library/aspnet:8.0-skiasharp AS base
USER app
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
RUN export hostip=$(cat /etc/resolv.conf |grep -oP '(?<=nameserver\ ).*') && \
    echo "nameserver 8.8.8.8" >> /etc/resolv.conf && \
    echo "nameserver 8.8.4.4" >> /etc/resolv.conf
WORKDIR /src
COPY ["nuget.config", "./"]
COPY ["FreeIM/FreeIM/FreeIM.csproj", "FreeIM/FreeIM/"]
COPY ["Modules/Tt.HttpClient.Weixin/Tt.HttpClient.Weixin.csproj", "Modules/Tt.HttpClient.Weixin/"]
COPY ["Modules/TtWork.Abp.AppManagement/TtWork.Abp.AppManagement.csproj", "Modules/TtWork.Abp.AppManagement/"]
COPY ["Modules/TtWork.Abp.Core/TtWork.Abp.Core.csproj", "Modules/TtWork.Abp.Core/"]
COPY ["Modules/TtWork.Abp.Entity/TtWork.Abp.Entity.csproj", "Modules/TtWork.Abp.Entity/"]
COPY ["Modules/TtWork.Abp.Oss.UpYun/TtWork.Abp.Oss.UpYun.csproj", "Modules/TtWork.Abp.Oss.UpYun/"]
COPY ["Modules/TtWork.Lib/TtWork.Lib.csproj", "Modules/TtWork.Lib/"]
COPY ["src/TtWork.Project/TtWork.Project.csproj", "src/TtWork.Project/"]
COPY ["src/TtWork.Project.Web.Core/TtWork.Project.Web.Core.csproj", "src/TtWork.Project.Web.Core/"]
COPY ["src/TtWork.Project.Core/TtWork.Project.Core.csproj", "src/TtWork.Project.Core/"]
COPY ["src/TtWork.Project.EntityFrameworkCore/TtWork.Project.EntityFrameworkCore.csproj", "src/TtWork.Project.EntityFrameworkCore/"]
COPY ["src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj", "src/TtWork.Project.Web.Host/"]
RUN dotnet restore "./src/TtWork.Project.Web.Host/TtWork.Project.Web.Host.csproj"
COPY . .
WORKDIR "/src/src/TtWork.Project.Web.Host"
RUN dotnet build "./TtWork.Project.Web.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TtWork.Project.Web.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN rm -rf /app/wwwroot
RUN mkdir -p /app/wwwroot
RUN mkdir -p /app/cert
RUN chmod -R 777 /app/wwwroot
ENTRYPOINT ["dotnet", "TtWork.Project.Web.Host.dll"]