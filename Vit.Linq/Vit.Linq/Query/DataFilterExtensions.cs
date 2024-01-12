using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Vit.Extensions.Json_Extensions;
using Vit.Linq.Query;

namespace Vit.Extensions.Linq_Extensions
{

    /// <summary>
    /// https://www.cnblogs.com/seriawei/p/entity-framework-dynamic-search.html 
    /// </summary>
    public static partial class DataFilterExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<T, bool> ToPredicate<T>(this IEnumerable<DataFilter> filters, ECondition condition = ECondition.and)
        {
            return filters.ToExpression<T>(condition)?.Compile();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Expression<Func<T, bool>> ToExpression<T>(this IEnumerable<DataFilter> filters, ECondition condition = ECondition.and)
        {
            var exp = filters.ToLambdaExpression(typeof(T), condition);
            if (exp == null)
            {
                return null;
            }
            else
            {
                var lambda = (Expression<Func<T, bool>>)exp;
                return lambda;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LambdaExpression ToLambdaExpression(this IEnumerable<DataFilter> filters, Type targetType, ECondition condition = ECondition.and)
        {
            if (filters == null)
            {
                return null;
            }

            if (filters.Count() > LinqHelp.MaxFilterCount)
            {
                throw new Exception("筛选条件个数过多");
            }

            ParameterExpression parameter = Expression.Parameter(targetType);

            Expression expression = null;

            foreach (var item in filters)
            {
                var curExp = ToExpression(item, parameter);
                if(curExp!=null) 
                expression = Append(expression, curExp);
            }

            if (expression == null)
            {
                return null;
            }
            return Expression.Lambda(expression, parameter);


            #region Method Append
            Expression Append(Expression exp1, Expression exp2)
            {
                if (exp1 == null)
                {
                    return exp2;
                }
                return condition == ECondition.or ? Expression.OrElse(exp1, exp2) : Expression.AndAlso(exp1, exp2);
            }
            #endregion

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LambdaExpression ToLambdaExpression(this DataFilter filter, Type targetType)
        {  
            ParameterExpression parameter = Expression.Parameter(targetType);
           
            return Expression.Lambda(ToExpression(filter, parameter), parameter);
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Expression ToExpression(this DataFilter filter, ParameterExpression parameter)
        {
            if (filter == null || string.IsNullOrWhiteSpace(filter.field)) return null;


            // (x.1)get memberExp
            MemberExpression memberExp = LinqHelp.BuildField_MemberExpression(parameter, filter.field);


            #region (x.2) get Expression

            Type fieldType = memberExp.Type;

            switch (filter.opt)
            {
                #region (x.x.1)数值               
                case "=":
                    {
                        return Expression.Equal(memberExp, ConvertValueExp());
                    }
                case "!=":
                    {
                        return Expression.NotEqual(memberExp, ConvertValueExp());

                    }           

                case ">":
                    {
                        return Expression.GreaterThan(memberExp, ConvertValueExp());

                    }
                case ">=":
                    {
                        return Expression.GreaterThanOrEqual(memberExp, ConvertValueExp());

                    }
                case "<":
                    {
                        return Expression.LessThan(memberExp, ConvertValueExp());

                    }
                case "<=":
                    {
                        return Expression.LessThanOrEqual(memberExp, ConvertValueExp());

                    }
                #endregion

                #region (x.x.2)字符串               
                case "Contains":
                    {
                        var nullCheck = Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                        var contains = Expression.Call(memberExp, "Contains", null, ConvertValueExp());

                        return Expression.AndAlso(Expression.Not(nullCheck), contains);

                    }
                case "NotContains":
                    {
                        var nullCheck = Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                        var contains = Expression.Call(memberExp, "Contains", null, ConvertValueExp());

                        return Expression.OrElse(nullCheck, Expression.Not(contains));
                    }
                case "StartsWith":
                    {
                        var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                        var startsWith = Expression.Call(memberExp, "StartsWith", null, ConvertValueExp());

                        return Expression.AndAlso(nullCheck, startsWith);
                    }              

                case "EndsWith":
                    {
                        var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                        var endsWith = Expression.Call(memberExp, "EndsWith", null, ConvertValueExp());
                        return Expression.AndAlso(nullCheck, endsWith);
                    }
                case "IsNullOrEmpty":
                    {
                        return Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                    }
                case "IsNotNullOrEmpty":
                    {
                        return Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                    }
                #endregion

                #region (x.x.3)数组              

                case "In":
                    {
                        Expression arrayExp = null;
                        #region build arrayExp
                        {
                            Type valueType = typeof(IEnumerable<>).MakeGenericType(fieldType);
                            object value = filter.value;
                            if (value != null)
                            {
                                value = value.ConvertBySerialize(valueType);
                            }
                            Expression<Func<object>> valueLamba = () => value;
                            arrayExp = Expression.Convert(valueLamba.Body, valueType);
                        }
                        #endregion
                        var inCheck = Expression.Call(typeof(System.Linq.Enumerable), "Contains", new[] { fieldType }, arrayExp, memberExp);
                        return inCheck;

                    }
                case "NotIn":
                    {
                        Expression arrayExp = null;
                        #region build arrayExp
                        {
                            Type valueType = typeof(IEnumerable<>).MakeGenericType(fieldType);
                            object value = filter.value;
                            if (value != null)
                            {
                                value = value.ConvertBySerialize(valueType);
                            }
                            Expression<Func<object>> valueLamba = () => value;
                            arrayExp = Expression.Convert(valueLamba.Body, valueType);
                        }
                        #endregion
                        var inCheck = Expression.Call(typeof(System.Linq.Enumerable), "Contains", new[] { fieldType }, arrayExp, memberExp);
                        return Expression.Not(inCheck);
                    }
                #endregion
            }
            #endregion


            return null;


            #region Method ConvertValueExp
            UnaryExpression ConvertValueExp()
            {
                object value = filter.value;
                if (value != null)
                {
                    Type valueType = Nullable.GetUnderlyingType(fieldType) ?? fieldType;
                    value = Convert.ChangeType(value, valueType);
                }

                Expression<Func<object>> valueLamba = () => value;
                return Expression.Convert(valueLamba.Body, fieldType);
            }
            #endregion


        }


    }
}