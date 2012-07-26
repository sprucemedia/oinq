using System.Linq.Expressions;

namespace Oinq.Expressions
{
    /// <summary>
    /// Represents an order by clause.
    /// </summary>
    internal class OrderByExpression
    {
        // constructors
        /// <summary>
        /// Initializes an member of the OrderByExpression class.
        /// </summary>
        /// <param name="key">An expression identifying the key of the order by clause.</param>
        /// <param name="direction">The direction of the order by clause.</param>
        internal OrderByExpression(Expression key, OrderByDirection direction)
        {
            Expression = key;
            Direction = direction;
        }

        // internal properties
        /// <summary>
        /// Gets the expression identifying the key of the order by clause.
        /// </summary>
        internal Expression Expression { get; private set; }

        /// <summary>
        /// Gets the direction of the order by clause.
        /// </summary>
        internal OrderByDirection Direction { get; private set; }
    }
}
