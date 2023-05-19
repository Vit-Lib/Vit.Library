using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Vit.Extensions.Linq_Extensions
{


    public static partial class IQueryable_Ef_Extensions
    {
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


        #region ToListAsync

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


    }
}