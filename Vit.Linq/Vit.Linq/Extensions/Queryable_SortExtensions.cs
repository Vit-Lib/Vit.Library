using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

using Vit.Core.Util.ComponentModel.Query;
using Vit.Linq.QueryBuilder;

namespace Vit.Extensions.Linq_Extensions
{
    /// <summary>
    /// 参考 https://www.cnblogs.com/Extnet/p/9848272.html
    /// </summary>
    public static partial class Queryable_SortExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, IEnumerable<SortItem> sort)
            where T : class
        {
            if (query == null || sort == null) return query;

            var sortCount = sort.Count();
            if (sortCount == 0) return query;


            var paramExp = Expression.Parameter(typeof(T));
            IOrderedQueryable<T> orderedQuery = null;

            foreach (var item in sort)
            {

                MemberExpression memberExp = LinqHelp.BuildField_MemberExpression(paramExp, item.field);
                LambdaExpression exp = Expression.Lambda(memberExp, paramExp);

                if (orderedQuery == null)
                {
                    orderedQuery = OrderBy(query, exp, item.asc);
                }
                else
                {
                    orderedQuery = ThenBy(orderedQuery, exp, item.asc);
                }
            }
            return orderedQuery ?? query;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="field">字段名</param>
        /// <param name="asc">是否为正向排序</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, string field, bool asc = true)
           where T : class
        {
            if (string.IsNullOrEmpty(field)) return query;

            var paramExp = Expression.Parameter(typeof(T));
            MemberExpression memberExp = LinqHelp.BuildField_MemberExpression(paramExp, field);
            LambdaExpression expr = Expression.Lambda(memberExp, paramExp);

            return OrderBy(query, expr, asc);
        }





        #region OrderBy
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, LambdaExpression exp, bool asc = true)
        {
            if (asc)
            {
                var genericMethod = MethodInfo_OrderBy.MakeGenericMethod(typeof(T), exp.ReturnType);
                return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, exp });
            }
            else
            {
                var genericMethod = MethodInfo_OrderByDescending.MakeGenericMethod(typeof(T), exp.ReturnType);
                return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, exp });
            }
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, LambdaExpression exp, bool asc = true)
        {
            if (asc)
            {
                var genericMethod = MethodInfo_ThenBy.MakeGenericMethod(typeof(T), exp.ReturnType);
                return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, exp });
            }
            else
            {
                var genericMethod = MethodInfo_ThenByDescending.MakeGenericMethod(typeof(T), exp.ReturnType);
                return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, exp });
            }
        }

        static readonly MethodInfo MethodInfo_OrderBy = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
        static readonly MethodInfo MethodInfo_OrderByDescending = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);
        static readonly MethodInfo MethodInfo_ThenBy = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "ThenBy" && m.GetParameters().Length == 2);
        static readonly MethodInfo MethodInfo_ThenByDescending = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == "ThenByDescending" && m.GetParameters().Length == 2);

        #endregion

    }
}