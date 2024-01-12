using System.Linq;
using System.Runtime.CompilerServices;

using Vit.Core.Util.ComponentModel.Data;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class IQueryable_PageExtensions
    {


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Page(this IQueryable query, PageInfo page)
        {
            if (query == null || page == null) return query;

            return query.IQueryable_Page(page.pageIndex, page.pageSize);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageIndex">页码，从1开始</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Page(this IQueryable query, int pageIndex, int pageSize)
        {
            return query.IQueryable_Skip((pageIndex - 1) * pageSize).IQueryable_Take(pageSize);
        }
    }
}