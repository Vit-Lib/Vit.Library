using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Vit.Core.Util.ConfigurationManager;

namespace Vit.Linq.Query
{
    internal partial class LinqHelp
    {
        #region config


        //    /*  Vit.Linq.dll 配置，可不指定。（Vit.Linq.MaxFilterCount  Vit.Linq.MaxSortCount）  */
        //    "Linq": {
        //      /* filter最大个数，超过这个个数则抛异常。可不指定，默认 50。 */
        //      "MaxFilterCount": 50,

        //      /* sort最大个数，超过这个个数则抛异常。可不指定，默认 50。 */
        //      "MaxSortCount": 50
        //    }      

        /// <summary>
        /// filter最大个数，超过这个个数则抛异常
        /// </summary>
        public static int MaxFilterCount = Appsettings.json.GetByPath<int?>("Vit.Linq.MaxFilterCount") ?? 50;

        /// <summary>
        /// sort最大个数，超过这个个数则抛异常
        /// </summary>
        public static int MaxSortCount = Appsettings.json.GetByPath<int?>("Vit.Linq.MaxSortCount") ?? 50;
        #endregion

        #region BuildField      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static MemberExpression BuildField_MemberExpression_ByName(Expression parameter, string propertyOrFieldName)
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
        internal static MemberExpression BuildField_MemberExpression(ParameterExpression parameter, string fieldPath)
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
        internal static MemberExpression BuildField_MemberExpression(Type type, string fieldPath)
        { 
            return BuildField_MemberExpression(Expression.Parameter(type), fieldPath);
        }

        #endregion



        #region BuildField_Selector
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Expression<Func<T, object>> BuildField_LambdaExpression<T>(string fieldPath)
        {
            var parammeter = Expression.Parameter(typeof(T));
            MemberExpression memberExp = BuildField_MemberExpression(parammeter, fieldPath);
            var lambda = Expression.Lambda(memberExp, parammeter).Compile();    
            return t => lambda.DynamicInvoke(t);
        }
        #endregion




    }
}
