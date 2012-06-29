using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq
{
    /// <summary>
    /// Move order-bys to the outermost select
    /// </summary>
    internal class OrderByRewriter : PigExpressionVisitor
    {
        private IList<OrderByExpression> _gatheredOrderings;
        private HashSet<String> _uniqueColumns;
        private Boolean _isOuterMostSelect;

        private OrderByRewriter()
        {
            _isOuterMostSelect = true;
        }

        internal static Expression Rewrite(Expression expression)
        {
            return new OrderByRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            Boolean saveIsOuterMostSelect = _isOuterMostSelect;
            try
            {
                _isOuterMostSelect = false;
                select = (SelectExpression)base.VisitSelect(select);

                Boolean hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                Boolean hasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
                Boolean canHaveOrderBy = saveIsOuterMostSelect || select.Take != null;
                Boolean canReceiveOrderings = canHaveOrderBy && !hasGroupBy;

                if (hasOrderBy)
                {
                    PrependOrderings(select.OrderBy);
                }

                IEnumerable<OrderByExpression> orderings = null;
                if (canReceiveOrderings)
                {
                    orderings = _gatheredOrderings;
                }
                else if (canHaveOrderBy)
                {
                    orderings = select.OrderBy;
                }
                Boolean canPassOnOrderings = !saveIsOuterMostSelect && !hasGroupBy;
                ReadOnlyCollection<ColumnDeclaration> columns = select.Columns;
                if (_gatheredOrderings != null)
                {
                    if (canPassOnOrderings)
                    {
                        HashSet<SourceAlias> producedAliases = AliasesProduced.Gather(select.From);
                        // reproject order expressions using this select's alias so the outer select will have properly formed expressions
                        BindResult project = RebindOrderings(_gatheredOrderings, select.Alias, producedAliases, select.Columns);
                        _gatheredOrderings = null;
                        PrependOrderings(project.Orderings);
                        columns = project.Columns;
                    }
                    else
                    {
                        _gatheredOrderings = null;
                    }
                }
                if (orderings != select.OrderBy || columns != select.Columns)
                {
                    select = new SelectExpression(select.Alias, columns, select.From, select.Where, orderings, select.GroupBy, select.Take);
                }
                return select;
            }
            finally
            {
                _isOuterMostSelect = saveIsOuterMostSelect;
            }
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            var saveOrderings = _gatheredOrderings;
            _gatheredOrderings = null;
            var result = base.VisitSubquery(subquery);
            _gatheredOrderings = saveOrderings;
            return result;
        }

        /// <summary>
        /// Add a sequence of order expressions to an accumulated list, prepending so as
        /// to give precedence to the new expressions over any previous expressions
        /// </summary>
        /// <param path="newOrderings"></param>
        protected void PrependOrderings(IList<OrderByExpression> newOrderings)
        {
            if (newOrderings != null)
            {
                if (_gatheredOrderings == null)
                {
                    _gatheredOrderings = new List<OrderByExpression>();
                    _uniqueColumns = new HashSet<String>();
                }
                for (Int32 i = newOrderings.Count - 1; i >= 0; i--)
                {
                    var ordering = newOrderings[i];
                    ColumnExpression column = ordering.Expression as ColumnExpression;
                    if (column != null)
                    {
                        String hash = column.Alias + ":" + column.Name;
                        if (!_uniqueColumns.Contains(hash))
                        {
                            _gatheredOrderings.Insert(0, ordering);
                            _uniqueColumns.Add(hash);
                        }
                    }
                    else
                    {
                        // unless we have full expression tree matching assume its different
                        _gatheredOrderings.Insert(0, ordering);
                    }
                }
            }
        }

        protected class BindResult
        {
            private ReadOnlyCollection<ColumnDeclaration> _columns;
            private ReadOnlyCollection<OrderByExpression> _orderings;

            public BindResult(IEnumerable<ColumnDeclaration> columns, IEnumerable<OrderByExpression> orderings)
            {
                _columns = columns as ReadOnlyCollection<ColumnDeclaration>;
                if (_columns == null)
                {
                    _columns = new List<ColumnDeclaration>(columns).AsReadOnly();
                }
                _orderings = orderings as ReadOnlyCollection<OrderByExpression>;
                if (_orderings == null)
                {
                    _orderings = new List<OrderByExpression>(orderings).AsReadOnly();
                }
            }
            public ReadOnlyCollection<ColumnDeclaration> Columns
            {
                get { return _columns; }
            }
            public ReadOnlyCollection<OrderByExpression> Orderings
            {
                get { return _orderings; }
            }
        }

        /// <summary>
        /// Rebind order expressions to reference a new alias and add to column declarations if necessary
        /// </summary>
        protected virtual BindResult RebindOrderings(IEnumerable<OrderByExpression> orderings, SourceAlias alias, HashSet<SourceAlias> existingAliases, IEnumerable<ColumnDeclaration> existingColumns)
        {
            List<ColumnDeclaration> newColumns = null;
            List<OrderByExpression> newOrderings = new List<OrderByExpression>();
            foreach (OrderByExpression ordering in orderings)
            {
                Expression expr = ordering.Expression;
                ColumnExpression column = expr as ColumnExpression;
                if (column == null || (existingAliases != null && existingAliases.Contains(column.Alias)))
                {
                    // check to see if a declared column already contains a similar expression
                    Int32 iOrdinal = 0;
                    foreach (ColumnDeclaration decl in existingColumns)
                    {
                        ColumnExpression declColumn = decl.Expression as ColumnExpression;
                        if (decl.Expression == ordering.Expression ||
                            (column != null && declColumn != null && column.Alias == declColumn.Alias && column.Name == declColumn.Name))
                        {
                            // found it, so make a reference to this column
                            expr = new ColumnExpression(column.Type, alias, decl.Name);
                            break;
                        }
                        iOrdinal++;
                    }
                    // if not already projected, add a new column declaration for it
                    if (expr == ordering.Expression)
                    {
                        if (newColumns == null)
                        {
                            newColumns = new List<ColumnDeclaration>(existingColumns);
                            existingColumns = newColumns;
                        }
                        String colName = column != null ? column.Name : "c" + iOrdinal;
                        newColumns.Add(new ColumnDeclaration(colName, ordering.Expression));
                        expr = new ColumnExpression(expr.Type, alias, colName);
                    }
                    newOrderings.Add(new OrderByExpression(expr, ordering.Direction));
                }
            }
            return new BindResult(existingColumns, newOrderings);
        }
    }
}
