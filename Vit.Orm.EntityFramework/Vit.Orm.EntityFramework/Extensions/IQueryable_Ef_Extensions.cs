using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Vit.Extensions.Linq_Extensions
{


    public static partial class IQueryable_Ef_Extensions
    {

        #region Count
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int Ef_Count(this IQueryable source)
        {
            return source.Ef_CountAsync().Result;
        }
        #endregion



        #region ToList
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static List<object> Ef_ToList(this IQueryable source)
        {
            return source.Ef_ToListAsync<object>().Result;
        }

        public static List<T> Ef_ToList<T>(this IQueryable source)
        {
            return source.Ef_ToListAsync<T>().Result;
        }
        #endregion



        #region ToListAsync
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<List<object>> Ef_ToListAsync(this IQueryable source)
        {
            return await source.Ef_ToListAsync<object>();
        }
        #endregion



        #region FirstOrDefault
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static T Ef_FirstOrDefault<T>(this IQueryable source)
            where T : class
        {
            return source.Ef_FirstOrDefault() as T;
        }
        #endregion


        #region FirstOrDefault
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static object Ef_FirstOrDefault(this IQueryable source)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.Execute(
                    Expression.Call(
                   typeof(Queryable), "FirstOrDefault",
                   new Type[] { source.ElementType }, source.Expression)
               );
            }
            throw new InvalidOperationException("IQueryableProvider is not async");
        }
        #endregion
    }
}