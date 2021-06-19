using System.Collections.Generic;
using System.Data;
using System.Linq;
using Vit.Db.Module.Schema;
using Vit.Db.Module.Schema.Extensions;
using Vit.Db.Util.Data;
using Vit.Extensions.Execute;

namespace Vit.Extensions
{
    public static partial class IDbConnection_Schema_MySql_Extensions
    {
     


        #region GetAllTableName
        /// <summary>
        /// 获取所有表的名称
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<string> MySql_GetAllTableName(this IDbConnection conn)
        {
            return conn.QueryScalar<string>("show full tables where Table_type = 'BASE TABLE'", commandTimeout: ConnectionFactory.CommandTimeout);
        }
        #endregion



        #region GetSchema    
        /// <summary>
        /// 获取表的字段信息
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableNames">若不指定则返回所有表的字段信息</param>
        /// <returns></returns>
        public static List<TableSchema> MySql_GetSchema(this IDbConnection conn, IEnumerable<string> tableNames=null)
        {
            #region (x.1)获取列名 和 数据库原始字段类型          
            var sql = string.Format(@"
SELECT 
	TABLE_NAME as table_name
	,COLUMN_NAME as column_name
	,DATA_TYPE as column_type
	,COLUMN_COMMENT as column_comment
	,if(COLUMN_KEY='PRI',1,0) as primary_key
	,if(EXTRA='auto_increment',1,0) as autoincrement
    FROM information_schema.COLUMNS
WHERE TABLE_NAME IN ('{0}')  and TABLE_SCHEMA='{1}'"
                    , string.Join("','", tableNames ?? conn.MySql_GetAllTableName()),conn.MySql_GetDbName()
                    );

            Dictionary<string, TableSchema> tableMap = new Dictionary<string, TableSchema>();

            conn.Query<ColumnSchemaExt>(sql, commandTimeout: ConnectionFactory.CommandTimeout)?.ForEach(
                field =>
                {
                    if (!tableMap.TryGetValue(field.table_name, out var curTableSchema))
                    {
                        curTableSchema = new TableSchema
                        {
                            table_name = field.table_name,
                            columns = new List<ColumnSchema>()
                        };
                        tableMap.Add(curTableSchema.table_name, curTableSchema);
                    }
                    field.column_clr_type_name = DbType_To_ClrType(field.column_type);
                    curTableSchema.columns.Add(field);
                }
            );
           

            var tables= tableMap.Values.ToList();
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
                        var dataType = column.DataType;
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


        #region DbType_To_ClrType

        static string DbType_To_ClrType(string dbType)
        {
            switch (dbType)
            {
                case "tinyint":
                case "smallint":
                case "mediumint":
                case "int":
                case "integer":
                    return "int";
                case "double":
                    return "double";
                case "float":
                    return "float";
                case "decimal":
                    return "decimal";
                case "numeric":
                case "real":
                    return "decimal";
                case "bit":
                    return "bool";
                case "date":
                case "time":
                case "year":
                case "datetime":
                case "timestamp":
                    return "DateTime";
                case "tinyblob":
                case "blob":
                case "mediumblob":
                case "longblog":
                case "binary":
                case "varbinary":
                    return "byte[]";
                case "char":
                case "varchar":
                case "tinytext":
                case "text":
                case "mediumtext":
                case "longtext":
                    return "string";
                case "point":
                case "linestring":
                case "polygon":
                case "geometry":
                case "multipoint":
                case "multilinestring":
                case "multipolygon":
                case "geometrycollection":
                case "enum":
                case "set":
                default:
                    return dbType;
            }
        }

        #endregion

        #endregion
       


    }


}
