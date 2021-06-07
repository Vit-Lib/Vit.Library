using System;

namespace Vit.Db.Module.Schema
{
    public class ColumnSchema
    {
        public string column_name { get; set; }

        /// <summary>
        /// 是否为主键(1：是,   其他：不是)
        /// </summary>
        public int? primary_key { get; set; }

        /// <summary>
        /// 是否为自增列(1：是,   其他：不是)
        /// </summary>
        public int? autoincrement { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string column_comment { get; set; }

        /// <summary>
        /// db Type
        /// </summary>
        public string column_type { get; set; }


        /// <summary>
        /// clr Type
        /// </summary>
        public string column_clr_type_name { get; set; }

        /// <summary>
        /// clr Type
        /// </summary>
        public Type column_clr_type { get; set; }

    }
}
