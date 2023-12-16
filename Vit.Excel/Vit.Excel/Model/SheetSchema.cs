using System;
using System.Collections.Generic;
using System.Text;

namespace Vit.Excel.Model
{
    public class SheetSchema
    {
        public string name;

        public List<ColumnSchema> columns;

    }
    public class ColumnSchema
    {
        public string name; 
        public Type type;
    }
}
