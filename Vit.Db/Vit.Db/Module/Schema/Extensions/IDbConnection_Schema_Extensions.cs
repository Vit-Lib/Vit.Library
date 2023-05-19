using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Vit.Db.Module.Schema;
using Vit.Db.Util.Data;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class IDbConnection_Schema_Extensions
    {

        #region GetAllTableName
        /// <summary>
        /// 获取所有表的名称
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static List<string> GetAllTableName(this IDbConnection conn)
        {
            switch (conn.GetDbType())
            {
                case EDbType.mssql: return conn.MsSql_GetAllTableName();
                case EDbType.mysql: return conn.MySql_GetAllTableName();
                case EDbType.sqlite: return conn.Sqlite_GetAllTableName();
            }

            throw new NotImplementedException($"NotImplementedException from IDbConnection.{nameof(GetAllTableName)} in {nameof(IDbConnection_Schema_Extensions)}.cs");

        }
        #endregion


        #region GetAllData
        /// <summary>
        /// 获取所有的表数据
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DataSet GetAllData(this IDbConnection conn)
        {
            var tableNames = conn.GetAllTableName();
            var sql = "select * from " + String.Join(";select * from ", tableNames.Select(conn.Quote)) + ";";
            var ds = conn.ExecuteDataSet(sql);
            for (int t = 0; t < tableNames.Count; t++)
            {
                ds.Tables[t].TableName = tableNames[t];
            }
            return ds;
        }
        #endregion


        #region GetSchema
        /// <summary>
        /// 获取表的字段信息
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableNames">若不指定则返回所有表的字段信息</param>
        /// <returns></returns>
        public static List<TableSchema> GetSchema(this IDbConnection conn, IEnumerable<string> tableNames = null)
        {
            switch (conn.GetDbType())
            {
                case EDbType.mssql: return conn.MsSql_GetSchema();
                case EDbType.mysql: return conn.MySql_GetSchema();
                case EDbType.sqlite: return conn.Sqlite_GetSchema();
            }

            throw new NotImplementedException($"NotImplementedException from IDbConnection.{nameof(GetSchema)} in {nameof(IDbConnection_Schema_Extensions)}.cs");

        }
        #endregion


    }
}
