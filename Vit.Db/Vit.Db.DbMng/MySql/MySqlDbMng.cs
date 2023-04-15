using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Vit.Core.Util.Common;
using Vit.Extensions;
using Vit.Extensions.Linq_Extensions;

using SqlConnection = MySql.Data.MySqlClient.MySqlConnection;

namespace Vit.Db.DbMng
{
    public class MySqlDbMng : BaseDbMng<SqlConnection>
    {


        #region 构造函数
        /// <summary>
        ///
        /// </summary>
        public MySqlDbMng(SqlConnection conn, string BackupPath = null):base(conn)
        {
     
            oriConnectionString = conn.ConnectionString;

            if (string.IsNullOrWhiteSpace(BackupPath))
            {
                BackupPath = CommonHelp.GetAbsPath("Data", "MySqlBackup");
            }

            this.BackupPath = BackupPath;

            dbName = conn.MySql_GetDbName();
        }
        #endregion


        #region 成员变量
    
        string oriConnectionString;

        /// <summary>
        /// 数据库名称
        /// </summary>
        private string dbName { get; set; } = null;

        #endregion




        #region 备份文件夹


        /// <summary>
        /// 数据库备份文件的文件夹路径。例：@"F:\\db"
        /// </summary>
        private string BackupPath { get; set; }


        #region BackupFile_GetPathByName
        public string BackupFile_GetPathByName(string fileName)
        {
            return Path.Combine(BackupPath, fileName);
        }
        #endregion      


        #region BackupFile_GetFileInfos

        /// <summary>
        /// <para>获取所有备份文件的信息</para>
        /// <para>返回的DataTable的列分别为 Name（包含后缀）、Remark、Size</para>
        /// </summary>
        /// <returns></returns>
        public List<BackupFileInfo> BackupFile_GetFileInfos()
        {
            DirectoryInfo bakDirectory = new DirectoryInfo(BackupPath);
            if (!bakDirectory.Exists)
            {
                return new List<BackupFileInfo>();
            }
            return bakDirectory.GetFiles().Select(f => new BackupFileInfo { fileName = f.Name, size = f.Length / 1024.0f / 1024.0f, createTime = f.CreationTime }).ToList();
        }


        #endregion


        #endregion



        #region Exec
        public T Exec<T>(Func<IDbConnection, T> run)
        {
            try
            {
                var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(oriConnectionString);
                builder.Database = "";
                conn.ConnectionString = builder.ToString();
                return run(conn);
            }
            finally
            {
                conn.ConnectionString = oriConnectionString;
            }
        }
        #endregion


        #region Quote
        protected override string Quote(string name) 
        {
            return conn.Quote(name);
        }
        #endregion


        #region GetAllDataBase 获取所有数据库
        /// <summary>
        /// 获取所有数据库
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllDataBase()
        {
            return Exec((conn) =>
            {
                return conn.ExecuteDataSet("show databases").Tables[0];
            });
        }
        #endregion


        #region GetDataBaseState 获取数据库状态
        /// <summary>
        /// 获取数据库状态(online、none、unknow)
        /// </summary>
        /// <returns></returns>
        public override EDataBaseState GetDataBaseState()
        {
            try
            {
                if (null != Exec(conn => conn.ExecuteScalar("show databases like @dbName", new { dbName = dbName })))
                {
                    return EDataBaseState.online;
                }
            }
            catch
            {
                return EDataBaseState.unknow;
            }

            return EDataBaseState.none;
        }


        #endregion


        #region 获取数据库当前连接数 

        /// <summary>
        /// 获取数据库当前连接数
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        public int GetProcessCount(string dbName = null)
        {
            // show full processlist  ;
            // select id, db, user, host, command, time, state, info	from information_schema.processlist	order by time desc 
            return Convert.ToInt32(Exec(conn => conn.ExecuteScalar("select count(*)	from information_schema.processlist	where db=@dbName", new { dbName = dbName ?? this.dbName })));
        }

