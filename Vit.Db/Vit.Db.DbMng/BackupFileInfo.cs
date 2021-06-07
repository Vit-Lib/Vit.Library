using System;

namespace Vit.Db.DbMng
{
    public class BackupFileInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string fileName { get; set; }

        /// <summary>
        /// 文件大小(MB)
        /// </summary>
        public float size { get; set; }

        public DateTime? createTime { get; set; }
    }
}
