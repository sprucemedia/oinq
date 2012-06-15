using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public class IsNullExpression : PigExpression
    {
        // constructors
        public IsNullExpression(Expression expression)
            : base(PigExpressionType.IsNull, typeof(Boolean))
        {
            Expression = expression;
        }

        // public properties
        public Expression Expression { get; private set; }
    }
}
