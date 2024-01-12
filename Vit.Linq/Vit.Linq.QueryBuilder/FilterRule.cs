using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Vit.Linq.QueryBuilder
{
    /// <summary>
    /// This class is used to define a hierarchical filter for a given collection. This type can be serialized/deserialized by JSON.NET without needing to modify the data structure from QueryBuilder.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FilterRule : IFilterRule
    {
        /// <summary>
        /// condition - acceptable values are "and" and "or".
        /// </summary>
        public string condition { get; set; }


        public string field { get; set; }


        public string @operator { get; set; }

        /// <summary>
        ///  nested filter rules.
        /// </summary>
        public List<FilterRule> rules { get; set; }


        /// <summary>
        /// Gets or sets the value of the filter.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object value { get; set; }

        IEnumerable<IFilterRule> IFilterRule.rules => rules;
    }
}
