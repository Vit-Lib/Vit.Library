

using System.Runtime.CompilerServices;

using Vit.Extensions.Linq_Extensions.Execute;

namespace Vit.Extensions
{
    public static partial class Sqlite_Extensions
    {

        #region BackupTo
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BackupTo(this Microsoft.Data.Sqlite.SqliteConnection sourceConn, Microsoft.Data.Sqlite.SqliteConnection destinationConn)
        {
            sourceConn.BackupDatabase(destinationConn);
        }
        #endregion


        #region Vacuum
        /// <summary>
        /// SQLite 的自带命令 VACUUM。用来重新整理整个数据库达到紧凑之用，比如把删除的彻底删掉等等。
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Vacuum(this Microsoft.Data.Sqlite.SqliteConnection conn, int? commandTimeout = null)
        {      
            conn.Execute("VACUUM",commandTimeout: commandTimeout);
        }
        #endregion

    }


}
