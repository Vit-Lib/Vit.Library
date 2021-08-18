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
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool ChangeEntityMappedTable(this DbContext data, Type clrType, string tableName)
        {
            if (data.Model.FindEntityType(clrType).Relational() is RelationalEntityTypeAnnotations relational)
            {
                relational.TableName = tableName;
                return true;
            }
            return false;
        }
        #endregion



   


 

    }
}