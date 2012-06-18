using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public class IsNullExpression : PigExpression
    {
        // private fields
        private Expression _expression;

        // constructors
        public IsNullExpression(Expression expression)
            : base(PigExpressionType.IsNull, typeof(Boolean))
        {
            _expression = expression;
        }

        // public properties
        public Expression Expression
        {
            get { return _expression; }
        }
    }
}