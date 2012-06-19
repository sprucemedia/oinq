using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Abstract class representing a subquery.
    /// </summary>
    internal abstract class SubqueryExpression : PigExpression
    {
        // private fields
        private SelectExpression _select;

        // constructors
        internal SubqueryExpression(PigExpressionType eType, Type type, SelectExpression select)
            : base(eType, type)
        {
            _select = select;
        }

        // internal properties
        internal SelectExpression Select
        {
            get { return _select; }
        }
    }
}