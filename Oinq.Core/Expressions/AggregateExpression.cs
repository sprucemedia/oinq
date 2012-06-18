using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public class AggregateExpression : PigExpression
    {
        // private fields
        private String _aggregateName;
        private Expression _argument;
        private Boolean _isDistinct;

        // constructors
        public AggregateExpression(Type type, String aggregateName, Expression argument, Boolean isDistinct)
            : base(PigExpressionType.Aggregate, type)
        {
            _aggregateName = aggregateName;
            _argument = argument;
            _isDistinct = isDistinct;
        }

        // public properties
        public String AggregateName
        {
            get { return _aggregateName; }
        }

        public Expression Argument
        {
            get { return _argument; }
        }

        public Boolean IsDistinct
        {
            get { return _isDistinct; }
        }
    }
}