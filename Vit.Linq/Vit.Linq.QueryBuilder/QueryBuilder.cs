using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Vit.Linq.QueryBuilder
{
    public static class QueryBuilder
    {
        public static Func<T, bool> ToPredicate<T>(IFilterRule rule)
        {
            return ToExpression<T>(rule)?.Compile();
        }

        public static string ToExpressionString<T>(IFilterRule rule)
        {
            return ToExpression<T>(rule)?.ToString();
        }


        public static Expression<Func<T, bool>> ToExpression<T>(IFilterRule rule)
        {
            var exp = ToLambdaExpression(rule, typeof(T));
            return (Expression<Func<T, bool>>)exp;
        }


        public static LambdaExpression ToLambdaExpression(IFilterRule rule, Type targetType)
        {
            ParameterExpression parameter = Expression.Parameter(targetType);
            var expression = ConvertToExpression(rule, parameter);
            if (expression == null)
            {
                return null;
            }
            return Expression.Lambda(expression, parameter);
        }


       


        public static ECondition GetCondition(IFilterRule filter)
        {
            return filter.condition?.ToLower() == "or" ? ECondition.or : ECondition.and;
        }

        static Expression ConvertToExpression(IFilterRule rule, ParameterExpression parameter)
        {
            if (rule == null) return null;

            // #1 nested filter rules
            if (rule.rules?.Any() == true)
            {
                return ConvertToExpression(rule.rules, parameter, GetCondition(rule));
            }



            // #2 simple rule
            if (string.IsNullOrWhiteSpace(rule.field))
            {
                return null;
            }

            MemberExpression memberExp = LinqHelp.BuildField_MemberExpression(parameter, rule.field);

            #region get Expression

            Type fieldType = memberExp.Type;

            switch (rule.@operator)
            {
                #region ##1  null
                case "is null":
                    {
                        return IsNull();
                    }
                case "is not null":
                    {
                        return Expression.Not(IsNull());
                    }
                #endregion


                #region ##2 number
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


                #region ##3 array
                case "in":
                    {
                        return In();
                    }
                case "not in":
                    {
                        return Expression.Not(In());
                    }
                #endregion


                #region ##4 string
                case "contains":
                    {
                        var nullCheck = Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                        var contains = Expression.Call(memberExp, "Contains", null, ConvertValueExp());

                        return Expression.AndAlso(Expression.Not(nullCheck), contains);

                    }
                case "not contains":
                    {
                        var nullCheck = Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                        var contains = Expression.Call(memberExp, "Contains", null, ConvertValueExp());

                        return Expression.OrElse(nullCheck, Expression.Not(contains));
                    }
                case "starts with":
                    {
                        var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                        var startsWith = Expression.Call(memberExp, "StartsWith", null, ConvertValueExp());

                        return Expression.AndAlso(nullCheck, startsWith);
                    }

                case "ends with":
                    {
                        var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                        var endsWith = Expression.Call(memberExp, "EndsWith", null, ConvertValueExp());
                        return Expression.AndAlso(nullCheck, endsWith);
                    }
                case "is null or empty":
                    {
                        return Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                    }
                case "is not null or empty":
                    {
                        return Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                    }
                    #endregion


            }
            #endregion


            return null;


            #region Method ConvertValueExp
            UnaryExpression ConvertValueExp()
            {
                object value = rule.value;
                if (value != null)
                {
                    Type valueType = Nullable.GetUnderlyingType(fieldType) ?? fieldType;
                    value = Convert.ChangeType(value, valueType);
                }

                Expression<Func<object>> valueLamba = () => value;
                return Expression.Convert(valueLamba.Body, fieldType);
            }


            Expression IsNull()
            {
                var isNullable = !fieldType.IsValueType || Nullable.GetUnderlyingType(fieldType) != null;

                if (isNullable)
                {
                    var nullValue = Expression.Constant(null, fieldType);
                    return Expression.Equal(memberExp, nullValue);
                }
                return Expression.Constant(false, typeof(bool));
            }

            Expression In()
            {
                Expression arrayExp = null;
                #region build arrayExp
                {
                    Type valueType = typeof(IEnumerable<>).MakeGenericType(fieldType);
                    object value = null;
                    if (rule.value != null)
                    {
                        //value = Vit.Core.Module.Serialization.Json.Deserialize(Vit.Core.Module.Serialization.Json.Serialize(rule.value), valueType);
                        if (rule.value is IEnumerable arr)
                        {
                            value = ConvertToList(arr, fieldType);
                        }
                    }
                    Expression<Func<object>> valueLamba = () => value;
                    arrayExp = Expression.Convert(valueLamba.Body, valueType);
                }
                #endregion
                var inCheck = Expression.Call(typeof(System.Linq.Enumerable), "Contains", new[] { fieldType }, arrayExp, memberExp);
                return inCheck;
            }
            #endregion
        }


        static Expression ConvertToExpression(IEnumerable<IFilterRule> rules, ParameterExpression parameter, ECondition condition = ECondition.and)
        {
            if (rules?.Any() != true)
            {
                return null;
            }

            Expression expression = null;

            foreach (var rule in rules)
            {
                var curExp = ConvertToExpression(rule, parameter);
                if (curExp != null)
                    expression = Append(expression, curExp);
            }

            return expression;


            #region Method Append
            Expression Append(Expression exp1, Expression exp2)
            {
                if (exp1 == null)
                {
                    return exp2;
                }

                if (exp2 == null)
                {
                    return exp1;
                }
                return condition == ECondition.or ? Expression.OrElse(exp1, exp2) : Expression.AndAlso(exp1, exp2);
            }
            #endregion

        }

        #region ConvertToList
        internal static object ConvertToList(IEnumerable values, Type fieldType)
        {
            var methodInfo = typeof(QueryBuilder).GetMethod("ConvertToListByType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(fieldType);
            return methodInfo.Invoke(null, new object[] { values });
        }
        internal static List<T> ConvertToListByType<T>(IEnumerable values)
        {
            Type valueType = typeof(T);
            var list = new List<T>();
            foreach (var item in values)
            {
                var value = (T)Convert.ChangeType(item, valueType);
                list.Add(value);
            }
            return list;
        }
        #endregion
    }
}
