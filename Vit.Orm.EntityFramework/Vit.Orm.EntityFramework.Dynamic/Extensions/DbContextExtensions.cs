using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using Vit.Db.Module.Schema;

namespace Vit.Extensions.Linq_Extensions
{


    public static partial class DbContextExtensions
    {
        #region AutoGenerateEntity

        /// <summary>
        /// 对数据库中未映射的表创建模型实体代码并映射
        /// </summary>
        /// <param name="data"></param>
        /// <param name="connInfo"></param>
        /// <param name="skipAlreadyMappedTable">跳过已经映射过的表。仅映射当前尚未映射的表（默认true）。若为false：自动生成所有表的实体模型并添加映射</param>
        /// <param name="model"></param>
        public static (List<TableSchema> schema, Type[] types) AutoGenerateEntity(this DbContext data,Vit.Db.Util.Data.ConnectionInfo connInfo, bool skipAlreadyMappedTable=true,IMutableModel model=null)
        {
            List<TableSchema> schema;
            Type[] types;

            #region (x.1)生成数据库所有表的实体模型
            using (var conn = Vit.Db.Util.Data.ConnectionFactory.GetConnection(connInfo))
            {
                types = conn.GenerateEntity(out schema);
            }
            #endregion

            #region (x.2)映射实体模型 到ef映射中    
            if (model == null) model = (IMutableModel)data.Model;

            Type[] typeToMap = types;

            if (skipAlreadyMappedTable)
            {
                var mappedTables = model.GetEntityTypes().Select(m => m.GetEntityTableName());
                typeToMap = typeToMap.Where(type => !mappedTables.Any(tableName => tableName == type.Name)).ToArray();               
            }

            foreach (var type in typeToMap)
            {
                model.AddEntityType(type);
            }
            #endregion
            return (schema, typeToMap);
        }
        #endregion



    }
}