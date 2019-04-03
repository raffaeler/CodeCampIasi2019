using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DataFiltering.Helpers
{
    internal static class ReflectionHelpers
    {
        private static MethodInfo _queryableWhere2 =
            GenericMethodOf(_ => Queryable.Where<int>(default(IQueryable<int>), default(Expression<Func<int, bool>>)));

        private static MethodInfo GenericMethodOf<TReturn>(Expression<Func<object, TReturn>> expression)
        {
            return GenericMethodOf(expression as Expression);
        }

        private static MethodInfo GenericMethodOf(Expression expression)
        {
            var lambdaExpression = expression as LambdaExpression;
            var bodyCall = lambdaExpression.Body as MethodCallExpression;
            var method = bodyCall.Method;
            var typeDefinition = method.GetGenericMethodDefinition();
            return typeDefinition;
        }

        public static MethodInfo QueryableWhere2Parameters
        {
            get { return _queryableWhere2; }
        }
    }

}
