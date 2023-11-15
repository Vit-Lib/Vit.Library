using System.Data;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using Vit.Extensions;
using Vit.Extensions.Linq_Extensions;
using System;
using Vit.Db.Module.Schema;
using System.Collections;

namespace Vit.Db.Util.Excel
{
    public partial class ExcelHelp
    {



        #region Read

        public static List<string> ReadSheetsName(Stream stream)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            return package.Workbook.Worksheets.AsQueryable().Select(m => m.Name).ToList();
        }

        public static List<int> ReadSheetsRowCount(Stream stream, bool firstRowIsColumnName = true)
        {
            using ExcelPackage package = new ExcelPackage(stream);

            return package.Workbook.Worksheets.AsQueryable()
                .Select(sheet => sheet.Dimension.Rows - (firstRowIsColumnName ? 1 : 0))
                .ToList();
        }


        public static int ReadSheetsCount(Stream stream)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            return package.Workbook.Worksheets.Count;
        }


        #region ReadSheet

        /// <summary>
        /// <para> readOriginalValue, 默认false,获取cell的Text                                                     </para>
        /// <para>     true: 获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// <para>    false: 获取xls内cell中的Text值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="OnGetSchema"></param>
        /// <param name="AppendRow"></param>
        /// <param name="readOriginalValue"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        static void ReadSheet(ExcelWorksheet worksheet, Action<TableSchema> OnGetSchema, Action<Object[]> AppendRow, bool readOriginalValue = false, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue)
        {
            Func<ExcelRange, object> GetCellValue = readOriginalValue ? cell => cell.Value : cell => cell.Text;
            ReadSheet(worksheet, OnGetSchema, AppendRow, GetCellValue, firstRowIsColumnName, rowOffset, maxRowCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="OnGetSchema"></param>
        /// <param name="AppendRow"></param>
        /// <param name="GetCellValue"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        static void ReadSheet(ExcelWorksheet worksheet, Action<TableSchema> OnGetSchema, Action<Object[]> AppendRow, Func<ExcelRange, object> GetCellValue, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue)
        {
            if (worksheet == null) return;

            int sourceRowCount = worksheet.Dimension.Rows;
            int sourceColCount = worksheet.Dimension.Columns;
            var cells = worksheet.Cells;

            int rowIndex = rowOffset;

            #region #1 get schema
            var schema = new TableSchema { table_name = worksheet.Name, columns = new List<ColumnSchema>() };

            if (firstRowIsColumnName)
            {
                rowIndex += 2;
                for (int i = 0; i < sourceColCount; i++)
                {
                    var cell = cells[1, i + 1];
                    var colName = GetCellValue(cell)?.ToString();
                    //var colName = cells[1, i + 1].Text?.ToString();
                    //var colName = cells[1, i + 1].Value?.ToString();
                    var type = typeof(string);
                    //try
                    //{
                    //    type = cells[1, 1].Value.GetType();
                    //}
                    //catch (System.Exception ex)
                    //{
                    //}
                    schema.columns.Add(new ColumnSchema { column_name = colName, column_clr_type = type });
                }
            }
            else
            {
                rowIndex += 1;
                for (int i = 0; i < sourceColCount; i++)
                {
                    var colName = "column" + (i + 1);
                    var type = typeof(string);
                    schema.columns.Add(new ColumnSchema { column_name = colName, column_clr_type = type });
                }
            }
            OnGetSchema?.Invoke(schema);
            #endregion

            #region #2 read rows
            for (; rowIndex <= sourceRowCount && maxRowCount > 0; rowIndex++, maxRowCount--)
            {
                var rowValue = new object[sourceColCount];
                for (int colIndex = 1; colIndex <= sourceColCount; colIndex++)
                {
                    var cell = cells[rowIndex, colIndex];
                    var cellValue = GetCellValue(cell);
                    rowValue[colIndex - 1] = cellValue;
                }
                AppendRow(rowValue);
            }
            #endregion
        }
        #endregion



        #endregion



        #region SaveSheets
        public static void SaveSheets(Stream stream, string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows)
        {
            SaveSheets(stream, new List<(string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows)> { (sheetName, columns, rows) });
        }
        public static void SaveSheets(Stream stream, IEnumerable<(string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows)> sheets)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            foreach (var sheet in sheets)
            {
                (string sheetName, List<string> columns, IEnumerable<IEnumerable<object>> rows) = sheet;
                // 添加worksheet
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
            }
            package.Save();
        }
        #endregion

    }
}
