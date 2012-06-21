using System.Linq.Expressions;
using System.Reflection;

namespace Oinq
{
    internal class ProjectionBuilder : PigExpressionVisitor
    {
        private ParameterExpression _row;
        private static MethodInfo _getValue;

        internal ProjectionBuilder()
        {
            if (_getValue == null)
            {
                _getValue = typeof(ProjectionRow).GetMethod("GetValue");
            }
        }

        internal LambdaExpression Build(Expression expression)
        {
            _row = Expression.Parameter(typeof(ProjectionRow), "row");
            Expression body = Visit(expression);
            return Expression.Lambda(body, _row);
        }

        protected override Expression VisitColumn(ColumnExpression node)
        {
            return Expression.Convert(Expression.Call(_row, _getValue, Expression.Constant(0)), node.Type);
        }
    }
}
