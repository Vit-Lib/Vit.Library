using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using OfficeOpenXml;

namespace Vit.Excel
{
    public class Excel_EPPlus : IDisposable
    {
        public bool firstRowIsColumnName { get; set; } = true;
        public int rowOffset { get; set; } = 0;
        public int maxRowCount { get; set; } = int.MaxValue;

        #region readOriginalValue
        Func<ExcelRange, object> GetCellValue = GetCellValue_Value;
        public bool _readOriginalValue = true;


        /// <summary>
        /// <para> readOriginalValue, 默认false,获取cell的Text                                                     </para>
        /// <para>     true: 获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// <para>    false: 获取xls内cell中的Text值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// </summary>
        public bool readOriginalValue
        {
            get => _readOriginalValue;
            set
            {
                _readOriginalValue = value;
                GetCellValue = _readOriginalValue ? GetCellValue_Value : GetCellValue_Text;
            }
        }
        static object GetCellValue_Value(ExcelRange cell) => cell.Value;
        static object GetCellValue_Text(ExcelRange cell) => cell.Text;
        #endregion



        Stream stream = null;
        bool needDisposeStream = false;

        ExcelPackage package = null;

        public void Dispose()
        {
            if (package != null)
            {
                package.Dispose(); 
                package = null;
            }

            if (stream != null && needDisposeStream)
            {
                stream.Dispose();
                stream = null;
            }
        }


        public Excel_EPPlus(string filePath) : this(new FileStream(filePath, FileMode.OpenOrCreate), true)
        {
        }

        public Excel_EPPlus(Stream stream, bool needDisposeStream = false) 
        {
            this.stream = stream;
            this.needDisposeStream = needDisposeStream;

            package = new ExcelPackage(stream);
        }


        #region Get sheets info

        public int GetSheetsCount()
        {
            return package.Workbook.Worksheets.Count;
        }
       
        public List<string> GetSheetsName()
        {
            return package.Workbook.Worksheets.AsQueryable().Select(m => m.Name).ToList();
        }

        public string GetSheetName(int sheetIndex)
        {
            var sheet = package.Workbook.Worksheets[sheetIndex + 1];
            return sheet?.Name;
        }
        public int GetSheetRowCount(int sheetIndex)
        {
            var sheet = package.Workbook.Worksheets[sheetIndex + 1];
            return sheet.Dimension.Rows - (firstRowIsColumnName ? 1 : 0);
        }


        #endregion

 

        #region Write


        #region #1 Cell Array
        public void SaveSheet(string sheetName, IEnumerable<IEnumerable<object>> rows, List<string> columns = null)
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == sheetName)
                    ?? package.Workbook.Worksheets.Add(sheetName);

            int rowIndex = 0;

            #region #1 columns
            if (columns != null)
            {
                rowIndex++;
                int colIndex = 0;
                foreach (var column in columns)
                {
                    colIndex++;

                    worksheet.Cells[rowIndex, colIndex].Value = column;
                    worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
                }

            }
            #endregion

            #region #2 rows
            foreach (var row in rows)
            {
                rowIndex++;
                int colIndex = 0;
                foreach (var cell in row)
                {
                    worksheet.Cells[rowIndex, colIndex].Value = cell;
                    colIndex++;
                }
            }
            #endregion

