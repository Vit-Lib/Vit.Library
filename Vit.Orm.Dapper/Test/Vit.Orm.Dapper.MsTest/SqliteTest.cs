using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions;
using System.Linq;
using Vit.Orm.Dapper.MsTest.Model;
using Vit.Db.Util.Data;

namespace Vit.Orm.Dapper.MsTest
{
    [TestClass]
    public class SqliteTest
    {
      
        static Microsoft.Data.Sqlite.SqliteConnection GetConnection() => ConnectionFactory.Sqlite_GetConnection("data source=Data\\sqlite.db");


        [TestMethod]       
        public void SchemaMethod( )
        {        

            using (var conn = GetConnection())
            {
                if (conn == null) return;

                var tables=conn.Sqlite_GetAllTableName();

                var schema = conn.Sqlite_GetSchema();

            }
        }



        [TestMethod]
        public void GetTest()
        {

            using (var conn = GetConnection())
            {
                if (conn == null) return;

                #region (x.1)                
                {
                    var m = conn.Get<Auth_Account>(2);
                    Assert.AreEqual(2, m?.id);
                }
                #endregion

                #region (x.2)                
                {
                    var m = conn.Query<Auth_Account>("select * from tb_account where id=@id",
                        new { id = 2 }).FirstOrDefault();

                    Assert.AreEqual(2, m?.id);
                }
                #endregion

            }
        }

    }
}
