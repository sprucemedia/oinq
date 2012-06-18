using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public abstract class SubqueryExpression : PigExpression
    {
        // private fields
        private SelectExpression _select;

        // constructors
        public SubqueryExpression(PigExpressionType eType, Type type, SelectExpression select)
            : base(eType, type)
        {
            _select = select;
        }

        // public properties
        public SelectExpression Select
        {
            get { return _select; }
        }
    }
}