            package.Save();
        }
        #endregion

        #region #2 Dictionary
        public void SaveSheet(string sheetName, IEnumerable<IDictionary<string, object>> rows, List<string> columns = null)
        {
            IEnumerable<IEnumerable<object>> cellRows = rows.Select(row => { if (columns == null) columns = row.Keys.ToList(); return columns.Select(name => row[name]); });
            SaveSheet(sheetName, cellRows, columns);
        }

        #endregion

        #region #2 Model
        public void SaveSheet(string sheetName, IQueryable rows, List<string> columns = null)
        {
            var type = rows.ElementType;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var valueGetterList =
                fields.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row))))
                .Union(properties.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row)))));

            if (columns == null) columns = valueGetterList.Select(m => m.Name).Distinct().ToList();

            var cellGetters = columns.Select(name => valueGetterList.FirstOrDefault(getter => getter.Name == name).Item2).ToList();

            Func<object, IEnumerable<object>> GetRow = row => cellGetters.Select(getter => getter?.Invoke(row));


            IEnumerable<IEnumerable<object>> cellRows = GetRowEnumerable(rows, GetRow);
            SaveSheet(sheetName, cellRows, columns);

            #region Method GetRowEnumerable
            IEnumerable<IEnumerable<object>> GetRowEnumerable(IQueryable rows, Func<object, IEnumerable<object>> GetRow)
            {
                foreach (var row in rows)
                {
                    yield return GetRow(row);
                }
            }
            #endregion
        }

        public void SaveSheet<Model>(string sheetName, IEnumerable<Model> rows, List<string> columns = null)
            where Model : class
        {
            IQueryable queryable = rows.AsQueryable();
            SaveSheet(sheetName, queryable, columns);
        }
        #endregion

        #endregion


        #region Read


        #region #1 Cell Array

        public (List<string> columnNames, IEnumerable<object[]> rows) ReadSheetByCell(int sheetIndex)
        {
            // worksheets start with 1
            var worksheet = package.Workbook.Worksheets[sheetIndex];
            if (worksheet == null) return default;

            int sourceRowCount = worksheet.Dimension.Rows;
            int sourceColCount = worksheet.Dimension.Columns;
            var cells = worksheet.Cells;

            int rowIndex = rowOffset;

            List<string> columnNames = new List<string>();
            #region get column
            if (firstRowIsColumnName)
            {
                for (int i = 0; i < sourceColCount; i++)
                {
                    var cell = cells[1, i + 1];
                    var colName = GetCellValue(cell)?.ToString();
                    //var colName = cells[1, i + 1].Text?.ToString();
                    //var colName = cells[1, i + 1].Value?.ToString();
                    //var type = typeof(string);
                    //try
                    //{
                    //    type = cells[1, 1].Value.GetType();
                    //}
                    //catch (System.Exception ex)
                    //{
                    //}
                    columnNames.Add(colName);
                }
                rowIndex += 2;
            }
            else
            {
                for (int i = 0; i < sourceColCount; i++)
                {
                    var colName = "column" + (i + 1);
                    //var type = typeof(string);
                    columnNames.Add(colName);
                }
                rowIndex += 1;
            }
            #endregion


            return (columnNames, GetRows());

            #region Method GetRows
            IEnumerable<object[]> GetRows()
            {
                for (; rowIndex <= sourceRowCount && maxRowCount > 0; rowIndex++, maxRowCount--)
                {
                    var rowValue = new object[sourceColCount];
                    for (int colIndex = 1; colIndex <= sourceColCount; colIndex++)
                    {
                        var cell = cells[rowIndex, colIndex];
                        var cellValue = GetCellValue(cell);
                        rowValue[colIndex - 1] = cellValue;
                    }
                    yield return rowValue;
                }
            }
            #endregion
        }

        #endregion


        #region #2 Dictionary
        public (List<string> columnNames, IEnumerable<Dictionary<string,object>> rows) ReadSheetByDictionary(int sheetIndex) 
        {
            (List<string> columnNames, IEnumerable<object[]> rows) = ReadSheetByCell(sheetIndex);
            if (columnNames == null) return default;

            List<(string name,int index)> nameIndexMap=columnNames.Select((name,index)=>(name,index)).ToList();
            IEnumerable<Dictionary<string, object>> rows_ = rows.Select(row => nameIndexMap.ToDictionary(item => item.name, item => row[item.index]));
            return (columnNames, rows_);
        }
        #endregion


        #region #3 Model
        public (List<string> columnNames, IEnumerable<Model> rows) ReadSheet<Model>(int sheetIndex)
            where Model:class,new()
        {
            (List<string> columnNames, IEnumerable<object[]> rows) = ReadSheetByCell(sheetIndex);
            if (columnNames == null) return default;

            var type = typeof(Model);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var valueSetterList =
                fields.Select(m => (m.Name, (Action<object, object>)m.SetValue))
                 .Union(properties.Select(m => (m.Name, (Action<object, object>)m.SetValue)));

            var cellSetters = columnNames.Select(name => valueSetterList.FirstOrDefault(item => item.Name == name).Item2).ToArray();

            List<(string name, int index)> nameIndexMap = columnNames.Select((name, index) => (name, index)).ToList();
            IEnumerable<Model> rows_ = rows.Select(CellToModel);
            return (columnNames, rows_);

            #region Method CellToModel
            Model CellToModel(object[] cells) 
            {
                var row=new Model();
                for (var i = 0; i < cells.Length; i++) 
                    cellSetters[i]?.Invoke(row, cells[i]);
                return row;
            }
            #endregion

        }
        #endregion


        #endregion




    }
}
