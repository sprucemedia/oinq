using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public abstract class SubqueryExpression : PigExpression
    {
        // constructors
        public SubqueryExpression(PigExpressionType eType, Type type, SelectExpression select)
            : base(eType, type)
        {
            Select = select;
        }

        // public properties
        public SelectExpression Select { get; private set; }
    }
}
