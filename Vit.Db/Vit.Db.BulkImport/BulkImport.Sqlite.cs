using System;
using System.Data;
using Vit.Db.Util.Data;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Db.BulkImport
{
    public partial class BulkImport
    {
        #region Import DataTable
        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="dt"></param>
        /// <param name="batchRowCount">使用事务时，每次事务导入的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void Sqlite_Import(string ConnectionString, DataTable dt
            , int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            using (var conn = ConnectionFactory.Sqlite_GetConnection(ConnectionString))
            {
                conn.Import(dt
                    , batchRowCount: batchRowCount, onProcess: onProcess
                    , useTransaction: useTransaction, commandTimeout: commandTimeout);
            }
        }
        #endregion


        #region Import DataReader       
        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="dr"></param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">使用事务时，每次事务导入的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static int Sqlite_Import(string ConnectionString
            , IDataReader dr, string tableName
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            using (var conn = ConnectionFactory.Sqlite_GetConnection(ConnectionString))
            {
                return conn.Import(dr, tableName
                    , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                    , useTransaction: useTransaction, commandTimeout: commandTimeout);
            }
        }
        #endregion


    }
}
