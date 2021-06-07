using System.Data;
using System.Runtime.CompilerServices;

namespace Vit.Extensions
{

    public static partial class IDbConnection_GetDbName_Extensions
    {

        #region MySql_GetDbName

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MySql_GetDbName(this IDbConnection conn)
        {
            return new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(conn.ConnectionString).Database;
        }
        #endregion




        #region MsSql_GetDbName

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MsSql_GetDbName(this IDbConnection conn)
        {
            return new System.Data.SqlClient.SqlConnectionStringBuilder(conn.ConnectionString).InitialCatalog;
        }
        #endregion
    }
}
