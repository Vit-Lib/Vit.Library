using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Vit.Extensions;

namespace Vit.Excel
{
    public static class IExcel_Extensions
    {
        public static int GetSheetsCount(this IExcel excel)
        {
            return excel.GetSheetNames().Count;
        }


        public static IEnumerable<IDictionary<string, object>> ReadSheetByDictionary(this IExcel excel, string sheetName)
        {
            return excel.ReadSheetByDictionary(sheetName, out _);
        }

        public static void AddSheetByModelEnumerable(this IExcel excel, string sheetName, IEnumerable sheet, string[] columnNames = null)
        {
            if (sheet.GetType().IsGenericType)
            {
                var type = sheet.GetType().GetGenericArguments()[0];
                var method = excel.GetType().GetMethod("AddSheetByModel").MakeGenericMethod(type);
                method.Invoke(excel, new object[] { sheetName, sheet, columnNames });
            }
        }



        #region Save single sheet

        public static void SaveSheetByEnumerable(this IExcel excel,string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames)
        {
            excel.AddSheetByEnumerable(sheetName, sheet, columnNames);
            excel.Save();
        }

        public static void SaveSheetByDictionary(this IExcel excel, string sheetName, IEnumerable<IDictionary<string, object>> sheet, string[] columnNames = null)
        {
            excel.AddSheetByDictionary(sheetName, sheet, columnNames);
            excel.Save();
        }
        public static void SaveSheetByModel<Model>(this IExcel excel, string sheetName, IEnumerable<Model> sheet, string[] columnNames = null) where Model : class
        {
            excel.AddSheetByModel(sheetName, sheet, columnNames);
            excel.Save();
        }
        public static void SaveSheetByDataTable(this IExcel excel, DataTable sheet, string sheetName = null)
        {
            excel.AddSheetByDataTable(sheet, sheetName);
            excel.Save();
        }
        #endregion



        #region Save SheetData
        public static void AddSheet(this IExcel excel, SheetData sheet)
        {
            switch (sheet.sheet)
            {
                case null: return;
                case IEnumerable<IDictionary<string, object>> rows:
                    excel.AddSheetByDictionary(sheet.sheetName, rows, sheet.columnNames);
                    return;
                case IEnumerable<IEnumerable<object>> rows:
                    excel.AddSheetByEnumerable(sheet.sheetName, rows, sheet.columnNames);
                    return;
                case IEnumerable rows:
                    excel.AddSheetByModelEnumerable(sheet.sheetName, rows, sheet.columnNames);
                    return;
            }
        }
        public static void AddSheet(this IExcel excel, IEnumerable<SheetData> sheets)
        {
            sheets?.IEnumerable_ForEach(excel.AddSheet);
        }

        public static void SaveSheet(this IExcel excel, IEnumerable<SheetData> sheets)
        {
            if (sheets?.Any() != true) return;

            excel.AddSheet(sheets);
            excel.Save();
        }
        public static void SaveSheet(this IExcel excel, SheetData sheet)
        {
            excel.AddSheet(sheet);
            excel.Save();
        }
        #endregion
    }
}
