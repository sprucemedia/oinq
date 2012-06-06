using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A visitor that replaces references to one specific instance of a node with another
    /// </summary>
    internal class Replacer : PigExpressionVisitor
    {
        private Expression _searchFor;
        private Expression _replaceWith;

        private Replacer(Expression searchFor, Expression replaceWith)
        {
            _searchFor = searchFor;
            _replaceWith = replaceWith;
        }
        internal static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith)
        {
            return new Replacer(searchFor, replaceWith).Visit(expression);
        }
        protected override Expression Visit(Expression node)
        {
            if (node == _searchFor)
            {
                return _replaceWith;
            }
            return base.Visit(node);
        }
    }
}
