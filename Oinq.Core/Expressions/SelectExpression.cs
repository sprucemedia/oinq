using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents a select expression.
    /// </summary>
    public class SelectExpression : AliasedExpression
    {       
        // constructors
        public SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where, IEnumerable<OrderExpression> orderBy, IEnumerable<Expression> groupBy,
            Boolean isDistinct, Expression skip, Expression take, Boolean reverse)
            : base(PigExpressionType.Select, typeof(void), alias)
        {
            Columns = columns as ReadOnlyCollection<ColumnDeclaration>;
            if (Columns == null)
            {
                Columns = new List<ColumnDeclaration>(columns).AsReadOnly();
            }
            OrderBy = orderBy as ReadOnlyCollection<OrderExpression>;
            if (OrderBy == null && orderBy != null)
            {
                OrderBy = new List<OrderExpression>(orderBy).AsReadOnly();
            }

            GroupBy = groupBy as ReadOnlyCollection<Expression>;
            if (GroupBy == null && groupBy != null)
            {
                GroupBy = new List<Expression>(groupBy).AsReadOnly();
            }
            From = from;
            Where = where;
            Skip = skip;
            Take = take;
        }


        public SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns, 
            Expression from, Expression where, IEnumerable<OrderExpression> orderBy, IEnumerable<Expression> groupBy)
            : this(alias, columns, from, where, orderBy, groupBy, false, null, null, false)
        {
        }

        public SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where)
            : this(alias, columns, from, where, null, null)
        {
        }

        // public properties
        public ReadOnlyCollection<ColumnDeclaration> Columns { get; private set; }
        public Expression From { get; private set; }
        public ReadOnlyCollection<Expression> GroupBy { get; private set; }
        public ReadOnlyCollection<OrderExpression> OrderBy { get; private set; }
        public Expression Skip { get; private set; }
        public Expression Take { get; private set; }
        public Expression Where { get; private set; }
    }
}
