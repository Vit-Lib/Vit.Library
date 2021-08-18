using System.Data;
using System.Runtime.CompilerServices;
using Vit.Db.Util.Data;

namespace Vit.Extensions
{

    public static partial class IDbConnection_GetDbType_Extensions
    {

        #region GetDbType
        /// <summary>
        /// 获取数据库类型 如 mysql/mssql/sqlite
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EDbType? GetDbType(this IDbConnection conn)
        {
            #region (x.1)判断是否为 mssql
            if (conn is System.Data.SqlClient.SqlConnection)
            {
                return EDbType.mssql;
            }
            #endregion

            #region (x.2)判断是否为 mysql
            if (conn is MySql.Data.MySqlClient.MySqlConnection)
            {
                return EDbType.mysql;
            }
            #endregion


            #region (x.3)判断是否为 sqlite            
            //if (conn is System.Data.SQLite.SQLiteConnection)
            //{
            //    return EDbType.sqlite;
            //}
            if (conn is Microsoft.Data.Sqlite.SqliteConnection)
            {
                return EDbType.sqlite;
            }
            #endregion


            //(x.4)根据类名和命名空间名称判断
            return GetDbTypeFromTypeName(conn);
        }
        #endregion


        #region GetDbTypeFromTypeName
        /// <summary>
        /// 获取数据库类型 如 mysql/mssql/sqlite
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EDbType? GetDbTypeFromTypeName(object obj) 
        {
            var typeFullName = obj?.GetType().FullName.ToLower();

            if (string.IsNullOrEmpty(typeFullName)) return null;

            if (typeFullName.Contains("sqlite"))
            {
                return EDbType.sqlite;
            }

            if (typeFullName.Contains("mysql"))
            {
                return EDbType.mysql;
            }

            if (typeFullName.Contains("mssql")|| typeFullName.Contains("sqlserver") || typeFullName.Contains(".sqlconnection"))
            {
                return EDbType.mssql;
            }

            return null;
        }
        #endregion

    }
}
