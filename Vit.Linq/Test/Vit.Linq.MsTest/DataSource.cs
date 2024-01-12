using System;
using System.Collections.Generic;
using System.Linq;

namespace Vit.Linq.MsTest
{
    public class ModelA
    {
        public int id;
        public int? pid;
        public string name { get; set; }
        public DateTime addTime;
        public string ext;
        public bool isEven;

        public ModelB b1;

        public ModelB[] ba;
        public ModelA BuildB()
        {
            b1 = new ModelB { name = name + "_b1", pid = pid };

            ba = new[] { b1 };
            return this;
        }
    }

    public class ModelB
    {
        public int? pid;
        public string name;
    }



    public class DataSource
    {
        public static List<ModelA> BuildDataSource(int count = 1000)
        {
            var Now = DateTime.Now;
            var list = new List<ModelA>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(new ModelA
                {
                    id = i,
                    pid = i / 10,
                    name = "name" + i,
                    addTime = Now.AddSeconds(i),
                    ext = "ext" + i,
                    isEven = i%2 == 0
                }.BuildB());

            }
            return list;
        }

        public static IQueryable GetIQueryable() => BuildDataSource().AsQueryable();
        public static IQueryable<ModelA> GetQueryable() => BuildDataSource().AsQueryable();
    }
}
