using System.IO;
using System.Collections.Generic;

namespace Vit.Db.Util.Excel
{
    public partial class ExcelHelp
    {


        public static List<string> ReadSheetsName(string filePath) 
        {
            using var stream = new FileStream(filePath,FileMode.Open);
            return ReadSheetsName(stream);
        }

    }
}
