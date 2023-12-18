using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


namespace Vit.Excel
{
    public interface IExcel:IDisposable
    {
        void Save();
        void AddSheetByEnumerable(string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames);

        void AddSheetByDictionary(string sheetName, IEnumerable<IDictionary<string, object>> sheet, string[] columnNames = null);

        void AddSheetByModel<Model>(string sheetName, IEnumerable<Model> sheet, string[] columnNames = null) where Model : class;
        void AddSheetByDataTable(DataTable sheet, string sheetName = null);


        (List<string> columnNames, IEnumerable<object[]> rows) ReadSheetByEnumerable(string sheetName);
        (List<string> columnNames, IEnumerable<IDictionary<string, object>> rows) ReadSheetByDictionary(string sheetName);

        IEnumerable<Model> ReadSheetByModel<Model>(string sheetName) where Model : class, new();




        List<string> GetSheetNames();


    }
}
