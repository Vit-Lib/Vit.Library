using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.Query;
using Vit.Linq.Query;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class IQueryable_Ef_ToPageDataExtensions
    {


        #region Ef_ToPageData       
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<PageData<T>> Ef_ToPageDataAsync<T>(this IQueryable query, PageInfo page)
        {
            if (query == null) return null;

            var totalCount = await query.Ef_CountAsync();

            var queryPaged = query;
            if (page != null)
                queryPaged = queryPaged.IQueryable_Page(page);

            var rows = await queryPaged.Ef_ToListAsync<T>();

            return new PageData<T>(page) { totalCount = totalCount, rows = rows };
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<PageData<T>> Ef_ToPageDataAsync<T>(this IQueryable query,
            IEnumerable<DataFilter> filter, IEnumerable<SortItem> sort, PageInfo page
        )
        {
            return await query?.IQueryable_Where(filter).IQueryable_Sort(sort).Ef_ToPageDataAsync<T>(page);
        }
        #endregion


        #region Ef_ToPageData  with selector
        /// <summary>
        /// 注：先查询，后调用selector
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<PageData<TResult>> Ef_ToPageDataAsync<T, TResult>(this IQueryable query, PageInfo page, Func<T, TResult> selector)
        {
            if (query == null) return null;

            var totalCount = await query.Ef_CountAsync();

            var queryPaged = query;
            if (page != null)
                queryPaged = queryPaged.IQueryable_Page(page);

            var rows = await queryPaged.Ef_ToListAsync<T>();

            return new PageData<TResult>(page) { totalCount = totalCount, rows = rows.Select(selector).ToList() };
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
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static async Task<PageData<TResult>> Ef_ToPageDataAsync<T, TResult>(this IQueryable query,
            IEnumerable<DataFilter> filter, IEnumerable<SortItem> sort, PageInfo page, Func<T, TResult> selector
        ) where T : class
        {
            return await query?.IQueryable_Where(filter).IQueryable_Sort(sort).Ef_ToPageDataAsync(page, selector);
        }
        #endregion



    }
}