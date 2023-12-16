using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using Vit.Excel;

namespace Vit.Extensions
{
    public static class IExcel_Model_DataTable_Extensions
    {
        public static void SaveSheets(this IExcel excel, Stream stream, IEnumerable<DataTable> ds)
        {
            var sheets = ds.Select(dt =>
            {
                var sheetName = dt.TableName;
                var columns = new List<string>();
                foreach (DataColumn col in dt.Columns) { columns.Add(col.ColumnName); }

                return (sheetName, columns, GetRowEnumerable(dt));
            });

            excel.SaveSheets(stream, sheets);
        }
        public static void SaveSheet(this IExcel excel, Stream stream, DataTable dt)
        {
            SaveSheets(excel,stream, new []{ dt });
        }


        static IEnumerable<IEnumerable<object>> GetRowEnumerable(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                yield return row.ItemArray;
            }
        }

    }
}
