using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Vit.Linq.QueryBuilder
{
    public partial class LinqHelp
    {
        #region BuildField      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberExpression BuildField_MemberExpression_ByName(Expression parameter, string propertyOrFieldName)
        {
            return Expression.PropertyOrField(parameter, propertyOrFieldName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="fieldPath">可多级。例如 "name" 、 "depart.name"</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberExpression BuildField_MemberExpression(ParameterExpression parameter, string fieldPath)
        {
            MemberExpression memberExp = null;
            foreach (var fieldName in fieldPath?.Split('.'))
            {
                memberExp = BuildField_MemberExpression_ByName(((Expression)memberExp) ?? parameter, fieldName);
            }
            return memberExp;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldPath">可多级。例如 "name" 、 "depart.name"</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemberExpression BuildField_MemberExpression(Type type, string fieldPath)
        {
            return BuildField_MemberExpression(Expression.Parameter(type), fieldPath);
        }

        #endregion



        #region BuildField_Selector
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression<Func<T, object>> BuildField_LambdaExpression<T>(string fieldPath)
        {
            var parammeter = Expression.Parameter(typeof(T));
            MemberExpression memberExp = BuildField_MemberExpression(parammeter, fieldPath);
            var lambda = Expression.Lambda(memberExp, parammeter).Compile();
            return t => lambda.DynamicInvoke(t);
        }
        #endregion




    }
}
