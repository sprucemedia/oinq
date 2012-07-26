using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionVisitor = Oinq.Expressions.ExpressionVisitor;

namespace Oinq
{
    /// <summary>
    /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
    /// </summary>
    internal class SubtreeEvaluator : ExpressionVisitor
    {
        private HashSet<Expression> _candidates;

        // constructors
        internal SubtreeEvaluator(HashSet<Expression> candidates)
        {
            _candidates = candidates;
        }

        // internal methods
        internal Expression Evaluate(Expression exp)
        {
            return Visit(exp);
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
        private static Expression EvaluateSubtree(Expression e)
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
