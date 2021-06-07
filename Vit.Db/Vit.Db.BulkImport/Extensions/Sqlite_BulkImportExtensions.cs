using System;
using System.Data;
using System.Text;
using Microsoft.Data.Sqlite;
using SqlConnection = Microsoft.Data.Sqlite.SqliteConnection;
using SqlCommand = Microsoft.Data.Sqlite.SqliteCommand; 
using Vit.Db.BulkImport;

namespace Vit.Extensions
{
    public static partial class Sqlite_BulkImportExtensions
    {

        #region Import DataTable
        /// <summary>
        /// 导入数据(无需手动打开连接，会自动检测连接状态进行打开或关闭)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <param name="batchRowCount">使用事务时，每次事务导入的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void Import(this SqlConnection conn, DataTable dt
            , int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            if (null == dt || dt.Columns.Count == 0 || dt.Rows.Count == 0) return; 


            #region importAction           
            Action importAction = () =>
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.Connection = conn;
                    var param = cmd.Parameters;
                    cmd.CommandText = Import_GetSqlInsert(dt, param);

                    cmd.CommandTimeout = commandTimeout ?? 0;

                    int importedRowCount = 0;

                    if (useTransaction)
                    {

                        #region (x.1)构建一次事务的逻辑             
                        var batchRowCount_ = batchRowCount ?? BulkImport.batchRowCount;
                        if (batchRowCount_ <= 0)
                        {
                            batchRowCount_ = dt.Rows.Count;
                        }

                        Func<int> action = () =>
                        {
                            int stopIndex = Math.Min(importedRowCount + batchRowCount_, dt.Rows.Count);
                            int rowCount = 0;
                            do
                            {
                                DataRow row = dt.Rows[importedRowCount];

                                for (int t = 0; t < param.Count; t++)
                                {
                                    param[t].Value = row[t] ?? DBNull.Value;
                                }
                                cmd.ExecuteNonQuery();

                                rowCount++;
                                importedRowCount++;

                            } while (importedRowCount < stopIndex);
                            return rowCount;

                        };
                        #endregion


                        #region (x.2)分批次导入数据
                        do
                        {
                            using (var trans = conn.BeginTransaction())
                            {
                                cmd.Transaction = trans;
                                try
                                {
                                    int rowCount = action();
                                    trans.Commit();
                                    onProcess?.Invoke(rowCount, importedRowCount);
                                }
                                catch
                                {
                                    trans.Rollback();
                                    throw;
                                }
                            }

                        } while (importedRowCount < dt.Rows.Count);
                        #endregion
                       
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            for (int t = 0; t < param.Count; t++)
                            {
                                param[t].Value = row[t] ?? DBNull.Value;
                            }
                            cmd.ExecuteNonQuery();
                            importedRowCount++;
                        }
                        onProcess?.Invoke(importedRowCount,importedRowCount);
                    }
                }

            };
            #endregion


            conn.MakeSureOpen(importAction);
        }



        static string Import_GetSqlInsert(DataTable dt, SqliteParameterCollection param)
        {

            StringBuilder sql = new StringBuilder("insert into [").Append(dt.TableName).Append("] (");

            foreach (DataColumn dc in dt.Columns)
            {
                string columnName = dc.ColumnName;

                sql.Append("[").Append(columnName).Append("],");
            }
            sql.Length--;
            sql.Append(") values(");


            //var item = dt.Rows[0];
            //foreach (DataColumn dc in dt.Columns)
            //{
            //    string columnName = dc.ColumnName;
            //    sql.Append("?,");
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

            return sql.ToString();
        }



        #endregion



        #region Import DataReader       
        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <param name="dr"></param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <param name="batchRowCount">使用事务时，每次事务导入的数据行数。若为null则使用默认值,若为负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static int Import(this SqlConnection conn, IDataReader dr, string tableName
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null)
        {
            int importedRowCount = 0;
            if (!dr.Read()) return importedRowCount;



            using (var cmd = conn.CreateCommand())
            {
                cmd.Connection = conn;

                cmd.CommandTimeout = commandTimeout ?? 0;

                var param = cmd.Parameters;
                cmd.CommandText = Import_GetSqlInsert(tableName, dr, param);

                if (useTransaction)
                {
                    #region (x.1)构建一次事务的逻辑

                    var batchRowCount_ = batchRowCount ?? BulkImport.batchRowCount;
                    if (batchRowCount_ <= 0)
                    {
                        batchRowCount_ = maxRowCount;
                    }

                    Func<int> action = () => 
                    {
                        int rowCount = 0;
                        int stopIndex = Math.Min(importedRowCount + batchRowCount_, maxRowCount);
                        do
                        {
                            for (int t = 0; t < param.Count; t++)
                            {
                                if (dr.IsDBNull(t))
                                {
                                    param[t].Value = DBNull.Value;
                                    continue;
                                }

                                if (dr.IsDateTime(t))
                                {
                                    param[t].Value = dr.GetDateTime(t);
                                    continue;
                                }
                                param[t].Value = dr[t];
                            }
                            cmd.ExecuteNonQuery();
                            rowCount++;
                            importedRowCount++;

                        } while (importedRowCount < stopIndex && dr.Read());

                        return rowCount;

                    };
                    #endregion


                    #region (x.2)
                    do
                    {
                        using (var trans = conn.BeginTransaction())
                        {
                            cmd.Transaction = trans;
                            try
                            {
                                int rowCount = action();
                                trans.Commit();
                                onProcess?.Invoke(rowCount, importedRowCount);
                            }
                            catch
                            {
                                trans.Rollback();
                                throw;
                            }
                        }

                    } while (importedRowCount < maxRowCount && dr.Read());
                    #endregion
                }
                else
                {
                    do
                    {
                        for (int t = 0; t < param.Count; t++)
                        {
                            if (dr.IsDBNull(t))
                            {
                                param[t].Value = DBNull.Value;
                                continue;
                            }

                            if (dr.IsDateTime(t))
                            {
                                param[t].Value = dr.GetDateTime(t);
                                continue;
                            }
                            param[t].Value = dr[t];
                        }
                        cmd.ExecuteNonQuery();
                        importedRowCount++;
                    } while (importedRowCount < maxRowCount &&  dr.Read());

                    onProcess?.Invoke(importedRowCount,importedRowCount);
                }
            }
            return importedRowCount;
        }


        static string Import_GetSqlInsert(string tableName, IDataReader dr, SqliteParameterCollection param)
        {

            StringBuilder sql = new StringBuilder("insert into [").Append(tableName).Append("] (");

            for (int i = 0; i < dr.FieldCount; i++)
            {
                string columnName = dr.GetName(i).Trim();
                sql.Append("[").Append(columnName).Append("],");
            }

            sql.Length--;
            sql.Append(") values(");

            for (int i = 0; i < dr.FieldCount; i++)
            {
                //sql.Append("?,");     
                //param.AddWithValue("param" + i, null);
                var paramName = "p" + i;
                sql.Append("@"+paramName+",");
                param.AddWithValue(paramName, null);
            }
            sql.Length--;
            sql.Append(")");

            return sql.ToString();
        }

        #endregion

    }
}
