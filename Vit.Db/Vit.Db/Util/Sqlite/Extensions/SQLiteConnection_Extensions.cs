using Vit.Db.Data.Extensions.Execute;
 

namespace Vit.Extensions
{
    public static partial class SQLiteConnection_Extensions
    {

        #region BackupTo
        public static void BackupTo(this System.Data.SQLite.SQLiteConnection sourceConn, System.Data.SQLite.SQLiteConnection destinationConn)
        {
            sourceConn.BackupDatabase(destinationConn, "main", "main", -1, null, 0);
        }
        #endregion


        #region Vacuum
        /// <summary>
        /// SQLite 的自带命令 VACUUM。用来重新整理整个数据库达到紧凑之用，比如把删除的彻底删掉等等。
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        public static void Vacuum(this System.Data.SQLite.SQLiteConnection conn, int? commandTimeout = null)
        {      
            conn.Execute("VACUUM",commandTimeout: commandTimeout);
        }
        #endregion

    }


}
