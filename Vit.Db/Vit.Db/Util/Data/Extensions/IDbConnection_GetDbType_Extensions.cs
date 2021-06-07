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


            #region (x.4)根据类名和命名空间名称判断
            var typeFullName = conn.GetType().FullName.ToLower();

            if (typeFullName.Contains("sqlite"))
            {
                return EDbType.sqlite;
            }

            if (typeFullName.Contains("mysql"))
            {
                return EDbType.mysql;
            }
            #endregion

            return null;          
        }
        #endregion
    }
}
