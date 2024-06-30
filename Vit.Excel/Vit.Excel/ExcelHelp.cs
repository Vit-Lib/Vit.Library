using System;
using System.Collections.Generic;
using System.Data;
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


        #region GetColumns
        public static List<string> GetColumns(Stream stream, string sheetName = null)
        {
            using var excel = GetExcel(stream);
            return excel.GetColumns(sheetName);
        }
        public static List<string> GetColumns(string filePath, string sheetName = null)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return GetColumns(stream, sheetName);
        }
        #endregion




        #region Read

   

        #region ReadModel
        public static List<Model> ReadModel<Model>(Stream stream, string sheetName) where Model : class, new()
        {
            using var excel = GetExcel(stream);
            return excel.ReadModel<Model>(sheetName).ToList();
        }
        public static List<Model> ReadAsModel<Model>(string filePath, string sheetName) where Model : class, new()
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return ReadModel<Model>(stream, sheetName);
        }
        #endregion


        #region ReadDictionary
        public static List<IDictionary<string, object>> ReadDictionary(Stream stream, string sheetName)
        {
            using var excel = GetExcel(stream);
            return excel.ReadDictionary(sheetName).ToList();
        }
        public static List<IDictionary<string, object>> ReadDictionary(string filePath, string sheetName)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return ReadDictionary(stream, sheetName);
        }
        #endregion

        #region ReadArray
        public static (List<string> columnNames, IEnumerable<object[]> rows) ReadArray(Stream stream, string sheetName)
        {
            using var excel = GetExcel(stream);
            return excel.ReadArray(sheetName);
        }
        public static (List<string> columnNames, IEnumerable<object[]> rows) ReadArray(string filePath, string sheetName)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return ReadArray(stream, sheetName);
        }
        #endregion

        #region ReadDataTable
        public static DataTable ReadDataTable(Stream stream, string sheetName)
        {
            using var excel = GetExcel(stream);
            return excel.ReadDataTable(sheetName);
        }
        public static DataTable ReadDataTable(string filePath, string sheetName)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            return ReadDataTable(stream, sheetName);
        }
        #endregion

        #endregion





        #region SaveSheet
        public static void SaveSheets(Stream stream, IEnumerable<SheetData> sheets)
        {
            using var excel = GetExcel(stream);
            excel.SaveSheets(sheets);
        }
        public static void SaveSheets(string filePath, IEnumerable<SheetData> sheets)
        {
            using var stream = new FileStream(filePath, FileMode.OpenOrCreate);
            SaveSheets(stream, sheets);
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


        #region DataReader
        public static void SaveSheet(string filePath, string sheetName, IDataReader reader)
        {
            SaveSheet(filePath, SheetData.DataReader(sheetName, reader));
        }
        #endregion


    }
}
