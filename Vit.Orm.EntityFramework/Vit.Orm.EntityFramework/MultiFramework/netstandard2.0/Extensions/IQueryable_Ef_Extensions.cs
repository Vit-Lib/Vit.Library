using Microsoft.EntityFrameworkCore.Query.Internal;

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
        public static async Task<int> Ef_CountAsync(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            IAsyncQueryProvider provider = source.Provider as IAsyncQueryProvider;
            if (provider != null)
            {
                return await provider.ExecuteAsync<int>(
                    Expression.Call(
                   typeof(Queryable), "Count",
                   new Type[] { source.ElementType }, source.Expression),
               default(CancellationToken));
            }

            throw new InvalidOperationException("IQueryableProvider is not async");
        }
        #endregion




        #region ToListAsync    

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerator<T> asyncEnumerator)
        {
            List<T> list = new List<T>();
            while (await asyncEnumerator.MoveNext())
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

                        using (var asyncEnumerator = e)
                        {
                            return await asyncEnumerator.ToListAsync();
                        }
                    }
                }
                #endregion

            }

            throw new InvalidOperationException("IQueryableProvider is not async");
        }
        #endregion







        #region FirstOrDefaultAsync 

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<object> Ef_FirstOrDefaultAsync(this IQueryable source)
        {
            if (source.Provider is IAsyncQueryProvider provider)
            {
                return await provider.ExecuteAsync<object>(
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