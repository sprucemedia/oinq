using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public class AggregateSubqueryExpression : PigExpression
    {
        // private fields
        private SubqueryExpression _aggregateAsSubquery;
        private Expression _aggregateInGroupSelect;
        private SourceAlias _groupByAlias;

        // constructor
        public AggregateSubqueryExpression(SourceAlias groupByAlias, Expression aggregateInGroupSelect, ScalarExpression aggregateAsSubquery)
            : base(PigExpressionType.AggregateSubquery, aggregateAsSubquery.Type)
        {
            _aggregateInGroupSelect = aggregateInGroupSelect;
            _groupByAlias = groupByAlias;
            _aggregateAsSubquery = aggregateAsSubquery;
        }

        // public properties
        public SourceAlias GroupByAlias
        {
            get { return _groupByAlias; }
        }

        public Expression AggregateInGroupSelect
        {
            get { return _aggregateInGroupSelect; }
        }

        public SubqueryExpression AggregateAsSubquery
        {
            get { return _aggregateAsSubquery; }
        }
    }
}