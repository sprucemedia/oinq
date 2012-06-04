using System;
using System.Linq.Expressions;

namespace Oinq.Core
{
    /// <summary>
    /// A pairing of an expression and an order type for use in anOrder By clause
    /// </summary>
    internal class OrderExpression
    {
        // construtors
        internal OrderExpression(OrderType orderType, Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            OrderType = orderType;
            Expression = expression;
        }

        // internal properties
        internal OrderType OrderType { get; private set; }
        internal Expression Expression { get; private set; }
    }
}
