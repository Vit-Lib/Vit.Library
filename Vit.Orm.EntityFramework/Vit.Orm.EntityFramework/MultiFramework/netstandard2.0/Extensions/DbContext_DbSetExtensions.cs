using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vit.Extensions
{


    public static partial class DbContext_DbSetExtensions
    {
        #region AddEntityType
        /// <summary>
        /// 注意：如果不在DbContext.OnModelCreating中调用，则添加的实体只可用以查询（做修改操作会导致报错）
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clrType"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static IMutableEntityType AddEntityType(this DbContext data, Type clrType)
        {
            var model = (IMutableModel)data.Model;

            //var displayName = clrType.DisplayName(true);
            //model.RemoveEntityType(displayName);

            return model.AddEntityType(clrType);

        }
        #endregion


        #region ChangeEntityMapedTable
        /// <summary>
        /// 改变实体映射的表名
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clrType"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool ChangeEntityMappedTable(this DbContext data, Type clrType, string tableName)
        {
            if (data.Model.FindEntityType(clrType).Relational() is RelationalEntityTypeAnnotations relational)
            {
                relational.TableName = tableName;
                return true;
            }
            return false;
        }
        #endregion



        #region GetEntityTypeMap
        /// <summary>
        /// 获取实体映射表。 key为数据库表名，value为clr实体类型
        /// </summary>
        /// <param name="data"></param>     
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Dictionary<string, Type> GetEntityTypeMap(this DbContext data)
        {
            var model = (IMutableModel)data.Model;

            var map = model.GetEntityTypes()?.ToDictionary(
                item => item.Relational().TableName,
                item => item.ClrType
                );
            return map;
        }
        #endregion

        #region ChangeDataBase

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void ChangeDataBase(this DbContext Context, string connString)
        {
            Context.Database.GetDbConnection().ConnectionString = connString;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static string GetConnectionString(this DbContext Context)
        {
            return Context.Database.GetDbConnection().ConnectionString;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static System.Data.Common.DbConnection GetDbConnection(this DbContext Context)
        {
            return Context.Database.GetDbConnection();
        }

        #endregion






        #region GetDbSet
        /// <summary>
        /// 通过反射获取DbSet
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static DbSet<T> GetDbSet<T>(this DbContext data)
            where T : class
        {
            return data.GetDbSet(typeof(T)) as DbSet<T>;
        }
        #endregion

        #region GetDbSet
        /// <summary>
        /// 通过反射获取DbSet
        /// </summary>
        /// <param name="data"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static object GetDbSet(this DbContext data, Type entityType)
        {
            return data.GetType().GetTypeInfo().GetMethod("Set", new Type[0]).MakeGenericMethod(entityType).Invoke(data, null);
        }
        #endregion



        #region GetQueryable

        /// <summary>
        /// 通过反射获取IQueryable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static IQueryable GetQueryable(this DbContext data, Type entityType)
        {
            return data.GetDbSet(entityType) as IQueryable;
        }
        #endregion



        #region GetQueryableByTableName
        /// <summary>
        /// 根据表名称通过反射获取IQueryable
        /// </summary>
        /// <param name="data"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static IQueryable GetQueryableByTableName(this DbContext data, string tableName)
        {
            var model = (IMutableModel)data.Model;

            var ClrType = model.GetEntityTypes()?.Where(item => item.Relational().TableName == tableName).FirstOrDefault()?.ClrType;

            if (ClrType != null)
            {
                return data.GetQueryable(ClrType);
            }
            return null;
        }
        #endregion

    }
}