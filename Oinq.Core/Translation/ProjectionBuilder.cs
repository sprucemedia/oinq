using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Oinq.Core
{
    /// <summary>
    /// ProjectionBuilder is a visitor that converts an projector expression
    /// that constructs result objects out of ColumnExpressions into an actual
    /// LambdaExpression that constructs result objects out of accessing fields
    /// of a ProjectionRow
    /// </summary>
    internal class ProjectionBuilder : PigExpressionVisitor 
    {
        // private fields
        private ParameterExpression _row;
        private SourceAlias _rowAlias;
        private IList<String> _columns;
        private static MethodInfo miGetValue;
        private static MethodInfo miExecuteSubQuery;
        
        // constructors
        private ProjectionBuilder(SourceAlias rowAlias, IList<String> columns) {
            _rowAlias = rowAlias;
            _columns = columns;
            _row = Expression.Parameter(typeof(ProjectionRow), "row");
            if (miGetValue == null) {
                miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
                miExecuteSubQuery = typeof(ProjectionRow).GetMethod("ExecuteSubQuery");
            }
        }

        // internal static methods
        internal static LambdaExpression Build(Expression expression, SourceAlias alias, IList<String> columns) {
            ProjectionBuilder builder = new ProjectionBuilder(alias, columns);
            Expression body = builder.Visit(expression);
            return Expression.Lambda(body, builder._row);
        }

        // protected override methods
        protected override Expression VisitColumn(ColumnExpression column) {
            if (column.Alias == _rowAlias) {
                Int32 iOrdinal = _columns.IndexOf(column.Name);
                return Expression.Convert(
                    Expression.Call(typeof(System.Convert), "ChangeType", null,
                        Expression.Call(_row, miGetValue, Expression.Constant(iOrdinal)),
                        Expression.Constant(column.Type)
                        ),                        
                        column.Type
                    );
            }
            return column;
        }

        protected override Expression VisitProjection(ProjectionExpression proj) {
            LambdaExpression subQuery = Expression.Lambda(base.VisitProjection(proj), _row);
            Type elementType = TypeHelper.GetElementType(subQuery.Body.Type);
            MethodInfo mi = miExecuteSubQuery.MakeGenericMethod(elementType);
            return Expression.Convert(
                Expression.Call(_row, mi, Expression.Constant(subQuery)),
                proj.Type
                );
        }
    }
}
