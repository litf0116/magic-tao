using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Transactions;
using Abp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Castle.Facilities.Logging;
using Abp.AspNetCore;
using Abp.AspNetCore.Localization;
using Abp.Json;
using Abp.Reflection.Extensions;
using Castle.Services.Logging.SerilogIntegration;
using FreeIM;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using TtWork.Project.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Tt.HttpClient.Weixin;
using TtWork.Abp.Dapper;
using TtWork.Abp.Identity;
using TtWork.Abp.Oss.UpYun;
using TtWork.Lib.Redis;
using TtWork.Project.Applications.Core.Users;
using TtWork.Project.Web.Common;
using TTWork.WeiXinMiddleware;
using TTWork.WeiXinMiddleware.Extensions;
using Serilog.Sinks.Async;
using TtWork.HttpClient.Weixin;
using TtWork.HttpClient.Weixin.Security;
using TtWork.HttpClient.Weixin.Security.PlatformCertificate;
using TtWork.HttpClient.Weixin.Signature;
using TtWork.Project.Web.Authentication.JwtBearer;
using Nest;
using TtWork.Abp;

namespace TtWork.Project.Web.Host.Startup
{
    public class Startup
    {
        private const string DefaultCorsPolicyName = "localhost";

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _appConfiguration;

        public Startup(IWebHostEnvironment hostingEnvironment, IConfiguration appConfiguration)
        {
            _hostingEnvironment = hostingEnvironment;
            _appConfiguration = appConfiguration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            JsonExtensions.UseNewtonsoft = true;

            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            services.Configure<RedisOptions>(_appConfiguration.GetSection("Redis"));
            services.AddSingleton<IRedisClient, RedisClient>();

            services.AddSingleton<IPlatformCertificateManager, PlatformCertificateManager>();
            services.AddSingleton<ISignatureGenerator, SignatureGenerator>();
            services.AddSingleton<IWeChatPayAuthorizationGenerator, WeChatPayAuthorizationGenerator>();
            // MVC
            services.AddControllersWithViews(opt => { opt.InputFormatters.Add(new XmlSerializerInputFormatter(opt)); })
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
            SetupHttpClient(services);

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            string orgs = _appConfiguration["App:CorsOrigins"];
            // SqlSugar
            services.AddSqlsugarSetup(_appConfiguration["ConnectionStrings:Default"]);
            //
            services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName, builder =>
                {
                    builder
                        .WithOrigins(
                            (orgs)
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            // .Select(o => o.RemovePostFix("/"))
                            .ToArray()
                        )
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            if (WebConsts.SwaggerUiEnabled)
            {
                //Swagger - Enable this line and the related lines in Configure method to enable swagger UI
                //ConfigureSwagger(services);
                //添加Swagger
                services.AddSwaggerMiddleware();
            }

            GlobalConfiguration.Configuration.UseStorage(
                new MySqlStorage(
                    _appConfiguration.GetConnectionString("Default"),
                    new MySqlStorageOptions
                    {
                        TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        JobExpirationCheckInterval = TimeSpan.FromHours(1),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = true,
                        DashboardJobListLimit = 50000,
                        TransactionTimeout = TimeSpan.FromMinutes(1),
                        TablesPrefix = "Hangfire"
                    }));

            services.AddHangfire(configuration => configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                )
                // .(_appConfiguration.GetConnectionString("Default")))
                ;

            services.AddHangfireServer();
            services.AddLogging(b => b.ClearProviders());
            var self = services.AddAbp<ProjectWebHostModule>(ConfigSerilog());
            return self;
        }


        public void Configure(IApplicationBuilder app)
        {
           
            //Initializes ABP framework.
            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); //used below: UseAbpRequestLocalization

            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors(DefaultCorsPolicyName); // Enable CORS!

            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            app.UseAbpRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("zh-CN");
                // var headerProvider = options.RequestCultureProviders
                //     .OfType<AbpLocalizationHeaderRequestCultureProvider>().First();
                // headerProvider.HeaderName =
                //     ".Aspnetcore-Culture"; //兼容处理。axios http2.0 会自动把AspNetCore-Culture 转成 .Aspnetcore-Culture"
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapSwagger();

                endpoints.MapHangfireDashboard(new DashboardOptions
                {
                    Authorization = new[] { new HangfireAuthorizationFilter() }
                });
            });

