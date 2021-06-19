using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Vit.Extensions.Execute
{
    public static partial class IDbConnection_Execute_Extensions
    {    


        #region (Lith Framework)

        /// <summary>
        /// (Lith Framework)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Execute(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
                        var p = cmd.CreateParameter();
                        p.ParameterName = entry.Key;
                        p.Value = entry.Value;
                        cmd.Parameters.Add(p);
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object ExecuteScalar(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
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
                        var p = cmd.CreateParameter();
                        p.ParameterName = entry.Key;
                        p.Value = entry.Value;
                        cmd.Parameters.Add(p);
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
        public static T ExecuteScalar<T>(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            return ExecuteScalar(conn, sql, param: param, commandTimeout: commandTimeout).Convert<T>();
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDataReader ExecuteReader(this IDbConnection conn, string sql, IDictionary<string, object> param = null, int? commandTimeout = null)
        {
            var needDisposeCmd = true;
            var cmd = conn.CreateCommand();
            try
            {
                if (conn is System.ComponentModel.Component com)
                {
                    com.Disposed += (sender, e) =>
                    {
                        cmd.Dispose();
                    };
                    needDisposeCmd = false;
                }

                if (commandTimeout.HasValue)
                    cmd.CommandTimeout = commandTimeout.Value;

                cmd.Connection = conn;
                cmd.CommandText = sql;

                #region param
                if (param != null)
                {
                    foreach (var entry in param)
                    {
                        var p = cmd.CreateParameter();
                        p.ParameterName = entry.Key;
                        p.Value = entry.Value;
                        cmd.Parameters.Add(p);
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
            finally
            {
                if (needDisposeCmd) cmd.Dispose();
            }
        }

        #endregion


    }
}
