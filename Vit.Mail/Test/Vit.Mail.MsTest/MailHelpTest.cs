using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                if (null == Appsettings.json.GetStringByPath("MailAccount.password")) return;
                MailHelp.Send("5678@outlook.com", "Vit.Mail Test", "Vit.Mail test,<p>html test</p>" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }


        }
    }
}
