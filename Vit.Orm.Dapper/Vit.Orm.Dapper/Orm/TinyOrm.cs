
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Vit.Db.Util.Data;

namespace Vit.Orm.Dapper.Orm
{
    /// <summary>
    /// SqlServer
    /// </summary>
    public class TinyOrm
    {

        public static Func<System.Data.IDbConnection> DefaultCreator { get; set; } = ConnectionFactory.GetConnectionCreator("App.Db");

        public static Func<SqlConnection> CreateConn
            //=    () => new SqlConnection(System.Configuration.ConfigurationManager.AppSettings[0]); 

            //"App.Db"
            = ()=> (DefaultCreator() as SqlConnection);  
        

        #region Insert
        /// <summary>
        /// 把数据模型插入到数据库表中
        /// <para> 返回值：新插入数据自增列的值</para>
        /// <para> 若数据模型中没有有效数据则抛异常 </para>
        /// <para> 会清空所有数据库参数 </para>
        /// </summary>    
        /// <param name="model"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int Insert( JObject model, String tableName)
        {

            using (var conn = CreateConn()) 
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;

                if (ConnectionFactory.CommandTimeout.HasValue)
                    cmd.CommandTimeout = ConnectionFactory.CommandTimeout.Value;

                StringBuilder builder1 = new StringBuilder(" insert into [").Append(tableName).Append("] ( [");
                StringBuilder builder2 = new StringBuilder("@");


                String fieldName;

                Object value;
                foreach (var entry in model)
                {
                    if (null != (value = entry.Value.Value<string>()))
                    {
                        builder1.Append(fieldName = entry.Key).Append("],[");
                        builder2.Append(fieldName).Append(",@");
                        cmd.Parameters.AddWithValue(fieldName, value);                     
                    }
                }
                if (builder2.Length < 2)
                {
                    throw new Exception("没有传入有效数据");
                }

                builder1.Length -= 2;
                builder2.Length -= 2;
                builder1.Append(") values(").Append(builder2).Append(");select convert(int,isnull(SCOPE_IDENTITY(),-1));");  

                cmd.CommandText = builder1.ToString();
                return (int)cmd.ExecuteScalar();            
            }
        }

        #endregion


