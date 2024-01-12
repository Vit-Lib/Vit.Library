using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Linq.QueryBuilder;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Linq.MsTest.QueryBuilder
{
    [TestClass]
    public class Queryable_QueryBuilder_Test
    {

        IQueryable<ModelA> GetQueryable() => DataSource.GetQueryable();


        [TestMethod]
        public void Test_ToList()
        {
            var query = GetQueryable();

            #region Count ToList ToArray
            {

                int count = query.Count();
                Assert.AreEqual(1000, count);


                var list1 = query.ToList<ModelA>();
                Assert.AreEqual(1000, list1.Count);

                var list2 = query.ToList();
                Assert.AreEqual(1000, list2.Count);


                var array1 = query.ToArray<ModelA>();
                Assert.AreEqual(1000, array1.Length);

                var array2 = query.ToArray();
                Assert.AreEqual(1000, array2.Length);
            }
            #endregion
        }






        #region QueryBuilder
        [TestMethod]
        public void Test_QueryBuilder()
        {

            #region #1 [object] is null | is not null

            #region ##1 is null
            {
                var query = GetQueryable();

                var item = query.Skip(10).FirstOrDefault();
                item.name = null;

                var strRule = "{'field':'name',  'operator': 'is null'  }".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);
                var result = query.Where(rule).ToList();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(10, result[0].id);
            }
            #endregion

            #region ##2 is not null
            {
                var query = GetQueryable();

                var item = query.Skip(10).FirstOrDefault();
                item.name = null;

                var strRule = "{'field':'name',  'operator': 'is not null'  }".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);
                var result = query.Where(rule).ToList();
                Assert.AreEqual(999, result.Count);
            }
            #endregion

            #endregion


            #region #2 [number | string | bool] compare

            #region ##1.1 [number] =
            {
                // ###1
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'id',  'operator': '=',  'value':10 }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(1, result.Count);
                    Assert.AreEqual(10, result[0].id);
                }

                // ###2
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'id',  'operator': '=',  'value': '10' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(1, result.Count);
                    Assert.AreEqual(10, result[0].id);
                }


                // ###3  = null
                {
                    var query = GetQueryable();

                    var item = query.Skip(10).FirstOrDefault();
                    item.pid = null;

                    var strRule = "{'field':'pid',  'operator': '=',  'value': null }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).FirstOrDefault();
                    Assert.AreEqual(10, result.id);
                }


            }
            #endregion

            #region ##1.2 [bool] =
            {
                // ###1
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'isEven',  'operator': '=',  'value':true }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(result.Count, 500);
                    Assert.AreEqual(0, result[0].id);
                }

                // ###2
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'isEven',  'operator': '=',  'value': 'false' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(result.Count, 500);
                    Assert.AreEqual(1, result[0].id);
                }
            }
            #endregion

            #region ##2.1 [number] !=
            {
                // ###1
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'id',  'operator': '!=',  'value':10 }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(999, result.Count);
                }

                // ###2 != null
                {
                    var query = GetQueryable();

                    var item = query.Skip(10).FirstOrDefault();
                    item.pid = null;

                    var strRule = "{'field':'pid',  'operator': '!=',  'value': null }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(999, result.Count);
                }
            }
            #endregion

            #region ##2.2 [bool] !=
            {
                // ###1
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'isEven',  'operator': '!=',  'value':true }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(result.Count, 500);
                    Assert.AreEqual(1, result[0].id);
                }

                // ###2
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'isEven',  'operator': '!=',  'value': 'false' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(result.Count, 500);
                    Assert.AreEqual(0, result[0].id);
                }
            }
            #endregion


            #region ##3 [number] > <
            {
                {
                    var query = GetQueryable();

                    var strRule = @"{'condition':'and', 'rules':[   
                                        {'field':'id',  'operator': '>',  'value':10  },
                                        {'field':'id',  'operator': '<',  'value': '20' } 
                                    ]}".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(9, result.Count);
                    Assert.AreEqual(11, result[0].id);
                }
            }
            #endregion


            #region ##4 [number] >= <=
            {
                {
                    var query = GetQueryable();

                    var strRule = @"{'condition':'and', 'rules':[   
                                        {'field':'id',  'operator': '>=',  'value':10  },
                                        {'field':'id',  'operator': '<=',  'value': '20' } 
                                    ]}".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(11, result.Count);
                    Assert.AreEqual(10, result[0].id);
                }
            }
            #endregion

            #endregion


            #region #3  in | not in

            #region ##1 in
            {
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'id',  'operator': 'in',  'value': [3,4,5] }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(3, result.Count);
                    Assert.AreEqual(5, result[2].id);
                }

                {
                    var query = GetQueryable();

                    var strRule = "{'field':'name',  'operator': 'in',  'value': [ 'name3', 'name4'] }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(2, result.Count);
                    Assert.AreEqual("name3", result[0].name);
                }

                {
                    var query = GetQueryable();
                    query.FirstOrDefault().name = null;

                    var strRule = @"{'condition':'or', 'rules':[
                                        {'field':'name',  'operator': 'is null' },
                                        {'field':'name',  'operator': 'in',  'value': [ 'name3', 'name4'] } 
                                    ]}".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(3, result.Count);
                    Assert.AreEqual(null, result[0].name);
                    Assert.AreEqual("name4", result[2].name);
                }
            }
            #endregion

            #region ##2  not in 
            {
                var query = GetQueryable();
                query.FirstOrDefault().name = null;

                var strRule = @"{'condition':'and', 'rules':[   
                                        {'field':'name',  'operator': 'is not null' },
                                        {'field':'name',  'operator': 'not in',  'value': [ 'name3', 'name4'] } 
                                    ]}".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);
                var result = query.Where(rule).ToList();
                Assert.AreEqual(997, result.Count);
            }
            #endregion

            #endregion


            #region #4 [string] operate

            #region ##1  contains
            {
                var query = GetQueryable();

                var strRule = "{'field':'name',  'operator': 'contains',  'value': '987' }".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);
                var result = query.Where(rule).ToList();

                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(987, result.First().id);
            }
            #endregion

            #region ##2  not contains
            {
                //###1
                {
                    var query = GetQueryable();

                    var strRule = "{'field':'name',  'operator': 'not contains',  'value': '987' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(999, result.Count);
                }


                //###2
                {
                    var query = GetQueryable();
                    query.Where(m => m.name.Contains("987")).FirstOrDefault().name = null;

                    var strRule = "{'field':'name',  'operator': 'not contains',  'value': '987' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(1000, result.Count);
                }

                //###3
                {
                    var query = GetQueryable();
                    query.Where(m => m.name.Contains("987")).FirstOrDefault().name = "";

                    var strRule = "{'field':'name',  'operator': 'not contains',  'value': '987' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();
                    Assert.AreEqual(1000, result.Count);
                }
            }
            #endregion

            #region ##3  starts with
            {
                var query = GetQueryable();

                var strRule = "{'field':'name',  'operator': 'starts with',  'value': 'name98' }".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);
                var result = query.Where(rule).ToList();

                Assert.AreEqual(11, result.Count);
            }
            #endregion

            #region ##4  ends with
            {
                var query = GetQueryable();

                var strRule = "{'field':'name',  'operator': 'ends with',  'value': '987' }".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);
                var result = query.Where(rule).ToList();

                Assert.AreEqual(1, result.Count);
            }
            #endregion

            #region ##5 is null or empty
            {
                //###1
                {
                    var query = GetQueryable();
                    query.Skip(10).First().name = null;

                    var strRule = "{'field':'name',  'operator': 'is null or empty' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();

                    Assert.AreEqual(1, result.Count);

                }

                //###2
                {
                    var query = GetQueryable();
                    query.Skip(10).First().name = "";

                    var strRule = "{'field':'name',  'operator': 'is null or empty' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();

                    Assert.AreEqual(1, result.Count);
                }
            }
            #endregion

            #region ##6  is not null or empty
            {
                //###1
                {
                    var query = GetQueryable();
                    query.Skip(10).First().name = null;

                    var strRule = "{'field':'name',  'operator': 'is not null or empty' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();

                    Assert.AreEqual(999, result.Count);
                }
                //###2
                {
                    var query = GetQueryable();
                    query.Skip(10).First().name = "";

                    var strRule = "{'field':'name',  'operator': 'is not null or empty' }".Replace("'", "\"");
                    var rule = Json.Deserialize<FilterRule>(strRule);
                    var result = query.Where(rule).ToList();

                    Assert.AreEqual(999, result.Count);
                }
            }
            #endregion


            #endregion


            #region #5  nested field
            {
                var query = GetQueryable();
                var strRule = "{'field':'b1.name',  'operator': '=',  'value': 'name987_b1' }".Replace("'", "\"");
                var rule = Json.Deserialize<FilterRule>(strRule);

                var result = query.Where(rule).ToList();

                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(987, result[0].id);
            }
            #endregion

        }

        #endregion




    }
}
