using System.IO;
using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using Vit.Extensions.Linq_Extensions;
using System;
using Vit.Db.Module.Schema;
using Vit.Core.Module.Serialization;

namespace Vit.Db.Util.Excel
{
    public partial class ExcelHelp
    {


        #region ReadSheet


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="firstRowIsColumnName"></param>
        /// <param name="readOriginalValue">是否获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"。默认false,获取cell的Text</param>
        /// <returns></returns>
        public static IEnumerable<ExcelSheet> ReadSheets(Stream stream, bool firstRowIsColumnName = true, bool readOriginalValue = false)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            foreach (var worksheet in package.Workbook.Worksheets)
            {
                var sheet = ReadSheet(worksheet, firstRowIsColumnName: firstRowIsColumnName, readOriginalValue: readOriginalValue);
                yield return sheet;
            }
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
        public static ExcelSheet ReadSheet(Stream stream, int sheetIndex = 0, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue, bool readOriginalValue = false)
        {
            using ExcelPackage package = new ExcelPackage(stream);
            //worksheet是从1开始的
            var worksheet = package.Workbook.Worksheets[sheetIndex + 1];

            return ReadSheet(worksheet, firstRowIsColumnName: firstRowIsColumnName, readOriginalValue: readOriginalValue);
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
        public static ExcelSheet ReadSheet(Stream stream, string sheetName, bool firstRowIsColumnName = true, int rowOffset = 0, int maxRowCount = int.MaxValue, bool readOriginalValue = false)
        {
            using ExcelPackage package = new ExcelPackage(stream);

            var worksheet = package.Workbook.Worksheets.AsQueryable().Where(m => m.Name == sheetName).FirstOrDefault();
            if (worksheet == null) return null;

            return ReadSheet(worksheet, firstRowIsColumnName: firstRowIsColumnName, readOriginalValue: readOriginalValue);
        }

        static ExcelSheet ReadSheet(ExcelWorksheet worksheet, bool firstRowIsColumnName = true, bool readOriginalValue = false)
        {
            var rows = new List<Object[]>();
            var sheet = new ExcelSheet { rows = rows };
            Action<TableSchema> OnGetSchema = (schema) =>
            {
                sheet.schema = schema;
            };
            Action<Object[]> AppendRow = row =>
            {
                rows.Add(row);
            };

            ReadSheet(worksheet, OnGetSchema: OnGetSchema, AppendRow: AppendRow, readOriginalValue: readOriginalValue, firstRowIsColumnName: firstRowIsColumnName);
            return sheet;
        }
        #endregion
        public class ExcelSheet
        {
            public TableSchema schema;

            public List<Object[]> rows;

            public string sheetName => schema?.table_name;
            public IEnumerable<Dictionary<string, object>> GetDictionary()
            {
                if (rows == null) yield break;

                var columnNames = schema.columns.Select(col => col.column_name).ToArray();
                foreach (var row in rows)
                {
                    var record = new Dictionary<string, object>();
                    for (int i = 0; i < row.Length; i++) record[columnNames[i]] = row[i];
                    yield return record;
                }
            }
            public IEnumerable<T> GetModel<T>()
            {
                if (rows == null) yield break;

                foreach (var row in GetDictionary())
                {
                    var record = Json.DeserializeFromString<T>(Json.Instance.SerializeToString(row));
                    yield return record;
                }
            }
        }
    }
}
