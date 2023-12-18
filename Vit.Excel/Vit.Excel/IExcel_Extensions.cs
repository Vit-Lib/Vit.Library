using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;

using Vit.Excel;

namespace Vit.Extensions
{
    public static class IExcel_Extensions
    {
        public static int GetSheetsCount(this IExcel excel)
        {
            return excel.GetSheetNames().Count;
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
    }
}
