using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace DataFiltering.Helpers
{
    public class CustomQueryCompiler : QueryCompiler
    {
        private readonly ILogger _logger;

        public CustomQueryCompiler(IQueryContextFactory queryContextFactory,
            ICompiledQueryCache compiledQueryCache,
            ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
            IDatabase database,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            ICurrentDbContext currentContext,
            IQueryModelGenerator queryModelGenerator)
            : base(queryContextFactory,
                  compiledQueryCache,
                  compiledQueryCacheKeyGenerator,
                  database,
                  logger,
                  currentContext,
                  queryModelGenerator)
        {
            _logger = logger.Logger;
        }

        protected override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(
            Expression query)
        {
            query = ProcessExpression(query);
            return base.CompileAsyncQuery<TResult>(query);
        }

        public override Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(
            Expression query)
        {
            query = ProcessExpression(query);
            return base.CreateCompiledAsyncEnumerableQuery<TResult>(query);
        }

        public override Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(
            Expression query)
        {
            query = ProcessExpression(query);
            return base.CreateCompiledAsyncTaskQuery<TResult>(query);
        }

        public override Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query)
        {
            query = ProcessExpression(query);
            return base.CreateCompiledQuery<TResult>(query);
        }

        public override TResult Execute<TResult>(Expression query)
        {
            query = ProcessExpression(query);
            return base.Execute<TResult>(query);
        }

        public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
        {
            query = ProcessExpression(query);
            return base.ExecuteAsync<TResult>(query);
        }

        public override Task<TResult> ExecuteAsync<TResult>(
            Expression query, CancellationToken cancellationToken)
        {
            query = ProcessExpression(query);
            return base.ExecuteAsync<TResult>(query, cancellationToken);
        }

        public Expression ProcessExpression(Expression input,
            [CallerMemberName] string requester = "")
        {
            _logger.LogInformation($"ProcessExpression: {requester}");

            ExpressionVisitor visitor;
            Expression expression;

            visitor = new EnforcerVisitor();
            expression = visitor.Visit(input);

            // multiple visitors can be applied ...
            //visitor = new PredicateVisitor();
            //expression = visitor.Visit(expression);


            return expression;
        }
    }
}
