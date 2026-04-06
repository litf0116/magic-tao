using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using FreeIM;
using Serilog;

namespace imServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration;

        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.GetEncoding("GB2312");
            Console.InputEncoding = Encoding.GetEncoding("GB2312");

            app.UseDeveloperExceptionPage();

            var redisClient = new FreeRedis.RedisClient(Configuration["ImServerOption:RedisClient"]);
            
#if DEBUG
                //Servers = "8.130.178.251:6001".Split(";"),
                //Server = "8.130.178.251:6001"
                var servers = "192.168.10.35:6001".Split(";");
                var server = "192.168.10.35:6001";
#else
                var servers = "ws.molitao.top".Split(";");
                var server = "ws.molitao.top";
#endif

            Console.WriteLine($"[ImServer] 配置FreeImServer: Redis={Configuration["ImServerOption:RedisClient"]}, Servers={string.Join(",", servers)}, Server={server}");
            
            app.UseFreeImServer(new ImServerOptions
            {
                Redis = redisClient,
                Servers = servers,
                Server = server
            });
            
            Console.WriteLine($"[ImServer] FreeImServer配置完成");
        }
    }
}