        #region Update
        /// <summary>
        /// 把数据模型中的数据更新到数据库。
        /// <para> 返回值：数据库表中受影响的行数</para>
        /// <para> 若数据模型中没有有效数据则抛异常 </para>
        /// <para> 会清空所有数据库参数 </para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tableName"></param>
        /// <param name="sqlWhere">如 " and id=5"</param>
        /// <returns></returns>
        public static int UpdateByWhere(JObject model, String tableName, string sqlWhere)
        {

            using (var conn = CreateConn())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = conn;
                if (ConnectionFactory.CommandTimeout.HasValue)
                    cmd.CommandTimeout = ConnectionFactory.CommandTimeout.Value;


                StringBuilder builder = new StringBuilder(" update [").Append(tableName).Append("] set [");


                String fieldName;

                Object value;
                foreach (var entry in model)
                {
                    if (null != (value = entry.Value.Value<string>()))
                    {
                        builder.Append(fieldName = entry.Key).Append("]= @").Append(fieldName).Append(",[");
                        cmd.Parameters.AddWithValue(fieldName, value);
                    }
                }

                if (',' != builder[builder.Length - 2])
                {
                    throw new Exception("没有传入有效数据");
                }

                builder.Length -= 2;
                builder.Append(" where  1=1 ").Append(sqlWhere);
                cmd.CommandText = builder.ToString();
                return cmd.ExecuteNonQuery();

            }
        }
        #endregion

        #region PagingSql 构建分页的sql语句(嵌套查询实现分页)

        /// <summary>
        ///  sql = TinyOrm.PagingSql(sql, "id", pageSize * (pageIndex - 1) + 1, pageSize);
        /// 
        /// 获取分页的sql语句(变量名 @sql 和 @sumCount 已被使用，3次使用select top 嵌套查询实现分页) 
        /// 返回两个结果集
        /// 结果集1 的首行首列为数据总个数
        /// 结果集2 为分页筛选的数据集
        /// </summary>
        /// <param name="strSql">筛选数据的sql语句（必须有主码[可唯一确定一条数据] ）,例： "select * from Sys_User where UserID!=5 "</param>
        /// <param name="keyName">sql筛选数据的主码名，例："UserID"</param>
        /// <param name="startIndex">返回数据的开始位置，从1开始，不能大于满足条件的数据的总个数</param>
        /// <param name="count">返回数据的个数</param>
        /// <returns></returns>
        public static string PagingSql(string strSql, string keyName, int startIndex, int count)
        {

            /*分页算法
select * into #Tb from (select * from Sys_User  ) t; 

select t.* from (select top  @count  ID  from   (  select top (@sumCount - @startIndex + 1)  ID 
from #Tb where 1=1  order by [ID] ASC  ) t1   
 order by ID DESC) t2  inner join #Tb t on t2.ID=t.ID   order by t.[ID] DESC
 
drop table #Tb;
             */

            /* 实现
declare @sql varchar(8000),@sumCount int;
             * 
select * into #Tb from ( select * from Sys_User  ) t; 
select @sumCount=count(*) from #Tb;
select @sumCount;

set @sql='select t.* from (select top @count * from (select top '+CONVERT(varchar(10),@sumCount-(@startIndex - 1) )
+' [UserID] from #Tb order by [UserID] ASC  ) t1 order by [UserID] DESC) t2 inner join #Tb t on t2.[UserID]=t.[UserID] order by t.[UserID] DESC ;';  

exec(@sql);

drop table #Tb
             */
            return new StringBuilder().Append(" declare @sql varchar(8000),@sumCount int;select * into #Tb from (").Append(strSql).Append(") t; select @sumCount=count(*) from #Tb;select @sumCount;set @sql='select t.* from (select top ").Append(count).Append(" * from (select top '+CONVERT(varchar(10),@sumCount-").Append(startIndex - 1).Append(" )+' [").Append(keyName).Append("] from #Tb order by [").Append(keyName).Append("] ASC  ) t1 order by [").Append(keyName).Append("] DESC) t2 inner join #Tb t on t2.[").Append(keyName).Append("]=t.[").Append(keyName).Append("] order by t.[").Append(keyName).Append("] DESC ;';  exec(@sql);drop table #Tb").ToString();
        }





        /// <summary>
        /// 获取分页的sql语句(变量名 @sql 和 @sumCount 已被使用，3次使用select top  嵌套查询实现分页) 
        /// 返回两个结果集
        /// 结果集1 的首行首列为数据总个数
        /// 结果集2 为分页筛选的数据集
        /// </summary> 
        /// <param name="strSql">筛选数据的sql语句（必须有主码[可唯一确定一条数据] ）,例： "select * from Sys_User where UserID!=5 "</param>
        /// <param name="orderKeys">排序用的列名数组，最后一列必须为主码,例：  ["SortCode","UserID"]</param>
        /// <param name="isAscs">orderKeys中按索引对应列是否按ASC方式排序,例： [true,false]</param>       
        /// <param name="startIndex">返回数据的开始位置，从1开始，不能大于满足条件的数据的总个数</param>
        /// <param name="count">返回数据的个数</param>
        /// <returns></returns>
        public static string PagingSql(string strSql, string[] orderKeys, bool[] isAscs, int startIndex, int count)
        {

            /*分页算法
select * into #Tb from (select * from Sys_User  ) t; 

select t.* from (select top  @count  ID  from   (  select top (@sumCount - @startIndex + 1)  ID 
from #Tb where 1=1  order by [ID] ASC  ) t1   
 order by ID DESC) t2  inner join #Tb t on t2.ID=t.ID   order by t.[ID] DESC
 
drop table #Tb;
             */

            /* 实现
declare @sql varchar(8000),@sumCount int;
             * 
select * into #Tb from ( select * from Sys_User  ) t; 
select @sumCount=count(*) from #Tb;
select @sumCount;

set @sql='select t.* from (select top @count * from (select top '+CONVERT(varchar(10),@sumCount-(@startIndex - 1) )
+' [UserID] from #Tb order by [UserID] ASC  ) t1 order by [UserID] DESC) t2 inner join #Tb t on t2.[UserID]=t.[UserID] order by t.[UserID] DESC ;';  

exec(@sql);

drop table #Tb
             */
            int t;
            var builder = new StringBuilder();
            builder.Append(" declare @sql varchar(8000),@sumCount int;select * into #Tb from (").Append(strSql).Append(") t; select @sumCount=count(*) from #Tb;select @sumCount;set @sql='select t.* from (select top ").Append(count).Append(" * from (select top '+CONVERT(varchar(10),@sumCount-").Append(startIndex - 1).Append(" )+' ");


            t = 0;
            builder.Append('[').Append(orderKeys[t]).Append("] ");
            t++;
            for (; t < orderKeys.Length; t++)
            {
                builder.Append(",[").Append(orderKeys[t]).Append("] ");
            }
            builder.Append(" from #Tb order by ");

            t = 0;
            builder.Append('[').Append(orderKeys[t]).Append("] ").Append(isAscs[t] ? "DESC" : "ASC");
            t++;
            for (; t < orderKeys.Length; t++)
            {
                builder.Append(",[").Append(orderKeys[t]).Append("] ").Append(isAscs[t] ? "DESC" : "ASC");
            }
            builder.Append(" ) t1 order by ");

            t = 0;
            builder.Append('[').Append(orderKeys[t]).Append("] ").Append(isAscs[t] ? "ASC" : "DESC");
            t++;
            for (; t < orderKeys.Length; t++)
            {
                builder.Append(",[").Append(orderKeys[t]).Append("] ").Append(isAscs[t] ? "ASC" : "DESC");
            }
            String primaryKey = orderKeys[orderKeys.Length - 1];
            builder.Append(" ) t2 inner join #Tb t on t2.[").Append(primaryKey).Append("]=t.[").Append(primaryKey).Append("] order by ");

            t = 0;
            builder.Append("t.[").Append(orderKeys[t]).Append("] ").Append(isAscs[t] ? "ASC" : "DESC");
            t++;
            for (; t < orderKeys.Length; t++)
            {
                builder.Append(",t.[").Append(orderKeys[t]).Append("] ").Append(isAscs[t] ? "ASC" : "DESC");
            }
            return builder.Append(" ;';  exec(@sql);drop table #Tb").ToString();
        }


        #endregion


        /// <summary>
        /// 执行查询，返回结果集所有数据。
        /// </summary>
        /// <param name="strSql">sql语句或存储过程名称</param>
        /// <returns></returns>
        public static DataSet ExecuteDataSet(string strSql)
        {
            using (var conn = CreateConn())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand();
                if (ConnectionFactory.CommandTimeout.HasValue)
                    cmd.CommandTimeout = ConnectionFactory.CommandTimeout.Value;

                cmd.Connection = conn;
                cmd.CommandText = strSql;
                DataSet myDs = new DataSet();
                using (SqlDataAdapter Adapter = new SqlDataAdapter(cmd))
                {
                    Adapter.Fill(myDs);
                }
                return myDs;
            }
        }

    }
}
