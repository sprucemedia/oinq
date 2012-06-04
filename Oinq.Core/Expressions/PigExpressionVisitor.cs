using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an Pig-specific implementation of ExpressionVisitor.
    /// </summary>
    internal class PigExpressionVisitor : ExpressionVisitor
    {
        // protected override methods
        protected override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }
            switch ((PigExpressionType)node.NodeType)
            {
                case PigExpressionType.Source:
                    return VisitSource((SourceExpression)node);
                case PigExpressionType.Column:
                    return VisitColumn((ColumnExpression)node);
                case PigExpressionType.Select:
                    return VisitSelect((SelectExpression)node);
                case PigExpressionType.Join:
                    return VisitJoin((JoinExpression)node);
                case PigExpressionType.Aggregate:
                    return VisitAggregate((AggregateExpression)node);
                case PigExpressionType.Subquery:
                    return VisitSubquery((SubqueryExpression)node);
                case PigExpressionType.AggregateSubquery:
                    return VisitAggregateSubquery((AggregateSubqueryExpression)node);
                case PigExpressionType.IsNull:
                    return VisitIsNull((IsNullExpression)node);
                case PigExpressionType.Projection:
                    return VisitProjection((ProjectionExpression)node);
                default:
                    return base.Visit(node);
            }
        }

        protected virtual Expression VisitAggregate(AggregateExpression node)
        {
            Expression arg = Visit(node.Argument);
            if (arg != node.Argument)
            {
                return new AggregateExpression(node.Type, node.AggregateName, arg, node.IsDistinct);
            }
            return node;
        }

        protected virtual Expression VisitAggregateSubquery(AggregateSubqueryExpression node)
        {
            Expression e = Visit(node.AggregateAsSubquery);
            System.Diagnostics.Debug.Assert(e is ScalarExpression);
            var subquery = (ScalarExpression)e;
            if (subquery != node.AggregateAsSubquery)
            {
                return new AggregateSubqueryExpression(node.GroupByAlias, node.AggregateInGroupSelect, subquery);
            }
            return node;
        }

        protected virtual Expression VisitColumn(ColumnExpression node)
        {
            return node;
        }

        protected ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> nodes)
        {
            List<ColumnDeclaration> alternate = null;
            for (Int32 i = 0, n = nodes.Count; i < n; i++)
            {
                ColumnDeclaration column = nodes[i];
                Expression e = Visit(column.Expression);
                if (alternate == null && e != column.Expression)
                {
                    alternate = nodes.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new ColumnDeclaration(column.Name, e));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return nodes;
        }

        protected virtual Expression VisitIsNull(IsNullExpression node)
        {
            Expression expr = Visit(node.Expression);
            if (expr != node.Expression)
            {
                return new IsNullExpression(expr);
            }
            return node;
        }

        protected virtual Expression VisitJoin(JoinExpression node)
        {
            Expression left = VisitSource(node.Left);
            Expression right = VisitSource(node.Right);
            Expression condition = Visit(node.Condition);
            if (left != node.Left || right != node.Right || condition != node.Condition)
            {
                return new JoinExpression(node.Join, left, right, condition);
            }
            return node;
        }

        protected ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> nodes)
        {
            if (nodes != null)
            {
                List<OrderExpression> alternate = null;
                for (int i = 0, n = nodes.Count; i < n; i++)
                {
                    OrderExpression expr = nodes[i];
                    Expression e = Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression)
                    {
                        alternate = nodes.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new OrderExpression(expr.OrderType, e));
                    }
                }
                if (alternate != null)
                {
                    return alternate.AsReadOnly();
                }
            }
            return nodes;
        }

        protected virtual Expression VisitProjection(ProjectionExpression node)
        {
            SelectExpression source = (SelectExpression)Visit(node.Source);
            Expression projector = Visit(node.Projector);
            if (source != node.Source || projector != node.Projector)
            {
                return new ProjectionExpression(source, projector);
            }
            return node;
        }

        protected virtual Expression VisitScalar(ScalarExpression scalar)
        {
            var select = (SelectExpression)this.Visit(scalar.Select);
            if (select != scalar.Select)
            {
                return new ScalarExpression(scalar.Type, select);
            }
            return scalar;
        }

        protected virtual Expression VisitSelect(SelectExpression node)
        {
            Expression from = VisitSource(node.From);
            Expression where = Visit(node.Where);
            ReadOnlyCollection<ColumnDeclaration> columns = VisitColumnDeclarations(node.Columns);
            ReadOnlyCollection<OrderExpression> orderBy = VisitOrderBy(node.OrderBy);
            ReadOnlyCollection<Expression> groupBy = Visit(node.GroupBy);
            if (from != node.From
                || where != node.Where
                || columns != node.Columns
                || orderBy != node.OrderBy
                || groupBy != node.GroupBy)
            {
                return new SelectExpression(node.Alias, columns, from, where, orderBy, groupBy);
            }
            return node;
        }

        protected virtual Expression VisitSource(Expression node)
        {
            return node;
        }

        protected virtual Expression VisitSubquery(SubqueryExpression node)
        {
            SelectExpression select = (SelectExpression)Visit(node.Select);
            if (select != node.Select)
            {
                return this.VisitScalar((ScalarExpression)node);
            }
            return node;
        }
    }
}
