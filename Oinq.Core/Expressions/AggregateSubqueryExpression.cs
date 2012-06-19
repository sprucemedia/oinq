using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an aggregate subquery expression.
    /// </summary>
    internal class AggregateSubqueryExpression : PigExpression
    {
        // private fields
        private SubqueryExpression _aggregateAsSubquery;
        private Expression _aggregateInGroupSelect;
        private SourceAlias _groupByAlias;

        // constructor
        internal AggregateSubqueryExpression(SourceAlias groupByAlias, Expression aggregateInGroupSelect, ScalarExpression aggregateAsSubquery)
            : base(PigExpressionType.AggregateSubquery, aggregateAsSubquery.Type)
        {
            _aggregateInGroupSelect = aggregateInGroupSelect;
            _groupByAlias = groupByAlias;
            _aggregateAsSubquery = aggregateAsSubquery;
        }

        // internal properties
        internal SourceAlias GroupByAlias
        {
            get { return _groupByAlias; }
        }

        internal Expression AggregateInGroupSelect
        {
            get { return _aggregateInGroupSelect; }
        }

        internal SubqueryExpression AggregateAsSubquery
        {
            get { return _aggregateAsSubquery; }
        }
    }
}