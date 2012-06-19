using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an IsNull expression.
    /// </summary>
    internal class IsNullExpression : PigExpression
    {
        // private fields
        private Expression _expression;

        // constructors
        internal IsNullExpression(Expression expression)
            : base(PigExpressionType.IsNull, typeof(Boolean))
        {
            _expression = expression;
        }

        // internal properties
        internal Expression Expression
        {
            get { return _expression; }
        }
    }
}