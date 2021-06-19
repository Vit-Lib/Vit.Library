using System;
using System.Data;
using System.Data.SqlClient;
using Vit.Extensions;

namespace Vit.Db.BulkImport
{
    public partial class BulkImport
    {

        #region MsSql_Import DataTable
        /// <summary>   
        /// 通过SqlBulkCopy驱动批量导入数据
        /// (导入表的名称为dt.TableName)
        /// </summary>
        /// <param name="ConnectionString"></param>      
        /// <param name="dt"></param>
        /// <param name="keepIdentity">保留源标识值（主码不变）。如果指定false，则由目标分配标识值</param>
        /// <param name="option"></param>
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若不指定则使用默认值,若指定负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        public static void MsSql_Import(string ConnectionString
            , DataTable dt
            , bool keepIdentity = true, SqlBulkCopyOptions? option = null
            , int? batchRowCount = null, Action<int,int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null
            )
        {
            /*
            delete [T_IF_Obj];
            truncate table [T_IF_Obj];
             * 
             * 
             * 
            -- SqlBulkCopy 导入方式 会自动设置IDENTITY_INSERT
            SET IDENTITY_INSERT [dbo].[T_IF_Obj] ON
            --
            SET IDENTITY_INSERT [dbo].[T_IF_Obj] OFF
             */


            /*
 SqlBulkCopyOptions            
 Default  对所有选项使用默认值。  
 KeepIdentity 保留源标识值。如果未指定，则由目标分配标识值。  
 CheckConstraints  请在插入数据的同时检查约束。默认情况下，不检查约束。  
 TableLock  在批量复制操作期间获取批量更新锁。如果未指定，则使用行锁。  
 KeepNulls 保留目标表中的空值，而不管默认值的设置如何。如果未指定，则空值将由默认值替换（如果适用）。  
 FireTriggers  指定后，会导致服务器为插入到数据库中的行激发插入触发器。  默认情况下，　是不激发触发器的……
 UseInternalTransaction  如果已指定，则每一批批量复制操作将在事务中发生。 在一个事务中执行，要么都成功，要么都不成功
————————————————
             
             */

            if (option == null)
            {
                option = SqlBulkCopyOptions.KeepNulls;
            }

            if (keepIdentity)
            {
                option |= SqlBulkCopyOptions.KeepIdentity;
            }
            if (useTransaction) 
            {
                option |= SqlBulkCopyOptions.UseInternalTransaction;
            }

            var batchRowCount_ = batchRowCount ?? BulkImport.batchRowCount;
            if (batchRowCount_ <= 0)
            {
                batchRowCount_ = dt.Rows.Count;
            }


            using (SqlBulkCopy sqlBC = new SqlBulkCopy(ConnectionString, option.Value))
            {
              
                if (onProcess != null)
                {
                    sqlBC.NotifyAfter = batchRowCount_;
                    int importedRowCount = 0;
                    sqlBC.SqlRowsCopied += (object sender, SqlRowsCopiedEventArgs e) =>
                    {
                        int rowCount = (int)e.RowsCopied;
                        importedRowCount += rowCount;
                        onProcess(rowCount,importedRowCount);
                    };
                }


                //设置永不超时
                sqlBC.BulkCopyTimeout = commandTimeout ??  0;

                sqlBC.BatchSize = batchRowCount_;


                sqlBC.DestinationTableName = dt.TableName;

                //若不加则默认按字段顺序导入，而字段顺序不一定一致，故需要手动添加列映射
                foreach (DataColumn column in dt.Columns)
                {
                    sqlBC.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }
                sqlBC.WriteToServer(dt);
                sqlBC.ColumnMappings.Clear();
            }
        }

        #endregion



        #region MsSql_Import DataReader

        /// <summary>
        /// 通过SqlBulkCopy驱动批量导入数据
        /// </summary>
        /// <param name="ConnectionString"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param> 
        /// <param name="keepIdentity">保留源标识值（主码不变）。如果指定false，则由目标分配标识值</param> 
        /// <param name="option"></param>
        /// <param name="maxRowCount">导入数据的最大行数</param>   
        /// <param name="batchRowCount">批量操作时每批次的数据行数。若不指定则使用默认值,若指定负数或零则不分批。</param>
        /// <param name="onProcess">在每（批）次导入成功后调用，参数分别为当前导入数据条数、总共导入数据条数。</param>
        /// <param name="useTransaction">是否使用事务</param>
        /// <param name="commandTimeout">sets the wait time before terminating the attempt to execute a command and generating an error.</param>
        /// <param name="beforeImport"></param>
        public static int MsSql_Import(string ConnectionString
            , IDataReader dr, string tableName
            , bool keepIdentity = true, SqlBulkCopyOptions? option = null
            , int maxRowCount = int.MaxValue, int? batchRowCount = null, Action<int, int> onProcess = null
            , bool useTransaction = true, int? commandTimeout = null,
            Action<DataTable> beforeImport = null)
        {
            int importedRowCount = 0;

            var batchRowCount_ = batchRowCount ?? BulkImport.batchRowCount;
            if (batchRowCount_ <= 0)
            {
                batchRowCount_ = maxRowCount;
            }

            while (true)
            {
                int stopIndex = Math.Min(importedRowCount + batchRowCount_, maxRowCount);

                var dt = dr.ReadDataTable(stopIndex - importedRowCount);
                if (dt == null || dt.Rows.Count==0) break;

                dt.TableName = tableName;

                beforeImport?.Invoke(dt);

                MsSql_Import(ConnectionString, dt, keepIdentity: keepIdentity, option: option
                    , batchRowCount:0            
                    , useTransaction: useTransaction, commandTimeout: commandTimeout);

                int rowCount = dt.Rows.Count;

                importedRowCount += rowCount;

                onProcess?.Invoke(rowCount, importedRowCount);

            }
            return importedRowCount;
        }
        #endregion
    }
}
