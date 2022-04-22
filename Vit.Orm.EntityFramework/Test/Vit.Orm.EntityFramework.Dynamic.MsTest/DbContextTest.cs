
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions;
using Vit.Linq.Query;
using Vit.Orm.EntityFramework.Extensions;
using Vit.Extensions.ObjectExt;
using Vit.Orm.EntityFramework.Dynamic.Extensions;

namespace Vit.Orm.EntityFramework.Dynamic.MsTest
{



    [TestClass]
    public class DbContextTest
    {
        [TestMethod]
        public void DbSet_TestMethod()
        {        

            #region (x.1)查询
            {
                //(x.x.1)创建DbContext
                var db = EfHelp.CreateDbContext<AutoMapDbContext>();
                var queryable = db.GetQueryableByTableName("tb_account");

                //(x.x.2)Count ToList
                var count = queryable.Ef_Count();
                var result = queryable.Ef_ToList();

                Assert.AreEqual(count, 4);
                Assert.AreEqual(result.Count, 4);
            }
            #endregion



            #region (x.2)查询
            {
                //(x.x.1)创建DbContext
                var db = EfHelp.CreateDbContext();
                db.AutoGenerateEntity(Vit.Core.Util.ConfigurationManager.Appsettings.json.GetByPath<Vit.Db.Util.Data.ConnectionInfo>("App.Db"));
                var queryable = db.GetQueryableByTableName("tb_account");


                //(x.x.2)FirstOrDefault
                var query = queryable.IQueryable_Where(new DataFilter { field = "id", opt = "=", value = 2 });
                var m = query.Ef_FirstOrDefault();
                var id =  m.GetProperty<long>("id");
                Assert.AreEqual(id, 2);
            }
            #endregion

        }
    }
}
