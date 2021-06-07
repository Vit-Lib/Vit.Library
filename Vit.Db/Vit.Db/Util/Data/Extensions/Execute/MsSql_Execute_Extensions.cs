using System.Collections.Generic;
using System.Data;
using SqlConnection = System.Data.SqlClient.SqlConnection;
using SqlCommand = System.Data.SqlClient.SqlCommand;
using SqlDataAdapter = System.Data.SqlClient.SqlDataAdapter;
using SqlDataReader = System.Data.SqlClient.SqlDataReader;
using System.Runtime.CompilerServices;
using System;

namespace Vit.Extensions.Execute
{
    public static partial class MsSql_Execute_Extensions
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
        public static DataSet MsSql_ExecuteDataSet(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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


        #region  (Lith Framework)
        ///*

        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MsSql_Execute(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
        ///  (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object MsSql_ExecuteScalar(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
        ///  (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MsSql_ExecuteScalar<T>(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            return MsSql_ExecuteScalar(conn, sql, param: param, commandTimeout: commandTimeout).Convert<T>();
        }

        #endregion




        /// <summary>
        ///  (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SqlDataReader MsSql_ExecuteReader(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