        #endregion

               
        #region GetDataBaseVersion      
        public override string GetDataBaseVersion()
        {
            try
            {
                return conn.ExecuteScalar<string>("select version()");
            }
            catch
            {
            }
            return null;
        }
        #endregion


        #region CreateDataBase 创建数据库       

        /// <summary>
        /// 创建数据库
        /// </summary>
        public override void CreateDataBase()
        {
            Exec(conn => conn.Execute("create database " + Quote(dbName)));
        }
        #endregion


        #region DropDataBase 删除数据库       

        /// <summary>
        /// 删除数据库
        /// </summary>
        public override void DropDataBase()
        {
            Exec(conn => conn.Execute("drop database " + Quote(dbName)));
        }

        #endregion



        #region BuildCreateDataBaseSql

        /// <summary>
        /// 构建建库语句
        /// </summary>
        /// <returns></returns>
        public override string BuildCreateDataBaseSql()
        {

            // show命令可以提供关于数据库、表、列，或关于服务器的状态信息
            // https://www.cnblogs.com/Rohn/p/11072228.html
            StringBuilder builder = new StringBuilder();

            string dbName = conn.ExecuteScalar("select database()") as string;

            string delimiter = "/*GO*/";

            #region (x.1)构建标头  备份时间、数据库版本、数据库名称 
            {
                builder.AppendLine("-- (x.1)备份信息");
                builder.AppendLine("-- 备份时间  ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                builder.AppendLine("-- MySql版本 ：" + GetDataBaseVersion());
                builder.AppendLine("-- 数据库名称：" + dbName);

                builder.AppendLine("-- DELIMITER " + delimiter);
            }
            #endregion


            #region (x.x)初始化环境
            builder.AppendLine();
            
            builder.AppendLine("-- 关闭外键检查,避免建表时因外键约束而导致建表失败（建表为随机顺序）");
            builder.AppendLine("SET FOREIGN_KEY_CHECKS = 0;");
            builder.AppendLine();
            #endregion


            #region (x.2)建表
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("-- (x.2)建表");

                var names = conn.MySql_GetAllTableName();
                int index = 0;
                foreach (var name in names)
                {
                    builder.AppendLine("  -- (x.x." + (++index) + ")创建表 " + name);

                    #region(x.x.x.1)建表语句
                    {
                        var dt = conn.ExecuteDataTable("SHOW CREATE table " + Quote(name));
                        string sql = dt.Rows[0][1] as string;
                        builder.Append(sql).AppendLine(";");
                        builder.AppendLine(delimiter);
                        builder.AppendLine();
                    }
                    #endregion


                    //建表语句已经指定索引，无需再次创建
                    continue;

                    #region(x.x.x.2)创建索引语句
                    {
                        #region const builder
                        string indexBuilderSql = @"SELECT
CONCAT('ALTER TABLE `',TABLE_NAME,'` ', 'ADD ', 
 IF(NON_UNIQUE = 1,
 CASE UPPER(INDEX_TYPE)
 WHEN 'FULLTEXT' THEN 'FULLTEXT INDEX'
 WHEN 'SPATIAL' THEN 'SPATIAL INDEX'
 ELSE CONCAT('INDEX `',
  INDEX_NAME,
  '` USING ',
  INDEX_TYPE
 )
END,
IF(UPPER(INDEX_NAME) = 'PRIMARY',
 CONCAT('PRIMARY KEY USING ',
 INDEX_TYPE
 ),
CONCAT('UNIQUE INDEX `',
 INDEX_NAME,
 '` USING ',
 INDEX_TYPE
)
)
),'(', GROUP_CONCAT(DISTINCT CONCAT('`', COLUMN_NAME, '`') ORDER BY SEQ_IN_INDEX ASC SEPARATOR ', '), ');') AS 'Show_Add_Indexes'
FROM information_schema.STATISTICS
WHERE TABLE_SCHEMA = @dbName and TABLE_NAME=@tableName
--  and UPPER(INDEX_NAME) != 'PRIMARY'       -- 剔除主码
GROUP BY TABLE_NAME, INDEX_NAME
ORDER BY TABLE_NAME ASC, INDEX_NAME ASC;";
                        #endregion


                        var sqlList = conn.Query<string>(indexBuilderSql, new { dbName = dbName, tableName = name }).ToList();
                        foreach (var sql in sqlList)
                        {
                            builder.Append(sql).AppendLine(";");
                            builder.AppendLine(delimiter);
                            builder.AppendLine();
                        }
                    }
                    #endregion

                }
            }
            #endregion


            #region (x.3)创建触发器
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("-- (x.3)创建触发器");
                var names = conn.Query<string>("show TRIGGERS;").ToList();
                var index = 0;
                foreach (var name in names)
                {
                    builder.AppendLine("  -- (x.x." + (++index) + ")创建触发器 " + name);

                    var dt = conn.ExecuteDataTable("SHOW CREATE TRIGGER " + Quote(name));
                    string sql = dt.Rows[0][2] as string;
                    builder.Append(sql).AppendLine(";");
                    builder.AppendLine(delimiter);
                    builder.AppendLine();
                }
            }
            #endregion



