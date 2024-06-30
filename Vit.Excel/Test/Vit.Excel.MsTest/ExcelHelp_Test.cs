using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Vit.Core.Util.Common;
using Vit.Extensions;

namespace Vit.Excel.MsTest
{

    [TestClass]
    public class ExcelHelp_Test
    {

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
                    var rows = ExcelHelp.ReadDictionary(filePath, sheetName);
                    userList_fromFile = rows;
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
            }
            #endregion

            #region #3 Model
            {
                var userList = modelList;

                // read
                List<UserInfo> userList_fromFile;
                {
                    var sheet = ExcelHelp.ReadAsModel<UserInfo>(filePath, sheetName);
                    userList_fromFile = sheet;
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
                        ExcelHelp.SaveSheet(filePath, SheetData.Dictionary("userList", userList));
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
                        ExcelHelp.SaveSheet(filePath, SheetData.Enumerable("userList", userList, columns));
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
                        ExcelHelp.SaveSheet(filePath, SheetData.Model("userList", userList));
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
                        ExcelHelp.SaveSheets(filePath, new[] {
                            SheetData.Model("userList", userList),
                            SheetData.Model("userList2", userList)
                        });
                    }

                    // read and assert
                    {

                        var sheet1 = ExcelHelp.ReadAsModel<UserInfo>(filePath, "userList");
                        var sheet2 = ExcelHelp.ReadAsModel<UserInfo>(filePath, "userList2");
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
