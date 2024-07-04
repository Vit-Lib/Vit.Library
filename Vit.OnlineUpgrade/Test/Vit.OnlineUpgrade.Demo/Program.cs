using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Vit.Extensions;

namespace App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();

            var app = builder.Build();

            // 加载在线升级api（从appsettings.json中读取配置。如不需要在线升级，可以不加载，在线升级地址为 /api/onlineupgrade/index.html ）
            app.Use_OnlineUpgrade();


            //app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

    }
}
