using System;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Represents a join in a Pig query.
    /// </summary>
    internal class JoinExpression : PigExpression
    {
        // private fields
        private Expression _condition;
        private Expression _left;
        private Expression _right;

        // constructors
        internal JoinExpression(Type type, Expression left, Expression right, Expression condition)
            : base(PigExpressionType.Join, type)
        {
            _left = left;
            _right = right;
            _condition = condition;
        }

        // internal properties
        internal Expression Condition
        {
            get { return _condition; }
        }

        internal Expression Left
        {
            get { return _left; }
        }

        internal Expression Right
        {
            get { return _right; }
        }
    }
}
