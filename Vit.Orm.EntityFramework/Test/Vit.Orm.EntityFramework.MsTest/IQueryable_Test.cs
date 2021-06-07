using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.Query;
using Vit.Extensions;
using Vit.Linq.Query;
using Vit.Orm.EntityFramework.Extensions;
using Vit.Orm.EntityFramework.MsTest.Model;

namespace Vit.Orm.EntityFramework.MsTest
{



    [TestClass]
    public class IQueryable_Test
    {

        #region (x.1)IQueryable 查询测试
       
        [TestMethod]
        public void IQueryable_TestMethod()
        {

            //(x.1)初始化
            var db = EfHelp.CreateDbContext();
            db.AddEntityType(typeof(tb_account));


            //(x.2)获取数据源
            var queryable = db.GetQueryable(typeof(tb_account));



            #region (x.3)查询
            {
                var query = queryable.IQueryable_Where(new DataFilter { field = "id", opt = "=", value = 2 });

                Assert.AreEqual(query.Ef_Count(), 1);

                Assert.AreEqual(query.Ef_ToList().Count, 1);

                Assert.AreEqual(query.Ef_ToList<tb_account>().Count, 1);


                Assert.AreEqual((query.Ef_FirstOrDefault() as tb_account).id, 2);
                Assert.AreEqual(query.Ef_FirstOrDefault<tb_account>().id, 2);

            }
            #endregion

            #region (x.4)异步查询
            {
                var query = queryable.IQueryable_Where(new DataFilter { field = "id", opt = "=", value = 2 });

                Assert.AreEqual(query.Ef_CountAsync().Result, 1);
                Assert.AreEqual(query.Ef_ToListAsync().Result.Count, 1);
                Assert.AreEqual(query.Ef_ToListAsync<tb_account>().Result.Count, 1);

                Assert.AreEqual((query.Ef_FirstOrDefaultAsync().Result as tb_account).id, 2);             
            }
            #endregion


            #region (x.5)SortAndPage
            {
                var result = queryable
                     .IQueryable_Sort(new[] {                 
                        new SortItem { field = "id", asc = false }
                    })
                    .IQueryable_Page(new PageInfo { pageIndex = 2, pageSize = 2 })
                    .Ef_ToList<tb_account>();

                Assert.AreEqual(result.Count, 2);
                Assert.AreEqual(result[0].id, 2);
                Assert.AreEqual(result[1].id, 1);
            }
            #endregion            

        }

        #endregion

        #region (x.2)GetQueryableByTableName

        [TestMethod]
        public void GetQueryableByTableName_TestMethod()
        {

            //(x.1)初始化
            var db = EfHelp.CreateDbContext();
            db.AddEntityType(typeof(tb_account));


            //(x.2)获取数据源
            var queryable = db.GetQueryableByTableName("tb_account");


            #region (x.3)查询
            {
                var query = queryable.IQueryable_Where(new DataFilter { field = "id", opt = "=", value = 2 });  
                Assert.AreEqual(query.Ef_FirstOrDefault<tb_account>().id, 2);
            }
            #endregion
                  

        }

        #endregion
    }
}
