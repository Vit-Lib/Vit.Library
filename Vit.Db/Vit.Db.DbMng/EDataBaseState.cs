namespace Vit.Db.DbMng
{
    /// <summary>
    /// 数据库状态
    /// </summary>
    public enum EDataBaseState
    {
        /// <summary>
        /// 状态不明
        /// </summary>
        unknow = 0,
        /// <summary>
        /// 当前数据库在线，（可删除或分离等）
        /// </summary>
        online = 1,
        /// <summary>
        /// 当前数据库已经离线（当前不存在数据库，但对应路径下有对应数据库文件（可附加数据库））
        /// </summary>
        offline = 2,
        /// <summary>
        /// 无数据库（当前不存在数据库，且对应路径下没有对应数据库文件（可创建数据库））
        /// </summary>
        none = 3
    }
}
