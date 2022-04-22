using System;
using System.Data;
using Vit.Db.Util.Data;

namespace Vit.Db.BulkImport
{
    public partial class BulkImport
    {
        /// <summary>
        /// 批量导入时每批次的数据行数(默认50万，初始值从appsettings.json::Vit.Db.BulkImport.batchRowCount获取)
        /// </summary>
        public static readonly int batchRowCount = Vit.Core.Util.ConfigurationManager.Appsettings.json.GetByPath<int?>("Vit.Db.BulkImport.batchRowCount") ?? 500000;


        #region Import DataTable
        /// <summary>
        /// 批量导入数据，目前支持 mssql、mysql、sqlite
        /// </summary>
        /// <param name="info"></param> 
        /// <param name="dt"></param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void Import(ConnectionInfo info, DataTable dt
            , int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            switch (info.type?.ToLower())
            {
                case "mssql": 
                    MsSql_Import(info.ConnectionString
                        , dt
                        , batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        ); 
                    return;
                case "mysql":
                    MySql_Import(info.ConnectionString
                        , dt
                        , batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;
                case "sqlite": 
                    Sqlite_Import(info.ConnectionString
                        , dt
                        , batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;
            }
            throw new NotImplementedException($"NotImplementedException from {nameof(Import)} in {nameof(BulkImport)}.cs");
        }
        #endregion

        #region Import DataReader
        /// <summary>
        /// 批量导入数据，目前支持 mssql、mysql、sqlite
        /// </summary>
        /// <param name="info"></param> 
        /// <param name="tableName"></param>
        /// <param name="dr"></param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void Import(ConnectionInfo info
            , IDataReader dr, string tableName
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            switch (info.type?.ToLower())
            {
                case "mssql":
                    MsSql_Import(info.ConnectionString
                        , dr, tableName
                        , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        ); 
                    return;
                case "mysql":
                    MySql_Import(info.ConnectionString
                        , dr, tableName
                        , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;
                case "sqlite":
                    Sqlite_Import(info.ConnectionString
                        , dr, tableName
                        , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                        , useTransaction: useTransaction, commandTimeout: commandTimeout
                        );
                    return;
            }
            throw new NotImplementedException($"NotImplementedException from {nameof(Import)} in {nameof(BulkImport)}.cs");
        }
        #endregion

    }
}
