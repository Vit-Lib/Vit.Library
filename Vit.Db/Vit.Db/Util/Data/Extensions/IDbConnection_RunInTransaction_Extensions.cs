using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace Vit.Extensions
{

    public static partial class IDbConnection_RunInTransaction_Extensions
    {

        #region RunInTransaction

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ret RunInTransaction<Conn, Ret>(this Conn conn, Func<Conn, Ret> action, Action<Exception> onException = null)
           where Conn : IDbConnection
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var ret= action(conn);
                    trans.Commit();
                    return ret;
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ret RunInTransaction<Conn, Ret>(this Conn conn, Func<Ret> action, Action<Exception> onException = null)
          where Conn : IDbConnection
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var ret = action();
                    trans.Commit();
                    return ret;
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunInTransaction<Conn>(this Conn conn, Action<Conn> action, Action<Exception> onException = null)
          where Conn : IDbConnection
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    action(conn);
                    trans.Commit();                 
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally 
                    {
                        trans.Rollback();
                    }                  
                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunInTransaction(this IDbConnection conn, Action action, Action<Exception> onException = null)    
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    action();
                    trans.Commit();
                }
                catch(Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }
        #endregion

        #region RunInTransaction with tran

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ret RunInTransaction<Conn, Ret>(this Conn conn, Func<Conn,IDbTransaction, Ret> action, Action<Exception> onException = null)
           where Conn : IDbConnection
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var ret = action(conn, trans);
                    trans.Commit();
                    return ret;
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Ret RunInTransaction<Conn, Ret>(this Conn conn, Func<IDbTransaction, Ret> action, Action<Exception> onException = null)
          where Conn : IDbConnection
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    var ret = action(trans);
                    trans.Commit();
                    return ret;
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunInTransaction<Conn>(this Conn conn, Action<Conn,IDbTransaction> action, Action<Exception> onException = null)
          where Conn : IDbConnection
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    action(conn, trans);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RunInTransaction(this IDbConnection conn, Action<IDbTransaction> action, Action<Exception> onException = null)
        {
            using (var trans = conn.BeginTransaction())
            {
                try
                {
                    action(trans);
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    try
                    {
                        onException?.Invoke(ex);
                    }
                    finally
                    {
                        trans.Rollback();
                    }
                    throw;
                }
            }
        }
        #endregion


    }
}
