using System;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// A translator from LINQ expression queries to Pig queries.
    /// </summary>
    public static class QueryTranslator
    {
        // public static methods
        /// <summary>
        /// Translates a LINQ expression into an actionable Pig query.
        /// </summary>
        /// <param name="query">IQueryable.</param>
        /// <returns>A TranslatedQuery.</returns>
        public static TranslatedQuery Translate(IQueryable query)
        {
            return Translate((QueryProvider)query.Provider, query.Expression);
        }

        /// <summary>
        /// Translates a LINQ expression into an actionable Pig query.
        /// </summary>
        /// <param name="provider">The QueryProvider.</param>
        /// <param name="expression">The LINQ expression.</param>
        /// <returns>A TranslatedQuery.</returns>
        public static TranslatedQuery Translate(QueryProvider provider, Expression expression)
        {
            var sourceType = GetSourceType(expression);

            ProjectionExpression projection = expression as ProjectionExpression;
            if (projection == null)
            {
                expression = PartialEvaluator.Evaluate(expression);
                expression = QueryBinder.Bind(provider, expression);
                expression = AggregateRewriter.Rewrite(expression);
                expression = OrderByRewriter.Rewrite(expression);
                expression = UnusedColumnRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                projection = (ProjectionExpression)expression;
            }
            
            // assume for now it is a SelectQuery       
            var selectQuery = new SelectQuery(provider.Source, sourceType);
            selectQuery.Translate(projection);
            return selectQuery;
        }

        // private static methods
        private static Type GetSourceType(Expression expression)
        {
            // look for the innermost nested constant of type MongoQueryable<T> and return typeof(T)
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
            {
                var constantType = constantExpression.Type;
                if (constantType.IsGenericType)
                {
                    var genericTypeDefinition = constantType.GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Query<>))
                    {
                        return constantType.GetGenericArguments()[0];
                    }
                }
            }

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null && methodCallExpression.Arguments.Count != 0)
            {
                return GetSourceType(methodCallExpression.Arguments[0]);
            }

            var message = String.Format("Unable to find document type of expression: {0}.", expression.NodeType.ToString());
            throw new ArgumentOutOfRangeException(message);
        }
    }
}
