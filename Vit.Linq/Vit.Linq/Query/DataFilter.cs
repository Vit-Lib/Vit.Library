using Newtonsoft.Json;
using Vit.Core.Util.ComponentModel.Model;

namespace Vit.Linq.Query
{
    public class DataFilter
    {
        /// <summary>
        /// 操作列,可多级（如 "b1.name"）
        /// </summary>
        [SsExample("id")]
        [SsDescription("操作列,可多级（如 \"b1.name\"）")]
        public string field { get; set; }



        /// <summary>
        /// 操作符。可为 "=", "!=", "&gt;", "&lt;" , "&gt;=", "&lt;=", "Contains", "NotContains", "StartsWith", "EndsWith" , "IsNullOrEmpty", "IsNotNullOrEmpty" ,  "In" ,"NotIn"
        /// </summary>
        [SsExample(">=")]
        [SsDescription("操作符。可为 \"=\", \"!=\", \">\", \"<\" , \">=\", \"<=\", \"Contains\", \"NotContains\", \"StartsWith\", \"EndsWith\", \"IsNullOrEmpty\", \"IsNotNullOrEmpty\", \"In\", \"NotIn\"")]
        public string opt { get; set; }
         

        /// <summary>
        /// 参数
        /// </summary>
        [SsExample("2")]
        [SsDescription("参数")]
        [JsonProperty(NullValueHandling =NullValueHandling.Ignore)]
        public object value { get; set; }         

    }
}
