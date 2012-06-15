using System;
using System.Linq.Expressions;

namespace Oinq.Core
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
            return Evaluate(expression, PartialEvaluator.CanBeEvaluatedLocally);
        }

        /// <summary>
        /// Performs evaluation and replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="canBeEvaluated">Whether the expression can be evaluated locally.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        internal static Expression Evaluate(Expression expression, Func<Expression, Boolean> canBeEvaluated)
        {
            return SubtreeEvaluator.Evaluate(Nominator.Nominate(canBeEvaluated, expression), expression);
        }

        // private static methods
        private static Boolean CanBeEvaluatedLocally(Expression expression)
        {
            // any operation on a query can't be done locally
            return expression.NodeType != ExpressionType.Parameter;
        }
    }
}