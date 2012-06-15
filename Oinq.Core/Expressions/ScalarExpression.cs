using System;

namespace Oinq.Core
{
    public class ScalarExpression : SubqueryExpression
    {
        public ScalarExpression(Type type, SelectExpression select)
            : base(PigExpressionType.Scalar, type, select)
        {
        }
    }
}
