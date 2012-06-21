using System;

namespace Oinq
{
    internal class ScalarExpression : SubqueryExpression
    {
        internal ScalarExpression(Type type, SelectExpression select)
            : base(PigExpressionType.Scalar, type, select)
        {
        }
    }
}