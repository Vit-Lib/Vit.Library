using System;
using System.Linq;
using System.Data;
using Vit.Extensions;
using Vit.DynamicCompile;
using Vit.Core.Util.Common;
using System.Collections.Generic;
using Vit.Db.Module.Schema;

namespace Vit.Extensions.Linq_Extensions
{
    public static class EntityGeneratorExtensions
    {


        #region GenerateEntityCode        

        static string Quote(string name) 
        {
            return name.Replace(".", "_");
        }

        public static string GenerateEntityCode(this TableSchema tableSchema,string strNamespace)
        {
            var fields = tableSchema.columns.Select(column =>
            {
                var code = Environment.NewLine;
                if (column.primary_key == 1) code += "        [Key]" + Environment.NewLine;
                if (column.autoincrement == 1) code += "        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]" + Environment.NewLine;


                var type = column.column_clr_type;
                string typeCode;

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    typeCode = type.GetGenericArguments()[0].FullName + " ? ";
                }
                else 
                {
                    typeCode = type.FullName;
                }

                code += "        public " + typeCode + " " + Quote(column.column_name) + " { get; set; }" + Environment.NewLine;
                return code;
            });
            var code_fields = String.Join(Environment.NewLine, fields.ToArray());
            var str = @"
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace " + strNamespace + @"
{
    [Table(""" + tableSchema.table_name + @""")]
    public class " + Quote(tableSchema.table_name) + @"
    {
" + code_fields + @"
    }
}
";               
            return str;
        }
        #endregion

        #region GenerateEntity
        /// <summary>
        /// 生成所有表的模型实体（类名为表名）
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="schema"></param>
        /// <param name="skipTableWithoutKey">跳过没有主键的表</param>
        /// <returns></returns>
        public static Type[] GenerateEntity(this IDbConnection conn,out List<TableSchema> schema,bool skipTableWithoutKey=true)
        {
            //(x.1) get table schema
            schema = conn.GetSchema();

            //(x.2)跳过没有主键的表
            if (skipTableWithoutKey)
            {
                schema = schema.Where(tb => tb.columns.Exists(col => col.primary_key == 1)).ToList();
            }

            //(x.3)generate model code
            var strNamespace = "Vit.Orm.EntityFramework.Dynamic.EntityGenerator.Temp" + CommonHelp.NewGuidLong();
            var codes = schema.Select(item => GenerateEntityCode(item, strNamespace)).ToArray();

            //(x.4)compile model code
            var assembly = CSharpCompiler.CompileToAssembly(
                  null,//new[] { Assembly.GetExecutingAssembly().Location },
                  codes);
          
            //(x.5)return model types
            return assembly.GetTypes();
        }
        #endregion

    }
}
