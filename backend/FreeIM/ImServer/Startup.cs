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

            app.UseFreeImServer(new ImServerOptions
            {
                Redis = new FreeRedis.RedisClient(Configuration["ImServerOption:RedisClient"]),

#if DEBUG
                //Servers = "8.130.178.251:6001".Split(";"),
                //Server = "8.130.178.251:6001"
                Servers = "127.0.0.1:6001".Split(";"),
                Server = "127.0.0.1:6001"
#else
                Servers = "ws.molitao.top".Split(";"),
                Server = "ws.molitao.top"
#endif
            });
        }
    }
}