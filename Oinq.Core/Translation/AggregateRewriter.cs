using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Rewrite node expressions, moving them into same select expression that has the group-by clause.
    /// </summary>
    internal class AggregateRewriter : PigExpressionVisitor
    {
        // private fields
        private ILookup<SourceAlias, AggregateSubqueryExpression> _lookup;
        private Dictionary<AggregateSubqueryExpression, Expression> _map;

        // constructors
        private AggregateRewriter(Expression expression)
        {
            _map = new Dictionary<AggregateSubqueryExpression, Expression>();
            _lookup = AggregateGatherer.Gather(expression).ToLookup(a => a.GroupByAlias);
        }

        // internal static fields
        internal static Expression Rewrite(Expression expression)
        {
            return new AggregateRewriter(expression).Visit(expression);
        }

        // protected override methods
        protected override Expression VisitSelect(SelectExpression node)
        {
            node = (SelectExpression)base.VisitSelect(node);
            if (_lookup.Contains(node.Alias))
            {
                List<ColumnDeclaration> aggColumns = new List<ColumnDeclaration>(node.Columns);
                foreach (AggregateSubqueryExpression ae in _lookup[node.Alias])
                {
                    String name = "agg" + aggColumns.Count;
                    ColumnDeclaration cd = new ColumnDeclaration(name, ae.AggregateInGroupSelect);
                    _map.Add(ae, new ColumnExpression(ae.Type, ae.GroupByAlias, name));
                    aggColumns.Add(cd);
                }
                return new SelectExpression(node.Alias, aggColumns, node.From, node.Where, node.OrderBy, node.GroupBy, node.Take);
            }
            return node;
        }

        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression node)
        {
            Expression mapped;
            if (_map.TryGetValue(node, out mapped))
            {
                return mapped;
            }
            return Visit(node.AggregateAsSubquery);
        }

        class AggregateGatherer : PigExpressionVisitor
        {
            // private fields
            private List<AggregateSubqueryExpression> _aggregates = new List<AggregateSubqueryExpression>();

            // constructors
            private AggregateGatherer()
            {
            }

            // internal static fields
            internal static List<AggregateSubqueryExpression> Gather(Expression expression)
            {
                AggregateGatherer gatherer = new AggregateGatherer();
                gatherer.Visit(expression);
                return gatherer._aggregates;
            }

            // protected override fields
            protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression node)
            {
                _aggregates.Add(node);
                return base.VisitAggregateSubquery(node);
            }
        }
    }
}