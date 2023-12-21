using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

using OfficeOpenXml;

using Vit.Extensions;
using Vit.Extensions.Json_Extensions;

namespace Vit.Excel
{
    public class Excel_EPPlus : IExcel
    {
        public bool useHeaderRow { get; set; } = true;

        #region readOriginalValue
        Func<ExcelRange, object> GetCellValue = GetCellValue_Auto;
        public bool? _readOriginalValue = true;


        /// <summary>
        /// <para> readOriginalValue, 默认false,获取cell的Text                                                     </para>
        /// <para>     null: try to read date and time    </para>
        /// <para>     true: 获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// <para>    false: 获取xls内cell中的Text值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// </summary>
        public bool? readOriginalValue
        {
            get => _readOriginalValue;
            set
            {
                _readOriginalValue = value;
                if (_readOriginalValue == null) { GetCellValue = GetCellValue_Auto; }
                else if (_readOriginalValue.Value) { GetCellValue = GetCellValue_Value; }
                else { GetCellValue = GetCellValue_Text; }
            }
        }
        static object GetCellValue_Value(ExcelRange cell) => cell.Value;
        static object GetCellValue_Text(ExcelRange cell) => cell.Text;
        static object GetCellValue_Auto(ExcelRange cell)
        {
            var Format = cell.Style?.Numberformat?.Format;
            if (string.IsNullOrEmpty(Format) || Format == "General" || Format == "general") return cell.Value;
            var value = cell.Text;
            try
            {
                if (DateTime.TryParse(value, out var time)) return time;
            }
            catch
            {
            }
            return value;
        }
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

            if (needDisposeStream && stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }




        public Excel_EPPlus(Stream stream, bool needDisposeStream = false)
        {
            this.stream = stream;
            this.needDisposeStream = needDisposeStream;

            package = new ExcelPackage(stream);
        }
        public Excel_EPPlus(string filePath) : this(new FileStream(filePath, FileMode.OpenOrCreate), true)
        {
        }



        #region SaveSheet

        public void Save()
        {
            package.Save();
        }


        #region #1 Enumerable
        static bool IsDateTime(object value)
        {
            return value != null && Type.GetTypeCode(value.GetType().GetUnderlyingTypeIfNullable()) == TypeCode.DateTime;
        }

        public void AddSheetByEnumerable(string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames = null)
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(worksheet => worksheet.Name == sheetName)
                    ?? package.Workbook.Worksheets.Add(sheetName);

            int rowIndex = 0;

            #region #1 columns
            if (columnNames != null)
            {
                rowIndex++;
                int colIndex = 0;
                foreach (var column in columnNames)
                {
                    colIndex++;

                    worksheet.Cells[rowIndex, colIndex].Value = column;
                    worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
                }
            }
            #endregion

            #region #2 column format
            var firstRow = sheet.FirstOrDefault();
            if (firstRow != null)
            {
                int colIndex = 0;
                foreach (var cell in firstRow)
                {
                    colIndex++;
                    if (IsDateTime(cell))
                    {
                        worksheet.Column(colIndex).Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                    }
                }
            }
            #endregion


            #region #3 rows
            foreach (var row in sheet)
            {
                rowIndex++;
                int colIndex = 0;
                foreach (var cell in row)
                {
                    colIndex++;
                    worksheet.Cells[rowIndex, colIndex].Value = cell;
                }
            }
            #endregion

            worksheet.Cells.AutoFitColumns();
        }
        #endregion

        #region #2 Dictionary
        public void AddSheetByDictionary(string sheetName, IEnumerable<IDictionary<string, object>> sheet, string[] columnNames = null)
        {
            if (columnNames == null) columnNames = sheet.FirstOrDefault()?.Keys.ToArray() ?? new string[] { };
            IEnumerable<IEnumerable<object>> sheetEnumerable = sheet.Select(row => columnNames.Select(name => row[name]));
            AddSheetByEnumerable(sheetName, sheetEnumerable, columnNames);
        }

        #endregion

