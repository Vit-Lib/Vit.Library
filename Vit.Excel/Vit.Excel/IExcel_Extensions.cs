using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Vit.Extensions;

namespace Vit.Excel
{
    public static class IExcel_Extensions
    {
        public static int GetSheetsCount(this IExcel excel)
        {
            return excel.GetSheetNames().Count;
        }


        public static void AddSheetByModel(this IExcel excel, string sheetName, IEnumerable sheet, string[] columnNames = null)
        {
            var modelType = sheet.GetType().GetGenericArguments()[0];
            var method = new Action<IExcel, string, IEnumerable<object>, string[]>(AddSheetByModel).Method.GetGenericMethodDefinition().MakeGenericMethod(modelType);
            method.Invoke(null, [excel, sheetName, sheet, columnNames]);
        }
        static void AddSheetByModel<Model>(IExcel excel, string sheetName, IEnumerable<Model> sheet, string[] columnNames = null) where Model : class
        {
            excel.AddSheetByModel<Model>(sheetName, sheet, columnNames);
        }



        #region Save single sheet

        public static void SaveSheetByEnumerable(this IExcel excel,string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames)
        {
            excel.AddSheetByArray(sheetName, sheet, columnNames);
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
        public static void SaveSheetByDataTable(this IExcel excel, string sheetName ,DataTable table)
        {
            excel.AddSheetByDataTable(sheetName, table);
            excel.Save();
        }
        public static void SaveSheetByDataReader(this IExcel excel, string sheetName, IDataReader reader)
        {
            excel.AddSheetByDataReader(sheetName, reader);
            excel.Save();
        }
        #endregion



        #region AddSheet AddSheets SaveSheet SaveSheets
        public static void AddSheet(this IExcel excel, SheetData sheet)
        {
            switch (sheet.sheet)
            {
                case null: return;
                case DataTable table:
                    excel.AddSheetByDataTable(sheet.sheetName, table);
                    return;
                case IDataReader reader:
                    excel.AddSheetByDataReader(sheet.sheetName, reader);
                    return;
                case IEnumerable<IDictionary<string, object>> rows:
                    excel.AddSheetByDictionary(sheet.sheetName, rows, sheet.columnNames);
                    return;
                case IEnumerable<IEnumerable<object>> rows:
                    excel.AddSheetByArray(sheet.sheetName, rows, sheet.columnNames);
                    return;
                case IEnumerable rows:
                    excel.AddSheetByModel(sheet.sheetName, rows, sheet.columnNames);
                    return;
            }
        }
        public static void AddSheets(this IExcel excel, IEnumerable<SheetData> sheets)
        {
            sheets?.IEnumerable_ForEach(excel.AddSheet);
        }


        public static void SaveSheet(this IExcel excel, SheetData sheet)
        {
            excel.AddSheet(sheet);
            excel.Save();
        }
        public static void SaveSheets(this IExcel excel, IEnumerable<SheetData> sheets)
        {
            if (sheets?.Any() != true) return;

            excel.AddSheets(sheets);
            excel.Save();
        }
       
        #endregion
    }
}
