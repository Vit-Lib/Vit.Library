using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

using Vit.Extensions.Json_Extensions;

namespace Vit.Extensions.Linq_Extensions.Execute
{
    public static partial class IDbConnection_Query_Extensions
    {

        #region QueryScalar
        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Type> QueryScalar<Type>(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            using (var dr = conn.ExecuteReader(sql, param: param, commandTimeout: commandTimeout)) 
            {
                List<Type> result = new List<Type>();
                while (dr.Read())
                {
                    result.Add((Type)dr[0]);
                }
                return result;
            } 
        }
        #endregion


        #region Query
        /// <summary>
        ///  通过DataTable作为中间结果序列化为最终类型
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<Type> Query<Type>(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            return conn.ExecuteDataTable(sql, param: param, commandTimeout: commandTimeout)?.ConvertBySerialize<List<Type>>();
        }
        #endregion


        #region Query

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="getRow"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Type> Query<Type>(this IDbConnection conn, string sql, Func<IDataReader, Type> getRow, IDictionary<string, object> param = null,  int? commandTimeout = null)
        {
            using (var dr = conn.ExecuteReader(sql, param: param, commandTimeout: commandTimeout))
            {
                while (dr.Read())
                {
                    yield return getRow(dr);
                }
            }
        }
        #endregion

    }
}
