using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vit.Core.Util.ConfigurationManager;
using Vit.Extensions;

namespace Vit.Orm.EntityFramework
{

    public class DbContextFactory : DbContextFactory<DbContext> { }


    public class DbContextFactory<TContext>
         where TContext : DbContext
    {
        ServiceCollection serviceCollection;
        ServiceProvider provider;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="info">如：{"type":"mysql","ConnectionString":"xxx"}</param>
        public DbContextFactory<TContext> Init(ConnectionInfo info)
        {
            serviceCollection = new ServiceCollection();
            serviceCollection.UseEntityFramework<TContext>(info);

            provider = serviceCollection.BuildServiceProvider();
            return this;
        }

        /// <summary>
        /// 使用appsettings.json中的配置初始化
        /// </summary>
        /// <param name="configPath">在appsettings.json中的路径，默认："App.Db"</param>
        public DbContextFactory<TContext> Init(string configPath = null)
        {
            return Init(ConfigurationManager.Instance.GetByPath<ConnectionInfo>(configPath ?? "App.Db"));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public IServiceScope CreateDbContext(out TContext context)
        {
            var scope = provider.CreateScope();
            context = scope.ServiceProvider.GetService<TContext>();
            return scope;
        }

    }
}
