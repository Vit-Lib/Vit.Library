using System;
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
    public class Excel_Test
    {

        
 

        public List<UserInfo> GenerateList(int rowCount)
        {
            return Enumerable.Range(0, rowCount).Select(i => new UserInfo
            {
                id = i,
                name = CommonHelp.NewGuid(),
                age = CommonHelp.Random(0, 100),
                birth = DateTime.Now.AddHours(CommonHelp.Random(0, 1000)
                )
            }).ToList();
        }

        //[TestMethod]
        public void Test()
        {
            var path = CommonHelp.GetAbsPath(CommonHelp.NewGuid(), "test.xlsx");
            try
            {
                var models = GenerateList(1000);
                IQueryable queryable = models.AsQueryable();
                //IEnumerable<IDictionary<string, object>> dictionarys = models.Select(m => Json.DeserializeFromString<IDictionary<string, object>>(Json.SerializeToString(m)));
                IEnumerable<IDictionary<string, object>> dictionarys = models.Select(m => Json.Deserialize<IDictionary<string, object>>(Json.Serialize(m)));
                #region #1 save



                using var excel = new Excel_EPPlus(path);
                excel.SaveSheet("userList", queryable);
                #endregion

                #region #2 read
                //var userList_readFromFile = excel.ReadSheet;
                #endregion

            }
            finally
            {
                File.Delete(path);
                Directory.GetParent(path).Delete(true);
            }
        }
    }
}
