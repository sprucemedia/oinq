using System;
using System.Linq.Expressions;
using System.Linq;

namespace Oinq
{
    /// <summary>
    /// A static class with methods to partially evaluate an Expression.
    /// </summary>
    internal static class PartialEvaluator
    {
        // internal static methods
        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        internal static Expression Evaluate(Expression expression)
        {
            return Evaluate(expression, null);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="queryProvider">The query provider.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        internal static Expression Evaluate(Expression expression, IQueryProvider queryProvider)
        {
            return new SubtreeEvaluator(
                Nominator.Nominate(e => CanBeEvaluatedLocally(e, queryProvider), expression))
                .Evaluate(expression);
        }

        // private static methods
        private static Boolean CanBeEvaluatedLocally(Expression expression, IQueryProvider queryProvider)
        {
            // any operation on a query can't be done locally
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
            {
                var query = constantExpression.Value as IQueryable;
                if (query != null && (queryProvider == null || query.Provider == queryProvider))
                {
                    return false;
                }
            }

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                var declaringType = methodCallExpression.Method.DeclaringType;
                if (declaringType == typeof(Enumerable) || declaringType == typeof(Queryable))
                {
                    return false;
                }
            }

            if (expression.NodeType == ExpressionType.Convert && expression.Type == typeof(Object))
            {
                return true;
            }

            if (expression.NodeType == ExpressionType.Parameter || expression.NodeType == ExpressionType.Lambda)
            {
                return false;
            }

            return true;
        }
    }
}