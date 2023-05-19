using System.Data;
using Dapper;
using System;
using System.Data.SqlClient; 
using Vit.Core.Module.Log;
using SqlConnection = System.Data.SqlClient.SqlConnection;
using System.IO;
using System.Collections.Generic;
using Vit.Core.Util.MethodExt;
using System.Linq;
using Vit.Extensions.Json_Extensions;

namespace Vit.Extensions.Linq_Extensions
{
    public static partial class IDbConnection_MsSqlExtensions
    {
        /*
 --------------------------------------------------------------------------------------
先读后写

declare @fileContent varbinary(MAX);

select @fileContent=BulkColumn  from OPENROWSET(BULK 'T:\机电合并.zip', SINGLE_BLOB) as content;

if Exists(select top 1 * from sysObjects where Id=OBJECT_ID(N'sqler_temp_filebuffer') and xtype='U')
	drop table sqler_temp_filebuffer;

select @fileContent as fileContent into sqler_temp_filebuffer;


exec master..xp_cmdshell 'bcp "select null union all select ''0'' union all select ''0'' union all select null union all select ''n'' union all select null " queryout "T:\file.fmt" /T /c'



exec master..xp_cmdshell 'BCP "SELECT fileContent FROM sqler_temp_filebuffer" queryout "T:\file.zip" -T -i "T:\file.fmt"'



if Exists(select top 1 * from sysObjects where Id=OBJECT_ID(N'sqler_temp_filebuffer') and xtype='U')
	drop table sqler_temp_filebuffer;             
             
             */

        public static int? commandTimeout = 0;
        //public static int? commandTimeout => Vit.Db.Util.Data.ConnectionFactory.CommandTimeout;

        #region MsSql_ReadFileFromDisk   

