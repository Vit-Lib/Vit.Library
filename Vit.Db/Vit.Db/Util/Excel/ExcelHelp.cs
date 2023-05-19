using System.Data;
using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using Vit.Extensions;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Db.Util.Excel
{
    public class ExcelHelp
    {


        #region SaveDataReader

        public static int SaveDataReader(string filePath, IDataReader dr,string tableName, bool firstRowIsColumnName = true, int startRowIndex = 0)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                // 添加worksheet
                ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sheet => sheet.Name == tableName)
                    ?? package.Workbook.Worksheets.Add(tableName);

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
                    if(dr.IsDateTime(t))
                        worksheet.Column(t+1).Style.Numberformat.Format = "yyyy-MM-dd hh:mm:ss";
                }

                worksheet.Cells.AutoFitColumns();

                package.Save();

                return worksheet.Dimension.Rows - startRowIndex - (firstRowIsColumnName ? 1 : 0);
            }
        }
        #endregion



        #region SaveDataTable

        public static void SaveDataTable(string filePath, DataTable dt, bool firstRowIsColumnName = true, int startRowIndex = 0)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
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
                        worksheet.Column(t+1).Style.Numberformat.Format = "yyyy-MM-dd hh:mm:ss";
                }

                worksheet.Cells.AutoFitColumns();


                package.Save();
            }
        }
        #endregion



        #region SaveDataSet

        public static void SaveDataSet(string filePath, DataSet ds, bool firstRowIsColumnName = true)
        {             
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
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
        }

        #endregion





        #region ReadData

        public static List<string> GetAllTableName(string filePath)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                return package.Workbook.Worksheets.AsQueryable().Select(m=>m.Name).ToList();
            }
        }

        public static List<int> GetAllTableRowCount(string filePath, bool firstRowIsColumnName = true)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                return package.Workbook.Worksheets.AsQueryable()
                    .Select(sheet =>  sheet.Dimension.Rows-(firstRowIsColumnName?1:0) )
                    .ToList();
            }
        }


        public static int GetTableCount(string filePath)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                return package.Workbook.Worksheets.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static DataSet ReadData(string filePath,bool firstRowIsColumnName=true,bool readOriginalValue=false)
        {
            var ds = new DataSet();
            DataTable dt;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                var count = package.Workbook.Worksheets.Count;
                for (int k = 1; k <= count; k++)  //worksheet是从1开始的
                {
                    var worksheet = package.Workbook.Worksheets[k];                     

                
                    if (readOriginalValue)
                    {
                        dt = WorksheetToDataTable_GetOriValue(worksheet, firstRowIsColumnName);
                    }
                    else 
                    {
                        dt = WorksheetToDataTable(worksheet, firstRowIsColumnName);
                    }
                    ds.Tables.Add(dt);                    
                }
            }
            return ds;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tableIndex">从0开始</param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static DataTable ReadTable(string filePath, int tableIndex, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue, bool readOriginalValue = false)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {            
                //worksheet是从1开始的
                var worksheet = package.Workbook.Worksheets[tableIndex + 1];

                if(readOriginalValue)
                    return WorksheetToDataTable_GetOriValue(worksheet,firstRowIsColumnName, rowOffset, maxRowCount);
                else
                    return WorksheetToDataTable(worksheet, firstRowIsColumnName, rowOffset, maxRowCount);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tableName"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static DataTable ReadTable(string filePath, string tableName, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue, bool readOriginalValue = false)
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {                
                var worksheet = package.Workbook.Worksheets.AsQueryable().Where(m => m.Name== tableName).FirstOrDefault(); 

                if (readOriginalValue)
                    return WorksheetToDataTable_GetOriValue(worksheet, firstRowIsColumnName, rowOffset, maxRowCount);
                else
                    return WorksheetToDataTable(worksheet, firstRowIsColumnName, rowOffset, maxRowCount);
            }
        }

        #region WorksheetToDataTable

      

        /// <summary>
        /// 获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        /// <returns></returns>
        static DataTable WorksheetToDataTable_GetOriValue(ExcelWorksheet worksheet, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue)
        {
            if (worksheet == null) return null;

            var dt = new DataTable(worksheet.Name);

            int sourceRowCount = worksheet.Dimension.Rows;
            int sourceColCount = worksheet.Dimension.Columns;
            var cells = worksheet.Cells;

            int rowIndex = rowOffset;

            #region (x.x.1) Column
            if (firstRowIsColumnName)
            {
                rowIndex += 2;
                for (int i = 0; i < sourceColCount; i++)
                {
                    var colName = cells[1, i + 1].Value?.ToString();
                    var type = typeof(string);
                    //try
                    //{
                    //    type = cells[1, 1].Value.GetType();
                    //}
                    //catch (System.Exception ex)
                    //{
                    //}
                    dt.Columns.Add(colName, type);
                }
            }
            else
            {
                rowIndex += 1;
                for (int i = 0; i < sourceColCount; i++)
                {
                    var colName = "column" + (i + 1);
                    var type = typeof(string);
                    dt.Columns.Add(colName, type);
                }
            }

            #endregion

            #region (x.x.2) row                    
            for (; rowIndex <= sourceRowCount && maxRowCount>0; rowIndex++, maxRowCount--)
            {
                var rowValue = new object[sourceColCount];
                for (int colIndex = 1; colIndex <= sourceColCount; colIndex++)
                {                     
                    //rowValue[colIndex - 1] = "" + cells[rowIndex, colIndex].Value;
                    rowValue[colIndex - 1] = cells[rowIndex, colIndex].Value;
                }
                dt.Rows.Add(rowValue);
            }
            #endregion
            return dt;
        }



        /// <summary>
        /// 获取xls内cell中的Text值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="rowOffset"></param>
        /// <param name="maxRowCount"></param>
        /// <returns></returns>
        static DataTable WorksheetToDataTable(ExcelWorksheet worksheet, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue)
        {
            if (worksheet == null) return null;

            var dt = new DataTable(worksheet.Name);

            int sourceRowCount = worksheet.Dimension.Rows;
            int sourceColCount = worksheet.Dimension.Columns;
            var cells = worksheet.Cells;

            int rowIndex = rowOffset;

            #region (x.x.1) Column
            if (firstRowIsColumnName)
            {
                rowIndex += 2;
                for (int i = 0; i < sourceColCount; i++)
                {
                    var colName = cells[1, i + 1].Text?.ToString();
                    var type = typeof(string);                   
                    dt.Columns.Add(colName, type);
                }
            }
            else
            {
                rowIndex += 1;
                for (int i = 0; i < sourceColCount; i++)
                {
                    var colName = "column" + (i + 1);
                    var type = typeof(string);
                    dt.Columns.Add(colName, type);
                }
            }

            #endregion

            #region (x.x.2) row                    
            for (; rowIndex <= sourceRowCount && maxRowCount > 0; rowIndex++, maxRowCount--)
            {
                var rowValue = new object[sourceColCount];
                for (int colIndex = 1; colIndex <= sourceColCount; colIndex++)
                {            
                    rowValue[colIndex - 1] = cells[rowIndex, colIndex].Text;
                }
                dt.Rows.Add(rowValue);
            }
            #endregion
            return dt;
        }
        #endregion



        #endregion
    }
}
