using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Vit.Excel.MsTest
{
    [TestClass]
    public class Excel_EPPlus_Test : BaseExcel_Test
    {

        public override IExcel GetExcel(string filePath) => new Excel_EPPlus(filePath);

        [TestMethod]
        public override void Test_ReadWrite()
        {
            base.Test_ReadWrite();
        }

        [TestMethod]
        public override void Test_MultiSheets()
        {
            base.Test_MultiSheets();
        }

    }
}
