using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Vit.Extensions;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Db.Util.Csv
{
    /// <summary>   
    /// 
    /// </summary>
    public class CsvHelp
    {
        /*          
           //以逗号（即,）作分隔符。
           //列为空则输出NULL。
           //字符串列为空也输出NULL，若内容为长度为4的NULL字符串则输出"NULL"。
           //列内容如存在逗号（即,）则用引号（即""）将该字段值包含起来。
           //列内容如存在换行（即\n）则用引号（即""）将该字段值包含起来。
           //列内容如存在引号（即"）则应替换成双引号（""）转义，并用引号（即""）将该字段值包含起来。
       */


        /// <summary>
        /// 换行符
        /// </summary>
        public static string NewLine = "\r\n";

        /// <summary>
        /// 时间序列化格式
        /// </summary>
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

        #region AppendString
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void AppendString(StreamWriter writer, string str)
        {
            if (str == null) 
            {
                str = "NULL";
            }
            else if (str.Contains(',') || str.Contains('\n'))
            {
                str = "\"" + str.Replace("\"", "\"\"") + "\"";
            }
            else if (str == "NULL")
            {
                str = "\"NULL\"";
            }
            writer.Write(str);
        }
        #endregion


        #region SaveToCsv DataTable

        /// <summary>
        /// 将数据保存为标准的CSV
        /// </summary>
        /// <param name="csvPath">文件全路径</param>
        /// <param name="dt">数据表</param>
        /// <param name="firstRowIsColumnName">首行是否为列名</param>
        /// <param name="firstRowIndex"></param>
        /// <param name="maxRowCount"></param>
        /// <param name="append">[默认false] true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates  a new file.</param>
        /// <param name="encoding">默认UTF8</param> 
        /// <returns>导入数据的条数</returns>
        public static int SaveToCsv(string csvPath, DataTable dt
            , bool firstRowIsColumnName = true, int firstRowIndex = 0, int maxRowCount = int.MaxValue
            , bool append = false, Encoding encoding = null)
        {
            int importedRowCount = 0;
            using (var writer = new StreamWriter(csvPath, append, encoding??Encoding.UTF8))
            {
                writer.NewLine = NewLine;

                int fieldCount = dt.Columns.Count;

                #region (x.1)save columnName
                if (firstRowIsColumnName) 
                {
                    for (int i = 0; i < fieldCount; i++)
                    {
                        if (i != 0) writer.Write(',');

                        AppendString(writer, dt.Columns[i].ColumnName);
                    }
                    writer.WriteLine();
                }
                #endregion


                #region (x.2)save data
                maxRowCount = Math.Min(dt.Rows.Count- firstRowIndex, maxRowCount);
                int maxRowIndex = firstRowIndex + maxRowCount;

                for(var t= firstRowIndex; t< maxRowIndex;t++)
                {
                    DataRow row = dt.Rows[t];

                    for (int i = 0; i < fieldCount; i++)
                    {
                        if (i != 0) writer.Write(',');

                        var value = row[i];
                        string text = null;
                        if (value != DBNull.Value && value != null) 
                        {
                            if (value is string str)
                            {
                                text = str;
                            }
                            else if (value is DateTime time)
                            {
                                text=time.ToString(DateTimeFormat);
                            }
                            else
                            {
                                text = value.ToString();
                            }
                        }
                        AppendString(writer, text);
                    }
                    writer.WriteLine();
                    importedRowCount++;
                }
                #endregion

                writer.Flush();
            }
            return importedRowCount;
        }

        #endregion


        #region SaveToCsv DataReader
        /// <summary>
        /// 将数据保存为标准的CSV
        /// </summary>
        /// <param name="csvPath">文件全路径</param>
        /// <param name="dr">数据表</param>
        /// <param name="firstRowIsColumnName">首行是否为列名</param>
        /// <param name="append">[默认false] true to append data to the file; false to overwrite the file. If the specified file does not exist, this parameter has no effect, and the constructor creates  a new file.</param>
        /// <param name="encoding">默认UTF8</param>
        /// <param name="maxRowCount">导入数据的最大行数</param>
        /// <returns>导入数据的条数</returns>
        public static int SaveToCsv(string csvPath,IDataReader dr, bool firstRowIsColumnName = true, bool append = false, Encoding encoding = null, int maxRowCount = int.MaxValue)
        {
            using (var writer = new StreamWriter(csvPath, append, encoding ?? Encoding.UTF8))
            {
                writer.NewLine = NewLine;

                int fieldCount = dr.FieldCount;

                #region (x.1)save columnName
                if (firstRowIsColumnName)
                {
                    for (int i = 0; i < fieldCount; i++)
                    {
                        if (i != 0) writer.Write(',');

                        AppendString(writer, dr.GetName(i));
                    }
                    writer.WriteLine();
                }
                #endregion


                #region (x.2)save data           
                int importedRowCount = 0;
                while (importedRowCount< maxRowCount && dr.Read())
                {
                    for (int i = 0; i < fieldCount; i++)
                    {
                        if (i != 0) writer.Write(',');

                        AppendString(writer, dr.SerializeToString(i, DateTimeFormat));                         
                    }
                    writer.WriteLine();

                    importedRowCount++;
                }
                #endregion

                writer.Flush();

                return importedRowCount;
            }
        }



        #endregion

    }
}
