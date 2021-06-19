using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vit.Linq.Query;

namespace Vit.Extensions
{
    public  static partial class Queryable_WhereExtensions
    {
        #region Where

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Where<T>(this IQueryable<T> query, IEnumerable<DataFilter> filters, ECondition condition = ECondition.AndAlso)
          where T : class
        {
            if (query == null || filters == null) return query;

            var predicate = filters.ToExpression<T>();
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