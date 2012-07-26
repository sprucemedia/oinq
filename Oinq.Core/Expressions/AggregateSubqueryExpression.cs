using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents an aggregate subquery expression.
    /// </summary>
    internal class AggregateSubqueryExpression : PigExpression
    {
        // constructor
        internal AggregateSubqueryExpression(SourceAlias groupByAlias, Expression aggregateInGroupSelect,
                                             ScalarExpression aggregateAsSubquery)
            : base(PigExpressionType.AggregateSubquery, aggregateAsSubquery.Type)
        {
            AggregateInGroupSelect = aggregateInGroupSelect;
            GroupByAlias = groupByAlias;
            AggregateAsSubquery = aggregateAsSubquery;
        }

        // internal properties
        internal SourceAlias GroupByAlias { get; private set; }
        internal Expression AggregateInGroupSelect { get; private set; }
        internal SubqueryExpression AggregateAsSubquery { get; private set; }
    }
}