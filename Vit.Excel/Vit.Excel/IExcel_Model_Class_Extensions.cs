using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Vit.Excel;
using Vit.Excel.Model;

namespace Vit.Extensions
{
    public static class IExcel_Model_Class_Extensions
    {

        public static void SaveSheets<T>(this IExcel excel, Stream stream, IEnumerable<(string sheetName, IEnumerable<T> rows)> sheets)
        {
            excel.SaveSheets(stream, sheets.Select(sheet => (sheet.sheetName, (IQueryable)sheet.rows.AsQueryable(), (List<string>)null)));
        }

        public static IEnumerable<T> ReadSheet<T>(this IExcel excel, Stream stream, string sheetName)
        {
            var rows = new List<T>();
            SheetSchema sheetSchema = null;


            Action<SheetSchema> OnGetSchema = (schema) =>
            {
                sheetSchema = schema;
            };
            Action<Object[]> AppendRow = row =>
            {
                //rows.Add(row);
            };

            excel.ReadSheet(stream,new ReadArgs { OnGetSchema=OnGetSchema,AppendRow=AppendRow,sheetName= sheetName });
            return rows;
        }

    }
}
