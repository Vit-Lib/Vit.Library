using System.Collections.Generic;
using System.Data;
using SqlConnection = Microsoft.Data.Sqlite.SqliteConnection;
using SqlCommand = Microsoft.Data.Sqlite.SqliteCommand;
//using SqlDataAdapter = Microsoft.Data.Sqlite.SQLiteDataAdapter;
using SqlDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using System.Runtime.CompilerServices;
using System;
using Vit.Extensions.Json_Extensions;

namespace Vit.Extensions.Linq_Extensions.Execute
{
    public static partial class Sqlite_Execute_Extensions
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
        public static DataSet Sqlite_ExecuteDataSet(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            //1
            using (var reader = conn.ExecuteReader(sql, param, commandTimeout))
            {
                return reader.ReadDataSet();
            }


            /*
            //2
            using (var cmd = new SqlCommand())
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
            //*/

        }
        #endregion



        #region (Lith Framework)

        ///*


        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        public static int Sqlite_Execute(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
        public static object Sqlite_ExecuteScalar(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
        public static T Sqlite_ExecuteScalar<T>(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            return Sqlite_ExecuteScalar(conn, sql, param: param, commandTimeout: commandTimeout).Convert<T>();
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
        public static SqlDataReader Sqlite_ExecuteReader(this SqlConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            //using (var cmd = conn.CreateCommand())
            var cmd = conn.CreateCommand();
            {
                conn.Disposed += (sender, e) =>
                {
                    cmd.Dispose();
                };


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
