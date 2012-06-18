using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Removes column declarations in SelectExpressions that are not referenced.
    /// </summary>
    internal class UnusedColumnRemover : PigExpressionVisitor
    {
        // private fields
        Dictionary<SourceAlias, HashSet<String>> _allColumnsUsed;

        // constructors
        private UnusedColumnRemover()
        {
            _allColumnsUsed = new Dictionary<SourceAlias, HashSet<String>>();
        }

        // internal static methods
        internal static Expression Remove(Expression expression)
        {
            return new UnusedColumnRemover().Visit(expression);
        }

        // protected override methods
        protected override Expression VisitColumn(ColumnExpression node)
        {
            MarkColumnAsUsed(node.Alias, node.Name);
            return node;
        }

        protected override Expression VisitSubquery(SubqueryExpression node)
        {
            System.Diagnostics.Debug.Assert(node.Select.Columns.Count == 1);
            MarkColumnAsUsed(node.Select.Alias, node.Select.Columns[0].Name);
            Expression result = base.VisitSubquery(node);
            return result;
        }

        protected override Expression VisitSelect(SelectExpression node)
        {
            // visit column projection first
            ReadOnlyCollection<ColumnDeclaration> columns = node.Columns;

            List<ColumnDeclaration> alternate = null;
            for (Int32 i = 0, n = node.Columns.Count; i < n; i++)
            {
                ColumnDeclaration decl = node.Columns[i];
                if (IsColumnUsed(node.Alias, decl.Name))
                {
                    Expression expr = Visit(decl.Expression);
                    if (expr != decl.Expression)
                    {
                        decl = new ColumnDeclaration(decl.Name, expr);
                    }
                }
                else
                {
                    decl = null; // null means it gets omitted
                }
                if (decl != node.Columns[i] && alternate == null)
                {
                    alternate = new List<ColumnDeclaration>();
                    for (Int32 j = 0; j < i; j++)
                    {
                        alternate.Add(node.Columns[j]);
                    }
                }
                if (decl != null && alternate != null)
                {
                    alternate.Add(decl);
                }
            }
            if (alternate != null)
            {
                columns = alternate.AsReadOnly();
            }

            Expression take = Visit(node.Take);
            ReadOnlyCollection<Expression> groupbys = VisitExpressionList(node.GroupBy);
            ReadOnlyCollection<OrderByExpression> orderbys = VisitOrderBy(node.OrderBy);
            Expression where = Visit(node.Where);
            Expression from = Visit(node.From);

            ClearColumnsUsed(node.Alias);

            if (columns != node.Columns
                || take != node.Take
                || orderbys != node.OrderBy
                || groupbys != node.GroupBy
                || where != node.Where
                || from != node.From)
            {
                node = new SelectExpression(node.Alias, columns, from, where, orderbys, groupbys, take);
            }

            return node;
        }

        protected override Expression VisitProjection(ProjectionExpression node)
        {
            // visit mapping in reverse order
            Expression projector = Visit(node.Projector);
            SelectExpression source = (SelectExpression)Visit(node.Source);
            if (projector != node.Projector || source != node.Source)
            {
                return new ProjectionExpression(source, projector, node.Aggregator);
            }
            return node;
        }

        // private methods
        private void MarkColumnAsUsed(SourceAlias alias, String name)
        {
            HashSet<String> columns;
            if (!_allColumnsUsed.TryGetValue(alias, out columns))
            {
                columns = new HashSet<String>();
                _allColumnsUsed.Add(alias, columns);
            }
            columns.Add(name);
        }

        private Boolean IsColumnUsed(SourceAlias alias, String name)
        {
            HashSet<String> columnsUsed;
            if (_allColumnsUsed.TryGetValue(alias, out columnsUsed))
            {
                if (columnsUsed != null)
                {
                    return columnsUsed.Contains(name);
                }
            }
            return false;
        }

        private void ClearColumnsUsed(SourceAlias alias)
        {
            this._allColumnsUsed[alias] = new HashSet<String>();
        }
    }
}