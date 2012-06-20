using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Oinq.Core
{
    internal class RedundantSubqueryRemover : PigExpressionVisitor
    {
        // constructors
        private RedundantSubqueryRemover()
        {
        }

        // internal static methods
        internal static Expression Remove(Expression expression)
        {
            return new RedundantSubqueryRemover().Visit(expression);
        }

        // protected override methods
        protected override Expression VisitSelect(SelectExpression node)
        {
            node = (SelectExpression)base.VisitSelect(node);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(node.From);
            if (redundant != null)
            {
                node = SubqueryRemover.Remove(node, redundant);
            }

            // next attempt to merge subqueries that would have been removed by the above
            // logic except for the existence of a where clause
            //while (CanMergeWithFrom(node))
            //{
            // SelectExpression fromSelect = (SelectExpression)node.From;

            // // remove the redundant subquery
            // node = SubqueryRemover.Remove(node, fromSelect);

            // // merge where expressions
            // Expression where = node.Where;
            // if (fromSelect.Where != null)
            // {
            // if (where != null)
            // {
            // where = Expression.And(fromSelect.Where, where);
            // }
            // else
            // {
            // where = fromSelect.Where;
            // }
            // }
            // if (where != node.Where)
            // {
            // node = new SelectExpression(node.Type, node.Alias, node.Columns, node.From, where, node.OrderBy, node.GroupBy);
            // }
            //}
            return node;
        }

        protected override Expression VisitSubquery(SubqueryExpression node)
        {
            return base.VisitSubquery(node);
        }

        protected override Expression VisitProjection(ProjectionExpression node)
        {
            node = (ProjectionExpression)base.VisitProjection(node);
            if (node.Source.From is SelectExpression)
            {
                List<SelectExpression> redundant = RedundantSubqueryGatherer.Gather(node.Source);
                if (redundant != null)
                {
                    node = SubqueryRemover.Remove(node, redundant);
                }
            }
            return node;
        }

        // private static methods
        private static Boolean CanMergeWithFrom(SelectExpression node)
        {
            SelectExpression fromSelect = node.From as SelectExpression;
            if (fromSelect == null) return false;
            return (ProjectionIsSimple(fromSelect) || ProjectionIsNameMapOnly(fromSelect))
                && (fromSelect.OrderBy == null || fromSelect.OrderBy.Count == 0)
                && (fromSelect.GroupBy == null || fromSelect.GroupBy.Count == 0);
        }

        private static Boolean ProjectionIsSimple(SelectExpression select)
        {
            foreach (ColumnDeclaration decl in select.Columns)
            {
                ColumnExpression col = decl.Expression as ColumnExpression;
                if (col == null || decl.Name != col.Name)
                {
                    return false;
                }
            }
            return true;
        }

        private static Boolean ProjectionIsNameMapOnly(SelectExpression select)
        {
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect == null || select.Columns.Count != fromSelect.Columns.Count)
                return false;
            // test that all _columns in 'select' are refering to _columns in the same position
            // in 'fromSelect'.
            for (Int32 i = 0, n = select.Columns.Count; i < n; i++)
            {
                ColumnExpression col = select.Columns[i].Expression as ColumnExpression;
                if (col == null || !(col.Name == fromSelect.Columns[i].Name))
                    return false;
            }
            return true;
        }

        class RedundantSubqueryGatherer : PigExpressionVisitor
        {
            // private fields
            List<SelectExpression> _redundant;

            // constructors
            private RedundantSubqueryGatherer()
            {
            }

            // internal static methods
            internal static List<SelectExpression> Gather(Expression source)
            {
                RedundantSubqueryGatherer gatherer = new RedundantSubqueryGatherer();
                gatherer.Visit(source);
                return gatherer._redundant;
            }

            // private static methods
            private static Boolean IsRedudantSubquery(SelectExpression select)
            {
                return (select.From is SelectExpression || select.From is SourceExpression)
                    && (ProjectionIsSimple(select) || ProjectionIsNameMapOnly(select))
                    && (select.Where == null)
                    && (select.Take == null)
                    && (select.OrderBy == null || select.OrderBy.Count == 0)
                    && (select.GroupBy == null || select.GroupBy.Count == 0);
            }

            // protected override methods
            protected override Expression VisitSelect(SelectExpression select)
            {
                if (IsRedudantSubquery(select))
                {
                    if (_redundant == null)
                    {
                        _redundant = new List<SelectExpression>();
                    }
                    _redundant.Add(select);
                }
                return select;
            }
        }
    }
}