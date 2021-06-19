

namespace Vit.Db.Util.Data
{
    public class ConnectionInfo
    {
        /// <summary>
        ///  数据库类型，可为  mysql mssql sqlite 等
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
    }
}
