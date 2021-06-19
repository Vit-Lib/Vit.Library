using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;

namespace Vit.Extensions
{
    public  static partial  class DbContext_EntityExtensions
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void Delete<TEntity>(this DbContext dbContext, TEntity entity) where TEntity : class
        {
            var dbSet = dbContext.Set<TEntity>();
            if (dbContext.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void DeleteByKey<TEntity>(this DbContext dbContext, object key) where TEntity : class
        {
            var dbSet = dbContext.Set<TEntity>();
            TEntity entity = dbSet.Find(key);
            if (entity != null)
                dbSet.Remove(entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param> 
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool DetachEntity<TEntity>(this DbContext dbContext, TEntity entity) where TEntity : class
        {
            var entry = dbContext.Entry(entity);

            if (entry.State != EntityState.Detached)
            {
                entry.State = EntityState.Detached;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新前会自动attach实体到dbContext
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="entity"></param>
        /// <param name="SaveChanges"></param>    
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool UpdateNotNullProperty<TEntity>(this DbContext dbContext, TEntity entity, bool SaveChanges = true) where TEntity : class
        {        
            var entry = dbContext.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                dbContext.Set<TEntity>().Attach(entity);
            }

            var entityProperties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var entryProperty in entry.Properties)
            {
                //不能修改主键
                if (entryProperty.Metadata.IsPrimaryKey()) continue;

                //存在对应的数据库字段
                var entityProperty = entityProperties.FirstOrDefault(p => p.Name == entryProperty.Metadata.Name);
                if (entityProperty == null) continue;

                //值不为null
                if (entityProperty.GetValue(entity, null) == null) continue;

                entryProperty.IsModified = true;
            }

            if (SaveChanges)
                return dbContext.SaveChanges() > 0;

            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool UpdateAllProperty<TEntity>(this DbContext dbContext, TEntity entity, bool SaveChanges = true) where TEntity : class
        {
            var entry = dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                dbContext.Set<TEntity>().Attach(entity);
            }

            var entityProperties = entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var entryProperty in entry.Properties)
            {
                //不能修改主键
                if (entryProperty.Metadata.IsPrimaryKey()) continue;

                //存在对应的数据库字段
                var entityProperty = entityProperties.FirstOrDefault(p => p.Name == entryProperty.Metadata.Name);
                if (entityProperty == null) continue;

                entryProperty.IsModified = true;
            }
            if (SaveChanges)
                return dbContext.SaveChanges() > 0;

            return true;
        }






    }
}
