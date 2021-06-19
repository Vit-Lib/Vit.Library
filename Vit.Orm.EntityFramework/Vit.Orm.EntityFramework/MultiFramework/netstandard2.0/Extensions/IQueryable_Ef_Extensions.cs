using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Vit.Orm.EntityFramework.Extensions
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


        #region CountAsync
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task<int> Ef_CountAsync(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            IAsyncQueryProvider provider = source.Provider as IAsyncQueryProvider;
            if (provider != null)
            {
                return provider.ExecuteAsync<int>(
                    Expression.Call(
                   typeof(Queryable), "Count",
                   new Type[] { source.ElementType }, source.Expression),
               default(CancellationToken));
            }

            throw new InvalidOperationException("IQueryableProvider is not async");
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

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<List<T>> Ef_ToListAsync<T>(this IQueryable source)
        {           
           
            if (source.Provider is IAsyncQueryProvider provider)
            {
                var DeclaredMethods = source.GetType().GetTypeInfo().DeclaredMethods;

                #region (x.x.1)IAsyncEnumerable
                {
                    var method = DeclaredMethods.FirstOrDefault(m =>
                      m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));

                    if (method != null)
                    {
                        var asyncEnumerable = method.Invoke(source, null) as IAsyncEnumerable<T>;

                        return await asyncEnumerable.ToList();
                    }
                }
                #endregion

                #region (x.x.2)IAsyncEnumerable
                {
                    var method = DeclaredMethods.FirstOrDefault(m =>
                      m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerator<>));

                    if (method != null)
                    {
                        var e = method.Invoke(source, null) as IAsyncEnumerator<T>;

                        //CancellationToken cancellationToken = default(CancellationToken);

                        using (var asyncEnumerator = e)
                        {
                            List<T> list = new List<T>();
                            while (await asyncEnumerator.MoveNext(/*cancellationToken*/))
                            {
                                list.Add(asyncEnumerator.Current);
                            }
                            return list;
                        }
                    }
                }
                #endregion
              
            }

            throw new InvalidOperationException("IQueryableProvider is not async");
        }
        #endregion

        #region FirstOrDefault
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static object Ef_FirstOrDefault(this IQueryable source)
        {
            return source.Ef_FirstOrDefaultAsync().Result;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static T Ef_FirstOrDefault<T>(this IQueryable source)
            where T:class
        {
            return source.Ef_FirstOrDefault() as T;
        }
        #endregion


        #region FirstOrDefaultAsync 

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static Task<object> Ef_FirstOrDefaultAsync(this IQueryable source)
        { 
            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.ExecuteAsync<object>(
                    Expression.Call(
                   typeof(Queryable), "FirstOrDefault",
                   new Type[] { source.ElementType }, source.Expression),
               default(CancellationToken));
            }

            throw new InvalidOperationException("IQueryableProvider is not async");
        }
        #endregion


    }
}