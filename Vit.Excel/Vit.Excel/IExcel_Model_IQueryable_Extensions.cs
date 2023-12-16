using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Vit.Excel;

namespace Vit.Extensions
{
    public static class IExcel_Model_IQueryable_Extensions
    {

        public static void SaveSheets(this IExcel excel, Stream stream, IEnumerable<(string sheetName, IQueryable rows, List<string> columns)> sheets)
        {
            var sheets_ = sheets.Select(sheet =>
            {
                var type = sheet.rows.ElementType;

                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var columnList =
                  fields.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row))))
                .Union(properties.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row)))))
                .ToDictionary(m => m.Name, m => m.Item2);

                var columns = sheet.columns;
                if (columns?.Any() != true)
                {
                    columns= columnList.Keys.ToList();
                }
               
                var cellsGettor = columns.Select(columnName => columnList[columnName]).ToList();

                Func<object, IEnumerable<object>> GetCellsInRow = (row) =>
                {
                    return cellsGettor.Select(cellGettor => cellGettor?.Invoke(row));
                };

                return (sheet.sheetName, columns, GetRowEnumerable(sheet.rows, GetCellsInRow));
            });

            excel.SaveSheets(stream, sheets_);
        }

        public static void SaveSheets(this IExcel excel, Stream stream, IEnumerable<(string sheetName, IQueryable rows)> sheets)
        {
            SaveSheets(excel, stream, sheets.Select(sheet => (sheet.sheetName, sheet.rows, (List<string>)null)));
        }

     

        static IEnumerable<IEnumerable<object>> GetRowEnumerable(IQueryable rows, Func<object, IEnumerable<object>> GetRow)
        {
            foreach (var row in rows)
            {
                yield return GetRow(row);
            }
        }


    }
}
