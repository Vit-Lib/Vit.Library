using Microsoft.EntityFrameworkCore.Query;
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
            
            if (source.Provider is IAsyncQueryProvider provider)
            {
                return provider.ExecuteAsync<Task<int>>(
                    Expression.Call(
                   typeof(Queryable), "Count",
                   new Type[] { source.ElementType }, source.Expression),
               default);
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

 

        static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable) 
        {
            List<T> list = new List<T>();
            await foreach (var item in asyncEnumerable)
            {
                list.Add(item);
            }
            return list;
        }

        static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerator<T> asyncEnumerator)
        {
            List<T> list = new List<T>();
            while (await asyncEnumerator.MoveNextAsync())
            {
                list.Add(asyncEnumerator.Current);
            }
            return list;
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
                      m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(CancellationToken) &&
                      m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerator<>));

                    if (method != null)
                    {
                        var asyncEnumerator = method.Invoke(source, new object[] { default(CancellationToken) }) as IAsyncEnumerator<T>;
                        return await asyncEnumerator.ToListAsync();
                    }
                }
                #endregion

                #region (x.x.2)IAsyncEnumerable
                {
                    var method = DeclaredMethods.FirstOrDefault(m =>
                      m.GetParameters().Length == 0 &&
                      m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerator<>));

                    if (method != null)
                    {
                        var asyncEnumerator = method.Invoke(source, null) as IAsyncEnumerator<T>;
                        return await asyncEnumerator.ToListAsync();
                    }
                }

                #endregion

                #region (x.x.3)IAsyncEnumerable
                {
                    var method = DeclaredMethods.FirstOrDefault(m =>
                      m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(CancellationToken) &&
                      m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));

                    if (method != null)
                    {
                        var asyncEnumerable = method.Invoke(source, new object[] { default(CancellationToken) }) as IAsyncEnumerable<T>;
                        return await asyncEnumerable.ToListAsync();
                    }
                }
                #endregion

                #region (x.x.4)IAsyncEnumerable
                {
                    var method = DeclaredMethods.FirstOrDefault(m =>
                      m.GetParameters().Length == 0 &&
                      m.ReturnType.IsGenericType && m.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>));

                    if (method != null)
                    {
                        var asyncEnumerable = method.Invoke(source, null) as IAsyncEnumerable<T>;
                        return await asyncEnumerable.ToListAsync();
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
                Task.Run(() =>
                {
                    return provider.ExecuteAsync<object>(
                        Expression.Call(
                       typeof(Queryable), "FirstOrDefault",
                       new Type[] { source.ElementType }, source.Expression),
                   default(CancellationToken));
                });
            }
            throw new InvalidOperationException("IQueryableProvider is not async");
        }
        #endregion


    }
}