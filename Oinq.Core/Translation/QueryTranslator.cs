using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A translator from LINQ expression queries to Pig queries.
    /// </summary>
    public static class QueryTranslator
    {
        // public static methods
        public static ProjectionExpression Translate(IQueryable query)
        {
            return Translate((QueryProvider)query.Provider, query.Expression);
        }

        public static ProjectionExpression Translate(IQueryProvider provider, Expression expression)
        {
            expression = PartialEvaluator.Evaluate(expression, CanBeEvaluatedLocally);
            expression = QueryBinder.Bind(provider, expression);
            expression = AggregateRewriter.Rewrite(expression);
            expression = UnusedColumnRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);
            return (ProjectionExpression)expression;
        }

        // private methods
        private static Boolean CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            ConstantExpression cex = expression as ConstantExpression;
            if (cex != null)
            {
                IQueryable query = cex.Value as IQueryable;
                if (query != null)
                    return false;
            }
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }
    }
}
