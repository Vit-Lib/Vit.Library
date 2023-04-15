using System;
using System.Collections.Generic;
using System.Data;
using Vit.Db.Util.Data;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Db.BulkImport
{
    public partial class BulkImport
    {

        #region Import Csv

        /// <summary>
        /// 通过MySqlBulkLoader驱动批量导入数据 
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="csvPath"></param>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="NumberOfLinesToSkip">是否使用事务</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>  
        public static int MySql_ImportCsv(string ConnectionString, string csvPath
            , string tableName, IEnumerable<string> columnNames, int NumberOfLinesToSkip = 1
            , bool useTransaction = true, int? commandTimeout = null)
        {
            ConnectionString += "AllowLoadLocalInfile=true;";

            using (var conn = ConnectionFactory.MySql_GetConnection(ConnectionString))
            {
                return conn.ImportCsv(csvPath, tableName, columnNames, NumberOfLinesToSkip
                    , useTransaction: useTransaction, commandTimeout: commandTimeout);
            }
        }
        #endregion



        #region Import DataTable
        /// <summary>
        /// 通过MySqlBulkLoader驱动批量导入数据
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="dt"></param>
        /// <param name="tempFilePath">临时文件路径，可不指定</param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param> 
        public static int MySql_Import(
            string ConnectionString
            , DataTable dt, string tempFilePath = null
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            ConnectionString += "AllowLoadLocalInfile=true;";

            using (var conn = ConnectionFactory.MySql_GetConnection(ConnectionString))
            {
                return conn.Import(
                    dt, tempFilePath: tempFilePath
                    , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                    , useTransaction: useTransaction, commandTimeout: commandTimeout);
            }
        }


        #endregion



        #region Import DataReader

        /// <summary>
        /// 通过MySqlBulkLoader驱动批量导入数据
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        /// <param name="tempFilePath">临时文件路径，可不指定</param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static int MySql_Import(string ConnectionString
            , IDataReader dr, string tableName, string tempFilePath = null
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            ConnectionString += "AllowLoadLocalInfile=true;";

            using (var conn = ConnectionFactory.MySql_GetConnection(ConnectionString))
            {
                return conn.Import(
                    dr, tableName, tempFilePath: tempFilePath,
                    maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess,
                    useTransaction: useTransaction, commandTimeout: commandTimeout);
            }
        }

        #endregion


    }
}
