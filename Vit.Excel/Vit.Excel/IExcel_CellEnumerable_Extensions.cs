using System.Collections.Generic;
using System.IO;

using Vit.Excel;

namespace Vit.Extensions
{
    public static class IExcel_CellEnumerable_Extensions
    {
        public static void SaveSheet(this IExcel excel, Stream stream, string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows)
        {
            excel.SaveSheets(stream, new[] { (sheetName, columns, rows) });
        }
        public static void SaveSheet(this IExcel excel, Stream stream, (string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows) sheet)
        {
            excel.SaveSheets(stream, new[] { sheet });
        }

    }
}
