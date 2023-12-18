using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using SqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using SqlCommand = MySql.Data.MySqlClient.MySqlCommand;
//using SqlDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
//using SqlDataReader= MySql.Data.MySqlClient.MySqlDataReader;

using System.Linq;
using System.IO;
using Vit.Db.BulkImport;
using Vit.Core.Util.MethodExt;
using Vit.Extensions.Linq_Extensions.Execute;
using Vit.Extensions.Data;

namespace Vit.Extensions.Linq_Extensions
{
    public static partial class MySql_BulkImportExtensions
    {


        //参考资料 

        /*
             .net core利用MySqlBulkLoader大数据批量导入MySQL
             https://www.cnblogs.com/xiaopotian/p/8515010.html

             https://www.cnblogs.com/qtiger/p/13652334.html
             远程导入csv文件时 数据库连接字符串要加上"AllowLoadLocalInfile=true"，MySqlBulkLoader中要设置Local = true。
        */

        /*
            csv转换 https://blog.csdn.net/pukuimin1226/article/details/52161081
            空日期入库后出现据0000-00-00 00:00:00问题
            DataTime? ，csv生成时会直接为空,例如：123,,"张三"
            实际需要：123,NULL,"张三"
            这样bulk load插入数据库就会是空，而不是0000-00-00 00:00:00 
        */







        #region Import Csv

        /// <summary>
        /// 通过MySqlBulkLoader驱动批量导入数据（连接字符串必须包含 "AllowLoadLocalInfile=true;"）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="csvPath"></param>
        /// <param name="tableName"></param>
        /// <param name="columnNames"></param>
        /// <param name="NumberOfLinesToSkip">是否使用事务</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>          
        public static int ImportCsv(this SqlConnection conn, string csvPath
            , string tableName, IEnumerable<string> columnNames, int NumberOfLinesToSkip = 1
            , bool useTransaction = true, int? commandTimeout = null)
        {

            Func<int> actionImportData = null;


            #region (x.2)导入数据的逻辑           
            actionImportData = () =>
            {
                //批量导入                    
                MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                {
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                    LineTerminator = "\r\n",
                    FileName = csvPath,
                    NumberOfLinesToSkip = NumberOfLinesToSkip,
                    TableName = conn.Quote(tableName),
                    Timeout = commandTimeout ?? 0,
                    Local = true,
                    CharacterSet = "utf8",
                };

                bulk.Columns.AddRange(columnNames.Select(n => conn.Quote(n)));
                return bulk.Load();
            };
            #endregion



            #region (x.3)使用事务             
            if (useTransaction)
            {
                Method.Wrap(ref actionImportData, (baseFunc) =>
                {
                    return conn.MakeSureOpen(() =>
                    {
                        return conn.RunInTransaction(baseFunc);
                    });
                });
            }
            #endregion





            #region (x.4)确保 连接字符串要加上AllowLoadLocalInfile=true
            //Method.Wrap(ref actionImportData,(baseAction) =>
            //{
            //    var ConnectionString = conn.ConnectionString;
            //    try
            //    {
            //        var connStringBuilder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(ConnectionString);
            //        connStringBuilder.Add("AllowLoadLocalInfile", "true");
            //        conn.ConnectionString = connStringBuilder.ToString();

            //        return baseAction();
            //    }
            //    finally
            //    {
            //        conn.ConnectionString = ConnectionString;
            //    }
            //});
            #endregion


            #region (x.5)确保开启允许本地导入数据
            {
                //(x.x.1)获取 是否开启允许本地导入数据
                bool local_infile;

                //var str_local_infile = conn.ExecuteDataTable("SHOW VARIABLES LIKE '%local%';")?.Rows?[0][1] as string;
                //local_infile = str_local_infile?.ToUpper() == "ON";

                local_infile = 1 == conn.ExecuteScalar<int>("SELECT @@local_infile;");

                //(x.x.2)
                if (!local_infile)
                {
                    Method.Wrap(ref actionImportData, (baseAction) =>
                    {
                        //1表示开启，0表示关闭
                        conn.Execute("SET GLOBAL local_infile=1");
                        try
                        {
                            return baseAction();
                        }
                        finally
                        {
                            conn.Execute("SET GLOBAL local_infile=0");
                        }
                    });
                }
            }
            #endregion


            #region (x.6)确保关闭外键检查       
            {
                //(x.x.1)获取 状态
                bool FOREIGN_KEY_CHECKS = 1 == conn.ExecuteScalar<int>("SELECT @@FOREIGN_KEY_CHECKS;");

                //(x.x.2)
                if (FOREIGN_KEY_CHECKS)
                {
                    Method.Wrap(ref actionImportData, (baseAction) =>
                    {
                        //1表示开启，0表示关闭
                        conn.Execute("SET FOREIGN_KEY_CHECKS = 0;");
                        try
                        {
                            return baseAction();
                        }
                        finally
                        {
                            conn.Execute("SET FOREIGN_KEY_CHECKS = 1;");
                        }
                    });
                }
            }
            #endregion


            return actionImportData();

        }

