using System;
using System.Threading;
using System.Threading.Tasks;
using Vit.Core.Util.ConfigurationManager;
using Vit.Extensions.Redis;

namespace Vit.Db.Redis.QpsTest
{
    public class RedisQpsTest
    {
        long requestCount = 0;
        public int threadCount = Vit.Core.Util.ConfigurationManager.ConfigurationManager.Instance.GetByPath<int>("threadCount");

        public RedisQpsTest()
        {
            requestCount = 0;
        }


        class ModelA {
            public string va;
        }

        string[] keyM = { "dbA", "model" };
        public void Start()
        {

            #region (x.1) set value to redis
            string connection = ConfigurationManager.Instance.GetStringByPath("ConnectionStrings.Redis");
            using (var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(connection))
            {
                var db = redis.GetDatabase(1);

                var maA = new ModelA { va = "asddas" };
                db.Set(maA, TimeSpan.FromSeconds(600),keyM);               
            }
            #endregion

            for (int t = 0; t < threadCount; t++) {
                Task.Run((Action)ThreadReadRedis);
            }

            ThreadPrintQps();
            //Task.Run((Action)ThreadPrintQps);
        }

        void ThreadReadRedis()
        {
            string connection = ConfigurationManager.Instance.GetStringByPath("ConnectionStrings.Redis");
            using (var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(connection))
            {
                var db = redis.GetDatabase(1);

                ModelA obj;
                while (true) {
                    obj=db.Get<ModelA>(keyM);
                    Interlocked.Increment(ref requestCount);
                }

            }
        }

        void ThreadPrintQps()
        {
            DateTime dtLast = DateTime.Now;
            long lastRequest = requestCount;

            DateTime dtNow;
            long curRequest;
            while (true)
            {
               
                Thread.Sleep(1000);

                dtNow = DateTime.Now;
                curRequest = requestCount;

                int qps =(int)(  (curRequest - lastRequest) / (dtNow - dtLast).TotalSeconds);
                Console.WriteLine("qps:" + qps);

                dtLast = dtNow;
                lastRequest = curRequest;

            }

        }


    }
}
