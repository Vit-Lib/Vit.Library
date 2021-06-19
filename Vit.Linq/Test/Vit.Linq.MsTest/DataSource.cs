using System;
using System.Collections.Generic;
using System.Linq;

namespace Vit.Linq.MsTest
{
    public class DataSource
    {
        #region (x.1) BuildDataSource
        public static List<ModelA> BuildDataSource()
        {
            var list = new List<ModelA>(1000);
            for (int i = 0; i < 1000; i++)
            {
                list.Add(new ModelA
                {
                    id = i,
                    pid = i / 10,
                    name = "name" + i,
                    addTime = DateTime.Now,
                    ext = "ext" + i
                }.BuildB());

            }
            return list;
        }

        public static IQueryable GetIQueryable() => BuildDataSource().AsQueryable();
        public static IQueryable<ModelA> GetQueryable() => BuildDataSource().AsQueryable();

        public class ModelA
        {
            public int id;
            public int? pid;
            public string name { get; set; }
            public DateTime addTime;
            public string ext;

            public ModelB b1;

            public ModelB[] ba;
            public ModelA BuildB()
            {
                b1 = new ModelB { name = name + "_b1", pid = pid };

                ba = new[] { b1 };
                return this;
            }



            public class ModelB
            {
                public int? pid;
                public string name;
            }

        }

        #endregion
    }
}
