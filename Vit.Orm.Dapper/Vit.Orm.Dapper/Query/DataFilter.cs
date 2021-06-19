using Newtonsoft.Json;
using Vit.Core.Util.ComponentModel.Model;

namespace Vit.Orm.Dapper.Query
{
    public class DataFilter
    {
        /// <summary>
        /// 操作列
        /// </summary>
        [SsExample("id")]
        [SsDescription("操作列")]
        public string field;


        /// <summary>
        /// 操作符。可为 "=", "!=", "like", "&gt;", "&lt;" , "&gt;=", "&lt;=", "in", "not in"
        /// </summary>
        [SsExample("=")]
        [SsDescription("操作符。可为 \"=\", \"!=\", \"like\", \">\", \"<\" , \">=\", \"<=\", \"in\", \"not in\"")]
        public string opt;


        /// <summary>
        /// 参数
        /// </summary>
        [SsExample("45")]
        [SsDescription("参数")]
        public object value;


        /// <summary>
        /// 传递给数据库的参数的名称，若不指定，则使用自定义参数名称("sf_"+fieldName)
        /// </summary>      
        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        [SsDescription("传递给数据库的参数的名称，若不指定，则使用自定义参数名称(\"sf_\"+fieldName)")]
        //[SsExample("sf_order_no")]
        public string sqlParamName;


    }
}
