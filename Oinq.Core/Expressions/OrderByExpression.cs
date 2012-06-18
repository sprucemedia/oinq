using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// Represents an order by clause.
    /// </summary>
    public class OrderByExpression
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
        public OrderByExpression(Expression key, OrderByDirection direction)
        {
            _key = key;
            _direction = direction;
        }

        // public properties
        /// <summary>
        /// Gets the expression identifying the key of the order by clause.
        /// </summary>
        public Expression Expression
        {
            get { return _key; }
        }

        /// <summary>
        /// Gets the direction of the order by clause.
        /// </summary>
        public OrderByDirection Direction
        {
            get { return _direction; }
        }
    }
}
