using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Vit.Orm.EntityFramework.DbContextInitor
{
    public interface IDbContextInitor
    {
        void AddDbContext<TContext>(IServiceCollection data, ConnectionInfo info) where TContext : DbContext;
    }

}
