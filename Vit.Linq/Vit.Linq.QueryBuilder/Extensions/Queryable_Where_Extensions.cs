using System.Linq;
using System.Runtime.CompilerServices;

using Vit.Linq.QueryBuilder;

namespace Vit.Extensions.Linq_Extensions
{
    public static partial class Queryable_Where_Extensions
    {
        #region Where

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Where<T>(this IQueryable<T> query, IFilterRule rule)
          where T : class
        {
            if (query == null || rule == null) return query;

            var predicate = QueryBuilderService.Instance.ToExpression<T>(rule);
            if (predicate == null)
            {
                return query;
            }
            else
            {
                return query.Where(predicate);
            }
        }
        #endregion

    }
}