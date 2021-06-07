using System;
using Microsoft.Data.Sqlite;
using Vit.Db.Util.Data;
using Vit.Extensions;

namespace Vit.Db.Util.Sqlite
{
    /// <summary>
    /// 
    /// 关闭连接前拷贝数据库：
    /// <para> //法1：                                                                                                          </para>
    /// <para> bool userMemoryCache = true;                                                                                     </para> 
    /// <para> using (var connSqlite = ConnectionFactory.Sqlite_GetOpenConnectionByFilePath(useMemoryCache ? null:sqlitePath))  </para>
    /// <para> using (new SQLiteBackup(connSqlite, filePath:useMemoryCache ? sqlitePath:null))                                  </para>
    /// <para> {                                                                                                                </para>
    /// <para>     //do something with  connSqlite                                                                              </para>
    /// <para> }                                                                                                                </para>
    /// <para>                                                                                                                  </para>
    /// <para> //法2：                                                                                                          </para>
    /// <para> using (var connSqlite = ConnectionFactory.Sqlite_GetOpenConnectionByFilePath())                                  </para>
    /// <para> using (new SQLiteBackup(connSqlite, filePath:sqlitePath))                                                        </para>
    /// <para> {                                                                                                                </para>
    /// <para>     //do something with  connSqlite                                                                              </para>
    /// <para> }                                                                                                                </para>
    /// <para>                                                                                                                  </para>
    /// <para> //法3：                                                                                                          </para>
    /// <para> using (var connSqlite = ConnectionFactory.Sqlite_GetOpenConnectionByFilePath())                                  </para>
    /// <para> using (var connSqliteLocal = ConnectionFactory.Sqlite_GetOpenConnectionByFilePath(sqlitePath))                   </para>
    /// <para> {                                                                                                                </para>
    /// <para>     //do something with  connSqlite                                                                              </para>
    /// <para>     connSqlite.BackupDatabase(connSqliteLocal);                                                                  </para>
    /// <para> }                                                                                                                </para>
    /// 
    /// </summary>
    public class SQLiteBackup : IDisposable
    {

        SqliteConnection sourceConn=null;
        string ConnectionString=null;      
        public SQLiteBackup(SqliteConnection sourceConn,string ConnectionString=null,string filePath=null)
        {
            this.sourceConn = sourceConn;

            if (ConnectionString == null && filePath != null) 
            {
                ConnectionString = ConnectionFactory.Sqlite_GetConnectionString(filePath);
            }

            this.ConnectionString = ConnectionString;
        }
        public void Dispose()
        {
            if (sourceConn == null|| ConnectionString == null)
                return;

            using (var connSqliteLocal = ConnectionFactory.Sqlite_GetOpenConnection(ConnectionString))
            {
                sourceConn.BackupDatabase(connSqliteLocal);
            }
        }
    }
}
