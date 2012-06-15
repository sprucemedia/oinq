using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Evaluates and replaces sub-trees when first candidate is reached (top-down)
    /// </summary>
    internal class SubtreeEvaluator : ExpressionVisitor
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
