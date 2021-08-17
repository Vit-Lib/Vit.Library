using System;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using Vit.Core.Util.Common;
using Vit.Core.Util.ConfigurationManager;

namespace Vit.Db.Util.Data
{

    public class ConnectionFactory
    {

        #region CommandTimeout
        /// <summary>
        /// 初始值从appsettings.json::Vit.Db.CommandTimeout获取
        /// </summary>
        public static int? CommandTimeout = Vit.Core.Util.ConfigurationManager.ConfigurationManager.Instance.GetByPath<int?>("Vit.Db.CommandTimeout");

        #endregion



        #region ConnectionCreator

        //public static Func<System.Data.IDbConnection> DefaultCreator { get; set; } = GetConnectionCreator("App.Db");


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configPath">在appsettings.json中的路径，默认："App.Db"</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<System.Data.IDbConnection> GetConnectionCreator(string configPath= "App.Db")
        {
            return GetConnectionCreator(ConfigurationManager.Instance.GetByPath<ConnectionInfo>(configPath ?? "App.Db"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<System.Data.IDbConnection> GetConnectionCreator(ConnectionInfo info)
        {
            var ConnectionString = info.ConnectionString;

            if (info.type == "mssql")
            {
                //使用SqlServer数据库
                return () => MsSql_GetConnection(ConnectionString);
            }
            else if (info.type == "mysql")
            {
                //使用mysql数据库
                return () => MySql_GetConnection(ConnectionString);
            }
            else if (info.type == "sqlite")
            {
                //使用sqlite数据库
                return () => Sqlite_GetConnection(ConnectionString);
            }
            return null;
        }
        #endregion

        #region GetConnection
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Data.IDbConnection GetConnection(ConnectionInfo info)
        {    
            switch (info?.type) 
            {
                case "mssql":
                    //使用SqlServer数据库
                    return MsSql_GetConnection(info.ConnectionString);
                case "mysql":
                    //使用mysql数据库
                    return MySql_GetConnection(info.ConnectionString);
                case "sqlite":
                    //使用sqlite数据库
                    return Sqlite_GetConnection(info.ConnectionString);         
            }            
            return null; 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configPath">在appsettings.json中的路径，默认："App.Db"</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Data.IDbConnection GetConnection(string configPath = "App.Db")
        {
            return GetConnection(ConfigurationManager.Instance.GetByPath<ConnectionInfo>(configPath?? "App.Db"));
        }
        #endregion


        #region GetOpenConnection
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Data.IDbConnection GetOpenConnection(ConnectionInfo info)
        {
            var connection = GetConnection(info);
            connection?.Open();
            return connection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configPath">在appsettings.json中的路径，默认："App.Db"</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Data.IDbConnection GetOpenConnection(string configPath = "App.Db")
        {
            var connection = GetConnection(configPath);
            connection?.Open();
            return connection;
        }
        #endregion



        #region MsSql
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SqlConnection MsSql_GetConnection(string ConnectionString)
        {
            return new SqlConnection(ConnectionString);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SqlConnection MsSql_GetOpenConnection(string ConnectionString)
        {
            var connection = MsSql_GetConnection(ConnectionString);
            connection.Open();
            return connection;
        }
        #endregion



        #region MySql

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MySql.Data.MySqlClient.MySqlConnection MySql_GetConnection(string ConnectionString)
        {
            return new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MySql.Data.MySqlClient.MySqlConnection MySql_GetOpenConnection(string ConnectionString)
        {
            var connection = MySql_GetConnection(ConnectionString);
            connection.Open();
            return connection;
        }
        #endregion


        #region Sqlite

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Data.Sqlite.SqliteConnection Sqlite_GetConnection(string ConnectionString)
        {
            
            return new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Data.Sqlite.SqliteConnection Sqlite_GetOpenConnection(string ConnectionString)
        {
            var conn= Sqlite_GetConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        /// <summary>
        /// 若filePath为空则使用内存
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="CacheSize"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sqlite_GetConnectionString(string filePath = null, int? CacheSize = null)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = ":memory:";
            }
            else
            {
                filePath = CommonHelp.GetAbsPath(filePath);
            }

            // https://docs.microsoft.com/zh-cn/dotnet/standard/data/sqlite/compare#connection-strings
            // https://www.cnblogs.com/zeroone/archive/2012/12/16/2820719.html

            //var connectionStringBuilder = new System.Data.SQLite.SQLiteConnectionStringBuilder();
            //connectionStringBuilder.DataSource = filePath;
            //if (Version.HasValue) connectionStringBuilder.Version = Version.Value;
            //if (CacheSize.HasValue) connectionStringBuilder.CacheSize = CacheSize.Value;

            var connectionStringBuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = filePath;
            //if (Version.HasValue) connectionStringBuilder.Add("Version", Version.Value);
            if (CacheSize.HasValue) connectionStringBuilder.Add("Cache Size", CacheSize.Value);

            return connectionStringBuilder.ConnectionString;

            // "Data Source=:memory:;Version=3;Cache Size=2000;"
        }

        /// <summary>
        /// 若filePath为空则使用内存
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="CacheSize"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Data.Sqlite.SqliteConnection Sqlite_GetConnectionByFilePath(string filePath=null, int? CacheSize=null)
        {     
            return Sqlite_GetConnection(Sqlite_GetConnectionString(filePath,CacheSize:CacheSize));          
        }

        /// <summary>
        /// 若filePath为空则使用内存
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="CacheSize"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Microsoft.Data.Sqlite.SqliteConnection Sqlite_GetOpenConnectionByFilePath(string filePath = null,int? CacheSize = null)
        {
            var conn = Sqlite_GetConnectionByFilePath(filePath, CacheSize:CacheSize);
            conn.Open();       
            return conn;
        } 


        #endregion

    }
}
