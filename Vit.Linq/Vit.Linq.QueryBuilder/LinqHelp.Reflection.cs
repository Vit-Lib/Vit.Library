using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Vit.Linq.QueryBuilder
{
    public partial class LinqHelp
    {

        public static Func<T, object> GetFieldSelector_ByReflection<T>(Func<T, object> before, string fieldName)
        {
            return (T ori) =>
            {

                var midValue = before == null ? ori : before(ori);
                if (midValue == null) return null;

                var midType = midValue.GetType();
                //#1 property
                var property = midType.GetProperty(fieldName);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(midValue);
                }

                //#2 field
                var field = midType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(midValue);
                }

                //#3 null
                return null;
            };
        }

        public static Func<T, object> GetFieldSelector_ByReflection<T>(string fieldPath)
        {
            Func<T, object> getField = null;
            foreach (var fieldName in fieldPath?.Split('.'))
            {
                getField = GetFieldSelector_ByReflection(getField, fieldName);
            }
            return getField;
        }


        public static Expression<Func<T, object>> GetFieldExpression_ByReflection<T>(string fieldPath)
        {
            Func<T, object> getField = GetFieldSelector_ByReflection<T>(fieldPath);
            return t => getField(t);
        }


    }
}
