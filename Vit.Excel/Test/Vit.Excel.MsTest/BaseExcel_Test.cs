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

    public abstract class BaseExcel_Test
    {

        public abstract IExcel GetExcel(string filePath);


        public virtual void DataFromExcelAreSame(List<UserInfo> modelList, string filePath, string sheetName = "userList")
        {
            #region #1 Dictionary
            {
                var dictionaries = modelList.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
                var userList = dictionaries.ToList();

                // read
                List<IDictionary<string, object>> userList_fromFile;
                {
                    using var excel = GetExcel(filePath);
                    var sheet = excel.ReadSheetByDictionary(sheetName).rows;
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
                    using var excel = GetExcel(filePath);
                    var sheet = excel.ReadSheetByEnumerable(sheetName).rows;
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

        }


        public virtual void Test_ReadWrite()
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
                        var dictionaries = modelList.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
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


        public virtual void Test_MultiSheets()
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
                        using var excel = GetExcel(filePath);
                        excel.AddSheetByModel("userList", userList);
                        excel.AddSheetByModel("userList2", userList);
                        excel.Save();
                    }

                    // read and assert
                    {
                        using var excel = GetExcel(filePath);
                        var sheetNames = excel.GetSheetNames();

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




     
    }
}
