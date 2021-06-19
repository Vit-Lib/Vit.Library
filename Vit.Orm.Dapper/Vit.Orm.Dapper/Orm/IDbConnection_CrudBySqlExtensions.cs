using System;
using System.Collections;
using System.Data;
using System.Text;
using Dapper;
using Vit.Db.Util.Data;

namespace Vit.Extensions
{
    public static partial class IDbConnection_CrudBySqlExtensions
    {

        #region Insert    

        /// <summary>
        /// (Lith Framework)通过生成sql语句把数据模型插入到数据库表中
        /// <para> 返回值：新插入数据自增列的值</para>
        /// <para> 若数据模型中没有有效数据则抛异常 </para>
        /// <para> 会清空所有数据库参数 </para>
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="model"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool Insert(this IDbConnection conn, IDictionary model, String tableName)
        {
            StringBuilder builder1 = new StringBuilder(" insert into ").Append(tableName).Append(" ( ");
            StringBuilder builder2 = new StringBuilder("@");

            DynamicParameters sqlParam = new DynamicParameters();

            String fieldName;
            Object value;
            foreach (DictionaryEntry entry in model)
            {
                if (null != (value = entry.Value))
                {
                    fieldName = entry.Key as string;

                    builder1.Append(fieldName).Append(",");
                    builder2.Append(fieldName).Append(",@");
                    sqlParam.Add(fieldName,value);
                }
            }


            if (builder2.Length < 2)
            {
                throw new Exception("没有传入有效数据");
            }

            builder1.Length -= 1;
            builder2.Length -= 2;
            builder1.Append(") values(").Append(builder2).Append(");");

            return conn.Execute(builder1.ToString(), sqlParam, commandTimeout: ConnectionFactory.CommandTimeout) >0;

        }
        #endregion


        #region Update
        /// <summary>
        /// (Lith Framework)通过生成sql语句更新数据模型，可批量更新
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="model">要修改的属性值</param>
        /// <param name="tableName"></param>
        /// <param name="sqlWhere">例如：  " and id=@id"</param>
        /// <param name="sqlParam">数据库参数</param>
        /// <param name="sendNullField">如果model中出现空值，是否同时更新空值到数据库</param>
        /// <returns></returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static int Update(this IDbConnection conn, IDictionary model, String tableName, string sqlWhere, DynamicParameters sqlParam = null, bool sendNullField=false)
        {
            if (sqlParam == null) sqlParam = new DynamicParameters();

            StringBuilder builder = new StringBuilder(" update ").Append(tableName).Append(" set ");

            string fieldName;
            Object value;

            if (sendNullField)
            {
                foreach (DictionaryEntry entry in model)
                {
                    fieldName = entry.Key.ToString();
                    value = entry.Value;
                    builder.Append(fieldName).Append("= @").Append(fieldName).Append(",");
                
                    sqlParam.Add(fieldName, value);
                }
            }
            else
            {
               
                foreach (DictionaryEntry entry in model)
                {
                    if (null != (value = entry.Value))
                    {
                        fieldName = entry.Key.ToString();
                        builder.Append(fieldName).Append("= @").Append(fieldName).Append(",");
                        sqlParam.Add(fieldName, value);
                    }
                }
            }

            if (',' != builder[builder.Length - 1])
            {
                throw new Exception("没有传入有效数据");
            }

            builder.Length -= 1;
            builder.Append(" where  1=1 ").Append(sqlWhere);
            return conn.Execute(builder.ToString(), sqlParam, commandTimeout: ConnectionFactory.CommandTimeout);
        }
        #endregion


    }
}
