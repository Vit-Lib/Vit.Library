using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.Query;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class IQueryable_ToPageDataExtensions
    {
        #region IQueryable_ToPageData       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PageData<T> IQueryable_ToPageData<T>(this IQueryable query, PageInfo page)
        {
            if (query == null) return null;

            var queryPaged = query;
            if (page != null)
                queryPaged = queryPaged.IQueryable_Page(page);

            return new PageData<T>(page) { totalCount = query.IQueryable_Count(), rows = queryPaged.IQueryable_ToList<T>() };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PageData<T> IQueryable_ToPageData<T>(this IQueryable query, IEnumerable<SortItem> sort, PageInfo page)
        {
            return query?.IQueryable_Sort(sort).IQueryable_ToPageData<T>(page);
        }
        #endregion


        #region IQueryable_ToPageData  with selector
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
        public static PageData<TResult> IQueryable_ToPageData<T, TResult>(this IQueryable query, PageInfo page, Func<T, TResult> selector)
        {
            if (query == null) return null;

            var queryPaged = query;
            if (page != null)
                queryPaged = queryPaged.IQueryable_Page(page);

            return new PageData<TResult>(page) { totalCount = query.IQueryable_Count(), rows = queryPaged.IQueryable_ToList<T>().Select(selector).ToList() };
        }

        /// <summary>
        /// 注：先查询，后调用selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="page"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PageData<TResult> IQueryable_ToPageData<T, TResult>(this IQueryable query, IEnumerable<SortItem> sort, PageInfo page, Func<T, TResult> selector)
            where T : class
        {
            return query?.IQueryable_Sort(sort).IQueryable_ToPageData(page, selector);
        }
        #endregion


    }
}