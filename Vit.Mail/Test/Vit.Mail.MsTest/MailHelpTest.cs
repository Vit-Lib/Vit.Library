using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Vit.Core.Util.ConfigurationManager;

namespace Vit.Mail.MsTest
{
    [TestClass]
    public class MailHelpTest
    {

        [TestMethod]
        public void Test()
        {
            try
            {
                if (null == ConfigurationManager.Instance.GetStringByPath("MailAccount.password")) return;
                MailHelp.Send("serset@yeah.net", "Vit.Mail测试", "Vit.Mail测试,<p>html测试</p>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }


        }
    }
}
