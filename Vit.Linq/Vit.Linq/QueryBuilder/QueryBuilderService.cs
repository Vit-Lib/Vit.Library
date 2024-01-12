using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Vit.Extensions.Json_Extensions;
using Vit.Linq.Query;

namespace Vit.Linq.QueryBuilder
{
    public class QueryBuilderService
    {

        public Func<T, bool> ToPredicate<T>(IFilterRule rule)
        {
            return ToExpression<T>(rule)?.Compile();
        }

        public string ToExpressionString<T>(IFilterRule rule)
        {
            return ToExpression<T>(rule)?.ToString();
        }


        public Expression<Func<T, bool>> ToExpression<T>(IFilterRule rule)
        {
            return ToExpression<T>(new[] { rule });
        }


        public Expression<Func<T, bool>> ToExpression<T>(IEnumerable<IFilterRule> rules, ECondition condition = ECondition.and)
        {
            var exp = ToLambdaExpression(rules, typeof(T), condition);
            return (Expression<Func<T, bool>>)exp;
        }


        public LambdaExpression ToLambdaExpression(IEnumerable<IFilterRule> rules, Type targetType, ECondition condition = ECondition.and)
        {
            ParameterExpression parameter = Expression.Parameter(targetType);
            return ToLambdaExpression(rules, parameter, condition);
        }


        protected LambdaExpression ToLambdaExpression(IEnumerable<IFilterRule> rules, ParameterExpression parameter, ECondition condition = ECondition.and)
        {
            if (rules?.Any() != true)
            {
                return null;
            }

            Expression expression = null;

            foreach (var rule in rules)
            {
                var curExp = ToExpression(rule, parameter);
                if (curExp != null)
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

                if (exp2 == null)
                {
                    return exp1;
                }
                return condition == ECondition.or ? Expression.OrElse(exp1, exp2) : Expression.AndAlso(exp1, exp2);
            }
            #endregion

        }





        ECondition GetCondition(IFilterRule filter)
        {
            return filter.condition?.ToLower() == "or" ? ECondition.or : ECondition.and;
        }

        Expression ToExpression(IFilterRule rule, ParameterExpression parameter)
        {
            if (rule == null) return null;

            // #1 nested filter rules
            if (rule.rules?.Any() == true)
            {
                return ToLambdaExpression(rule.rules, parameter, GetCondition(rule));
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
                    object value = rule.value;
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
            #endregion


        }


    }
}
