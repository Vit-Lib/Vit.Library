using Dapper;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Vit.Core.Module.Log;
using Vit.Db.DbMng.SqlerFile;
using Vit.Db.Util.Data;
using Vit.Db.Util.Sqlite;
using Vit.Extensions;
using Vit.Extensions.Linq_Extensions;
using Vit.Extensions.Json_Extensions;

namespace Vit.Db.DbMng
{
    public abstract class BaseDbMng<DbConnection>
        where DbConnection : IDbConnection
    {

        protected DbConnection conn;

        public BaseDbMng(DbConnection conn) 
        {
            this.conn = conn; 
        }

        public static int commandTimeout = 0;
        //public static int? commandTimeout => Vit.Db.Util.Data.ConnectionFactory.CommandTimeout;

        /// <summary>
        /// 获取数据库状态
        /// </summary>
        /// <returns></returns>
        public abstract EDataBaseState GetDataBaseState();

        public abstract string GetDataBaseVersion(); 

        /// <summary>
        /// 创建数据库
        /// </summary>
        public abstract void CreateDataBase();
 

        /// <summary>
        /// 删除数据库
        /// </summary>
        public abstract void DropDataBase();


        /// <summary>
        /// 数据库是否存在
        /// </summary>
        /// <returns></returns>
        public virtual bool DataBaseIsOnline()
        {
            return GetDataBaseState() == EDataBaseState.online;
        }




        /// <summary>
        /// 构建建库语句
        /// </summary>
        /// <returns></returns>
        public abstract string BuildCreateDataBaseSql();


        protected abstract string Quote(string name);

        protected virtual void Log(string msg) 
        {
            Logger.Info(msg);
        }



        /// <summary>
        /// 批量导入表数据（可以通过先停用索引，在导入数据后再启用来提高效率）
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="tableName"></param>
        /// <param name="tableRowCount"></param>
        /// <returns></returns>
        protected virtual int BulkImport(IDataReader dr, string tableName,int tableRowCount) 
        {
            int index = 0;
            return conn.BulkImport(dr, tableName
                , onProcess: (cur, sum) =>
                {
                    index++;
                    var process = (((float)sum) / tableRowCount * 100).ToString("f2");
                    Log($"           {index}.[{process}%] {sum }/{tableRowCount}");
                }
                , useTransaction: true, commandTimeout: commandTimeout);
        }




        #region BackupSqler
        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="filePath">备份的文件路径。demo:@"F:\\website\appdata\dbname_2020-02-02_121212.bak"</param>
        /// <param name="useMemoryCache">是否使用内存进行全量缓存，默认:true。缓存到内存可以加快备份速度。在数据源特别庞大时请禁用此功能。</param>
        /// <returns>备份的文件路径</returns>
        public virtual string BackupSqler(string filePath,bool useMemoryCache = true)
        {
            var tempPath = filePath + "_Temp"; 

            try
            {
                var startTime = DateTime.Now;
                var lastTime = DateTime.Now;
                TimeSpan span;

                Log("");
                Log("[BackupSqler]start backup");

                //创建临时文件夹
                Directory.CreateDirectory(tempPath);


                #region (x.1)创建(info.json)
                Log("");
                Log(" --(x.1)创建(info.json)");
                var backupInfo = new BackupInfo();
                backupInfo.type = conn.GetDbType()?.ToString();
                backupInfo.version = GetDataBaseVersion();
                backupInfo.backupTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                backupInfo.cmd = BackupInfo.defaultCmd;
                File.WriteAllText(
                    Path.Combine(tempPath, "info.json"),
                    backupInfo.Serialize(),
                    System.Text.Encoding.UTF8);
                #endregion



                #region (x.2)创建建库语句文件（CreateDataBase.sql）
                Log("");
                Log(" --(x.2)创建建库语句文件（CreateDataBase.sql）");
                var sqlText = BuildCreateDataBaseSql();
                var sqlPath = Path.Combine(tempPath, "CreateDataBase.sql");          
                File.WriteAllText(sqlPath, sqlText, System.Text.Encoding.UTF8);

                Log("     成功");
                span = (DateTime.Now - lastTime);
                Log($"     当前耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                span = (DateTime.Now - startTime);
                Log($"     总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                lastTime = DateTime.Now;
                #endregion



                #region (x.3)备份所有表数据（Data.sqlite3）
                Log("");
                Log(" --(x.3)备份所有表数据（Data.sqlite3）");

                int sumRowCount = 0;
                int sumTableCount;
                string sqlitePath = Path.Combine(tempPath, "Data.sqlite3");

                using (var connSqlite = ConnectionFactory.Sqlite_GetOpenConnectionByFilePath(useMemoryCache ? null : sqlitePath))
                using (new SQLiteBackup(connSqlite, filePath: useMemoryCache ? sqlitePath : null))
                {
                    var tableNames = conn.GetAllTableName();
                    sumTableCount = tableNames.Count;

                    int tbIndex = 0;

                    foreach (var tableName in tableNames)
                    {
                        tbIndex++;

                        int tableRowCount = conn.ExecuteScalar<int>("select count(*) from " + conn.Quote(tableName), commandTimeout: commandTimeout);


                        Log("");
                        Log($" ----[{tbIndex}/{sumTableCount}]backup table " + tableName);
                        Log($"         rowCount: " + tableRowCount);

                        int rowCount =0;

                        bool ignoreTable = false;
           

                        //若表数据条数为0则跳过                   
                        ignoreTable = 0 == tableRowCount;

                        if (!ignoreTable)
                        {
                            using (IDataReader dr = conn.ExecuteReader("select * from " + Quote(tableName), commandTimeout: commandTimeout))
                            {
                                connSqlite.Sqlite_CreateTable(dr, tableName);
                                int index = 0;
                                rowCount = connSqlite.Import(dr, tableName
                                    , onProcess: (cur,sum) =>
                                    {
                                        index++;
                                        var process = (((float)sum) / tableRowCount * 100).ToString("f2");
                                        Log($"           {index}.[{process}%] {sum }/{tableRowCount}");
                                    }
                                    , useTransaction: true, commandTimeout: commandTimeout);
                            }
                        }
                        sumRowCount += rowCount;
                        Log($"      table backuped. cur: " + rowCount + "  sum: " + sumRowCount);
                    }
                }

                Log("");
                Log("     成功,sum table count: " + sumTableCount + ",sum row count: " + sumRowCount);
                span = (DateTime.Now - lastTime);
                Log($"     当前耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                span = (DateTime.Now - startTime);
                Log($"     总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                lastTime = DateTime.Now;

                #endregion

          

                #region (x.4)压缩备份文件
                Log("");
                Log(" --(x.4)压缩备份文件");

                //待压缩文件夹
                string input = tempPath;
                //压缩后文件名
                string output = filePath;

                var archiveType = ArchiveType.Zip;
                var compressionType = SharpCompress.Common.CompressionType.Deflate;

                var writerOptions = new WriterOptions(compressionType);
                writerOptions.ArchiveEncoding.Default = System.Text.Encoding.GetEncoding("utf-8");

                using (var fileStream = File.OpenWrite(output))
                using (var writer = WriterFactory.Open(fileStream, archiveType, writerOptions))
                {
                    writer.WriteAll(input, "*", SearchOption.AllDirectories);
                }

                Log("     成功");
                span = (DateTime.Now - lastTime);
                Log($"     当前耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                span = (DateTime.Now - startTime);
                Log($"     总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                lastTime = DateTime.Now;
                #endregion
                

                Log("");
                Log("   backup success, table count: "+ sumTableCount + ",  row count: " + sumRowCount);
                span = (DateTime.Now - startTime);
                Log($"   总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                Log("");

            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return filePath;
        }

        #endregion



        #region RestoreSqler

        protected virtual Regex RestoreSqler_SqlSplit => null;
        

        /// <summary>
        /// 远程还原数据库   
        /// </summary>
        /// <param name="filePath">数据库备份文件的路径</param>
        /// <returns>备份文件的路径</returns>
        public virtual string RestoreSqler(string filePath)
        {
            var tempPath = filePath + "_Temp";

            try
            {
                var startTime = DateTime.Now;
                var lastTime = DateTime.Now;
                TimeSpan span;

                Log("");
                Log("[RestoreSqler]start restore");

                //(x.1)若数据库存在，则删除数据库
                Log("");
                Log(" --(x.1)若数据库存在，则删除数据库");
                if (DataBaseIsOnline())
                {
                    Log("     数据库存在，删除数据库");
                    DropDataBase();
                }
                else
                {
                    Log("     数据库不存在，无需删除");
                }

                //(x.2)创建数据库
                Log("");
                Log(" --(x.2)创建数据库");
                CreateDataBase();


                #region (x.3)解压备份文件到临时文件夹
                Log("");
                Log(" --(x.3)解压备份文件到临时文件夹");

                //创建临时文件夹
                Directory.CreateDirectory(tempPath);

                //待解压文件
                var input = filePath;
                var output = tempPath;
                using (var archive = ArchiveFactory.Open(input))
                {
                    foreach (var entry in archive.Entries)
                    {
                        entry.WriteToDirectory(output, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }

                Log("");
                Log("     成功");
                span = (DateTime.Now - lastTime);
                Log($"     当前耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                span = (DateTime.Now - startTime);
                Log($"     总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                lastTime = DateTime.Now;
                #endregion


                #region (x.4)获取备份文件信息
                Log("");
                Log(" --(x.4)获取备份文件信息");
                SqlerBackuper backuper = new SqlerBackuper();
                backuper.dirPath = tempPath;
                backuper.conn = conn;
                backuper.Log = Log;
                backuper.BulkImport = this.BulkImport;

                backuper.sqlSplit = RestoreSqler_SqlSplit;

                backuper.ReadBackupInfo();
                #endregion


                #region (x.5)执行命令
                Log("");
                Log(" --(x.5)执行命令");
                int t = 0;
                foreach (var cmd in backuper.backupInfo.cmd) 
                {
                    t++;

                    Log("");
                    Log(" ----(x.x." + t + ")执行命令");
                    Log("    " + cmd.Serialize());
                    backuper.ExecCmd(cmd, commandTimeout);

              
                    span = (DateTime.Now - lastTime);
                    Log($"     当前耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                    span = (DateTime.Now - startTime);
                    Log($"     总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                    lastTime = DateTime.Now;
                }
                #endregion
            

                Log(""); 
                Log("   restore success, table count: " + backuper.importedTableCount + ",row count: " + backuper.importedRowCount);
                span = (DateTime.Now - startTime);
                Log($"   总共耗时:{span.Hours}小时{span.Minutes}分{span.Seconds}秒{span.Milliseconds}毫秒");
                Log("");
            }
            finally
            {
                try
                {
                    if (Directory.Exists(tempPath))
                    {
                        Directory.Delete(tempPath, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            return filePath;

        }
        #endregion
    }
}
