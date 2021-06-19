using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions;
using Vit.Orm.EntityFramework.Extensions;
using Vit.Orm.EntityFramework.MsTest.Model;

namespace Vit.Orm.EntityFramework.MsTest
{



    [TestClass]
    public class DbSet_Test
    {


        [TestMethod]
        public void DbSet_TestMethod()
        {

            //(x.1)��ʼ��
            var db = EfHelp.CreateDbContext();
            db.AddEntityType(typeof(tb_account));


            //(x.2)��ȡ����Դ
            var dbSet = db.GetDbSet<tb_account>();


            #region (x.3)Linq��ѯ
            {
                var query = (from m in dbSet where m.id == 2 select m);
                Assert.AreEqual(query.FirstOrDefault().id, 2);
            }
            #endregion

            #region (x.4)��ѯ
            {
                var query = dbSet.Where(m => m.id == 2);

                Assert.AreEqual(query.Count(), 1);
                Assert.AreEqual(query.ToList().Count, 1);

                Assert.AreEqual(query.FirstOrDefault().id, 2);
            }
            #endregion

            #region (x.5)�첽��ѯ
            {
                var query = dbSet.Where(m => m.id == 2);

                Assert.AreEqual(query.CountAsync().Result, 1);
                Assert.AreEqual(query.ToListAsync().Result.Count, 1);

                Assert.AreEqual(query.FirstOrDefaultAsync().Result.id, 2);
            }
            #endregion          



            #region (x.6)Ef_ToSql
            {
                var query = dbSet.Where(m => m.id == 2);

                var sql = query.Ef_ToSql(); 
            }
            #endregion


            #region (x.7)�޸�
            {
                //�ֶ���ӵ�ʵ�壨AddEntityType���ǲ��������޸Ĳ�����
                //var query = dbSet.Where(m => m.id == 2);
                //var model = query.FirstOrDefault();

                //model.name = "";
                //db.Update(model);
                //db.SaveChanges();

            }
            #endregion
        }
    }
}
