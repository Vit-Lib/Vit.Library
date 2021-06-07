using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Core.Util.Common;
using Vit.Extensions;
using Vit.Orm.EntityFramework.MsTest.Model;

namespace Vit.Orm.EntityFramework.MsTest
{



    [TestClass]
    public class AddEntity_Test
    {

        public partial class DBContext : DbContext
        {
            public DBContext(DbContextOptions<DBContext> options)
                : base(options)
            {

            }


            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                //(x.1)
                base.OnModelCreating(modelBuilder);

                modelBuilder.Model.AddEntityType(typeof(tb_account));

            }

        }



        [TestMethod]
        public void TestMethod()
        {

            //(x.1)初始化
            var db = EfHelp.CreateDbContext<DBContext>();
            //db.AddEntityType(typeof(tb_account));

            //(x.2)获取数据源
            var dbSet = db.GetDbSet<tb_account>();

            string tempName = CommonHelp.NewGuid();

            #region 修改
            {
                var query = dbSet.Where(m => m.id == 2);
                var model = query.FirstOrDefault();

                model.name = tempName;
                db.Update(model);
                db.SaveChanges();                  
            }
            #endregion

            #region 查询检验
            {
                var query = dbSet.Where(m => m.id == 2);
                var model = query.FirstOrDefault();

                Assert.AreEqual(model.name,tempName);                
            }
            #endregion


        }
    }
}
