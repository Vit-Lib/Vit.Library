using System;
using System.Linq.Expressions;

namespace Vit.Linq.QueryBuilder
{
    public partial class LinqHelp
    {

        public static MemberExpression GetFieldMemberExpression_ByName(Expression parameter, string propertyOrFieldName)
        {
            return Expression.PropertyOrField(parameter, propertyOrFieldName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="fieldPath"> could be nasted , example: "name"  "depart.name"</param>
        /// <returns></returns>
        public static MemberExpression GetFieldMemberExpression(ParameterExpression parameter, string fieldPath)
        {
            MemberExpression memberExp = null;
            foreach (var fieldName in fieldPath?.Split('.'))
            {
                memberExp = GetFieldMemberExpression_ByName(((Expression)memberExp) ?? parameter, fieldName);
            }
            return memberExp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldPath"> could be nasted , example: "name"  "depart.name"</param>
        /// <returns></returns>
        public static MemberExpression GetFieldMemberExpression(Type type, string fieldPath)
        {
            return GetFieldMemberExpression(Expression.Parameter(type), fieldPath);
        }



        public static Expression<Func<T, object>> GetFieldExpression<T>(string fieldPath)
        {
            var parammeter = Expression.Parameter(typeof(T));
            MemberExpression memberExp = GetFieldMemberExpression(parammeter, fieldPath);
            var lambda = Expression.Lambda(memberExp, parammeter).Compile();
            return t => lambda.DynamicInvoke(t);
        }




    }
}
