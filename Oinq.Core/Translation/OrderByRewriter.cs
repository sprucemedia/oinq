using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Move order-bys to the outermost select
    /// </summary>
    internal class OrderByRewriter : PigExpressionVisitor
    {
        IList<OrderByExpression> gatheredOrderings;
        HashSet<string> uniqueColumns;
        bool isOuterMostSelect;

        private OrderByRewriter()
        {
            this.isOuterMostSelect = true;
        }

        internal static Expression Rewrite(Expression expression)
        {
            return new OrderByRewriter().Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            bool saveIsOuterMostSelect = this.isOuterMostSelect;
            try
            {
                this.isOuterMostSelect = false;
                select = (SelectExpression)base.VisitSelect(select);

                bool hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                bool hasGroupBy = select.GroupBy != null && select.GroupBy.Count > 0;
                bool canHaveOrderBy = saveIsOuterMostSelect || select.Take != null;
                bool canReceiveOrderings = canHaveOrderBy && !hasGroupBy;

                if (hasOrderBy)
                {
                    this.PrependOrderings(select.OrderBy);
                }

                IEnumerable<OrderByExpression> orderings = null;
                if (canReceiveOrderings)
                {
                    orderings = this.gatheredOrderings;
                }
                else if (canHaveOrderBy)
                {
                    orderings = select.OrderBy;
                }
                bool canPassOnOrderings = !saveIsOuterMostSelect && !hasGroupBy;
                ReadOnlyCollection<ColumnDeclaration> columns = select.Columns;
                if (this.gatheredOrderings != null)
                {
                    if (canPassOnOrderings)
                    {
                        HashSet<SourceAlias> producedAliases = AliasesProduced.Gather(select.From);
                        // reproject order expressions using this select's alias so the outer select will have properly formed expressions
                        BindResult project = this.RebindOrderings(this.gatheredOrderings, select.Alias, producedAliases, select.Columns);
                        this.gatheredOrderings = null;
                        this.PrependOrderings(project.Orderings);
                        columns = project.Columns;
                    }
                    else
                    {
                        this.gatheredOrderings = null;
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
                this.isOuterMostSelect = saveIsOuterMostSelect;
            }
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery)
        {
            var saveOrderings = this.gatheredOrderings;
            this.gatheredOrderings = null;
            var result = base.VisitSubquery(subquery);
            this.gatheredOrderings = saveOrderings;
            return result;
        }

        /// <summary>
        /// Add a sequence of order expressions to an accumulated list, prepending so as
        /// to give precedence to the new expressions over any previous expressions
        /// </summary>
        /// <param name="newOrderings"></param>
        protected void PrependOrderings(IList<OrderByExpression> newOrderings)
        {
            if (newOrderings != null)
            {
                if (this.gatheredOrderings == null)
                {
                    this.gatheredOrderings = new List<OrderByExpression>();
                    this.uniqueColumns = new HashSet<string>();
                }
                for (int i = newOrderings.Count - 1; i >= 0; i--)
                {
                    var ordering = newOrderings[i];
                    ColumnExpression column = ordering.Expression as ColumnExpression;
                    if (column != null)
                    {
                        string hash = column.Alias + ":" + column.Name;
                        if (!this.uniqueColumns.Contains(hash))
                        {
                            this.gatheredOrderings.Insert(0, ordering);
                            this.uniqueColumns.Add(hash);
                        }
                    }
                    else
                    {
                        // unless we have full expression tree matching assume its different
                        this.gatheredOrderings.Insert(0, ordering);
                    }
                }
            }
        }

        protected class BindResult
        {
            ReadOnlyCollection<ColumnDeclaration> columns;
            ReadOnlyCollection<OrderByExpression> orderings;
            public BindResult(IEnumerable<ColumnDeclaration> columns, IEnumerable<OrderByExpression> orderings)
            {
                this.columns = columns as ReadOnlyCollection<ColumnDeclaration>;
                if (this.columns == null)
                {
                    this.columns = new List<ColumnDeclaration>(columns).AsReadOnly();
                }
                this.orderings = orderings as ReadOnlyCollection<OrderByExpression>;
                if (this.orderings == null)
                {
                    this.orderings = new List<OrderByExpression>(orderings).AsReadOnly();
                }
            }
            public ReadOnlyCollection<ColumnDeclaration> Columns
            {
                get { return this.columns; }
            }
            public ReadOnlyCollection<OrderByExpression> Orderings
            {
                get { return this.orderings; }
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
                    int iOrdinal = 0;
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
                        string colName = column != null ? column.Name : "c" + iOrdinal;
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