        #region #3 Model
        public void AddSheetByQueryable(string sheetName, IQueryable sheet, string[] columnNames = null)
        {
            var type = sheet.ElementType;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var valueGetterList =
                fields.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row))))
                .Union(properties.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row)))));

            if (columnNames == null) columnNames = valueGetterList.Select(m => m.Name).Distinct().ToArray();

            var cellGetters = columnNames.Select(name => valueGetterList.FirstOrDefault(getter => getter.Name == name).Item2).ToList();

            Func<object, IEnumerable<object>> GetRow = row => cellGetters.Select(getter => getter?.Invoke(row));


            IEnumerable<IEnumerable<object>> cellRows = GetRowEnumerable(sheet, GetRow);
            AddSheetByEnumerable(sheetName, cellRows, columnNames);

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

        public void AddSheetByModel<Model>(string sheetName, IEnumerable<Model> sheet, string[] columnNames = null)
            where Model : class
        {
            IQueryable queryable = sheet.AsQueryable();
            AddSheetByQueryable(sheetName, queryable, columnNames);
        }
        #endregion

        #region #4 DataTable

        public void AddSheetByDataTable(DataTable sheet, string sheetName = null)
        {
            if (sheetName == null) sheetName = sheet.TableName;

            var columnNames = new List<string>();
            foreach (DataColumn col in sheet.Columns) { columnNames.Add(col.ColumnName); }

            var row = GetRowEnumerable(sheet);

            AddSheetByEnumerable(sheetName, row, columnNames.ToArray());
        }


        static IEnumerable<IEnumerable<object>> GetRowEnumerable(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                yield return row.ItemArray;
            }
        }
        #endregion

        #endregion







        #region Read


        #region #1 Enumerable

        public (List<string> columnNames, IEnumerable<object[]> rows) ReadSheetByEnumerable(string sheetName)
        {
            // worksheets start with 1
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == sheetName);
            if (worksheet == null) return default;

            int sourceRowCount = worksheet.Dimension.Rows;
            int sourceColCount = worksheet.Dimension.Columns;
            var cells = worksheet.Cells;

            int rowIndex = 0;

            List<string> columnNames = new List<string>();
            #region get column
            if (useHeaderRow)
            {
                for (int i = 0; i < sourceColCount; i++)
                {
                    var cell = cells[1, i + 1];
                    //var colName = GetCellValue(cell)?.ToString();
                    var colName = cell.Text;
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
                for (; rowIndex <= sourceRowCount; rowIndex++)
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
        public IEnumerable<IDictionary<string, object>> ReadSheetByDictionary(string sheetName, out List<string> columnNames)
        {
            (columnNames, IEnumerable<object[]> rows) = ReadSheetByEnumerable(sheetName);
            if (columnNames == null) return default;

            List<(string name, int index)> nameIndexMap = columnNames.Select((name, index) => (name, index)).ToList();
            IEnumerable<IDictionary<string, object>> rows_ = rows.Select(row => nameIndexMap.ToDictionary(item => item.name, item => row[item.index]));
            return rows_;
        }
        #endregion


        #region #3 Model
        public IEnumerable<Model> ReadSheetByModel<Model>(string sheetName)
            where Model : class, new()
        {
            (List<string> columnNames, IEnumerable<object[]> rows) = ReadSheetByEnumerable(sheetName);
            if (columnNames == null) return default;

            var type = typeof(Model);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<(string Name, Action<object, object> SetValue, Type FieldType)> columnList =
                    fields.Select(m => (m.Name, (Action<object, object>)m.SetValue, m.FieldType))
                    .Union(properties.Select(m => (m.Name, (Action<object, object>)m.SetValue, m.PropertyType)))
                    .ToList();

            List<(string Name, Action<object, object> Setter)> valueSetterList = columnList
                .Select(column => (column.Name, Excel_MiniExcel.Model_BuildSetter(column.SetValue, column.FieldType)))
                .ToList();

            var cellSetters = columnNames.Select(name => valueSetterList.FirstOrDefault(item => item.Name == name).Setter).ToArray();

            IEnumerable<Model> rows_ = rows.Select(CellToModel);
            return rows_;

            #region Method CellToModel
            Model CellToModel(object[] cells)
            {
                var row = new Model();
                for (var i = 0; i < cells.Length; i++)
                    cellSetters[i]?.Invoke(row, cells[i]);
                return row;
            }
            #endregion
        }



        #endregion


        #endregion


        #region Read SheetInfo


        public List<string> GetSheetNames()
        {
            return package.Workbook.Worksheets.AsQueryable().Select(m => m.Name).ToList();
        }

        public int GetSheetRowCount(int sheetIndex)
        {
            var sheet = package.Workbook.Worksheets[sheetIndex + 1];
            return sheet.Dimension.Rows - (useHeaderRow ? 1 : 0);
        }

        public int GetSheetRowCount(string sheetName)
        {
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == sheetName);
            if (worksheet == null) return default;
            return worksheet.Dimension.Rows - (useHeaderRow ? 1 : 0);
        }

        #endregion




    }
}
