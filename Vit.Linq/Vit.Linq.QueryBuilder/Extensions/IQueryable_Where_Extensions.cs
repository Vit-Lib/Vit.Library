using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Vit.Linq.QueryBuilder;

namespace Vit.Extensions.Linq_Extensions
{
    public static partial class IQueryable_Where_Extensions
    {
        #region Where

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Where(this IQueryable query, IFilterRule rule)
        {
            LambdaExpression lambda = QueryBuilder.ToLambdaExpression(rule, query.ElementType);
            if (lambda == null) return query;
            return query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Where",
                    new Type[] { query.ElementType },
                    query.Expression, Expression.Quote(lambda)));
        }
        #endregion

    }
}