        #endregion


        #region Import DataTable


        /// <summary>
        /// 通过MySqlBulkLoader驱动批量导入数据（连接字符串必须包含 "AllowLoadLocalInfile=true;"）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <param name="tempFilePath">临时文件路径，可不指定</param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param> 
        public static int Import(this SqlConnection conn
            , DataTable dt, string tempFilePath = null
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null
            )
        {
            if (string.IsNullOrEmpty(tempFilePath))
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), "MysqlImport_" + dt.TableName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".csv");
            }

            try
            {

                var columns = dt.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList();


                var batchRowCount_ = batchRowCount ?? BulkImport.batchRowCount;
                if (batchRowCount_ <= 0)
                {
                    batchRowCount_ = maxRowCount;
                }

                int importedRowCount = 0;

                while (true)
                {

                    //(x.x.1)保存数据到csv文件
                    int readedRowCount = CsvHelp.SaveToCsv(dt, tempFilePath,
                        addColumnName: true,
                        firstRowIndex: importedRowCount,
                        maxRowCount: Math.Min(batchRowCount_, maxRowCount - importedRowCount)
                        );

                    if (readedRowCount == 0) break;

                    //(x.x.2)导入
                    var curImportedRowCount = ImportCsv(conn, tempFilePath, dt.TableName, columns, useTransaction: useTransaction, commandTimeout: commandTimeout);

                    //(x.x.3)事件
                    importedRowCount += curImportedRowCount;
                    onProcess?.Invoke(curImportedRowCount, importedRowCount);

                    if (curImportedRowCount < batchRowCount_) break;
                }
                return importedRowCount;
            }
            finally
            {
                File.Delete(tempFilePath);
            }
        }

        #endregion



        #region Import DataReader
        /// <summary>
        /// 通过MySqlBulkLoader驱动批量导入数据（连接字符串必须包含 "AllowLoadLocalInfile=true;"）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        /// <param name="tempFilePath">临时文件路径，可不指定</param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static int Import(this SqlConnection conn
            , IDataReader dr, string tableName, string tempFilePath = null
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null
            )
        {

            if (string.IsNullOrEmpty(tempFilePath))
            {
                tempFilePath = Path.Combine(Path.GetTempPath(), "MysqlImport_" + tableName + "_" + DateTime.Now.ToString("yyyy-MM-dd_HHmmss") + ".csv");
            }

            try
            {

                var batchRowCount_ = batchRowCount ?? BulkImport.batchRowCount;
                if (batchRowCount_ <= 0)
                {
                    batchRowCount_ = maxRowCount;
                }


                int fieldCount = dr.FieldCount;
                var columns = new string[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    columns[i] = dr.GetName(i);
                }


                int importedRowCount = 0;

                while (true)
                {

                    //(x.x.1)保存数据到csv文件
                    int rowCount = CsvHelp.SaveToCsv(dr, tempFilePath, addColumnName: true, maxRowCount: Math.Min(batchRowCount_, maxRowCount - importedRowCount));

                    if (rowCount == 0) break;

                    //(x.x.2)导入
                    rowCount = ImportCsv(conn, tempFilePath, tableName, columns, useTransaction: useTransaction, commandTimeout: commandTimeout);


                    //(x.x.3)事件
                    importedRowCount += rowCount;
                    onProcess?.Invoke(rowCount, importedRowCount);

                    if (rowCount < batchRowCount_) break;
                }

                return importedRowCount;

            }
            finally
            {
                File.Delete(tempFilePath);
            }

        }


        #endregion






        #region Import DataTable By Sql

        /// <summary>
        /// 导入数据(无需手动打开连接，会自动检测连接状态进行打开或关闭)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <param name="useTransaction">内部是否创建事务并使用</param>
        /// <param name="tran"></param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void ImportBySql(this SqlConnection conn, DataTable dt, bool useTransaction = true, MySqlTransaction tran = null, int? commandTimeout = null)
        {
            if (null == dt || dt.Columns.Count == 0 || dt.Rows.Count == 0) return;

            #region importAction           
            Action importAction = () =>
            {
                using (var cmd = conn.CreateCommand())
                {

                    var param = cmd.Parameters;

                    #region (x.1)创建表结构的SQL语句
                    // CREATE TABEL IF NOT EXISTS ，一般情况下用这句比较好，如果原来就有同名的表，没有这句就会出错
                    StringBuilder sql = new StringBuilder("insert into ").Append(conn.MySql_Quote(dt.TableName)).Append(" (");
                    foreach (DataColumn dc in dt.Columns)
                    {
                        string columnName = dc.ColumnName;

                        sql.Append(conn.Quote(columnName)).Append(",");
                    }
                    sql.Length--;
                    sql.Append(") values(");


                    //var item = dt.Rows[0];
                    //foreach (DataColumn dc in dt.Columns)
                    //{
                    //    string columnName = dc.ColumnName;
                    //    sql.Append("@").Append(columnName).Append(",");
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
                    #endregion

                    cmd.Connection = conn;
                    cmd.CommandText = sql.ToString();

                    cmd.CommandTimeout = commandTimeout ?? 0;


                    #region (x.3)批量导入数据                   
                    if (useTransaction)
                    {

                        int rowCount = 0;

                        #region (x.1)构建一次事务的逻辑             

                        Action action = () =>
                        {
                            int stopIndex = Math.Min(rowCount + BulkImport.batchRowCount, dt.Rows.Count);
                            do
                            {
                                DataRow row = dt.Rows[rowCount];

                                for (int t = 0; t < param.Count; t++)
                                {
                                    param[t].Value = row[t] ?? DBNull.Value;
                                }
                                cmd.ExecuteNonQuery();

                                rowCount++;

                            } while (rowCount < stopIndex);

                        };
                        #endregion


                        #region (x.2)
                        do
                        {
                            using (var trans = conn.BeginTransaction())
                            {
                                try
                                {
                                    cmd.Transaction = trans;
                                    action();
                                    trans.Commit();
                                }
                                catch
                                {
                                    trans.Rollback();
                                    throw;
                                }
                            }

                        } while (rowCount < dt.Rows.Count);
                        #endregion

                    }
                    else
                    {
                        if (tran != null)
                            cmd.Transaction = tran;

                        foreach (DataRow row in dt.Rows)
                        {
                            for (int t = 0; t < param.Count; t++)
                            {
                                param[t].Value = row[t];
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    #endregion
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
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    int sumCount = 0;
                    foreach (DataTable dt in ds.Tables)
                    {
                        conn.ImportBySql(dt, false);
                        sumCount += dt.Rows.Count;
                    }
                    trans.Commit();
                    return sumCount;
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
        #endregion


    }
}
