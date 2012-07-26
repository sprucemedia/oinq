using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents a select Pig query.
    /// </summary>
    internal class SelectExpression : AliasedExpression
    {
        // constructors
        internal SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where)
            : this(alias, columns, from, where, null, null)
        {
        }

        internal SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where, IEnumerable<OrderByExpression> orderBy, 
            IEnumerable<Expression> groupBy)
            : this(alias, columns, from, where, orderBy, groupBy, null)
        {
        }

        internal SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where, IEnumerable<OrderByExpression> orderBy,
            IEnumerable<Expression> groupBy, Expression take)
            : base(PigExpressionType.Select, typeof(void), alias)
        {
            Columns = columns as ReadOnlyCollection<ColumnDeclaration> ??
                      new List<ColumnDeclaration>(columns).AsReadOnly();
            OrderBy = orderBy as ReadOnlyCollection<OrderByExpression>;
            if (OrderBy == null && orderBy != null)
            {
                OrderBy = new List<OrderByExpression>(orderBy).AsReadOnly();
            }
            GroupBy = groupBy as ReadOnlyCollection<Expression>;
            if (GroupBy == null && groupBy != null)
            {
                GroupBy = new List<Expression>(groupBy).AsReadOnly();
            }
            Take = take;
            From = from;
            Where = where;
        }

        // internal properties
        internal ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
        internal Expression From { get; private set; }
        internal ReadOnlyCollection<Expression> GroupBy { get; private set; }
        internal ReadOnlyCollection<OrderByExpression> OrderBy { get; private set; }
        internal Expression Take { get; private set; }
        internal Expression Where { get; private set; }
    }
}
