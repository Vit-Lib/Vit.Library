using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Core.Util.ConfigurationManager;
using Vit.Extensions.Redis_Extensions;

namespace Vit.Db.Redis.MsTest
{
    [TestClass]
    public class DbRedisTest
    {

        public class ModelA
        {
            public string va;
        }
        public class ModelB
        {
            public string va;
        }

        [TestMethod]
        public void Test()
        {
            string connection = Appsettings.json.GetStringByPath("ConnectionStrings.Redis");
            if (string.IsNullOrEmpty(connection)) return;

            using (var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(connection))
            {
                var db = redis.GetDatabase(1);


                //model
                string[] keyM = { "dbA", "model" };
                var maA = new ModelA { va = "Va:afs" };
                db.Set(maA, TimeSpan.FromSeconds(100), keyM);
                var maB = db.Get<ModelB>(keyM);
                Assert.AreEqual(maA.va, maB.va);


                //long
                long longA = 1234567890;
                string[] keyLong = { "dbA", "long" };
                db.Set(longA, TimeSpan.FromSeconds(100), keyLong);
                var longB = db.Get<long>(keyLong);
                Assert.AreEqual(longA, longB);

            }


        }
    }
}
