using System.Collections.Generic;
using System.Data;
using System.Linq;
using Vit.Db.Module.Schema;
using Vit.Db.Util.Data;
using Vit.Extensions.Linq_Extensions.Execute;

namespace Vit.Extensions.Linq_Extensions
{
    public static partial class IDbConnection_Schema_Sqlite_Extensions
    {
 

        #region GetAllTableName
        /// <summary>
        /// 获取所有表的名称(不含系统表sqlite_sequence)
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<string> Sqlite_GetAllTableName(this IDbConnection conn)
        {
            return conn.QueryScalar<string>("SELECT name FROM sqlite_master WHERE type = 'table'  and name!='sqlite_sequence' "
                , commandTimeout: ConnectionFactory.CommandTimeout);
        }
        #endregion



        #region GetSchema    
        /// <summary>
        /// 获取表的字段信息
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableNames">若不指定则返回所有表的字段信息</param>
        /// <returns></returns>
        public static List<TableSchema> Sqlite_GetSchema(this IDbConnection conn, IEnumerable<string> tableNames = null)
        {
            List<TableSchema> tables = new List<TableSchema>();
            foreach (var tableName in tableNames ?? conn.Sqlite_GetAllTableName())
            {
                #region (x.1)获取列名 和 数据库原始字段类型
                var entity = new TableSchema
                {
                    table_name = tableName,
                    //columns = conn.Query("PRAGMA table_info('" + tableName + "')", commandTimeout: ConnectionFactory.CommandTimeout)
                    //.Select(m => new ColumnSchema
                    //{
                    //    column_name = m.name,
                    //    column_type = m.type,
                    //    primary_key =  m.pk==1?1:0
                    //}).ToList()
                    columns = conn.Query<ColumnSchema>("PRAGMA table_info('" + tableName + "')"
                    , dr => new ColumnSchema { column_name = (string)dr["name"], column_type = (string)dr["type"], primary_key = "1" == dr["pk"].ToString() ? 1 : 0 }
                    , commandTimeout: ConnectionFactory.CommandTimeout)
                     .ToList()
                };
                #endregion


                #region (x.2)获取字段Clr类型
                var dt = conn.ExecuteDataTable("select * from "+ conn.Quote(tableName) +" where 1=2");
                foreach (DataColumn column in dt.Columns)
                {
                    var field = entity.columns.Where(f => f.column_name == column.ColumnName).FirstOrDefault();
                    if (field != null)
                    {
                        var dataType = column.DataType;
                        if (column.AllowDBNull && dataType.IsValueType)
                        {
                            dataType = typeof(System.Nullable<>).MakeGenericType(dataType);
                        }
                        field.column_clr_type = dataType;
                    }
                }
                #endregion

                tables.Add(entity);                
            }
            return tables;
        }




        #endregion







    }


}
