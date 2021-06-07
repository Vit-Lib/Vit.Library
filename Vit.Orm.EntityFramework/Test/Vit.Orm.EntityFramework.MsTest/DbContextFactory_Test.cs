using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vit.Extensions;
using Vit.Orm.EntityFramework.MsTest.Model;

namespace Vit.Orm.EntityFramework.MsTest
{



    [TestClass]
    public class DbContextFactory_Test
    {


        [TestMethod]
        public void TestMethod()
        {

            //(x.1)��ʼ��
            var factory = new DbContextFactory();
            factory.Init();
            using (var scope = factory.CreateDbContext(out var db))
            {
                db.AddEntityType(typeof(tb_account)); 
            }
         

            #region Linq��ѯ
            {
                Task[] tasks = new Task[100];
                int successCount = 0;
                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = Task.Run(() =>
                    {
                        Thread.Sleep(new Random().Next(10));
                        using (var scope = factory.CreateDbContext(out var db))
                        {
                            //��ȡ����Դ
                            var dbSet = db.GetDbSet<tb_account>();

                            var query = (from m in dbSet where m.id == 2 select m);
                            var isSuccess = query.FirstOrDefault().id == 2;
                            if (isSuccess)
                            {
                                Interlocked.Increment(ref successCount);
                            }
                            Assert.IsTrue(isSuccess);                          
                        }
                    });
                }
                Task.WaitAll(tasks);
                Assert.AreEqual(successCount, tasks.Length);            
            }
            #endregion

        }
    }
}
