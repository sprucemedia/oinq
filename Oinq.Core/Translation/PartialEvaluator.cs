using System;
using System.Collections.Generic;
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
        /// <param name="queryProvider">The query provider when the expression is a LINQ query (can be null).</param>
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

        /// <summary>
        /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        class SubtreeEvaluator : ExpressionVisitor
        {
            private HashSet<Expression> _candidates;

            // constructors
            private SubtreeEvaluator(HashSet<Expression> candidates)
            {
                _candidates = candidates;
            }

            // internal methods
            internal static Expression Evaluate(HashSet<Expression> candidates, Expression exp)
            {
                return new SubtreeEvaluator(candidates).Visit(exp);
            }

            // protected override methods
            protected override Expression Visit(Expression exp)
            {
                if (exp == null)
                {
                    return null;
                }
                if (_candidates.Contains(exp))
                {
                    return EvaluateSubtree(exp);
                }
                return base.Visit(exp);
            }

            // private methods
            private Expression EvaluateSubtree(Expression e)
            {
                if (e.NodeType == ExpressionType.Constant)
                {
                    return e;
                }
                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }
    }
}