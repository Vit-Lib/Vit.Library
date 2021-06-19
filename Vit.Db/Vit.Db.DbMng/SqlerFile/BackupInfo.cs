namespace Vit.Db.DbMng.SqlerFile
{
    class BackupInfo
    {
        /// <summary>
        /// 数据库类型：mssql、mysql
        /// </summary>
        public string type;

        /// <summary>
        /// 数据库版本
        /// </summary>
        public string version;


        /// <summary>
        /// 备份时间
        /// </summary>
        public string backupTime;

        /// <summary>
        /// 数据库还原指令
        /// </summary>
        public string[][] cmd;


        /// <summary>
        /// 拆分sql脚本的字符串（正则）
        /// </summary>
        public string sqlSplit;


        public static string[][] defaultCmd => new[] {
                       new[]{ "execSqlFile", "CreateDataBase.sql"},
                        new[]{ "importSqlite", "Data.sqlite3" }
                    };

    }
}
