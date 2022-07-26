using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vit.Core.Util.ComponentModel.Data;
using Vit.Core.Util.ComponentModel.Query;
using Vit.Core.Util.ConfigurationManager;

namespace Vit.Orm.Dapper.Query.MySql
{
    public class SqlBuild
    {



        //    /* Vit.Orm.Dapper.dll 配置，可不指定。（Vit.Dapper.MaxFilterCount  Vit.Dapper.MaxSortCount）  */
        //    "Dapper": {
        //      /* filter最大个数，超过这个个数则抛异常。可不指定，默认 50。 */
        //      "MaxFilterCount": 50,

        //      /* sort最大个数，超过这个个数则抛异常。可不指定，默认 50。 */
        //      "MaxSortCount": 50
        //    }      





        /// <summary>
        /// filter最大个数，超过这个个数则抛异常
        /// </summary>
        public static int MaxFilterCount =  Appsettings.json.GetByPath<int?>("Vit.Dapper.MaxFilterCount") ?? 50;

        /// <summary>
        /// sort最大个数，超过这个个数则抛异常
        /// </summary>
        public static int MaxSortCount = Appsettings.json.GetByPath<int?>("Vit.Dapper.MaxSortCount") ?? 50;

        public PageInfo page;
        public IEnumerable<DataFilter>  filter;
        public IEnumerable<SortItem> sort;
        public Action<string, object> addSqlParam;



        /// <summary>
        /// " and name like @sf_name   and name2 like @sf_name2 "
        /// </summary>
        public string sqlWhere;
        /// <summary>
        /// " name asc,id desc "
        /// </summary>
        public string sqlOrderBy;
        /// <summary>
        /// " limit 1000,10 "
        /// </summary>
        public string sqlLimit;

       


        #region Build


        public SqlBuild Build()
        {
            /*
             select * from tb_order 
             where  1=1         
             and  filter1 like value1

             order by id desc
             limit {(pager.pageIndex - 1) * pager.pageSize}, {pager.pageSize}; 
             */
            string sql;

            #region (x.2)filter    
            sql = "";
            if (null != filter)
            {
                if (filter.Count() > MaxFilterCount)
                {
                    throw new Exception("筛选条件个数过多");
                }

                int paramIndex = 0;
                foreach (var item in filter)
                {
                    if (item.field == null)
                    {
                        if (!string.IsNullOrEmpty(item.sqlParamName))
                        {
                            addSqlParam(item.sqlParamName, item.value);
                        }
                        continue;
                    }

                    if (!ValidFieldName(item.field))
                    {
                        throw new Exception("筛选条件存在不合法的字段名。");
                    }

                    if (!ValidOpt(item.opt))
                    {
                        throw new Exception("筛选条件存在不合法的操作符");
                    }
                    string paramName = item.sqlParamName;
                    if (string.IsNullOrWhiteSpace(paramName))
                    {
                        paramName = "sf_" + item.field + (paramIndex++);
                    }
                    sql += " and " + item.field + " " + item.opt + " @" + paramName;
                    addSqlParam(paramName, item.value);
                }
            }
            sqlWhere = sql;
            #endregion


            #region (x.3)sort
            if (null == sort)
            {
                sqlOrderBy = null;
            }
            else
            {
                if (sort.Count() > MaxSortCount)
                {
                    throw new Exception("排序条件个数过多");
                }

                sqlOrderBy = "";
                foreach (var item in sort)
                {
                    if (!ValidFieldName(item.field))
                    {
                        throw new Exception("排序条件存在不合法的字段名。");
                    }
                    sqlOrderBy += "," + item.field + " " + (item.asc ? " asc" : " desc");
                }

                if (string.IsNullOrEmpty(sqlOrderBy))
                {
                    sqlOrderBy= null;
                }
                else
                {
                    sqlOrderBy = sqlOrderBy.Substring(1);
                }
            }
            #endregion

            #region (x.4)limit           
            sqlLimit = null == page ? null : $" limit {(page.pageIndex - 1) * page.pageSize}, {page.pageSize} ";
            #endregion
            return this;
        }


        //判断输入的字符串是否只包含 数字、大小写字母、下划线 和 .
        static readonly Regex regex = new Regex(@"^[A-Za-z0-9\\_\\.]+$");
        static bool ValidFieldName(string input)
        {            
            return regex.IsMatch(input);
        }


        //判断比较操作符是否合法
        static readonly string[] opts = new[] { "=", "!=", "like", ">", "<" , "<=", ">=", "in", "not in"};
        static bool ValidOpt(string input)
        {            
            return opts.Contains(input);
        }
        #endregion
    }
}
