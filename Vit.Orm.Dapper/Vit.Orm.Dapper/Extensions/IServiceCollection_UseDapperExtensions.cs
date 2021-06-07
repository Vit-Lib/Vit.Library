
using Microsoft.Extensions.DependencyInjection;
using Vit.Core.Util.ConfigurationManager;
using Vit.Db.Util.Data;

namespace Vit.Extensions
{
    /// <summary>
    ///  
    /// </summary>
    public static partial class IServiceCollection_UseDapperExtensions
    {

        /// <summary>
        /// 配置Dapper
        /// </summary> 
        /// <param name="data"></param>
        /// <param name="configPath">在appsettings.json中的路径，如："Auth.EntityFramework"</param>
        public static bool UseDapper(this IServiceCollection data,string configPath)
        {         

            // 数据库类型，可为  mysql mssql sqlite
            string type = ConfigurationManager.Instance.GetStringByPath(configPath + ".type");
            // 数据库类型，可为  mysql mssql sqlite
            string ConnectionString = ConfigurationManager.Instance.GetStringByPath(configPath + ".ConnectionString");

            if (type == "mssql")
            {
                //使用SqlServer数据库
                data.AddScoped<System.Data.IDbConnection>(provider => ConnectionFactory.MsSql_GetConnection(ConnectionString));               
            }
            else if (type == "mysql")
            {
                //使用mysql数据库
                data.AddScoped<System.Data.IDbConnection>(provider => ConnectionFactory.MySql_GetConnection(ConnectionString));
            }
            else if (type == "sqlite")
            {
                //使用sqlite数据库
                data.AddScoped<System.Data.IDbConnection>(provider => ConnectionFactory.Sqlite_GetConnection(ConnectionString));
            }
            else
            {
                return false;
            }
            return true;
        }

    }
}
