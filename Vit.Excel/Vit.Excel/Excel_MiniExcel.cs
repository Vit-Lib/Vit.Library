using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

using MiniExcelLibs;
using MiniExcelLibs.OpenXml;

using Vit.Core.Module.Serialization;

namespace Vit.Excel
{
    public class Excel_MiniExcel : IExcel
    {
        public bool useHeaderRow { get; set; } = true;


        Stream stream = null;
        readonly bool streamNeedDispose = false;


        public Excel_MiniExcel(Stream stream, bool needDisposeStream = false)
        {
            this.stream = stream;
            this.streamNeedDispose = needDisposeStream;
        }


        public Excel_MiniExcel(string filePath) : this(new FileStream(filePath, FileMode.OpenOrCreate), true)
        {
        }

        public void Dispose()
        {
            if (streamNeedDispose && stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        #region SaveSheet
        readonly Dictionary<string, object> sheets = new();

        public void Save()
        {
            var config = new OpenXmlConfiguration
            {
                AutoFilter = false
            };
            MiniExcel.SaveAs(stream, sheets, configuration: config);
            /* note: sheet insheets could be
              * 1.DataTable
              * 2.IDataReader
              * 3.IEnumerable
              *     row must be IDictionary/IDictionary<string,object>/DTO
              * */
        }
        //public void AddSheetByCells(string sheetName, IEnumerable<object[]> sheet, string[] columnNames)
        //{
        //    sheets[sheetName] = new DataReader_Cells(sheet, columnNames);
        //}

        public void AddSheetByArray(string sheetName, IEnumerable<IEnumerable<object>> sheet, string[] columnNames)
        {
            sheets[sheetName] = new DataReader_IEnumerable(sheet, columnNames);
        }
        public void AddSheetByDictionary(string sheetName, IEnumerable<IDictionary> sheet, string[] columnNames = null)
        {
            columnNames ??= sheet.FirstOrDefault()?.Keys.Cast<string>().ToArray() ?? Array.Empty<string>();
            sheets[sheetName] = new DataReader_IDictionary(sheet, columnNames);
        }
        public void AddSheetByDictionary(string sheetName, IEnumerable<IDictionary<string, object>> sheet, string[] columnNames = null)
        {
            columnNames ??= sheet.FirstOrDefault()?.Keys.Cast<string>().ToArray() ?? Array.Empty<string>();
            sheets[sheetName] = new DataReader_Dictionary(sheet, columnNames);
        }
        public void AddSheetByModel<Model>(string sheetName, IEnumerable<Model> sheet, string[] columnNames = null) where Model : class
        {
            sheets[sheetName] = new DataReader_DTO<Model>(sheet, columnNames);
        }
        public void AddSheetByDataTable(string sheetName, DataTable sheet)
        {
            sheets[sheetName] = sheet;
        }
        public void AddSheetByDataReader(string sheetName, IDataReader reader)
        {
            sheets[sheetName] = reader;
        }

        #region Cell Array
        class DataReader_Cells : BaseDataReader<object[]>
        {
            public DataReader_Cells(IEnumerable<object[]> enumerable, string[] columnNames)
               : base(enumerable, columnNames) { }

            public override object GetValue(int i)
            {
                return enumerator.Current?[i];
            }
        }
        #endregion


        #region IEnumerable
        class DataReader_IEnumerable : BaseDataReader<IEnumerable<object>>
        {
            public DataReader_IEnumerable(IEnumerable<IEnumerable<object>> enumerable, string[] columnNames)
              : base(enumerable, columnNames) { }


            IEnumerator<object> curRow;
            public override object GetValue(int i)
            {
                curRow?.MoveNext();
                return curRow?.Current;
            }
            public override bool Read()
            {
                if (enumerator.MoveNext())
                {
                    curRow = enumerator.Current?.GetEnumerator();
                    return true;
                }
                curRow = null;
                return false;
            }
        }
        #endregion

        #region Dictionary
        class DataReader_Dictionary : BaseDataReader<IDictionary<string, object>>
        {
            public DataReader_Dictionary(IEnumerable<IDictionary<string, object>> enumerable, string[] columnNames)
             : base(enumerable, columnNames) { }


            public override object GetValue(int i)
            {
                if (true == enumerator.Current?.TryGetValue(columnNames[i], out var value))
                    return value;
                return null;
            }

        }

        class DataReader_IDictionary : BaseDataReader<IDictionary>
        {
            public DataReader_IDictionary(IEnumerable<IDictionary> enumerable, string[] columnNames)
             : base(enumerable, columnNames) { }

            public override object GetValue(int i)
            {
                var cur = enumerator.Current;
                if (cur != null)
                {
                    var columnName = columnNames[i];
                    if (cur.Contains(columnName)) return cur[columnName];
                }
                return null;
            }

        }
        #endregion

        #region Model
        class DataReader_DTO<Model> : BaseDataReader<Model> where Model : class
        {
            protected Func<object, object>[] FuncList_GetCellValue;

            public DataReader_DTO(IEnumerable<Model> enumerable, string[] columnNames = null)
                : base(enumerable, columnNames)
            {
                Type type = typeof(Model);
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var valueGetterList =
                    fields.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row))))
                    .Union(properties.Select(m => (m.Name, (Func<object, object>)(row => m.GetValue(row)))));
                this.columnNames ??= valueGetterList.Select(m => m.Name).Distinct().ToArray();

