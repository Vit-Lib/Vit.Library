using System.Linq;
using System.Runtime.CompilerServices;
using Vit.Core.Util.ComponentModel.Data;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class Queryable_PageExtensions
    {


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Page<T>(this IQueryable<T> query, PageInfo page)
            where T : class
        {
            if (query == null || page == null) return query;

            return query.Page(page.pageIndex, page.pageSize); 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Page<T>(this IQueryable<T> query,int pageIndex, int pageSize)
          where T : class
        {  
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}