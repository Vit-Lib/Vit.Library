using System.Collections.Generic;
using System.Data;
using SqlConnection = MySql.Data.MySqlClient.MySqlConnection;
using SqlCommand = MySql.Data.MySqlClient.MySqlCommand;
using SqlDataAdapter = MySql.Data.MySqlClient.MySqlDataAdapter;
using SqlDataReader= MySql.Data.MySqlClient.MySqlDataReader;
using System.Runtime.CompilerServices;
using System;

namespace Vit.Extensions.Execute
{
    public static partial class MySql_Execute_Extensions
    {


        #region ExecuteDataSet    
        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataSet MySql_ExecuteDataSet(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {            
            using (var cmd = conn.CreateCommand())
            {
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Connection = conn;
                cmd.CommandText = sql;

                #region param
                if (param != null)
                {
                    foreach (var entry in param)
                    {
                        cmd.Parameters.AddWithValue((string)entry.Key, entry.Value ?? DBNull.Value);
                    }
                }
                #endregion

                DataSet ds = new DataSet();

                conn.MakeSureOpen(() =>
                {
                    using (var Adapter = new SqlDataAdapter(cmd))
                    {
                        Adapter.Fill(ds);
                    }
                });

                return ds;
            }


        }
        #endregion



        #region  Lith Framework
        ///*

        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        public static int MySql_Execute(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Connection = conn;
                cmd.CommandText = sql;

                #region param
                if (param != null)
                {
                    foreach (var entry in param)
                    {
                        cmd.Parameters.AddWithValue((string)entry.Key, entry.Value ?? DBNull.Value);
                    }
                }
                #endregion

                return conn.MakeSureOpen(() =>
                {
                    return cmd.ExecuteNonQuery();
                });            
            }
        }



        #region ExecuteScalar      

        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        public static object MySql_ExecuteScalar(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Connection = conn;
                cmd.CommandText = sql;

                #region param
                if (param != null)
                {
                    foreach (var entry in param)
                    {
                        cmd.Parameters.AddWithValue((string)entry.Key, entry.Value ?? DBNull.Value);
                    }
                }
                #endregion

                return conn.MakeSureOpen(() =>
                {
                    return cmd.ExecuteScalar();
                });
            }
        }

        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        public static T MySql_ExecuteScalar<T>(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            return MySql_ExecuteScalar(conn,sql,param: param, commandTimeout:commandTimeout).Convert<T>();
        }

        #endregion



        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        public static SqlDataReader MySql_ExecuteReader(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Connection = conn;
                cmd.CommandText = sql;

                #region param
                if (param != null)
                {
                    foreach (var entry in param)
                    {
                        cmd.Parameters.AddWithValue((string)entry.Key, entry.Value ?? DBNull.Value);
                    }
                }
                #endregion

                #region ExecuteReader
                if (conn.State == ConnectionState.Open)
                {
                    return cmd.ExecuteReader();
                }
                else
                {
                    conn.Open();
                    try
                    {
                        return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    }
                    catch
                    {
                        conn.Close();
                        throw;
                    }
                }
                #endregion
            }
        }

        //*/
        #endregion




    }
}
