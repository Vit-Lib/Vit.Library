using System.Collections.Generic;
using System.Data;

namespace Vit.Excel
{
    public class SheetData
    {
        public string sheetName { get; protected set; }
        public object sheet { get; protected set; }
        public string[] columnNames { get; protected set; }

        protected SheetData() { }

        public static SheetData Enumerable(string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames)
        {
            return new SheetData
            {
                sheetName = sheetName,
                sheet = sheet,
                columnNames = columnNames
            };
        }


        public static SheetData Dictionary(string sheetName, IEnumerable<IDictionary<string, object>> sheet, string[] columnNames = null)
        {
            return new SheetData
            {
                sheetName = sheetName,
                sheet = sheet,
                columnNames = columnNames
            };
        }

        public static SheetData DataTable(string sheetName, DataTable table)
        {
            return new SheetData
            {
                sheetName = sheetName,
                sheet = table
            };
        }

        public static SheetData DataReader(string sheetName, IDataReader reader)
        {
            return new SheetData
            {
                sheetName = sheetName,
                sheet = reader
            };
        }

        public static SheetData Model<M>(string sheetName, IEnumerable<M> sheet, string[] columnNames = null)
               where M : class, new()
        {
            return new SheetData
            {
                sheetName = sheetName,
                sheet = sheet,
                columnNames = columnNames
            };
        }
    }


}
