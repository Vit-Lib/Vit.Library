using System;

namespace Vit.Orm.EntityFramework.Index
{


    /// <summary>
    /// 指定当前字段为索引
    /// </summary>
    [AttributeUsage(AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class IndexAttribute : System.Attribute
    {
        /// <summary>
        /// name of index. can be null
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsUnique { get; set; } = false;

        /// <summary>
        /// 指定当前字段为索引
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="IsUnique"></param>
        public IndexAttribute(string Name = null, bool IsUnique = false)
        {
            this.Name = Name;
            this.IsUnique = IsUnique;
        }
    }
}
