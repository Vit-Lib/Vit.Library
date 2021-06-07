
using Dapper.Contrib.Extensions;
using System;
using System.Data;
using Vit.Db.Util.Data;

namespace Vit.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class IDbConnection_UpdateByDapperExtensions
    {

        /// <summary>
        /// (Vit.Orm.Dapper)更新表，若更新失败则返回null
        /// </summary>
        /// <typeparam name="DbModel"></typeparam>
        /// <param name="conn"></param>
        /// <param name="keyValue"></param>
        /// <param name="howToChangeData"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static DbModel Update<DbModel>(this IDbConnection conn, object keyValue, Action<DbModel> howToChangeData) where DbModel : class
        {
            var dbData = conn.Get<DbModel>(keyValue, commandTimeout: ConnectionFactory.CommandTimeout);
            if (null == dbData) return null;

            howToChangeData(dbData);

            return conn.Update(dbData, commandTimeout: ConnectionFactory.CommandTimeout) ? dbData : null;
        }



        /// <summary>
        /// (Vit.Orm.Dapper)更新表，若更新失败则返回null
        /// </summary>
        /// <typeparam name="DbModel"></typeparam>
        /// <typeparam name="UserModel"></typeparam>
        /// <param name="conn"></param>
        /// <param name="keyValue"></param>
        /// <param name="userData"></param>
        /// <param name="howToChangeData"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static DbModel Update<DbModel, UserModel>(this IDbConnection conn,object keyValue, UserModel userData, Action<DbModel, UserModel> howToChangeData) where DbModel : class
        {
            var dbData = conn.Get<DbModel>(keyValue, commandTimeout: ConnectionFactory.CommandTimeout);
            if (null == dbData) return null;

            howToChangeData(dbData, userData);

            return conn.Update(dbData, commandTimeout: ConnectionFactory.CommandTimeout) ? dbData : null;
        }


      







    }
}
