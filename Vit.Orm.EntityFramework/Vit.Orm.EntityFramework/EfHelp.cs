using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using Vit.Extensions;
using Vit.Orm.EntityFramework.DbContextInitor;

namespace Vit.Orm.EntityFramework
{
    public class EfHelp
    {

        #region DbContextInitor
        public static ConcurrentDictionary<string, IDbContextInitor> DbContextInitorMap
            = new ConcurrentDictionary<string, IDbContextInitor>();


        static EfHelp()
        {
            DbContextInitorMap["mssql"] = new DbContextInitor_mssql();
            DbContextInitorMap["mysql"] = new DbContextInitor_mysql();
            DbContextInitorMap["sqlite"] = new DbContextInitor_sqlite();
        }
        #endregion



        #region CreateDbContext       
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="configPath">在appsettings.json中的路径，默认："App.Db"</param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static TContext CreateDbContext<TContext>(string configPath = "App.Db")
            where TContext : DbContext
        {
         
            var serviceCollection = new ServiceCollection();
            serviceCollection.UseEntityFramework<TContext>(configPath);

            return serviceCollection.BuildServiceProvider().GetService<TContext>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configPath">在appsettings.json中的路径，默认："App.Db"</param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static DbContext CreateDbContext(string configPath = "App.Db")
        {
            return CreateDbContext<DbContext>(configPath);
        }
        #endregion

    }
}