        /// <summary>
        /// 读取SqlServer所在服务器中的文件内容，存储到本地。
        /// 若服务器中不存在指定的文件则抛异常。
        /// （文件内容直接存储到文件，可读取超大文件）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="serverFilePath"></param>
        /// <param name="localFilePath"></param>
        /// <returns>读取的文件大小。单位：byte</returns>
        public static int ReadFileFromDisk(this SqlConnection conn, string serverFilePath, string localFilePath)
        {
            // Sql DataReader中读取大字段到文件的方法
            // https://www.cnblogs.com/sundongxiang/archive/2009/09/14/1566443.html

            // select BulkColumn  from OPENROWSET(BULK N'T:\机电合并.zip', SINGLE_BLOB) as content;      

            return conn.MsSql_RunUseMaster((c) =>
             {
                 return c.MakeSureOpen(() =>
                 {
                     var sql = " select BulkColumn  from OPENROWSET(BULK N'" + serverFilePath + "', SINGLE_BLOB) as content";

                     int readedCount = 0;
                     using (var cmd = new SqlCommand())
                     {
                         cmd.Connection = conn;
                         cmd.CommandText = sql;

                         cmd.CommandTimeout = commandTimeout ?? 0;

                         using (var dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                         {
                             if (dr.Read())
                             {
                                 Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

                                 using (var output = new FileStream(localFilePath, FileMode.Create))
                                 {
                                     int bufferSize = 100 * 1024;
                                     byte[] buff = new byte[bufferSize];

                                     while (true)
                                     {
                                         int buffCount = (int)dr.GetBytes(0, readedCount, buff, 0, bufferSize);

                                         output.Write(buff, 0, buffCount);
                                         readedCount += buffCount;

                                         if (buffCount < bufferSize) break;
                                     }
                                 }
                             }
                         }
                     }
                     return readedCount;
                 });
             });
        }


        /// <summary>
        /// 从磁盘读取文件内容(文件内容会先缓存到内存，若读取超大文件，请使用ReadFileFromDisk代替)
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="filePath"></param>
        /// <returns>读取的文件的内容</returns>
        public static byte[] MsSql_ReadFileFromDisk(this IDbConnection conn, string filePath)
        {
            // select BulkColumn  from OPENROWSET(BULK N'T:\机电合并.zip', SINGLE_BLOB) as content;                 

            return conn.MsSql_RunUseMaster((c) =>
            {
                return conn.ExecuteScalar<byte[]>(
                " select BulkColumn  from OPENROWSET(BULK N'" + filePath + "', SINGLE_BLOB) as content"
                , commandTimeout: commandTimeout);
            });
        }
        //*/

        #endregion


        #region MsSql_DeleteFile    
        /// <summary>
        /// 删除服务器文件
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static void MsSql_DeleteFile(this IDbConnection conn, string filePath, Action<string> OnLog = null)
        {
            OnLog?.Invoke("[DbMng]MsSql,删除服务器文件：" + filePath);
            var msg = conn.MsSql_Cmdshell("del \"" + filePath + "\"");
            OnLog?.Invoke(msg);
        }
        #endregion



        #region MsSql_WriteFileToDisk    
        /// <summary>
        /// 写入文件到数据库所在服务器
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="serverFilePath"></param>
        /// <param name="fileContent"></param>
        public static void MsSql_WriteFileToDisk(this IDbConnection conn, string serverFilePath, byte[] fileContent)
        {
            conn.MsSql_Cmdshell(runCmd =>
            {
                string fmtFilePath = serverFilePath + ".MsDbMng.temp.fmt";

                try
                {
                    DataTable cmdResult;


                    //(x.1)创建fmt文件
                    cmdResult = runCmd("bcp \"select null union all select '0' union all select '0' union all select null union all select 'n' union all select null \" queryout \"" + fmtFilePath + "\" /T /c");
                    //Logger.Info("[MsDbMng] 写入文件到磁盘. 创建fmt文件，outlog: " + cmdResult.Serialize());


                    //(x.2)把文件内容写入到临时表
                    conn.Execute(@"
if Exists(select top 1 * from sysObjects where Id=OBJECT_ID(N'sqler_temp_filebuffer') and xtype='U')
	drop table sqler_temp_filebuffer;
select @fileContent as fileContent into sqler_temp_filebuffer;
", new { fileContent }, commandTimeout: commandTimeout);


                    //(x.3)从临时表读取二进制内容到目标文件
                    cmdResult = runCmd("BCP \"SELECT fileContent FROM sqler_temp_filebuffer\" queryout \"" + serverFilePath + "\" -T -i \"" + fmtFilePath + "\"");
                    //Logger.Info("[MsDbMng] 写入文件到磁盘.创建文件，outlog: " + cmdResult.Serialize());


                }
                finally
                {
                    //(x.1)删除fmt文件
                    try
                    {
                        runCmd("del \"" + fmtFilePath + "\"");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    //(x.2)删除临时表
                    try
                    {
                        conn.Execute(@"
if Exists(select top 1 * from sysObjects where Id=OBJECT_ID(N'sqler_temp_filebuffer') and xtype='U')
	drop table sqler_temp_filebuffer;"
                        , commandTimeout: commandTimeout);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

            });

        }
        #endregion


        #region MsSql_WriteFileToDisk    
        /// <summary>
        /// 分片写入文件到数据库所在服务器
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="serverFilePath"></param>
        /// <param name="fileReader"></param>
        /// <param name="sliceByte">每个文件分片的大小（byte）</param>
        /// <returns>文件总大小(byte)</returns>
        public static int MsSql_WriteFileToDisk(this SqlConnection conn, string serverFilePath, BinaryReader fileReader, int sliceByte = 100 * 1024 * 1024)
        {

            int fileSize = 0;

            conn.MsSql_Cmdshell(runCmd =>
            {
                string serverTempFilePath = serverFilePath + ".MsDbMng.temp.tmp";
                string fmtFilePath = serverFilePath + ".MsDbMng.temp.fmt";

                try
                {
                    DataTable cmdResult;

                    // (x.1)创建fmt文件
                    cmdResult = runCmd("bcp \"select null union all select '0' union all select '0' union all select null union all select 'n' union all select null \" queryout \"" + fmtFilePath + "\" /T /c");
                    //Logger.Info("[MsDbMng] 写入文件到磁盘. 创建fmt文件，outlog: " + cmdResult.Serialize());



                    #region (x.2)创建临时表
                    conn.Execute(@"
if Exists(select top 1 * from sysObjects where Id=OBJECT_ID(N'sqler_temp_filebuffer') and xtype='U')
	drop table sqler_temp_filebuffer;
create table sqler_temp_filebuffer (fileContent varbinary(MAX) null);
", commandTimeout: commandTimeout);
                    #endregion


                    #region (x.3)分片写入文件                

                    var fileContent = new byte[sliceByte];
                    while (true)
                    {
                        int readLen = fileReader.Read(fileContent, 0, sliceByte);
                        if (readLen == 0) break;
                        if (readLen < sliceByte)
                        {
                            fileContent = fileContent.AsSpan().Slice(0, readLen).ToArray();
                        }

                        //(x.x.1)把二进制数据写入到临时表
                        conn.Execute(@"
truncate table sqler_temp_filebuffer;
insert into sqler_temp_filebuffer select @fileContent as fileContent;
", new { fileContent = fileContent }, commandTimeout: commandTimeout);

                        // (x.x.2)从临时表读取二进制内容保存到临时文件
                        cmdResult = runCmd("BCP \"SELECT fileContent FROM sqler_temp_filebuffer\" queryout \"" + serverTempFilePath + "\" -T -i \"" + fmtFilePath + "\"");
                        //Logger.Info("[MsDbMng] 写入文件到磁盘.创建文件，outlog: " + cmdResult.Serialize());


                        // (x.x.3)把临时文件内容追加到目标文件
                        cmdResult = runCmd("Type \"" + serverTempFilePath + "\" >> \"" + serverFilePath + "\"");


                        fileSize += readLen;
                        if (readLen < sliceByte) break;
                    }
                    #endregion

                }
                finally
                {
                    //(x.1)删除fmt文件
                    try
                    {
                        runCmd("del \"" + fmtFilePath + "\"");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    //(x.2)删除临时文件
                    try
                    {
                        runCmd("del \"" + serverTempFilePath + "\"");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    //(x.3)删除临时表
                    try
                    {
                        conn.Execute(@"
if Exists(select top 1 * from sysObjects where Id=OBJECT_ID(N'sqler_temp_filebuffer') and xtype='U')
	drop table sqler_temp_filebuffer;"
                        , commandTimeout: commandTimeout);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }

            });

            return fileSize;
        }
        #endregion


        #region MsSql_RunUseMaster
        public static T MsSql_RunUseMaster<T>(this IDbConnection conn, Func<IDbConnection, T> run)
        {
            string oriConnectionString = conn.ConnectionString;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(oriConnectionString);
                builder.InitialCatalog = "";
                conn.ConnectionString = builder.ToString();
                return run(conn);
            }
            finally
            {
                conn.ConnectionString = oriConnectionString;
            }
        }

        public static void MsSql_RunUseMaster(this IDbConnection conn, Action<IDbConnection> run)
        {
            string oriConnectionString = conn.ConnectionString;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(oriConnectionString);
                builder.InitialCatalog = "";
                conn.ConnectionString = builder.ToString();
                run(conn);
            }
            finally
            {
                conn.ConnectionString = oriConnectionString;
            }
        }
        #endregion




        #region MsSql_Cmdshell
        public static string MsSql_Cmdshell(this IDbConnection conn, string cmd)
        {
            DataTable dt = null;
            conn.MsSql_Cmdshell(runCmd => dt = runCmd(cmd));
            return dt.ToString(string.Empty);
        }

        /// <summary>
        /// 执行cmd命令。
        /// 参考：
        ///   https://www.cnblogs.com/feiquan/p/8673093.html
        ///   https://www.freebuf.com/articles/web/55577.html
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="handleToRun"></param>
        public static void MsSql_Cmdshell(this IDbConnection conn, Action<Func<string, DataTable>> handleToRun)
        {
            #region (x.1)method Body           
            Action methodBody = () =>
            {
                Func<string, DataTable> runCmd = (cmd) => conn.ExecuteDataTable("exec master..xp_cmdshell @cmd ", new Dictionary<string, object> { ["cmd"] = cmd });
                handleToRun(runCmd);
            };
            #endregion


            #region (x.2)确保启用cmdshell
            Method.Wrap(ref methodBody, (baseFunc) =>
                {
                    #region (x.x.1)获取是否启用                   
                    bool cmdshellIsEnabled = false;
                    try
                    {
                        cmdshellIsEnabled = conn.ExecuteDataTable("EXEC SP_CONFIGURE 'xp_cmdshell'").Rows[0]["config_value"]?.Convert<string>() != "0";
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                    #endregion

                    #region (x.x.2)若启用则直接执行后续任务                
                    if (cmdshellIsEnabled)
                    {
                        baseFunc();
                        return;
                    }
                    #endregion

                    #region (x.x.3)没有启用，故手动启用，并在返回前手动关闭                  
                    try
                    {
                        conn.Execute(@"
--启用CMD执行命令
EXEC SP_CONFIGURE 'xp_cmdshell', 1;
RECONFIGURE;
", commandTimeout: commandTimeout);

                        baseFunc();
                    }
                    finally
                    {
                        try
                        {
                            conn.Execute(@"
--关闭CMD执行命令
EXEC SP_CONFIGURE 'xp_cmdshell', 0;
RECONFIGURE;
", commandTimeout: commandTimeout);

                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                    #endregion
                }
                );
            #endregion



            #region (x.3)确保启用高级选项
            Method.Wrap(ref methodBody,
            (baseFunc) =>
            {
                #region (x.x.1)获取是否启用                   
                bool advancedOptionsIsOpened = false;
                try
                {
                    advancedOptionsIsOpened = conn.ExecuteDataTable("EXEC SP_CONFIGURE 'show advanced options'").Rows[0]["config_value"]?.Convert<string>() != "0";
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                #endregion

                #region (x.x.2)若启用则直接执行后续任务                
                if (advancedOptionsIsOpened)
                {
                    baseFunc();
                    return;
                }
                #endregion

                #region (x.x.3)没有启用，故手动启用，并在返回前手动关闭                  
                try
                {
                    conn.Execute(@"
--打开高级选项
EXEC SP_CONFIGURE 'show advanced options', 1;
RECONFIGURE;
", commandTimeout: commandTimeout);

                    baseFunc();
                }
                finally
                {
                    try
                    {
                        conn.Execute(@"
--关闭高级选项
EXEC SP_CONFIGURE 'show advanced options', 0;
RECONFIGURE;
", commandTimeout: commandTimeout);

                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                #endregion
            }
            );
            #endregion


            #region (x.4)执行methodBody(并且RunUseMaster)
            conn.MsSql_RunUseMaster((c) =>
            {
                methodBody();
            });
            #endregion
        }
        #endregion

    }


}
