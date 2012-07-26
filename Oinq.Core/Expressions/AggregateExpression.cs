using System;
using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents an aggregate expression.
    /// </summary>
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
        internal string AggregateName { get; private set; }
        internal Expression Argument { get; private set; }
        internal bool IsDistinct { get; private set; }
    }
}