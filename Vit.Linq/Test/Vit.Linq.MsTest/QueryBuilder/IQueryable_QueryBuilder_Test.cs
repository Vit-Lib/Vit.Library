using Vit.Extensions.Linq_Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Vit.Linq.MsTest
{
    [TestClass]
    public class IQueryable_QueryBuilder_Test
    {
        IQueryable  GetQueryable() => DataSource.GetIQueryable();


        [TestMethod]
        public void Test_ToList()
        {
            var query = GetQueryable();

            #region Count ToList ToArray
            {

                int count = query.IQueryable_Count();
                Assert.AreEqual(1000, count);


                var list1 = query.IQueryable_ToList<ModelA>();
                Assert.AreEqual(1000, list1.Count);

                var list2 = query.IQueryable_ToList() as List<ModelA>;
                Assert.AreEqual(1000, list2.Count);


                var array1 = query.IQueryable_ToArray<ModelA>();
                Assert.AreEqual(1000, array1.Length);

                var array2 = query.IQueryable_ToArray() as ModelA[];
                Assert.AreEqual(1000, array2.Length);
            }
            #endregion
        }


        /*

        #region (x.2)DataFilter        
        [TestMethod]
        public void TestDataFilter()
        {
            var query = DataSource.GetIQueryable();

         
 

            #region (x.1)  =
            {

                //(x.x.1)
                {
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "id", opt = "=", value = 10 } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 1);
                    Assert.AreEqual(result[0].id, 10);
                }

                //(x.x.2) == null
                {
                    var item = query.IQueryable_Skip(10).IQueryable_FirstOrDefault<ModelA>();
                    var pid = item.pid;
                    item.pid = null;

                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "pid", opt = "=", value = null } }).IQueryable_FirstOrDefault<ModelA>();
                    Assert.AreEqual(result?.id, 10);
                    item.pid = pid;
                }
            }
            #endregion


            #region (x.2)  !=
            {
                //(x.x.1)
                {
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "id", opt = "!=", value = 10 } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 999);
                }

                //(x.x.2) != null
                {
                    var item = query.IQueryable_Skip(10).IQueryable_FirstOrDefault<ModelA>();
                    var pid = item.pid;
                    item.pid = null;

                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "pid", opt = "!=", value = null } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 999);
                    item.pid = pid;
                }
            }
            #endregion

            #region (x.3)  > <
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "id", opt = ">", value = 10 }, new DataFilter { field = "id", opt = "<", value = 20 } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 9);
            }
            #endregion

            #region (x.4)  >= <=
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "id", opt = ">=", value = 10 }, new DataFilter { field = "id", opt = "<=", value = 20 } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 11);
            }
            #endregion


            #region (x.5)  Contains
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "Contains", value = "987" } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].id, 987);
            }
            #endregion

            #region (x.x)  NotContains
            {

                //(x.x.1)
                {
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "NotContains", value = "987" } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 999);
                }


                //(x.x.2)
                {
                    var item = query.IQueryable_Where(
                        new[] { new DataFilter { field = "name", opt = "Contains",value="987" } }
                        ).IQueryable_FirstOrDefault<ModelA>();
                    var oriName = item.name;

                    item.name = null;
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "NotContains", value = "987" } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 1000);
                    item.name = oriName;
                }

                //(x.x.3)
                {
                    var item = query.IQueryable_Where(
                        new[] { new DataFilter { field = "name", opt = "Contains", value = "987" } }
                        ).IQueryable_FirstOrDefault<ModelA>();
                    var oriName = item.name;

                    item.name = "";
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "NotContains", value = "987" } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 1000);
                    item.name = oriName;
                }
            }
            #endregion

            #region (x.6)  StartsWith
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "StartsWith", value = "name98" } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 11);
            }
            #endregion

            #region (x.7)  EndsWith
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "EndsWith", value = "987" } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 1);
            }
            #endregion

            #region (x.x)  IsNullOrEmpty
            {
                var item = query.IQueryable_Skip(10).IQueryable_FirstOrDefault<ModelA>();
                var oriName = item.name;

                //(x.x.1)
                {
                    item.name = null;
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "IsNullOrEmpty" } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 1);
                    item.name = oriName;
                }
                //(x.x.2)
                {
                    item.name = "";
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "IsNullOrEmpty" } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 1);
                    item.name = oriName;
                }
            }
            #endregion

            #region (x.x)  IsNotNullOrEmpty
            {
                var item = query.IQueryable_Skip(10).IQueryable_FirstOrDefault<ModelA>();
                var oriName = item.name;

                //(x.x.1)
                {
                    item.name = null;
                    var result = query.IQueryable_Where(
                        new[] { new DataFilter { field = "name", opt = "IsNotNullOrEmpty" } }
                        )
                        .IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 999);
                    item.name = oriName;
                }
                //(x.x.2)
                {
                    item.name = "";
                    var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "IsNotNullOrEmpty" } }).IQueryable_ToList<ModelA>();
                    Assert.AreEqual(result.Count, 999);
                    item.name = oriName;
                }
            }
            #endregion

            #region (x.8)  In 1
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "id", opt = "In", value = new int[] { 3, 4, 5 } } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 3);
            }
            #endregion

            #region (x.9)  In 2
            {
                query.IQueryable_FirstOrDefault<ModelA>().name = null;
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "In", value = new[] { "name3", "name4", null } } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 3);
            }
            #endregion

            #region (x.10)  NotIn 
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "name", opt = "NotIn", value = new[] { "name3", "name4", null } } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 997);
            }
            #endregion


            #region (x.11)  多级field
            {
                var result = query.IQueryable_Where(new[] { new DataFilter { field = "b1.name", opt = "=", value = "name987_b1" } }).IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 1);
                Assert.AreEqual(result[0].id, 987);
            }
            #endregion

        }

        #endregion

        //*/

   


    }
}
