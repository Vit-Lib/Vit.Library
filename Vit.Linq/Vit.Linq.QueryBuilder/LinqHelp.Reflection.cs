using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Vit.Linq.QueryBuilder
{
    public partial class LinqHelp
    {

        #region BuildField_LambdaExpression

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, object> BuildField_Selector_ByReflection<T>(Func<T, object> before, string fieldName)
        {
            return (T ori) =>
            {

                var midValue = before == null ? ori : before(ori);
                if (midValue == null) return null;

                var midType = midValue.GetType();
                //(x.1)property
                var property = midType.GetProperty(fieldName);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(midValue);
                }

                //(x.2)field
                var field = midType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(midValue);
                }

                //(x.3) null
                return null;
            };
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, object> BuildField_Selector_ByReflection<T>(string fieldPath)
        {
            Func<T, object> getField = null;
            foreach (var fieldName in fieldPath?.Split('.'))
            {
                getField = BuildField_Selector_ByReflection(getField, fieldName);
            }
            return getField;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression<Func<T, object>> BuildField_LambdaExpression_ByReflection<T>(string fieldPath)
        {
            Func<T, object> getField = BuildField_Selector_ByReflection<T>(fieldPath);
            return t => getField(t);
        }
        #endregion

    }
}
