using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Extensions;

namespace Vit.Excel.MsTest
{
    [TestClass]
    public class Excel_MiniExcel_Test : BaseExcel_Test
    {

        public override IExcel GetExcel(string filePath) => new Excel_MiniExcel(filePath);


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


        public override void DataFromExcelAreSame(List<UserInfo> modelList, string filePath, string sheetName = "userList")
        {
            base.DataFromExcelAreSame(modelList, filePath, sheetName);

            #region #4 native Dictionary
            {
                var dictionaries = modelList.Select(m => m.ToDictionary());
                var userList = dictionaries.ToList();

                // read
                List<IDictionary<string, object>> userList_fromFile;
                {
                    using var excel = GetExcel(filePath) as Excel_MiniExcel;
                    var sheet = excel.ReadDictionary(sheetName);
                    userList_fromFile = sheet.ToList();
                }

                for (var i = 0; i < userList.Count; i++)
                {
                    var user = userList[i];
                    var user2 = userList_fromFile[i];
                    Assert.IsTrue(UserInfo.AreEqual(user, user2));
                }
            }
            #endregion


            #region #5 native Model
            {
                var userList = modelList;

                // read
                List<UserInfo> userList_fromFile;
                {
                    using var excel = GetExcel(filePath) as Excel_MiniExcel;
                    var sheet = excel.ReadModel<UserInfo>(sheetName);
                    userList_fromFile = sheet.ToList();
                }
                for (var i = 0; i < userList.Count; i++)
                {
                    var user = userList[i];
                    var user2 = userList_fromFile[i];

                    Assert.AreEqual(user, user2);
                }
            }
            #endregion

        }



        [TestMethod]
        public void Test_HugeData()
        {
            var filePath = GetTempFilePath();
            try
            {
                int rowCount = 1000; // 100000
                int columnCount = 100;
                string cellValue = String.Join(",", Enumerable.Repeat("012345678", 100));
                var columnNames = Enumerable.Range(0, columnCount).Select(i => "column" + i).ToArray();


                #region Method GetStream
                IEnumerable<IEnumerable<object>> GetStream_Enumerable()
                {
                    for (int curRow = 0; curRow < rowCount; curRow++)
                    {

                        yield return Enumerable.Range(0, columnCount).Select(i => cellValue);
                    }
                }
                IEnumerable<object[]> GetStream_Array()
                {
                    for (int curRow = 0; curRow < rowCount; curRow++)
                    {
                        yield return Enumerable.Range(0, columnCount).Select(i => cellValue).ToArray();
                    }
                }
                #endregion



                #region Enumerable
                {
                    // write
                    {
                        File.Delete(filePath);
                        using var excel = GetExcel(filePath);
                        excel.AddSheetByArray("userList", GetStream_Enumerable(), columnNames);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = GetExcel(filePath) as Excel_MiniExcel;
                        var sheetNames = excel.GetSheetNames();
                        var columns1 = excel.GetColumns("userList");
                        Assert.AreEqual(columnCount, columns1.Count);

                        var sheet = excel.ReadDictionary("userList");
                        int rowCount_FromFile = 0;
                        foreach (var row in sheet)
                        {
                            rowCount_FromFile++;
                        }
                        Assert.AreEqual(rowCount, rowCount_FromFile);
                    }
                }
                #endregion

                #region Enumerable
                {
                    // write
                    {
                        File.Delete(filePath);
                        using var excel = GetExcel(filePath) as Excel_MiniExcel;
                        excel.AddSheetByArray("userList", GetStream_Array(), columnNames);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = GetExcel(filePath) as Excel_MiniExcel;
                        var sheetNames = excel.GetSheetNames();
                        var columns1 = excel.GetColumns("userList");
                        Assert.AreEqual(columnCount, columns1.Count);

                        var sheet = excel.ReadDictionary("userList");
                        int rowCount_FromFile = 0;
                        foreach (var row in sheet)
                        {
                            rowCount_FromFile++;
                        }
                        Assert.AreEqual(rowCount, rowCount_FromFile);
                    }
                }
                #endregion

            }
            finally
            {
                File.Delete(filePath);
            }
        }

    }
}
