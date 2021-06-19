using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

using SqlConnection = System.Data.SqlClient.SqlConnection;
using SqlCommand = System.Data.SqlClient.SqlCommand;
//using SqlDataAdapter = System.Data.SqlClient.SqlDataAdapter;
//using SqlDataReader = System.Data.SqlClient.SqlDataReader;

using Vit.Db.BulkImport;

namespace Vit.Extensions
{
    public static partial class MsSql_BulkImportExtensions
    {


        #region Import DataTable 

        /// <summary>   
        /// 通过SqlBulkCopy驱动批量导入数据（连接字符串必须包含 "persist security info=true;"）
        /// (导入表的名称为dt.TableName)
        /// </summary>
        /// <param name="conn"></param>      
        /// <param name="dt"></param>
        /// <param name="keepIdentity">保留源标识值（主码不变）。如果指定false，则由目标分配标识值</param>
        /// <param name="option"></param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若不指定则使用默认值。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void Import(this SqlConnection conn
            , DataTable dt
            , bool keepIdentity = true, SqlBulkCopyOptions? option = null
            , int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null
            )
        {
            BulkImport.MsSql_Import(conn.ConnectionString
                , dt
                , keepIdentity: keepIdentity, option: option
                , batchRowCount: batchRowCount, onProcess: onProcess
                , useTransaction: useTransaction, commandTimeout: commandTimeout);
        }
        #endregion




        #region Import DataReader
        /// <summary>
        /// 通过SqlBulkCopy驱动批量导入数据（连接字符串必须包含 "persist security info=true;"）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param> 
        /// <param name="keepIdentity">保留源标识值（主码不变）。如果指定false，则由目标分配标识值</param> 
        /// <param name="option"></param>
        /// <param name="maxRowCount">导入数据的最大行数</param>   
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static int Import(this SqlConnection conn
            , IDataReader dr, string tableName
            , bool keepIdentity = true, SqlBulkCopyOptions? option = null
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null
            )
        {
            return BulkImport.MsSql_Import(conn.ConnectionString
                , dr, tableName
                , keepIdentity: keepIdentity, option: option
                , maxRowCount: maxRowCount, batchRowCount: batchRowCount, onProcess: onProcess
                , useTransaction: useTransaction, commandTimeout: commandTimeout
                );
        }

        #endregion





        #region ImportBySql

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void ImportBySql(this SqlConnection conn, DataTable dt, bool useTransaction = true, int? commandTimeout = null)
        {
            if (null == dt || dt.Columns.Count == 0 || dt.Rows.Count == 0) return;

            #region importAction           
            Action importAction = () =>
            {

                StringBuilder sql = new StringBuilder("insert into ").Append(conn.MsSql_Quote(dt.TableName)).Append("(");

                using (var cmd = conn.CreateCommand())
                {

                    foreach (DataColumn dc in dt.Columns)
                    {
                        string columnName = dc.ColumnName;

                        sql.Append(columnName).Append(",");
                    }
                    sql.Length--;
                    sql.Append(") values(");

                    var param = cmd.Parameters;


                    //var item = dt.Rows[0];
                    //foreach (DataColumn dc in dt.Columns)
                    //{
                    //    string columnName = dc.ColumnName;
                    //    sql.Append("@").Append(columnName).Append(",");
                    //    //sql.Append("?,");
                    //    param.AddWithValue(columnName, item[dc]);
                    //}
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        var paramName = "p" + i;
                        sql.Append("@" + paramName + ",");
                        param.AddWithValue(paramName, null);
                    }



                    sql.Length--;
                    sql.Append(")");

                    cmd.Connection = conn;
                    cmd.CommandText = sql.ToString();


                    cmd.CommandTimeout = commandTimeout ?? 0;

                    if (useTransaction)
                    {
                        using (var tran = conn.BeginTransaction())
                        {
                            try
                            {
                                cmd.Transaction = tran;
                                foreach (DataRow row in dt.Rows)
                                {
                                    for (int t = 0; t < param.Count; t++)
                                    {
                                        param[t].Value = row[t] ?? DBNull.Value;
                                    }
                                    cmd.ExecuteNonQuery();
                                }
                                tran.Commit();
                            }
                            catch (Exception)
                            {
                                tran.Rollback();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            for (int t = 0; t < param.Count; t++)
                            {
                                param[t].Value = row[t];
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            };
            #endregion


            conn.MakeSureOpen(importAction);


        }



        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        public static int ImportBySql(this SqlConnection conn, DataSet ds)
        {
            return conn.RunInTransaction(() =>
            {
                int sumCount = 0;
                foreach (DataTable dt in ds.Tables)
                {
                    conn.ImportBySql(dt, false);
                    sumCount += dt.Rows.Count;
                }
                return sumCount;
            });

        }
        #endregion


    }
}
