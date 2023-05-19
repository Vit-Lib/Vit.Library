using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Vit.Core.Util.ComponentModel.Query;
using Vit.Linq.Query;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class IQueryable_SortExtensions
    {

        #region Sort
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Sort(this IQueryable source, IEnumerable<SortItem> sort)
        {

            if (sort == null || sort.Count() == 0) return source;

            #region GetSortMethodName
            bool isFirst = true;
            string GetSortMethodName(bool asc)
            {
                if (isFirst)
                {
                    isFirst = false;
                    return asc ? "OrderBy" : "OrderByDescending";
                }
                return asc ? "ThenBy" : "ThenByDescending";
            }
            #endregion


            var targetType = source.ElementType;
            ParameterExpression parameter = Expression.Parameter(targetType);

            Expression queryExpr = source.Expression;

            foreach (var item in sort)
            {
                //(x.1)get memberExp     
                MemberExpression memberExp = LinqHelp.BuildField_MemberExpression(parameter, item.field);


                #region (x.2)Call
                queryExpr = Expression.Call(
                  typeof(Queryable), GetSortMethodName(item.asc),
                  new Type[] { source.ElementType, memberExp.Type },
                  queryExpr, Expression.Quote(Expression.Lambda(memberExp, parameter)));
                #endregion
            }

            return source.Provider.CreateQuery(queryExpr);
        }
        #endregion



        /// <summary>
        /// 
        /// </summary>      
        /// <param name="query"></param>
        /// <param name="field">字段名</param>
        /// <param name="asc">是否为正向排序</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Sort(this IQueryable query, string field, bool asc = true)
        {
            return query.IQueryable_Sort(new[] { new SortItem { field = field, asc = asc } });
        }

    }
}