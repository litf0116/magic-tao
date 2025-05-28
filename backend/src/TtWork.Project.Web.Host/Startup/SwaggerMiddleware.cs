using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtWork.Project.Web.Host.Startup;

/// <summary>
/// Swagger中间件静态类，提供Swagger相关的扩展方法
/// </summary>
public static class SwaggerMiddleware
{
    /// <summary>
    /// 添加Swagger中间件服务
    /// </summary>
    /// <param name="services">IServiceCollection实例</param>
    /// <returns>配置后的IServiceCollection实例</returns>
    public static IServiceCollection AddSwaggerMiddleware(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // 设置Swagger文档信息
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "TtWork.Project.Web.Host", Version = "v1" });

            // 设置文档包含谓词，这里设置为包含所有API
            options.DocInclusionPredicate((docName, description) => true);

            // 自定义Schema ID生成方式，使用完整类型名
            options.CustomSchemaIds(type => type.FullName);
            //忽略没有[HttpMethod]特性的Action
            options.DocInclusionPredicate((name, api) => api.HttpMethod != null);
            //添加用户token
            //options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //{
            //    Description = "请输入正确的Token格式： Bearer xxx",
            //    Name = "Authorization",
            //    In = ParameterLocation.Header,
            //    Type = SecuritySchemeType.ApiKey,
            //    BearerFormat = "JWT",
            //    Scheme = "Bearer"
            //});
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
            {
                Description = "请输入正确的Token格式： Bearer xxx",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            // 安全要求
            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
               {
                  new OpenApiSecurityScheme
                  {
                     Reference = new OpenApiReference()
                     {
                       Type = ReferenceType.SecurityScheme,
                       Id = "Bearer"
                     }
                  },
                  new string[]{}

               }
             });
            // 安全要求
            options.AddSecurityDefinition(CookieAuthenticationDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
            {
                Name = CookieAuthenticationDefaults.AuthenticationScheme,
                Scheme = CookieAuthenticationDefaults.AuthenticationScheme
            });

            // 遍历所有程序集，添加XML注释文件
            string path = Directory.GetCurrentDirectory();
            string[] xmlFiles = Directory.GetFiles(path, "*.xml");
            foreach (var item in xmlFiles)
            {
                // 获取应用程序基础目录
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // 构造XML文件名
                var commentsFileName = item + ".xml";

                // 组合完整的XML文件路径
                var xmlPath = Path.Combine(baseDirectory, commentsFileName);

                // 如果XML文件存在，则包含到Swagger中
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, true);
                    //对action的名称进行排序，如果有多个，就可以看见效果了。
                    options.OrderActionsBy(o => o.RelativePath);
                }
            }
        });

        return services;
    }

    /// <summary>
    /// 使用Swagger中间件
    /// </summary>
    /// <param name="app">IApplicationBuilder实例</param>
    /// <param name="env"></param>
    /// <returns>配置后的IApplicationBuilder实例</returns>
    public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 启用Swagger中间件
        app.UseSwagger();
        // 启用SwaggerUI中间件，并配置Swagger JSON端点
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Abp.NET API V1");
            //如果不想带/swagger路径访问的话，就放开下面的注释
            options.RoutePrefix = string.Empty;
            //使用自定义的页面(主要是增加友好的身份认证体验)
            string path = Path.Combine(env.WebRootPath, "swagger/ui/index.html");
            if (File.Exists(path)) options.IndexStream = () => new MemoryStream(File.ReadAllBytes(path));
        });
        return app;
    }
}