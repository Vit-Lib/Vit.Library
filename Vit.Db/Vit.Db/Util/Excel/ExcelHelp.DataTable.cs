using System.Data;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using Vit.Extensions;
using Vit.Extensions.Linq_Extensions;
using System;
using Vit.Db.Module.Schema;

namespace Vit.Db.Util.Excel
{
    public partial class ExcelHelp
    {

        #region Read

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static DataSet ReadDataSet(Stream stream, bool firstRowIsColumnName = true, bool readOriginalValue = false)
        {
            var ds = new DataSet();
            using ExcelPackage package = new ExcelPackage(stream);

            //var count = package.Workbook.Worksheets.Count;
            //var worksheet = package.Workbook.Worksheets[k]; //worksheet是从1开始的
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                var dt = ReadDataTable(worksheet, firstRowIsColumnName: firstRowIsColumnName, readOriginalValue: readOriginalValue);
                ds.Tables.Add(dt);
            }
            return ds;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sheetIndex">start from 0</param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static DataTable ReadDataTable(Stream stream, int sheetIndex = 0, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue, bool readOriginalValue = false)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            //worksheet是从1开始的
            var worksheet = package.Workbook.Worksheets[sheetIndex + 1];

            return ReadDataTable(worksheet, firstRowIsColumnName: firstRowIsColumnName, readOriginalValue: readOriginalValue);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="sheetName"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static DataTable ReadDataTable(Stream stream, string sheetName, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue, bool readOriginalValue = false)
        {
            using ExcelPackage package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets.AsQueryable().Where(m => m.Name == sheetName).FirstOrDefault();
            if (worksheet == null) return null;

            return ReadDataTable(worksheet, firstRowIsColumnName: firstRowIsColumnName, readOriginalValue: readOriginalValue);
        }

        static DataTable ReadDataTable(ExcelWorksheet worksheet, bool firstRowIsColumnName = true, bool readOriginalValue = false)
        {
            var dt = new DataTable();
            Action<TableSchema> OnGetSchema = (schema) =>
            {
                dt.TableName = schema.table_name;
                schema.columns.ForEach(col => dt.Columns.Add(col.column_name, col.column_clr_type));
            };
            Action<Object[]> AppendRow = row =>
            {
                dt.Rows.Add(row);
            };

            ReadSheet(worksheet, OnGetSchema: OnGetSchema, AppendRow: AppendRow, readOriginalValue: readOriginalValue, firstRowIsColumnName: firstRowIsColumnName);
            return dt;
        }

        #endregion



        #region Save

        #region SaveDataReader

        public static int SaveDataReader(Stream stream, IDataReader dr, string sheetName, bool firstRowIsColumnName = true, int startRowIndex = 0)
        {
            using ExcelPackage package = new ExcelPackage(stream);

            // 添加worksheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == sheetName)
                ?? package.Workbook.Worksheets.Add(sheetName);

            ExcelRangeBase cells;

            if (startRowIndex == 0)
                cells = worksheet.Cells;
            else
                cells = worksheet.Cells.Offset(startRowIndex, 0);

            var dt = dr.ReadDataTable();
            cells.LoadFromDataTable(dt, true);
            //cells.LoadFromDataReader(dr, firstRowIsColumnName);

            //格式化时间
            for (var t = 0; t < dr.FieldCount; t++)
            {
                if (dr.IsDateTime(t))
                    worksheet.Column(t + 1).Style.Numberformat.Format = "yyyy-MM-dd hh:mm:ss";
            }

            worksheet.Cells.AutoFitColumns();

            package.Save();

            return worksheet.Dimension.Rows - startRowIndex - (firstRowIsColumnName ? 1 : 0);

        }
        #endregion



        #region SaveDataTable

        public static void SaveDataTable(Stream stream, DataTable dt, bool firstRowIsColumnName = true, int startRowIndex = 0)
        {
            using ExcelPackage package = new ExcelPackage(stream);

            // 添加worksheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == dt.TableName)
                ?? package.Workbook.Worksheets.Add(dt.TableName);

            ExcelRangeBase cells;

            if (startRowIndex == 0)
                cells = worksheet.Cells;
            else
                cells = worksheet.Cells.Offset(startRowIndex, 0);

            cells.LoadFromDataTable(dt, firstRowIsColumnName);


            //格式化时间
            for (var t = 0; t < dt.Columns.Count; t++)
            {
                //dt.Columns[t].DataType..IsAssignableFrom(typeof(DateTime)
                if (dt.Columns[t].DataType.Name.ToLower().Contains("datetime"))
                    worksheet.Column(t + 1).Style.Numberformat.Format = "yyyy-MM-dd hh:mm:ss";
            }

            worksheet.Cells.AutoFitColumns();


            package.Save();

        }
        #endregion



        #region SaveDataSet

        public static void SaveDataSet(Stream stream, DataSet ds, bool firstRowIsColumnName = true)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            foreach (DataTable dt in ds.Tables)
            {
                // 添加worksheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == dt.TableName)
                        ?? package.Workbook.Worksheets.Add(dt.TableName);


                worksheet.Cells.LoadFromDataTable(dt, firstRowIsColumnName);

                /*/
                continue;

                #region (x.x.1) 保存Column
                int colIndex = 0;
                foreach (DataColumn column in dt.Columns)
                {
                    colIndex++;

                    worksheet.Cells[1, colIndex].Value = column.ColumnName;
                    worksheet.Cells[1, colIndex].Style.Font.Bold = true;
                }
                #endregion

                #region (x.x.2) 保存 rows
                int rowIndex = 1;
                foreach (DataRow row in dt.Rows)
                {
                    rowIndex++;

                    for (colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                    {
                        worksheet.Cells[rowIndex, colIndex].Value = row[colIndex];
                    }
                }
                #endregion


                //*/
            }
            package.Save();

        }

        #endregion

        #endregion


    }
}
