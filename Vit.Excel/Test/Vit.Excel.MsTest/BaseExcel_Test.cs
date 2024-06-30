using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Util.Common;
using Vit.Extensions;

namespace Vit.Excel.MsTest
{

    public abstract class BaseExcel_Test
    {

        public abstract IExcel GetExcel(string filePath);
        public string GetTempFilePath() => CommonHelp.GetAbsPath(DateTime.Now.ToString("HHmmss_") + CommonHelp.NewGuid() + "_test.xlsx");


        public virtual void DataFromExcelAreSame(List<UserInfo> modelList, string filePath, string sheetName = "userList")
        {
            #region #1 Dictionary
            {
                var dictionaries = modelList.Select(m => m.ToDictionary());
                var userList = dictionaries.ToList();

                // read
                List<IDictionary<string, object>> userList_fromFile;
                {
                    using var excel = GetExcel(filePath);
                    var rows = excel.ReadDictionary(sheetName);
                    userList_fromFile = rows.ToList();
                }

                for (var i = 0; i < userList.Count; i++)
                {
                    var user = userList[i];
                    var user2 = userList_fromFile[i];
                    Assert.IsTrue(UserInfo.AreEqual(user, user2));
                }
            }
            #endregion

            #region #2 Enumerable
            {
                var dictionaries = modelList.Select(m => m.ToDictionary());
                var userList = dictionaries.Select(m => m.Values.ToArray()).ToList();

                // read
                List<Object[]> userList_fromFile;
                {
                    using var excel = GetExcel(filePath);
                    var sheet = excel.ReadArray(sheetName).rows;
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

            #region #3 Model
            {
                var userList = modelList;

                // read
                List<UserInfo> userList_fromFile;
                {
                    using var excel = GetExcel(filePath);
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
        public virtual void Test_ReadWrite()
        {
            var filePath = GetTempFilePath();
            try
            {
                var modelList = UserInfo.GenerateList(1000);

                #region #1 Dictionary
                {
                    // write
                    {
                        var dictionaries = modelList.Select(m => m.ToDictionary());
                        var userList = dictionaries;

                        File.Delete(filePath);
                        using var excel = GetExcel(filePath);
                        excel.SaveSheetByDictionary("userList", userList);
                    }

                    // read and assert
                    DataFromExcelAreSame(modelList, filePath);
                }
                #endregion


                #region #2 Enumerable
                {
                    // write
                    {
                        var dictionaries = modelList.Select(m => m.ToDictionary());
                        var columns = dictionaries.First().Keys.ToArray();
                        var userList = dictionaries.Select(m => m.Values.ToArray()).ToList();

                        File.Delete(filePath);
                        using var excel = GetExcel(filePath);
                        excel.SaveSheetByEnumerable("userList", userList, columns);
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
                        using var excel = GetExcel(filePath);
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
        public virtual void Test_MultiSheets()
        {
            var filePath = GetTempFilePath();
            try
            {
                var modelList = UserInfo.GenerateList(1000);


                #region # Model
                {
                    // write
                    {
                        var userList = modelList;

                        File.Delete(filePath);
                        using var excel = GetExcel(filePath);
                        excel.AddSheetByModel("userList", userList);
                        excel.AddSheetByModel("userList2", userList);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = GetExcel(filePath);
                        var sheetNames = excel.GetSheetNames();

                        var sheet1 = excel.ReadModel<UserInfo>("userList").ToList();
                        var sheet2 = excel.ReadModel<UserInfo>("userList2").ToList();
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





    }
}
