using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using OfficeOpenXml;
using Vit.Excel.Model;

namespace Vit.Excel
{
    public interface IExcel
    {
        void SaveSheets(Stream stream, IEnumerable<(string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows)> sheets);


        List<string> ReadSheetsName(Stream stream);
        List<int> ReadSheetsRowCount(Stream stream);
        int ReadSheetsCount(Stream stream);


        bool ReadSheet(Stream stream,ReadArgs args);

    }
}
