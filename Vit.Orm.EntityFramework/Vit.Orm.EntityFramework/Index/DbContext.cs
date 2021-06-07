using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

namespace Vit.Orm.EntityFramework.Index
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(Microsoft.EntityFrameworkCore.DbContextOptions options)
          : base(options)
        {
        }


        public static void BindIndexByIndexAttribute(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder) 
        {
            var model = modelBuilder.Model;
            var mappedTables = model.GetEntityTypes();

            mappedTables.ToList().ForEach(entityType =>
            {
                var type = entityType.ClrType;
                var entityTypeBuilder = modelBuilder.Entity(type);
                type.GetProperties(bindingAttr: System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .ToList().ForEach(item =>
                {
                    var attr = item.GetCustomAttribute<IndexAttribute>();
                    if (attr == null) return;

                    var indexBuilder = entityTypeBuilder.HasIndex(item.Name);
                    if (attr.IsUnique) 
                    {
                        indexBuilder.IsUnique(true);
                    }
                    if (!string.IsNullOrWhiteSpace(attr.Name)) indexBuilder.HasName(attr.Name);
                });
            });
        }


        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            BindIndexByIndexAttribute(modelBuilder);

        }
    }
}