                FuncList_GetCellValue = this.columnNames.Select(name => valueGetterList.FirstOrDefault(getter => getter.Name == name).Item2).ToArray();
            }

            public override object GetValue(int i)
            {
                return FuncList_GetCellValue[i]?.Invoke(enumerator.Current);
            }
        }
        #endregion

        #region BaseDataReader
        abstract class BaseDataReader<T> : IDataReader
        {
            #region Base
            protected string[] columnNames;

            protected IEnumerable<T> enumerable;
            protected IEnumerator<T> enumerator;

            public BaseDataReader(IEnumerable<T> enumerable, string[] columnNames)
            {
                this.columnNames = columnNames;
                this.enumerable = enumerable;
                enumerator = enumerable.GetEnumerator();
            }

            public virtual int FieldCount => columnNames.Length;
            public virtual string GetName(int i)
            {
                return columnNames[i];
            }

            public virtual bool Read()
            {
                return enumerator.MoveNext();
            }
            #endregion

            //public abstract int FieldCount { get; }
            //public abstract string GetName(int i);

            public abstract object GetValue(int i);
            //public abstract bool Read();
            public virtual void Close()
            {
            }
            public virtual void Dispose()
            {
            }

            #region NotImplemented
            public object this[int i] => throw new NotImplementedException();

            public object this[string name] => throw new NotImplementedException();

            public int Depth => throw new NotImplementedException();

            public bool IsClosed => throw new NotImplementedException();

            public int RecordsAffected => throw new NotImplementedException();

            public bool GetBoolean(int i)
            {
                throw new NotImplementedException();
            }

            public byte GetByte(int i)
            {
                throw new NotImplementedException();
            }

            public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public char GetChar(int i)
            {
                throw new NotImplementedException();
            }

            public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
            {
                throw new NotImplementedException();
            }

            public IDataReader GetData(int i)
            {
                throw new NotImplementedException();
            }

            public string GetDataTypeName(int i)
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime(int i)
            {
                throw new NotImplementedException();
            }

            public decimal GetDecimal(int i)
            {
                throw new NotImplementedException();
            }

            public double GetDouble(int i)
            {
                throw new NotImplementedException();
            }

            public Type GetFieldType(int i)
            {
                throw new NotImplementedException();
            }

            public float GetFloat(int i)
            {
                throw new NotImplementedException();
            }

            public Guid GetGuid(int i)
            {
                throw new NotImplementedException();
            }

            public short GetInt16(int i)
            {
                throw new NotImplementedException();
            }
            public int GetInt32(int i)
            {
                throw new NotImplementedException();
            }
            public long GetInt64(int i)
            {
                throw new NotImplementedException();
            }
            public int GetOrdinal(string name)
            {
                throw new NotImplementedException();
            }
            public DataTable GetSchemaTable()
            {
                throw new NotImplementedException();
            }
            public string GetString(int i)
            {
                throw new NotImplementedException();
            }
            public int GetValues(object[] values)
            {
                throw new NotImplementedException();
            }
            public bool IsDBNull(int i)
            {
                throw new NotImplementedException();
            }
            public bool NextResult()
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        #endregion

        #endregion






        #region Read

        #region #1 ReadDictionary
        public IEnumerable<IDictionary<string, object>> ReadDictionary(string sheetName)
        {
            IEnumerable<dynamic> rows = MiniExcel.Query(stream, sheetName: sheetName, useHeaderRow: useHeaderRow);
            return rows.Cast<IDictionary<string, object>>();
        }
        #endregion


        #region #2 ReadArray
        public (List<string> columnNames, IEnumerable<object[]> rows) ReadArray(string sheetName)
        {
            var rows = ReadDictionary(sheetName);
            if (rows?.Any() != true) return default;

            var columnNames = rows.First().Keys.ToList();
            IEnumerable<object[]> rows_ = rows.Select(row => row.Values.ToArray());
            return (columnNames, rows_);
        }
        #endregion


        #region #3 ReadModel
        ///<summary>
        ///native read by Model, not recommand
        /// </summary>
        /// <typeparam name="Model"></typeparam>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public IEnumerable<Model> ReadModel<Model>(string sheetName) where Model : class, new()
        {
            return MiniExcel.Query<Model>(stream, sheetName: sheetName);
        }

        private IEnumerable<Model> ReadModel_<Model>(string sheetName)
            where Model : class, new()
        {
            var rows = ReadDictionary(sheetName);
            if (rows == null) return default;

            var type = typeof(Model);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<(string Name, Action<object, object> SetValue, Type FieldType)> columnList =
                            fields.Select(m => (m.Name, (Action<object, object>)m.SetValue, m.FieldType))
                            .Union(properties.Select(m => (m.Name, (Action<object, object>)m.SetValue, m.PropertyType)))
                            .ToList();

            List<(string Name, Action<object, object> Setter)> valueSetterList = columnList
                .Select(column => (column.Name, Model_BuildSetter(column.SetValue, column.FieldType)))
                .ToList();

            var cellSetters = valueSetterList.GroupBy(item => item.Name).ToDictionary(item => item.Key, item => item.First().Setter);

            IEnumerable<Model> rows_ = rows.Select(CellToModel);
            return rows_;

            #region Method 
            Model CellToModel(IDictionary<string, object> row)
            {
                var model = new Model();
                foreach (var kv in row)
                {
                    if (cellSetters.TryGetValue(kv.Key, out var Setter))
                        Setter?.Invoke(model, kv.Value);
                }
                return model;
            }

            Action<object, object> Model_BuildSetter(Action<object, object> SetValue, Type FieldType)
            {
                void Setter(object row, object cellValue)
                {
                    try
                    {
                        if (cellValue == null) return;
                        if (cellValue.GetType() != FieldType)
                        {
                            cellValue = Json.Deserialize(Json.Serialize(cellValue), FieldType);
                        }
                        SetValue(row, cellValue);
                    }
                    catch { }
                }
                return Setter;
            }
            #endregion
        }


        #endregion


        #region #4 ReadDataTable
        public DataTable ReadDataTable(string sheetName)
        {
            return MiniExcel.QueryAsDataTable(stream, sheetName: sheetName, useHeaderRow: useHeaderRow);
        }
        #endregion


        #endregion






        #region GetSheetNames GetColumns

        public List<string> GetSheetNames()
        {
            return MiniExcel.GetSheetNames(stream);
        }

        public List<string> GetColumns(string sheetName = null)
        {
            return MiniExcel.GetColumns(stream, useHeaderRow: useHeaderRow, sheetName: sheetName).ToList();
        }
        #endregion


    }
}
