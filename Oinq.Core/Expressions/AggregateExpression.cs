using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an aggregate expression.
    /// </summary>
    internal class AggregateExpression : PigExpression
    {
        // private fields
        private String _aggregateName;
        private Expression _argument;
        private Boolean _isDistinct;

        // constructors
        internal AggregateExpression(Type type, String aggregateName, Expression argument, Boolean isDistinct)
            : base(PigExpressionType.Aggregate, type)
        {
            _aggregateName = aggregateName;
            _argument = argument;
            _isDistinct = isDistinct;
        }

        // internal properties
        internal String AggregateName
        {
            get { return _aggregateName; }
        }

        internal Expression Argument
        {
            get { return _argument; }
        }

        internal Boolean IsDistinct
        {
            get { return _isDistinct; }
        }
    }
}