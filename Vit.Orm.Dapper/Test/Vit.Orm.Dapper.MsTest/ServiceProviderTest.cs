using Dapper.Contrib.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions.Linq_Extensions;
using Vit.Orm.Dapper.MsTest.Model;

namespace Vit.Orm.Dapper.MsTest
{
    [TestClass]
    public class ServiceProviderTest
    {


        [TestMethod]       
        public void ServiceCollectionTest( )
        {

            var serviceCollection = new ServiceCollection();
            serviceCollection.UseDapper("App.Db");

            using (var scope = serviceCollection.BuildServiceProvider().CreateScope())
            {
                var conn = scope.ServiceProvider.GetService<System.Data.IDbConnection>();

                var m1 = conn.Get<Auth_Account>(2);
                Assert.AreEqual(2, m1?.id);
            }            
        }
         
    }
}
