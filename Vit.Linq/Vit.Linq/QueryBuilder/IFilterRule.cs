using System.Collections.Generic;

namespace Vit.Linq.QueryBuilder
{
    /// <summary>
    /// This interface is used to define a hierarchical filter for a given collection.
    /// </summary>
    public interface IFilterRule
    {
        /// <summary>
        ///  condition - acceptable values are "and" and "or".
        /// </summary>
        string condition { get; }
        /// <summary>
        /// 
        /// </summary>
        string field { get; }


        /// <summary>
        ///  Supported value : 
        ///  
        ///     "is null", "is not null" ,
        ///     "=", "!=", "&gt;", "&lt;" , "&gt;=", "&lt;=", 
        ///     "in" , "not in" ,
        ///     "contains", "not contains", "starts with", "ends with" , "is null or empty", "is not null or empty"
        ///     
        ///    //TODO [array]   "is empty", "is not empty"
        /// </summary>
        string @operator { get; }

        /// <summary>
        /// nested filter rules
        /// </summary>
        IEnumerable<IFilterRule> rules { get; }
 
 

        /// <summary>
        /// value of the filter. Supported value types are "integer", "double", "string", "date", "datetime", and "boolean".
        /// </summary>
        object value { get; }
    }
}
