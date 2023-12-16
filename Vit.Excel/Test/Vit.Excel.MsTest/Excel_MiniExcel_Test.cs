using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Module.Serialization;
using Vit.Core.Util.Common;
using Vit.Core.Util.ConfigurationManager;
using Vit.Excel;
using Vit.Extensions;

namespace Vit.Excel.MsTest
{
    [TestClass]
    public class Excel_MiniExcel_Test
    {


        void DataFromExcelAreSame(List<UserInfo> modelList, string filePath, string sheetName = "userList")
        {
            #region #1 Dictionary
            {
                var dictionaries = modelList.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
                var userList = dictionaries.ToList();

                // read
                List<IDictionary<string, object>> userList_fromFile;
                {
                    using var excel = new Excel_MiniExcel(filePath);
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

            #region #2 Cell Array
            {
                var dictionaries = modelList.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
                var userList = dictionaries.Select(m => m.Values.ToArray()).ToList();

                // read
                List<Object[]> userList_fromFile;
                {
                    using var excel = new Excel_MiniExcel(filePath);
                    var sheet = excel.ReadSheetByCells(sheetName);
                    userList_fromFile = sheet.rows.ToList();
                }
                for (var i = 0; i < userList.Count; i++)
                {
                    var user = userList[i];
                    var user2 = userList_fromFile[i];
                    Assert.IsTrue(UserInfo.AreEqual(user, user2));
                }
            }
            #endregion

            #region #3 Model
            {
                var userList = modelList;

                // read
                List<UserInfo> userList_fromFile;
                {
                    using var excel = new Excel_MiniExcel(filePath);
                    var sheet = excel.ReadSheetByModel<UserInfo>(sheetName);
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

            #region #4 native Model
            {
                var userList = modelList;

                // read
                List<UserInfo> userList_fromFile;
                {
                    using var excel = new Excel_MiniExcel(filePath);
                    var sheet = excel.Read<UserInfo>(sheetName);
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
        public void Test_ReadWrite()
        {
            var filePath = CommonHelp.GetAbsPath(CommonHelp.NewGuid() + "_test.xlsx");
            try
            {
                var modelList = UserInfo.GenerateList(1000);

                #region #1 Dictionary
                {
                    // write
                    {
                        var dictionaries = modelList.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
                        var userList = dictionaries;

                        File.Delete(filePath);
                        using var excel = new Excel_MiniExcel(filePath);
                        excel.SaveSheetByDictionary("userList", userList);
                    }

                    // read and assert
                    DataFromExcelAreSame(modelList, filePath);
                }
                #endregion


                #region #2 Cell Array
                {
                    // write
                    {
                        var dictionaries = modelList.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
                        var columns = dictionaries.First().Keys.ToArray();
                        var userList = dictionaries.Select(m => m.Values.ToArray()).ToList();

                        File.Delete(filePath);
                        using var excel = new Excel_MiniExcel(filePath);
                        excel.SaveSheetByCells("userList", userList, columns);
                    }

                    // read and assert
                    DataFromExcelAreSame(modelList, filePath);
                }
                #endregion


                #region #3 Model
                {
                    // write
                    {
                        var userList = modelList;

                        File.Delete(filePath);
                        using var excel = new Excel_MiniExcel(filePath);
                        excel.SaveSheetByModel("userList", userList);
                    }

                    // read and assert
                    DataFromExcelAreSame(modelList, filePath);
                }
                #endregion

            }
            finally
            {
                File.Delete(filePath);
            }
        }


        [TestMethod]
        public void Test_MultiSheets()
        {
            var filePath = CommonHelp.GetAbsPath(CommonHelp.NewGuid() + "_test.xlsx");
            try
            {
                var modelList = UserInfo.GenerateList(1000);


                #region # Model
                {
                    // write
                    {
                        var userList = modelList;

                        File.Delete(filePath);
                        using var excel = new Excel_MiniExcel(filePath);
                        excel.AddSheetByModel("userList", userList);
                        excel.AddSheetByModel("userList2", userList);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = new Excel_MiniExcel(filePath);
                        var sheetNames = excel.GetSheetNames();
                        var columns1 = excel.GetColumns("userList");
                        var columns2 = excel.GetColumns("userList2");

                        var sheet1 = excel.ReadSheetByModel<UserInfo>("userList").ToList();
                        var sheet2 = excel.ReadSheetByModel<UserInfo>("userList2").ToList();
                    }

                    DataFromExcelAreSame(modelList, filePath, "userList");
                    DataFromExcelAreSame(modelList, filePath, "userList2");
                }
                #endregion

            }
            finally
            {
                File.Delete(filePath);
            }
        }




        [TestMethod]
        public void Test_HugeData()
        {
            var filePath = CommonHelp.GetAbsPath(CommonHelp.NewGuid() + "_test.xlsx");
            try
            {
                int rowCount = 100000;
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
                        using var excel = new Excel_MiniExcel(filePath);
                        excel.AddSheetByEnumerable("userList", GetStream_Enumerable(), columnNames);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = new Excel_MiniExcel(filePath);
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
                        using var excel = new Excel_MiniExcel(filePath);
                        excel.AddSheetByCells("userList", GetStream_Array(), columnNames);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = new Excel_MiniExcel(filePath);
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
