using System;
using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents a join in a Pig query.
    /// </summary>
    internal class JoinExpression : PigExpression
    {
        // constructors
        internal JoinExpression(Type type, Expression left, Expression right, Expression condition)
            : base(PigExpressionType.Join, type)
        {
            Left = left;
            Right = right;
            Condition = condition;
        }

        // internal properties
        internal new Expression Condition { get; private set; }
        internal Expression Left { get; private set; }
        internal Expression Right { get; private set; }
    }
}
