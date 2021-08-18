using System;
using System.Data;
using System.Runtime.CompilerServices;
using Vit.Db.Util.Data;

namespace Vit.Extensions
{

    public static partial class IDbConnection_Quote_Extensions
    {

        #region IDbConnection Quote

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Quote(this IDbConnection conn, string name)
        {
            switch (conn.GetDbType())
            {
                case EDbType.sqlite:
                    return conn.Sqlite_Quote(name);

                case EDbType.mysql:
                    return conn.MySql_Quote(name);

                case EDbType.mssql:
                    return conn.MsSql_Quote(name);
            }

            throw new NotImplementedException($"NotImplementedException from {nameof(Quote)} in {nameof(IDbConnection_Quote_Extensions)}.cs");
        }
        #endregion



        #region MsSql_Quote
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MsSql_Quote(this IDbConnection conn, string name)
        {
            return "[" + name + "]";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Quote(this System.Data.SqlClient.SqlConnection conn, string name)
        {
            return MsSql_Quote(conn, name);
        }

        #endregion


        #region MySql_Quote
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MySql_Quote(this IDbConnection conn, string name)
        {
            return "`" + name + "`";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Quote(this MySql.Data.MySqlClient.MySqlConnection conn, string name)
        {
            return MySql_Quote(conn,name);
        }

        #endregion


        #region Sqlite_Quote

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sqlite_Quote(this IDbConnection conn, string name)
        {
            return "[" + name + "]";
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static string Quote(this System.Data.SQLite.SQLiteConnection conn, string name)
        //{
        //    return Sqlite_Quote(conn, name);
        //}


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Quote(this Microsoft.Data.Sqlite.SqliteConnection conn, string name)
        {
            return Sqlite_Quote(conn, name);
        }

        #endregion

    }
}
