using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Vit.Excel
{
    public class ExcelHelp
    {

        public static Func<Stream, IExcel> GetExcel = (Stream stream) => new Excel_MiniExcel(stream, false);




        #region GetSheetNames  
        public static List<string> GetSheetNames(Stream stream)
        {
            using var excel = GetExcel(stream);
            return excel.GetSheetNames();
        }
        public static List<string> GetSheetNames(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return GetSheetNames(stream);
        }
        #endregion



        #region ReadSheet


        public static List<Model> ReadSheetByModel<Model>(Stream stream, string sheetName) where Model : class, new()
        {
            using var excel = GetExcel(stream);
            return excel.ReadSheetByModel<Model>(sheetName).ToList();
        }
        public static List<Model> ReadSheetByModel<Model>(string filePath, string sheetName) where Model : class, new()
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return ReadSheetByModel<Model>(stream, sheetName);
        }




        public static List<IDictionary<string, object>> ReadSheetByDictionary(Stream stream, string sheetName)
        {
            using var excel = GetExcel(stream);
            return excel.ReadSheetByDictionary(sheetName).ToList();
        }
        public static List<IDictionary<string, object>> ReadSheetByDictionary(string filePath, string sheetName)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return ReadSheetByDictionary(stream, sheetName);
        }

        #endregion

        #region SaveSheet
        public static void SaveSheet(Stream stream, IEnumerable<SheetData> sheets)
        {
            using var excel = GetExcel(stream);
            excel.SaveSheet(sheets);
        }
        public static void SaveSheet(string filePath, IEnumerable<SheetData> sheets)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            SaveSheet(stream, sheets);
        }


        public static void SaveSheet(Stream stream, SheetData sheet)
        {
            using var excel = GetExcel(stream);
            excel.SaveSheet(sheet);
        }
        public static void SaveSheet(string filePath, SheetData sheet)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            SaveSheet(stream, sheet);
        }
        #endregion


    }
}
