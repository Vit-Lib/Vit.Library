using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using Vit.Extensions.Execute;

namespace Vit.Extensions
{
    public static partial class IDbConnection_ExecuteData_Extensions
    {


        #region ExecuteDataSet

        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataSet ExecuteDataSet(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            using (var reader = conn.ExecuteReader(sql, param, commandTimeout))
            {
                return reader.ReadDataSet();
            }

        }
        #endregion


        #region ExecuteDataTable      
        /// <summary>
        ///
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataTable ExecuteDataTable(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            using (var reader = conn.ExecuteReader(sql, param, commandTimeout: commandTimeout))
            {
                return reader.ReadDataTable();
            }
        }
        #endregion


    }
}
