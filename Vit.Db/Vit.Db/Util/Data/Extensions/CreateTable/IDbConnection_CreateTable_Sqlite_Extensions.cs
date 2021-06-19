using System;
using System.Data;
using System.Text;
using Vit.Db.Util.Data;
using Vit.Extensions.Execute;

namespace Vit.Extensions
{
    public static partial class IDbConnection_CreateTable_Sqlite_Extensions
    {


        #region CreateTable by DataTable
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public static void Sqlite_CreateTable(this IDbConnection conn, DataTable dt)
        {

            if (null == dt || dt.Columns.Count == 0) return;

            //创建表结构的SQL语句

            // CREATE TABEL IF NOT EXISTS ，一般情况下用这句比较好，如果原来就有同名的表，没有这句就会出错
            StringBuilder sql = new StringBuilder("Create Table IF NOT EXISTS ").Append(conn.Sqlite_Quote(dt.TableName)).Append(" (");


            foreach (DataColumn dc in dt.Columns)
            {
                string columnName = dc.ColumnName;
                Type type = dc.DataType;

                sql.Append(conn.Sqlite_Quote(columnName)).Append(" ").Append(ClrType_To_DbType(type)).Append(",");
            }
            sql.Length--;
            sql.Append(")");
            conn.Execute(sql.ToString(), commandTimeout: ConnectionFactory.CommandTimeout);
        }


        #endregion


        #region CreateTable by DataReader
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        public static void Sqlite_CreateTable(this IDbConnection conn, IDataReader dr, string tableName)
        {

            if (null == dr || dr.FieldCount == 0) return;

            //创建表结构的SQL语句

            // CREATE TABEL IF NOT EXISTS ，一般情况下用这句比较好，如果原来就有同名的表，没有这句就会出错
            StringBuilder sql = new StringBuilder("Create Table IF NOT EXISTS ").Append(conn.Sqlite_Quote(tableName)).Append(" (");

            for (int index = 0; index < dr.FieldCount; index++)
            {
                string columnName = dr.GetName(index);
                Type type;
                if (dr.IsDateTime(index))
                {
                    type = typeof(DateTime);
                }
                else
                {
                    type = dr.GetFieldType(index);
                }
                sql.Append(conn.Sqlite_Quote(columnName)).Append(" ").Append(ClrType_To_DbType(type)).Append(",");
            }
            sql.Length--;
            sql.Append(")");
            conn.Execute(sql.ToString(), commandTimeout: ConnectionFactory.CommandTimeout);
        }
        #endregion

        #region ClrType_To_DbType
        static string ClrType_To_DbType(Type type)
        {
            if (type == typeof(int))
            {
                return "INT";
            }
            if (type == typeof(long))
            {
                return "INT64";
            }

            if (type == typeof(float))
            {
                return "FLOAT";
            }
            if (type == typeof(double))
            {
                return "DOUBLE";
            }

            if (type == typeof(DateTime))
            {
                //return "DATETIME";
                return "DATETEXT";
            }
            if (type == typeof(bool))
            {
                return "BOOL";
            }

            if (type == typeof(string))
            {
                return "TEXT";
            }

            return "TEXT";
        }
        #endregion





    }


}
