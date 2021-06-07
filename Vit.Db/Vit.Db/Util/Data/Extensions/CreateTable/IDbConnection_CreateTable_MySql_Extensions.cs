using System.Data;
using System.Text;
using System;
using Vit.Db.Util.Data;
using Vit.Extensions.Execute;

namespace Vit.Extensions
{
    public static partial class IDbConnection_CreateTable_MySql_Extensions
    {


        #region CreateTable        
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dt"></param>
        public static void MySql_CreateTable(this IDbConnection conn, DataTable dt)
        {

            if (null == dt || dt.Columns.Count == 0) return;

            //创建表结构的SQL语句

            StringBuilder sql = new StringBuilder("Create Table ").Append(conn.MySql_Quote(dt.TableName)).Append(" (");


            foreach (DataColumn dc in dt.Columns)
            {
                string columnName = dc.ColumnName;
                Type type = dc.DataType;

                sql.Append(conn.MySql_Quote(columnName)).Append(" ").Append(ClrType_To_DbType(type)).Append(",");
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
        public static void MySql_CreateTable(this IDbConnection conn, IDataReader dr, string tableName)
        {

            if (null == dr || dr.FieldCount == 0) return;

            //创建表结构的SQL语句
            StringBuilder sql = new StringBuilder("Create Table ").Append(conn.MySql_Quote(tableName)).Append(" (");

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
                sql.Append(conn.MySql_Quote(columnName)).Append(" ").Append(ClrType_To_DbType(type)).Append(",");
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
                return "int";
            }
            if (type == typeof(long))
            {
                return "bigint";
            }

            if (type == typeof(float))
            {
                return "float";
            }
            if (type == typeof(double))
            {
                return "double";
            }

            if (type == typeof(DateTime))
            {
                return "datetime";
            }

            if (type == typeof(string))
            {
                return "text";
            }

            return "text";
        }
        #endregion
    }


}
