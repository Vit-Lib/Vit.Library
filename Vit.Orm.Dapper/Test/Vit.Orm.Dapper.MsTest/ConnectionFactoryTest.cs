using Dapper.Contrib.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Db.Util.Data;
using Vit.Orm.Dapper.MsTest.Model;

namespace Vit.Orm.Dapper.MsTest
{
    [TestClass]
    public class ConnectionFactoryTest
    {        

        [TestMethod]
        public void CreatorTest()
        {
            #region (x.1)
            {
                var connCreator = ConnectionFactory.GetConnectionCreator("App.Db");          

                using (var conn = connCreator())
                {
                    var m1 = conn.Get<Auth_Account>(2);
                    Assert.AreEqual(2, m1?.id);
                }
            }
            #endregion

            #region (x.2)
            {
                var connCreator = ConnectionFactory.GetConnectionCreator("App.Db");

                using (var conn = connCreator())
                {
                    var m1 = conn.Get<Auth_Account>(2);
                    Assert.AreEqual(2, m1?.id);
                }
            }
            #endregion
        }


        [TestMethod]
        public void ConnectTest()
        {
            #region (x.1)
            {
                using (var conn = ConnectionFactory.Sqlite_GetConnection("data source=Data\\sqlite.db"))
                {
                    var m1 = conn.Get<Auth_Account>(2);
                    Assert.AreEqual(2, m1?.id);
                }
            }
            #endregion

            #region (x.2)
            {
                using (var conn = ConnectionFactory.Sqlite_GetConnectionByFilePath("Data\\sqlite.db"))
                {
                    var m1 = conn.Get<Auth_Account>(2);
                    Assert.AreEqual(2, m1?.id);
                }
            }
            #endregion
        }

    }
}
