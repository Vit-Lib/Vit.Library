using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Vit.Linq.QueryBuilder
{
    public class QueryBuilderService
    {
        public static QueryBuilderService Instance = new QueryBuilderService();


        public Dictionary<string, string> operatorMap = new Dictionary<string, string>();
        public bool operatorIsIgnoreCase = true;
        public bool ignoreError = false;

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
            var exp = ToLambdaExpression(rule, typeof(T));
            return (Expression<Func<T, bool>>)exp;
        }


        public LambdaExpression ToLambdaExpression(IFilterRule rule, Type targetType)
        {
            ParameterExpression parameter = Expression.Parameter(targetType);
            var expression = ConvertToExpression(rule, parameter);
            if (expression == null)
            {
                return null;
            }
            return Expression.Lambda(expression, parameter);
        }





        public ECondition GetCondition(IFilterRule filter)
        {
            return filter.condition?.ToLower() == "or" ? ECondition.or : ECondition.and;
        }

        public string GetOperator(IFilterRule filter)
        {
            var operate = filter.@operator ?? "";
            if (operatorIsIgnoreCase) operate = operate.ToLower();
            if (operatorMap.TryGetValue(operate, out var op2)) return operatorIsIgnoreCase ? op2?.ToLower() : op2;
            return operate;
        }

        Expression ConvertToExpression(IFilterRule rule, ParameterExpression parameter)
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

            MemberExpression memberExp = LinqHelp.GetFieldMemberExpression(parameter, rule.field);

            #region Get Expression

            Type fieldType = memberExp.Type;

            var Operator = GetOperator(rule);
            var cmpType = operatorIsIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;


            {
                #region ##1  null
                if (FilterRuleOperator.IsNull.Equals(Operator, cmpType))
                {
                    return IsNull();
                }
                if (FilterRuleOperator.IsNotNull.Equals(Operator, cmpType))
                {
                    return Expression.Not(IsNull());
                }
                #endregion


                #region ##2 number
                if (FilterRuleOperator.Equal.Equals(Operator, cmpType))
                {
                    return Expression.Equal(memberExp, ConvertValueExp());
                }
                if (FilterRuleOperator.NotEqual.Equals(Operator, cmpType))
                {
                    return Expression.NotEqual(memberExp, ConvertValueExp());
                }

                if (FilterRuleOperator.GreaterThan.Equals(Operator, cmpType))
                {
                    return Expression.GreaterThan(memberExp, ConvertValueExp());
                }
                if (FilterRuleOperator.GreaterThanOrEqual.Equals(Operator, cmpType))
                {
                    return Expression.GreaterThanOrEqual(memberExp, ConvertValueExp());
                }
                if (FilterRuleOperator.LessThan.Equals(Operator, cmpType))
                {
                    return Expression.LessThan(memberExp, ConvertValueExp());

                }
                if (FilterRuleOperator.LessThanOrEqual.Equals(Operator, cmpType))
                {
                    return Expression.LessThanOrEqual(memberExp, ConvertValueExp());
                }
                #endregion


                #region ##3 array
                if (FilterRuleOperator.In.Equals(Operator, cmpType))
                {
                    return In();
                }
                if (FilterRuleOperator.NotIn.Equals(Operator, cmpType))
                {
                    return Expression.Not(In());
                }
                #endregion


                #region ##4 string
                if (FilterRuleOperator.Contains.Equals(Operator, cmpType))
                {
                    var nullCheck = Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                    var contains = Expression.Call(memberExp, "Contains", null, ConvertValueExp());

                    return Expression.AndAlso(Expression.Not(nullCheck), contains);

                }
                if (FilterRuleOperator.NotContains.Equals(Operator, cmpType))
                {
                    var nullCheck = Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                    var contains = Expression.Call(memberExp, "Contains", null, ConvertValueExp());

                    return Expression.OrElse(nullCheck, Expression.Not(contains));
                }
                if (FilterRuleOperator.StartsWith.Equals(Operator, cmpType))
                {
                    var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                    var startsWith = Expression.Call(memberExp, "StartsWith", null, ConvertValueExp());

                    return Expression.AndAlso(nullCheck, startsWith);
                }

                if (FilterRuleOperator.EndsWith.Equals(Operator, cmpType))
                {
                    var nullCheck = Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                    var endsWith = Expression.Call(memberExp, "EndsWith", null, ConvertValueExp());
                    return Expression.AndAlso(nullCheck, endsWith);
                }
                if (FilterRuleOperator.IsNullOrEmpty.Equals(Operator, cmpType))
                {
                    return Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp);
                }
                if (FilterRuleOperator.IsNotNullOrEmpty.Equals(Operator, cmpType))
                {
                    return Expression.Not(Expression.Call(typeof(string), "IsNullOrEmpty", null, memberExp));
                }
                #endregion

            }
            #endregion

            if (!ignoreError) throw new Exception("unrecognized operator : " + rule.@operator);
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


        Expression ConvertToExpression(IEnumerable<IFilterRule> rules, ParameterExpression parameter, ECondition condition = ECondition.and)
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
        internal object ConvertToList(IEnumerable values, Type fieldType)
        {
            var methodInfo = typeof(QueryBuilderService).GetMethod("ConvertToListByType", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(fieldType);
            return methodInfo.Invoke(this, new object[] { values });
        }
        internal List<T> ConvertToListByType<T>(IEnumerable values)
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
