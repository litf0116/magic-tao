using System.Threading;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TtWork.Project.Web.Host.Startup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(200, 200);
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
#if DEBUG
#else
                    .UseUrls("http://*:5000")
#endif
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", false, true)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>()
                .Build();
        }
    }
}