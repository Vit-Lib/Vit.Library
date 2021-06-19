using System;

namespace Vit.Db.Redis.QpsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            new RedisQpsTest().Start();
        }
    }
}
