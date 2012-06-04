using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    internal class AggregateExpression : PigExpression
    {
        // constructors
        internal AggregateExpression(Type type, String aggregateName, Expression argument, Boolean isDistinct)
            : base(PigExpressionType.Aggregate, type)
        {
            AggregateName = aggregateName;
            Argument = argument;
            IsDistinct = isDistinct;
        }

        // internal properties
        internal String AggregateName { get; private set; }
        internal Expression Argument { get; private set; }
        internal Boolean IsDistinct { get; private set; }
    }
}
