using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Projector is a visitor that splits an expression representing the result of a query into
    /// two parts, a list of column declarations of expressions that must be evaluated on the server
    /// and a _projector expression that describes how to combine the _columns back into the result object.
    /// </summary>
    internal class Projector : PigExpressionVisitor
    {
        // private fields
        private Dictionary<ColumnExpression, ColumnExpression> _map;
        private List<ColumnDeclaration> _columns;
        private HashSet<String> _columnNames;
        private HashSet<Expression> _candidates;
        private SourceAlias[] _existingAliases;
        private SourceAlias _newAlias;
        private Int32 _iColumn;

        // constructors
        private Projector(Func<Expression, Boolean> canBeColumn, Expression expression, SourceAlias newAlias, params SourceAlias[] existingAliases)
        {
            _map = new Dictionary<ColumnExpression, ColumnExpression>();
            _columns = new List<ColumnDeclaration>();
            _columnNames = new HashSet<String>();
            _newAlias = newAlias;
            _existingAliases = existingAliases;
            _candidates = Nominator.Nominate(canBeColumn, expression);
        }

        // internal static methods
        internal static ProjectedColumns ProjectColumns(Func<Expression, Boolean> canBeColumn, Expression expression,
            SourceAlias newAlias, params SourceAlias[] existingAliases)
        {
            Projector projector = new Projector(canBeColumn, expression, newAlias, existingAliases);
            Expression expr = projector.Visit(expression);
            return new ProjectedColumns(expr, projector._columns.AsReadOnly());
        }

        // protected override methods
        protected override Expression Visit(Expression expression)
        {
            if (_candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType)PigExpressionType.Column)
                {
                    ColumnExpression column = (ColumnExpression)expression;
                    ColumnExpression mapped;
                    if (_map.TryGetValue(column, out mapped))
                    {
                        return mapped;
                    }
                    if (_existingAliases.Contains(column.Alias))
                    {
                        String columnName = GetUniqueColumnName(column.Name);
                        _columns.Add(new ColumnDeclaration(columnName, column));
                        mapped = new ColumnExpression(column.Type, _newAlias, columnName);
                        _map[column] = mapped;
                        _columnNames.Add(columnName);
                        return mapped;
                    }
                    // must be referring to outer scope
                    return column;
                }
                else
                {
                    String columnName = GetNextColumnName();
                    _columns.Add(new ColumnDeclaration(columnName, expression));
                    return new ColumnExpression(expression.Type, _newAlias, columnName);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }

        // private methods
        private String GetNextColumnName()
        {
            return GetUniqueColumnName("node" + (_iColumn++));
        }

        private String GetUniqueColumnName(String name)
        {
            String baseName = name;
            Int32 suffix = 1;
            while (IsColumnNameInUse(name))
            {
                name = baseName + (suffix++);
            }
            return name;
        }

        private Boolean IsColumnNameInUse(String name)
        {
            return _columnNames.Contains(name);
        }
    }
}