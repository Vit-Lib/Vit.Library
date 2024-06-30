using System;
using System.Collections.Generic;
using System.Data;


namespace Vit.Excel
{
    public interface IExcel:IDisposable
    {
        void Save();
        void AddSheetByArray(string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames);

        void AddSheetByDictionary(string sheetName, IEnumerable<IDictionary<string, object>> sheet, string[] columnNames = null);

        void AddSheetByModel<Model>(string sheetName, IEnumerable<Model> sheet, string[] columnNames = null) where Model : class;
        void AddSheetByDataTable(string sheetName, DataTable sheet);
        void AddSheetByDataReader(string sheetName, IDataReader reader);



        IEnumerable<Model> ReadModel<Model>(string sheetName) where Model : class, new();
        IEnumerable<IDictionary<string, object>> ReadDictionary(string sheetName);
        (List<string> columnNames, IEnumerable<object[]> rows) ReadArray(string sheetName);

        DataTable ReadDataTable(string sheetName);




        List<string> GetSheetNames();
        List<string> GetColumns(string sheetName = null);

    }
}
