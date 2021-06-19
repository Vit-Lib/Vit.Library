using System;
using System.Data;

namespace Vit.Extensions
{

    public static partial class IDbConnection_BulkImportExtensions
    {

        #region BulkImport DataTable
        /// <summary>
        /// 批量导入数据，目前支持 mssql、mysql、sqlite
        /// <para> mssql连接字符串必须包含"persist security info=true;" </para>
        /// <para> mysql连接字符串必须包含"AllowLoadLocalInfile=true;"  </para>
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param> 
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <returns></returns>
        public static void BulkImport(this IDbConnection conn, DataTable dt
            , int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            switch (conn)
            {
                case System.Data.SqlClient.SqlConnection msSqlConn:
                    msSqlConn.Import(
                        dt
                        , batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;

                case MySql.Data.MySqlClient.MySqlConnection mySqlConn:
                    mySqlConn.Import(
                        dt
                        , batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;

                //case System.Data.SQLite.SQLiteConnection sqliteConn:
                case Microsoft.Data.Sqlite.SqliteConnection sqliteConn:
                    sqliteConn.Import(
                        dt
                        , batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;
            }

            throw new NotImplementedException($"NotImplementedException from IDbConnection.{nameof(BulkImport)} in {nameof(IDbConnection_BulkImportExtensions)}.cs");
        }
        #endregion



        #region BulkImport DataReader
        /// <summary>
        /// 批量导入数据，目前支持 mssql、mysql、sqlite
        /// <para> mssql连接字符串必须包含"persist security info=true;" </para>
        /// <para> mysql连接字符串必须包含"AllowLoadLocalInfile=true;"  </para>
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <returns></returns>
        public static int BulkImport(this IDbConnection conn, IDataReader dr, string tableName
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            switch (conn)
            {
                case System.Data.SqlClient.SqlConnection msSqlConn:
                    return msSqlConn.Import(
                        dr, tableName
                        , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );

                case MySql.Data.MySqlClient.MySqlConnection mySqlConn:
                    return mySqlConn.Import(
                        dr, tableName
                        , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );

                //case System.Data.SQLite.SQLiteConnection sqliteConn:
                case Microsoft.Data.Sqlite.SqliteConnection sqliteConn:
                    return sqliteConn.Import(
                        dr, tableName
                        , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );                
            }

            throw new NotImplementedException($"NotImplementedException from IDbConnection.{nameof(BulkImport)} in {nameof(IDbConnection_BulkImportExtensions)}.cs");
        }
        #endregion

    }
}
