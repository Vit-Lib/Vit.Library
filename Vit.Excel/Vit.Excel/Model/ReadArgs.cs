using System;
using System.Collections.Generic;
using System.Text;

namespace Vit.Excel.Model
{
    public class ReadArgs
    {
        public Action<SheetSchema> OnGetSchema;
        public Action<Object[]> AppendRow;


        public string sheetName;
        public int? sheetIndex;

        /// <summary>
        /// <para> readOriginalValue, 默认false,获取cell的Text                                                     </para>
        /// <para>     true: 获取xls内cell中的原始值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// <para>    false: 获取xls内cell中的Text值。如xls中的日期格式，原始值可能为 43046,而Text为"2017/11/7"    </para>
        /// </summary>
        //public bool readOriginalValue = false;
        //public bool firstRowIsColumnName = true;
        //public int rowOffset = 0;
        //public int maxRowCount = int.MaxValue;
    }
}
