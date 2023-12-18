using System.Data;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using Vit.Extensions;
using System;



namespace Vit.Db.Util.Excel
{
    public partial class ExcelHelp
    {


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





    }
}