            #region (x.4)创建事件
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("-- (x.4)创建事件");
                var dtEvents = conn.ExecuteDataTable("show EVENTs;");
                var index = 0;
                foreach (DataRow row in dtEvents.Rows)
                {
                    if (row["Db"].ToString() != dbName) continue;

                    var name = row["Name"].ToString();
                    var enabled = row["Status"].ToString().ToUpper() == "ENABLED";

                    //(x.x.1)创建事件
                    {
                        builder.AppendLine("  -- (x.x." + (++index) + ")创建事件 " + name);
                        var dt = conn.ExecuteDataTable("SHOW CREATE EVENT " + Quote(name));
                        string sql = dt.Rows[0][3] as string;
                        builder.Append(sql).AppendLine(";");
                        builder.AppendLine(delimiter);
                        builder.AppendLine();
                    }

                    //(x.x.2)启用事件
                    if (enabled)
                    {
                        builder.AppendLine("  -- (x.x." + index + ")启用事件 " + name);
                        string sql = "ALTER EVENT " + Quote(name) + " ON COMPLETION PRESERVE ENABLE";
                        builder.Append(sql).AppendLine(";");
                        builder.AppendLine(delimiter);
                        builder.AppendLine();
                    }
                }
            }
            #endregion


            #region (x.5)创建函数
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("-- (x.5)创建函数");
                var dtName = conn.ExecuteDataTable("show FUNCTION status;");
                var index = 0;
                foreach (DataRow row in dtName.Rows)
                {
                    if (row["Db"].ToString() != dbName) continue;

                    var name = row["Name"].ToString();

                    builder.AppendLine("  -- (x.x." + (++index) + ")创建函数 " + name);

                    var dt = conn.ExecuteDataTable("SHOW CREATE FUNCTION " + Quote(name));
                    string sql = dt.Rows[0][2] as string;
                    builder.Append(sql).AppendLine(";");
                    builder.AppendLine(delimiter);
                    builder.AppendLine();
                }
            }
            #endregion
                       

            #region (x.6)创建存储过程
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("-- (x.6)创建存储过程");

                var dtName = conn.ExecuteDataTable("show procedure status WHERE Db = @dbName AND `Type` = 'PROCEDURE'", new Dictionary<string,object> { ["dbName"] = dbName });


                var index = 0;
                foreach (DataRow row in dtName.Rows)
                {
                    var name = row["Name"].ToString();

                    builder.AppendLine("  -- (x.x." + (++index) + ")创建存储过程 " + name);

                    var dt = conn.ExecuteDataTable("SHOW CREATE procedure " + Quote(name));
                    string sql = dt.Rows[0][2] as string;
                    builder.Append(sql).AppendLine(";");
                    builder.AppendLine(delimiter);
                    builder.AppendLine();
                }
            }
            #endregion


            #region (x.7)创建视图                 
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("-- (x.7)创建视图");
                //var dtName = conn.ExecuteDataTable("SELECT TABLE_NAME as Name from information_schema.VIEWS;");
                var dtName = conn.ExecuteDataTable("SELECT TABLE_NAME as Name from information_schema.VIEWS where Table_Schema=@dbName", new Dictionary<string, object> { ["dbName"] = dbName });
                var index = 0;
                foreach (DataRow row in dtName.Rows)
                {
                    var name = row["Name"].ToString();

                    builder.AppendLine("  -- (x.x." + (++index) + ")创建存储过程 " + name);

                    var dt = conn.ExecuteDataTable("SHOW CREATE view " + Quote(name) );
                    string sql = dt.Rows[0][1] as string;
                    builder.Append(sql).AppendLine(";");
                    builder.AppendLine(delimiter);
                    builder.AppendLine();
                }
            }
            #endregion


            //builder.AppendLine("-- DELIMITER ;");


            #region (x.x)初始化环境
            builder.AppendLine();
            builder.AppendLine("SET FOREIGN_KEY_CHECKS = 1;");
            builder.AppendLine();
            #endregion

            return builder.ToString();
        }

        #endregion



        #region SqlerBackuper
        //无需做 额外操作，mysql批量导入会处理索引问题，手动停用索引反而让效率变低
        
        /*
        /// <summary>
        /// 批量导入表数据（可以通过先停用索引，在导入数据后再启用来提高效率）
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        /// <param name="tableRowCount"></param>
        /// <returns></returns>
        protected override int BulkImport(IDataReader dr, string tableName, int tableRowCount)
        {
            try
            {
                conn.Execute("ALTER TABLE " + conn.Quote(tableName) + " DISABLE KEYS;", commandTimeout: commandTimeout);

                return base.BulkImport(dr, tableName, tableRowCount);
            }
            finally
            {
                try
                {
                    conn.Execute("ALTER TABLE " + conn.Quote(tableName) + " ENABLE KEYS;", commandTimeout: commandTimeout);
                }
                catch (Exception ex)
                {
                    Core.Module.Log.Logger.Error(ex);
                }
            }
        }
        //*/
        #endregion


        #region BackupSqler
        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="filePath">备份的文件路径，若不指定则自动构建。demo:@"F:\\website\appdata\dbname_2020-02-02_121212.bak"</param>
        /// <param name="useMemoryCache">是否使用内存进行全量缓存，默认:true。缓存到内存可以加快备份速度。在数据源特别庞大时请禁用此功能。</param>
        /// <returns>备份的文件路径</returns>
        public override string BackupSqler(string filePath = null, bool useMemoryCache = true)
        {
            if (string.IsNullOrEmpty(filePath)) filePath = Path.Combine(BackupPath, GenerateBackupFileName(dbName, ".sqler.zip"));

            base.BackupSqler(filePath, useMemoryCache: useMemoryCache);
            return filePath;
        }


        #endregion

        #region GenerateBackupFileName
        static string GenerateBackupFileName(string dbName, string fileExtension = ".zip")
        {
            // dbname_2010-02-02_121212.bak
            return dbName + "_" + DateTime.Now.ToString("yyyy-MM-dd") + "_" + DateTime.Now.ToString("HHmmss") + fileExtension;
        }
        #endregion

  


        #region Restore

        /// <summary>
        /// 远程还原数据库   
        /// </summary>
        /// <param name="filePath">数据库备份文件的路径</param>
        /// <returns>备份文件的路径</returns>
        public void Restore(string filePath)
        {
            RestoreSqler(filePath);     
        }
        #endregion


      
                                 


    }
}
