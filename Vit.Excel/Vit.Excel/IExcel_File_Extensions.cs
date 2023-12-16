using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using Vit.Excel;

namespace Vit.Extensions
{
    public static class IExcel_File_Extensions
    {
        public static void SaveSheets(this IExcel excel, string filePath, IEnumerable<(string sheetName, IQueryable rows)> sheets)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            excel?.SaveSheets(stream, sheets);
        }

    }
}
