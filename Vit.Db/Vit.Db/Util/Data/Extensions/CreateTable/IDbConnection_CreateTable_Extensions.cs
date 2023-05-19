using System;
using System.Data;
using Vit.Db.Util.Data;

namespace Vit.Extensions.Linq_Extensions
{

    public static partial class IDbConnection_CreateTable_Extensions
    {


        #region CreateTable by DataTable
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static void CreateTable(this IDbConnection conn, DataTable dt)
        {
            switch (conn.GetDbType())
            {
                case EDbType.mssql: conn.MsSql_CreateTable(dt); return;
                case EDbType.mysql: conn.MySql_CreateTable(dt); return;
                case EDbType.sqlite: conn.Sqlite_CreateTable(dt); return;
            }

            throw new NotImplementedException($"NotImplementedException from {nameof(CreateTable)} in {nameof(IDbConnection_CreateTable_Extensions)}.cs");

        }
        #endregion

        #region CreateTable by DataReader
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static void CreateTable(this IDbConnection conn, IDataReader dr, string tableName)
        {
            switch (conn.GetDbType())
            {
                case EDbType.mssql: conn.MsSql_CreateTable(dr, tableName); return;
                case EDbType.mysql: conn.MySql_CreateTable(dr, tableName); return;
                case EDbType.sqlite: conn.Sqlite_CreateTable(dr, tableName); return;
            }

            throw new NotImplementedException($"NotImplementedException from {nameof(CreateTable)} in {nameof(IDbConnection_CreateTable_Extensions)}.cs");

        }
        #endregion
    }
}
