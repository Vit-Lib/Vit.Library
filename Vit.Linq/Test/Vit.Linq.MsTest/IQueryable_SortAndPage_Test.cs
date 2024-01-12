using Vit.Extensions.Linq_Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.Query;


namespace Vit.Linq.MsTest
{
    [TestClass]
    public class IQueryable_SortAndPage_Test
    {

        #region TestSortAndPage
        [TestMethod]
        public void TestSortAndPage()
        {
            var query = DataSource.GetIQueryable();

            #region #1
            {
                var result = query
                    .IQueryable_Sort(new[] {
                        new SortItem { field = "b1.pid", asc = false },
                        new SortItem { field = "id", asc = true }
                    })
                    .IQueryable_Page(new PageInfo { pageIndex = 1, pageSize = 10 })
                    .IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 10);
                Assert.AreEqual(result[0].id, 990);
            }
            #endregion


            #region #2
            {
                var result = query
                    .IQueryable_Sort("id", false)
                    .IQueryable_Page(2, 10)
                    .IQueryable_ToList<ModelA>();
                Assert.AreEqual(result.Count, 10);
                Assert.AreEqual(result[0].id, 989);
            }
            #endregion


            #region #3
            {
                var result = query
                    .IQueryable_Sort(new[] {
                        new SortItem { field = "b1.pid", asc = false },
                        new SortItem { field = "id", asc = true }
                    })
                    .IQueryable_ToPageData<ModelA>(new PageInfo { pageIndex = 1, pageSize = 10 });

                Assert.AreEqual(result.totalCount, 1000);
                Assert.AreEqual(result.rows.Count, 10);
                Assert.AreEqual(result.rows[0].id, 990);
            }
            #endregion

        }
        #endregion


    }
}
