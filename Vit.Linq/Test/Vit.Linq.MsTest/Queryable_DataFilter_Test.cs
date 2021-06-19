using Vit.Extensions;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Linq.Query;
using static Vit.Linq.MsTest.DataSource;

namespace Vit.Linq.MsTest
{
    [TestClass]
    public class Queryable_DataFilter_Test
    {

        #region DataFilter        
        [TestMethod]
        public void TestDataFilter()
        {
            var query = DataSource.GetQueryable();

            //操作符。可为 "=", "!=", ">", "<" , ">=", "<=", "Contains", "NotContains", "StartsWith", "EndsWith", "IsNullOrEmpty", "IsNotNullOrEmpty", "In", "NotIn"

            #region (x.0) Count ToList ToArray
            {

                int count = query.Count();

                var list1 = query.ToList<ModelA>();
                var list2 = query.ToList();

                var array1 = query.ToArray<ModelA>();
                var array2 = query.ToArray();
            }
            #endregion


            #region (x.1)  =
            {
                //(x.x.1)
                {
                    var result = query.Where(new[] { new DataFilter { field = "id", opt = "=", value = 10 } }).ToList();
                    Assert.AreEqual(result.Count, 1);
                    Assert.AreEqual(result[0].id, 10);
                }


                //(x.x.2) == null
                {
                    var item = query.Skip(10).FirstOrDefault();
                    var pid = item.pid;
                    item.pid = null;

                    var result = query.Where(new[] { new DataFilter { field = "pid", opt = "=", value = null } }).FirstOrDefault();                
                    Assert.AreEqual(result?.id, 10);
                    item.pid = pid;
                }


            }
            #endregion

            #region (x.2)  !=
            {
                //(x.x.1)
                {
                    var result = query.Where(new[] { new DataFilter { field = "id", opt = "!=", value = 10 } }).ToList();
                    Assert.AreEqual(result.Count, 999);
                }

                //(x.x.2) != null
                {
                    var item = query.Skip(10).FirstOrDefault();
                    var pid = item.pid;
                    item.pid = null;

                    var result = query.Where(new[] { new DataFilter { field = "pid", opt = "!=", value = null } }).ToList();
                    Assert.AreEqual(result.Count, 999);
                    item.pid = pid;
                }
            }
            #endregion

            #region (x.3)  > <
            {
                var result = query.Where(new[] { new DataFilter { field = "id", opt = ">", value = 10 }, new DataFilter { field = "id", opt = "<", value = 20 } }).ToList();
                Assert.AreEqual(result.Count, 9);
            }
            #endregion

            #region (x.4)  >= <=
            {
                var result = query.Where(new[] { new DataFilter { field = "id", opt = ">=", value = 10 }, new DataFilter { field = "id", opt = "<=", value = 20 } }).ToList();
                Assert.AreEqual(result.Count, 11);
            }
            #endregion


            #region (x.5)  Contains
            {
                var result = query.Where(new[] { new DataFilter { field = "name", opt = "Contains", value = "987" } }).ToList();
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].id, 987);
            }
            #endregion

            #region (x.x)  NotContains
            {              

                //(x.x.1)
                {
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "NotContains", value = "987" } }).ToList();
                    Assert.AreEqual(result.Count, 999);                  
                }


                //(x.x.2)
                {
                    var item = query.Where(m => m.name.Contains("987")).FirstOrDefault();
                    var oriName = item.name;

                    item.name = null;
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "NotContains", value = "987" } }).ToList();
                    Assert.AreEqual(result.Count, 1000);
                    item.name = oriName;
                }

                //(x.x.3)
                {
                    var item = query.Where(m=>m.name.Contains("987")).FirstOrDefault();
                    var oriName = item.name;

                    item.name = "";
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "NotContains", value = "987" } }).ToList();
                    Assert.AreEqual(result.Count, 1000);
                    item.name = oriName;
                }
            }
            #endregion


            #region (x.6)  StartsWith
            {
                var result = query.Where(new[] { new DataFilter { field = "name", opt = "StartsWith", value = "name98" } }).ToList();
                Assert.AreEqual(result.Count, 11);
            }
            #endregion

            #region (x.7)  EndsWith
            {
                var result = query.Where(new[] { new DataFilter { field = "name", opt = "EndsWith", value = "987" } }).ToList();
                Assert.AreEqual(result.Count, 1);
            }
            #endregion

            #region (x.x)  IsNullOrEmpty
            {
                var item = query.Skip(10).FirstOrDefault();
                var oriName = item.name;

                //(x.x.1)
                {
                    item.name = null;
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "IsNullOrEmpty" } }).ToList();
                    Assert.AreEqual(result.Count, 1);
                    item.name = oriName;
                }
                //(x.x.2)
                {
                    item.name = "";
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "IsNullOrEmpty" } }).ToList();
                    Assert.AreEqual(result.Count, 1);
                    item.name = oriName;
                }
            }
            #endregion

            #region (x.x)  IsNotNullOrEmpty
            {
                var item = query.Skip(10).FirstOrDefault();
                var oriName = item.name;

                //(x.x.1)
                {
                    item.name = null;
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "IsNotNullOrEmpty" } }).ToList();
                    Assert.AreEqual(result.Count, 999);
                    item.name = oriName;
                }
                //(x.x.2)
                {
                    item.name = "";
                    var result = query.Where(new[] { new DataFilter { field = "name", opt = "IsNotNullOrEmpty" } }).ToList();
                    Assert.AreEqual(result.Count, 999);
                    item.name = oriName;
                }
            }
            #endregion



            #region (x.8)  In 1
            {
                var result = query.Where(new[] { new DataFilter { field = "id", opt = "In", value = new int[] { 3, 4, 5 } } }).ToList();
                Assert.AreEqual(result.Count, 3);
            }
            #endregion

            #region (x.9)  In 2
            {
                query.FirstOrDefault().name = null;
                var result = query.Where(new[] { new DataFilter { field = "name", opt = "In", value = new[] { "name3", "name4", null } } }).ToList();
                Assert.AreEqual(result.Count, 3);
            }
            #endregion

            #region (x.10)  NotIn 
            {
                var result = query.Where(new[] { new DataFilter { field = "name", opt = "NotIn", value = new[] { "name3", "name4", null } } }).ToList();
                Assert.AreEqual(result.Count, 997);
            }
            #endregion


            #region (x.11)  多级field
            {
                var result = query.Where(new[] { new DataFilter { field = "b1.name", opt = "=", value = "name987_b1" } }).ToList();
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].id, 987);
            }
            #endregion

        }

        #endregion


     

    }
}
