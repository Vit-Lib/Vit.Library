using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Vit.Orm.EntityFramework.DbContextInitor
{
    public class DbContextInitor_mssql : IDbContextInitor
    {
        public void AddDbContext<TContext>(IServiceCollection data, ConnectionInfo info) where TContext : DbContext
        {
            //使用SqlServer数据库

#if NETSTANDARD2_0
            //注：UseRowNumberForPaging 为了解决sql server 2008不支持 FETCH和NETX语句（Linq分页）的问题（sql server2012才支持）
            data.AddDbContext<TContext>(opt => opt.UseSqlServer(info.ConnectionString, b => b.UseRowNumberForPaging()));
#endif

#if NETSTANDARD2_1
                           data.AddDbContext<TContext>(opt => opt.UseSqlServer(info.ConnectionString));
#endif

        }

    }
}
