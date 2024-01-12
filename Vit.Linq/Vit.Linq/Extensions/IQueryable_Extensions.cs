using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Vit.Extensions.Linq_Extensions
{


    /// <summary>
    /// ref DynamicQueryable
    /// </summary>
    public static partial class IQueryable_Extensions
    {


        #region Count
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IQueryable_Count(this IQueryable source)
        {
            return source.Provider.Execute<int>(
                Expression.Call(
                    typeof(Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }
        #endregion

        #region Skip
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Skip(this IQueryable source, int count)
        {
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }
        #endregion

        #region Take
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable IQueryable_Take(this IQueryable source, int count)
        {
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }
        #endregion



        #region ToList
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> IQueryable_ToList<T>(this IQueryable source)
        {
            return source.Provider.Execute<List<T>>(
                Expression.Call(
                    typeof(Enumerable), "ToList",
                    new Type[] { source.ElementType }, source.Expression));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object IQueryable_ToList(this IQueryable source)
        {
            return source.Provider.Execute(
                Expression.Call(
                    typeof(Enumerable), "ToList",
                    new Type[] { source.ElementType }, source.Expression));
        }
        #endregion


        #region ToArray
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] IQueryable_ToArray<T>(this IQueryable source)
        {
            return source.Provider.Execute<T[]>(
                Expression.Call(
                    typeof(Enumerable), "ToArray",
                    new Type[] { source.ElementType }, source.Expression));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object IQueryable_ToArray(this IQueryable source)
        {
            return source.Provider.Execute(
                Expression.Call(
                    typeof(Enumerable), "ToArray",
                    new Type[] { source.ElementType }, source.Expression));
        }
        #endregion


        #region FirstOrDefault
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object IQueryable_FirstOrDefault(this IQueryable source)
        {
            return source.Provider.Execute<object>(
                Expression.Call(typeof(Queryable), "FirstOrDefault",
                new Type[] { source.ElementType },
                source.Expression)
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T IQueryable_FirstOrDefault<T>(this IQueryable source)
        {
            return source.Provider.Execute<T>(
                Expression.Call(typeof(Queryable), "FirstOrDefault", new Type[] { source.ElementType }, source.Expression)
                );
        }
        #endregion







    }
}