            if (WebConsts.SwaggerUiEnabled)
            {
                // 启用Swagger中间件
                app.UseSwaggerMiddleware(_hostingEnvironment);
                if (_hostingEnvironment.IsDevelopment())
               {
                // Enable middleware to serve generated Swagger as a JSON endpoint
                app.UseSwagger();
                // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1 Docs");
                    options.RoutePrefix = "swagger";
                });

                //app.UseReDoc(c =>
                //{
                //    c.RoutePrefix = "docs";
                //    c.SpecUrl("/swagger/v1/swagger.json");
                //    c.EnableUntrustedSpec();
                //    c.ScrollYOffset(10);
                //    c.HideHostname();
                //    c.HideDownloadButton();
                //    c.ExpandResponses("200,201");
                //    c.RequiredPropsFirst();
                //    c.NoAutoAuth();
                //    c.PathInMiddlePanel();
                //    c.HideLoading();
                //    c.NativeScrollbars();
                //    c.DisableSearch();
                //    c.OnlyRequiredInSamples();
                //    c.SortPropsAlphabetically();
                //});
                }
            }

            app.UseMiddleware<RealIpMiddleware>();
            //微信消息中间件
            // app.UseWeiXin(options: new WeiXinOptions()
            //     { Path = "/api/wx", MutilTenant = false });


            ImHelper.Initialization(new ImClientOptions
            {
                // PathMatch = ":ws",

#if DEBUG
                Redis = new FreeRedis.RedisClient("127.0.0.1:6379,poolsize=10,syncTimeout=5000,abortConnect=false"),
                Servers = ["127.0.0.1:6001"]
#else
                Redis = new FreeRedis.RedisClient(
                  "8.130.178.251:6379,poolsize=10,password=7yD3Ddd34,syncTimeout=5000,abortConnect=false"),
                Servers = ["ws.molitao.top"]
#endif
            });

            ImHelper.Instance.OnSend += (s, e) =>
                Console.WriteLine(
                    $"ImClient.SendMessage(server={e.Server},data={JsonConvert.SerializeObject(e.Message)})");

#if RELEASE
  ImHelper.EventBus(
                t => { Console.WriteLine(t.ClientId + "up IP:" + t.Ip); },
                t => Console.WriteLine(t.ClientId + "down"));
#endif
        }

        private void SetupHttpClient(IServiceCollection services)
        {
            // services.AddScoped<IWeiXinProvider, AbpWeiXinProvider>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<ISqlConnectionFactory>(s =>
                new SqlConnectionFactory(_appConfiguration["ConnectionStrings:Default"]));

            // HTTPClient
            services.AddHttpClient<IWeixinApi, WeixinApi>(cfg => { cfg.BaseAddress = new Uri("https://api.weixin.qq.com/"); })
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler { Proxy = null, UseProxy = false });

            services.AddHttpClient<IV3PayApi, V3PayApi>(
                    cfg => { cfg.BaseAddress = new Uri("https://api.mch.weixin.qq.com/"); })
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler { Proxy = null, UseProxy = false });

            //又拍云上传
            services.AddHttpClient<IUpyunApi, UpyunApi>(cfg => { cfg.BaseAddress = new Uri("https://v0.api.upyun.com/"); })
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler { Proxy = null, UseProxy = false });
        }


        private Action<AbpBootstrapperOptions> ConfigSerilog()
        {
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                // .MinimumLevel.Warning()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

#if DEBUG
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
#endif
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", _hostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("Application", "TtWork.Project.Web.Host")
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .WriteTo.Async(c => c.Console())
                .WriteTo.Async(c => c.Seq(_appConfiguration["Seq:Uri"] ?? "http://localhost:5341", apiKey: _appConfiguration["Seq:Key"] ?? null))
                .CreateLogger();

            return options =>
                options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.LogUsing(new SerilogFactory(Log.Logger)));
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                if (_hostingEnvironment.IsDevelopment())
                {
                    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Molitao API", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true); //不能去掉
                    var xmlPath = Path.Combine(AppContext.BaseDirectory,
                        $"{typeof(UserAppService).GetAssembly().GetName().Name}.xml");
                    options.IncludeXmlComments(xmlPath, true);
                    // options.OrderActionsBy(o=>o.GroupName);
                }
            }).AddSwaggerGenNewtonsoftSupport();
        }
    }
}