using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.Query;
using Vit.Linq.Query;

namespace Vit.Extensions
{

    public static partial class Queryable_ToPageDataExtensions
    {

        #region ToPageData       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PageData<T> ToPageData<T>(this IQueryable<T> query, PageInfo page)
            where T : class
        {
            if (query == null) return null;

            var queryPaged = query;
            if (page != null)
                queryPaged = queryPaged.Page(page);

            return new PageData<T>(page) { totalCount = query.Count(), rows = queryPaged.ToList() };
        }

        public static PageData<T> ToPageData<T>(this IQueryable<T> query,
            IEnumerable<DataFilter> filter, IEnumerable<SortItem> sort, PageInfo page
        ) where T : class
        {
            return query?.Where(filter).Sort(sort).ToPageData(page);
        }
        #endregion


        #region ToPageData with selector
        /// <summary>
        /// 注：先查询，后调用selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PageData<TResult> ToPageData<T, TResult>(this IQueryable<T> query, PageInfo page, Func<T, TResult> selector)
            where T : class
        {
            if (query == null) return null;

            var queryPaged = query;
            if (page != null)
                queryPaged = queryPaged.Page(page);

            return new PageData<TResult>(page) { totalCount = query.Count(), rows = queryPaged.ToList().Select(selector).ToList() };
        }

        /// <summary>
        /// 注：先查询，后调用selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PageData<TResult> ToPageData<T, TResult>(this IQueryable<T> query,
            IEnumerable<DataFilter> filter, IEnumerable<SortItem> sort, PageInfo page, Func<T, TResult> selector
        ) where T : class
        {
            return query?.Where(filter).Sort(sort).ToPageData(page, selector);
        }
        #endregion



    }
}