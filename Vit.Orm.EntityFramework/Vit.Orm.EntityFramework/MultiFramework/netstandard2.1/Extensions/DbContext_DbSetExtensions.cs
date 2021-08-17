using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vit.Extensions
{


    public static partial class DbContext_DbSetExtensions
    {     


        #region ChangeEntityMapedTable
        /// <summary>
        /// 改变实体映射的表名
        /// </summary>
        /// <param name="data"></param>
        /// <param name="clrType"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool ChangeEntityMappedTable(this DbContext data, Type clrType, string tableName)
        {           
            var entityType = data.Model.FindEntityType(clrType);
            if (entityType is IMutableEntityType m)
            {
                m.SetTableName(tableName);
                return true;
            }
            else if (entityType is IConventionEntityType c)
            {
                c.SetTableName(tableName);
                return true;
            }

            return false;
        }
        #endregion

 
 




    }
}