using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A pairing of an expression and an order type for use in anOrder By clause
    /// </summary>
    public class OrderExpression
    {
        // construtors
        public OrderExpression(OrderType orderType, Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            OrderType = orderType;
            Expression = expression;
        }

        // public properties
        public OrderType OrderType { get; private set; }
        public Expression Expression { get; private set; }
    }
}
