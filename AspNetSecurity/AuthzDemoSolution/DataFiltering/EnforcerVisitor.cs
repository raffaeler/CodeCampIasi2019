using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using DataFiltering.Helpers;
using System.Linq;
using ModelLibrary;

namespace DataFiltering
{
    public class EnforcerVisitor : ExpressionVisitor
    {
        private bool _insideEnforcer;
        private Expression _articlePredicate;

        private MethodInfo _where2;
        private static Dictionary<Type, MethodInfo> _whereMap =
            new Dictionary<Type, MethodInfo>();

        private static Type _queryableArticleType = typeof(IQueryable<Article>);
        private static Type _articleType = typeof(Article);
        private static PropertyInfo _articleOwner;
        private static PropertyInfo _articleMaturity;
        private static PropertyInfo _articleState;
        private static PropertyInfo _articleBuyer;

        public EnforcerVisitor()
        {
            _where2 = ReflectionHelpers.QueryableWhere2Parameters;
            _articleOwner = _articleType.GetProperty(nameof(Article.Owner));
            _articleMaturity = _articleType.GetProperty(nameof(Article.Maturity));
            _articleState = _articleType.GetProperty(nameof(Article.State));
            _articleBuyer = _articleType.GetProperty(nameof(Article.Buyer));
        }

        private MethodInfo GetWhereMethod(Type type)
        {
            if (!_whereMap.TryGetValue(type, out MethodInfo methodInfo))
            {
                methodInfo = _where2.MakeGenericMethod(type);
                _whereMap[type] = methodInfo;
            }

            return methodInfo;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Enforce.EnforceAgeAndOwner))
            {
                _insideEnforcer = true;
                var usernameConstant = node.Arguments.Skip(1).FirstOrDefault();
                if (!(usernameConstant is ConstantExpression))
                {
                    _insideEnforcer = false;
                    return base.VisitMethodCall(node);
                }

                var maturityConstant = node.Arguments.Skip(2).FirstOrDefault();
                if (!(maturityConstant is ConstantExpression))
                {
                    _insideEnforcer = false;
                    return base.VisitMethodCall(node);
                }

                var source = node.Arguments.First();

                // prepare the predicate to apply to the IQueryable
                _articlePredicate = MakeEnforceArticle(
                    usernameConstant, maturityConstant);

                _insideEnforcer = false;
                //return source;  // removes the Enforce method
                return base.Visit(source);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (_articlePredicate != null &&
                _queryableArticleType.IsAssignableFrom(node.Type))
            {
                return CreateWhereCall(_articlePredicate, _articleType, node);
            }

            return base.VisitConstant(node);
        }

        private Expression CreateWhereCall(Expression predicate,
            Type type, Expression source)
        {
            var whereMethod = GetWhereMethod(type);

            return Expression.Call(null, whereMethod,
                source, predicate);
        }

        private Expression MakeEnforceArticle(
            Expression usernameConstant,
            Expression maturityConstant)
        {
            var par = Expression.Parameter(_articleType);

            var pred1 = Expression.LessThanOrEqual(
                Expression.Convert(Expression.MakeMemberAccess(par, _articleMaturity), typeof(int)),
                Expression.Convert(maturityConstant, typeof(int)));

            var pred2 = MakeStateConditions(par, usernameConstant);

            var predFinal = Expression.AndAlso(pred1, pred2);
            var lambda = Expression.Lambda(predFinal, par);
            return lambda;
        }

        // a.Owner == username
        private Expression MakeOwnerCondition(ParameterExpression par,
            Expression usernameConstant)
        {
            return Expression.Equal(
                Expression.MakeMemberAccess(par, _articleOwner),
                usernameConstant);
        }

        // a.State == ArticleState.Sold || a.State == ArticleState.Returned
        private Expression MakeSoldOrReturned(ParameterExpression par)
        {
            return Expression.OrElse(
                MakeArticleState(par, ArticleState.Sold),
                MakeArticleState(par, ArticleState.Returned));
        }

        private Expression MakeBuyerCondition(ParameterExpression par,
            Expression usernameConstant)
        {
            var isBuyer = Expression.Equal(
                Expression.MakeMemberAccess(par, _articleBuyer),
                usernameConstant);

            return Expression.AndAlso(isBuyer, MakeSoldOrReturned(par));
        }

        // a.State == ArticleState.???
        private Expression MakeArticleState(ParameterExpression par, ArticleState state)
        {
            return Expression.Equal(
                Expression.MakeMemberAccess(par, _articleState),
                Expression.Constant(state, typeof(ArticleState)));
        }

        private Expression MakeStateConditions(ParameterExpression par,
            Expression usernameConstant)
        {
            return Expression.OrElse(
                Expression.OrElse(
                    MakeOwnerCondition(par, usernameConstant),
                    MakeArticleState(par, ArticleState.ListedForSelling)),
                MakeBuyerCondition(par, usernameConstant));
        }

        private Expression<Func<Article, bool>> MakeEnforceArticle(string username, Maturity maturity)
        {
            return a =>
                a.Maturity < maturity &&
                    (
                        a.Owner == username ||  // owners can see all
                        a.State == ArticleState.ListedForSelling ||
                        (
                            a.Buyer == username &&
                            (a.State == ArticleState.Sold ||
                                a.State == ArticleState.Returned)
                        )
                    );
        }
    }
}
