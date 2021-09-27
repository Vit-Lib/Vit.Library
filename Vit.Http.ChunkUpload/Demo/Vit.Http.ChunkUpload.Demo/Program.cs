using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Vit.Extensions;

namespace Vit.OnlineUpgrade.Netcore.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
            .UseUrlsFromConfig()
            .UseStartup<Startup>();
    }
}
