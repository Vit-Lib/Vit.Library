using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Vit.Orm.EntityFramework.DbContextInitor
{
    public class DbContextInitor_sqlite : IDbContextInitor
    {
        public void AddDbContext<TContext>(IServiceCollection data, ConnectionInfo info) where TContext : DbContext
        {
            //使用sqlite数据库
            data.AddDbContext<TContext>(opt => opt.UseSqlite(info.ConnectionString));
        }
    }

}
