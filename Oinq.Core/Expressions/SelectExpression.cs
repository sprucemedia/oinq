using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Oinq.Core
{
    public class SelectExpression : AliasedExpression
    {
        // private fields
        private ReadOnlyCollection<ColumnDeclaration> _columns;
        private Expression _from;
        private ReadOnlyCollection<Expression> _groupBy;
        private ReadOnlyCollection<OrderByExpression> _orderBy;
        private Expression _take;
        private Expression _where;

        // constructors
        public SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where)
            : this(alias, columns, from, where, null, null)
        {
        }

        public SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where, IEnumerable<OrderByExpression> orderBy, 
            IEnumerable<Expression> groupBy)
            : this(alias, columns, from, where, orderBy, groupBy, null)
        {
        }

        public SelectExpression(SourceAlias alias, IEnumerable<ColumnDeclaration> columns,
            Expression from, Expression where, IEnumerable<OrderByExpression> orderBy,
            IEnumerable<Expression> groupBy, Expression take)
            : base(PigExpressionType.Select, typeof(void), alias)
        {
            _columns = columns as ReadOnlyCollection<ColumnDeclaration>;
            if (_columns == null)
            {
                _columns = new List<ColumnDeclaration>(columns).AsReadOnly();
            }
            _orderBy = orderBy as ReadOnlyCollection<OrderByExpression>;
            if (_orderBy == null && orderBy != null)
            {
                _orderBy = new List<OrderByExpression>(orderBy).AsReadOnly();
            }
            _groupBy = groupBy as ReadOnlyCollection<Expression>;
            if (_groupBy == null && groupBy != null)
            {
                _groupBy = new List<Expression>(groupBy).AsReadOnly();
            }
            _take = take;
            _from = from;
            _where = where;
        }

        // public properties
        public ReadOnlyCollection<ColumnDeclaration> Columns
        {
            get { return _columns; }
        }

        public Expression From
        {
            get { return _from; }
        }

        public ReadOnlyCollection<Expression> GroupBy
        {
            get { return _groupBy; }
        }

        public ReadOnlyCollection<OrderByExpression> OrderBy
        {
            get { return _orderBy; }
        }

        public Expression Take
        {
            get { return _take; }
        }

        public Expression Where
        {
            get { return _where; }
        }
    }
}
