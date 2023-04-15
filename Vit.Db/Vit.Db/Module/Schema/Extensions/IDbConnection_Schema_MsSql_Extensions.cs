using System.Collections.Generic;
using System.Data;
using System.Linq;
using Vit.Db.Module.Schema;
using Vit.Db.Module.Schema.Extensions;
using Vit.Db.Util.Data;
using Vit.Extensions.Linq_Extensions.Execute;

namespace Vit.Extensions.Linq_Extensions
{
    public static partial class IDbConnection_Schema_MsSql_Extensions
    {


        #region GetAllTableName
        /// <summary>
        /// 获取所有表的名称
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<string> MsSql_GetAllTableName(this IDbConnection conn)
        {
            //获取所有的用户表
            //SELECT [Name] into #tb FROM sysobjects WHERE [type] = 'U'  and [Name]!='dtproperties'      
            return conn.QueryScalar<string>("SELECT [Name]  FROM sysobjects WHERE [type] = 'U'  and [Name]!='dtproperties'"
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
        public static List<TableSchema> MsSql_GetSchema(this IDbConnection conn, IEnumerable<string> tableNames = null)
        {

            #region (x.1)获取列名 和 数据库原始字段类型          
            var sql = string.Format(@"
SELECT
tb.name AS table_name
,col.name AS column_name
-- ''as column_type
,colProp.value AS column_comment

,(case when exists(SELECT 1 FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE k where k.TABLE_NAME=tb.name and k.COLUMN_NAME=col.name ) 
then 1 else 0 end) as primary_key

,convert(int,col.is_identity)AS autoincrement
--,col.is_identity AS autoincrement

FROM sys.tables tb
INNER JOIN sys.columns col ON col.object_id = tb.object_id
LEFT JOIN sys.extended_properties colProp ON colProp.major_id = col.object_id AND colProp.minor_id = col.column_id

WHERE tb.name IN ('{0}') "
                    , string.Join("','", tableNames ?? conn.MsSql_GetAllTableName()));

            Dictionary<string, TableSchema> tableMap = new Dictionary<string, TableSchema>();

            conn.Query<ColumnSchemaExt>(sql,commandTimeout: ConnectionFactory.CommandTimeout)?.ForEach(col=> 
            {
                if (!tableMap.TryGetValue(col.table_name, out var curTableSchema))
                {
                    curTableSchema = new TableSchema
                    {
                        table_name = col.table_name,
                        columns = new List<ColumnSchema>()
                    };                 
                    tableMap.Add(curTableSchema.table_name, curTableSchema);
                }
                curTableSchema.columns.Add(col);
            });
             

            var tables = tableMap.Values.ToList();
            #endregion

            #region (x.2)获取字段Clr类型
            foreach (var entity in tables)
            {
                var dt = conn.ExecuteDataTable("select * from " + conn.Quote(entity.table_name) + " where 1=2");
                foreach (DataColumn column in dt.Columns)
                {
                    var field = entity.columns.Where(f => f.column_name == column.ColumnName).FirstOrDefault();
                    if (field != null)
                    {
                        var dataType= column.DataType;
                        if (column.AllowDBNull && dataType.IsValueType) 
                        {
                            dataType = typeof(System.Nullable<>).MakeGenericType(dataType);
                        }                       
                        field.column_clr_type = dataType;
                    }
                }
            }
            #endregion

            return tables;             
        }

        #endregion



    }


}
