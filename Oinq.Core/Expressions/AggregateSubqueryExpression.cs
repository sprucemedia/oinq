using System;
using System.Linq.Expressions;

namespace Oinq.Core
{

    internal class AggregateSubqueryExpression : PigExpression
    {
        internal AggregateSubqueryExpression(SourceAlias groupByAlias, Expression aggregateInGroupSelect, ScalarExpression aggregateAsSubquery)
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
