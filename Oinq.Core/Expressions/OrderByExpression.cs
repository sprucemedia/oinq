using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an order by clause.
    /// </summary>
    internal class OrderByExpression
    {
        // private fields
        private Expression _key;
        private OrderByDirection _direction;

        // constructors
        /// <summary>
        /// Initializes an instance of the OrderByExpression class.
        /// </summary>
        /// <param name="key">An expression identifying the key of the order by clause.</param>
        /// <param name="direction">The direction of the order by clause.</param>
        internal OrderByExpression(Expression key, OrderByDirection direction)
        {
            _key = key;
            _direction = direction;
        }

        // internal properties
        /// <summary>
        /// Gets the expression identifying the key of the order by clause.
        /// </summary>
        internal Expression Expression
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the direction of the order by clause.
        /// </summary>
        internal OrderByDirection Direction
        {
            get { return _direction; }
        }
    }
}
