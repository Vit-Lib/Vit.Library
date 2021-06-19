using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace Vit.Extensions
{

    public static partial class IDbConnection_MakeSureOpen_Extensions
    {

        #region TryOpen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryOpen(this IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }
        #endregion


        #region MakeSureOpen



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ret MakeSureOpen<Conn, Ret>(this Conn conn, Func<Conn, Ret> action)
           where Conn : IDbConnection
        {
            if (conn.State == ConnectionState.Open)
            {
                return action(conn);
            }

            try
            {
                conn.Open();
                return action(conn);
            }
            finally
            {
                conn.Close();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ret MakeSureOpen<Ret>(this IDbConnection conn, Func<Ret> action)
        {
            if (conn.State == ConnectionState.Open)
            {
                return action();
            }

            try
            {
                conn.Open();
                return action();
            }
            finally
            {
                conn.Close();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeSureOpen<Conn>(this Conn conn, Action<Conn> action)
            where Conn : IDbConnection
        {
            if (conn.State == ConnectionState.Open)
            {
                action(conn);
                return;
            }

            try
            {
                conn.Open();
                action(conn);
            }
            finally
            {
                conn.Close();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MakeSureOpen(this IDbConnection conn, Action action)
        {
            if (conn.State == ConnectionState.Open)
            {
                action();
                return;
            }

            try
            {
                conn.Open();
                action();
            }
            finally
            {
                conn.Close();
            }
        }
        #endregion
    }
}
